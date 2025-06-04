using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using Core;
using Core.Helper;
using Core.ViewModel.Base;
using Data;
using Helper;
using KursAM2.Managers.Invoices;
using KursAM2.Repositories.RedisRepository;
using KursAM2.ViewModel.Management.Calculations;
using KursDomain;
using KursDomain.Documents.Bank;
using KursDomain.Documents.CommonReferences;
using KursDomain.Documents.Currency;
using KursDomain.ICommon;
using KursDomain.References;
using KursDomain.WindowsManager.WindowsManager;
using Newtonsoft.Json;
using StackExchange.Redis;
using Application = System.Windows.Application;

namespace KursAM2.Managers;

public class BankOperationsManager
{
    private readonly WindowManager winManager = new WindowManager();

    public ObservableCollection<BankStatements> GetBankStatements()
    {
        var result = new ObservableCollection<BankStatements>();
        try
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                var data = ctx.SD_114.Where(_ => (_.IsDeleted ?? false) == false).ToList();
                var bankAcc = ctx.Database.SqlQuery<AccessRight>(
                        $"SELECT DocCode AS DocCode, USR_ID as UserId FROM HD_114 WHERE USR_ID = {GlobalOptions.UserInfo.Id}")
                    .ToList();
                foreach (var item in data.Where(_ => bankAcc.Any(a => a.DocCode == _.DOC_CODE)))
                {
                    var newItem = new BankStatements
                    {
                        BankDC = ((IDocCode)GlobalOptions.ReferencesCache.GetBankAccount(item.DOC_CODE).Bank)
                            .DocCode,
                        Bank = GlobalOptions.ReferencesCache.GetBankAccount(item.DOC_CODE).Bank as Bank,
                        // ReSharper disable once PossibleInvalidOperationException
                        Currency = GlobalOptions.ReferencesCache.GetCurrency(item.CurrencyDC) as Currency,
                        Name = item.BA_ACC_SHORTNAME?.Trim() + " " + item.BA_RASH_ACC.Trim(),
                        Account = item.BA_RASH_ACC,
                        DocCode = item.DOC_CODE,
                        RemainderCHF = 0,
                        RemainderRUB = 0,
                        RemainderSEK = 0,
                        RemainderUSD = 0,
                        RemainderGBP = 0,
                        RemainderEUR = 0
                    };
                    result.Add(newItem);
                }
            }
        }
        catch (Exception e)
        {
            WindowManager.ShowError(e);
        }

        return result;
    }

    public ObservableCollection<BankOperationsViewModel> GetBankOperations(DateTime dateStart, DateTime dateEnd,
        decimal docCode /*SD114DC*/)
    {
        var result = new ObservableCollection<BankOperationsViewModel>();
        try
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                var data = ctx.TD_101
                    .Include(_ => _.SD_101)
                    .Include(_ => _.SD_33)
                    .Include(_ => _.SD_34)
                    .Include(_ => _.SD_34.SD_22)
                    .Include(_ => _.SD_33.SD_22)
                    .Include(_ => _.SD_26)
                    .Include(_ => _.SD_84)
                    .Include(_ => _.AccuredAmountOfSupplierRow)
                    .Where(_ => _.SD_101.VV_ACC_DC == docCode && _.SD_101.VV_START_DATE >= dateStart
                                                              && _.SD_101.VV_START_DATE <= dateEnd).AsNoTracking()
                    .ToList();
                foreach (var item in data)
                    result.Add(new BankOperationsViewModel(item));
            }
        }
        catch (Exception e)
        {
            WindowManager.ShowError(e);
        }

        return result;
    }

    [Obsolete]
    // ReSharper disable once IdentifierTypo
    // ReSharper disable once UnusedMember.Local
    private void RecalRemainder(BankOperationsViewModel item, ALFAMEDIAEntities ctx, decimal delta, decimal bankDc)
    {
        try
        {
            var allCurrencyRemainderCol = new List<UD_101>(ctx.UD_101.Include(_ => _.SD_101)
                .Where(_ => _.SD_101.VV_ACC_DC == bankDc
                            && _.SD_101.VV_START_DATE >= item.Date).ToList());
            var remainderCol = allCurrencyRemainderCol.Where(_ => _.VVU_CRS_DC == item.VVT_CRS_DC).ToList();
            var localRemainderCol = new List<UD_101>();
            DateTime? previosDateRemainder = null;
            if (ctx.SD_101.Any())
                previosDateRemainder = ctx.SD_101
                    .FirstOrDefault(_ => _.VV_START_DATE < item.Date && _.VV_ACC_DC == bankDc)?.VV_START_DATE;
            var previosRemainders = new List<UD_101>();
            if (previosDateRemainder != null)
                previosRemainders = ctx.UD_101.Include(_ => _.SD_101).Where(_ =>
                    _.VVU_CRS_DC == item.VVT_CRS_DC && _.VVU_REST_TYPE == 1 &&
                    _.SD_101.VV_START_DATE == previosDateRemainder).ToList();
            if (remainderCol.Count != 0)
            {
                var thisDate = remainderCol.Where(_ => _.SD_101.VV_START_DATE == item.Date).ToList();
                if (thisDate.Count != 0)
                {
                    var startRemainder = previosRemainders.FirstOrDefault(_ => _.VVU_CRS_DC == item.VVT_CRS_DC)
                        ?.VVU_VAL_SUMMA ?? 0;
                    var endRemainder = thisDate.First(_ => _.VVU_REST_TYPE == 1).VVU_VAL_SUMMA;
                    AddItemInLocalRemaindedCol(item.VVT_CRS_DC, startRemainder,
                        Convert.ToDecimal(endRemainder + delta), item.DocCode);
                    RecalcLocalRemaindersCol();
                }
                else
                {
                    AddNewRemainder();
                    RecalcLocalRemaindersCol();
                }
            }
            else
            {
                AddNewRemainder();
            }

            void AddNewRemainder()
            {
                if (previosRemainders.Count != 0)
                    foreach (var crs in previosRemainders.Select(_ => _.VVU_CRS_DC).Distinct().ToList())
                    {
                        var previosEndRemainder =
                            Convert.ToDecimal(previosRemainders.First(_ => _.VVU_CRS_DC == crs).VVU_VAL_SUMMA);
                        var endRemainder = previosEndRemainder + delta;
                        AddItemInLocalRemaindedCol(crs, previosEndRemainder, endRemainder, item.DocCode);
                    }
                else
                    AddItemInLocalRemaindedCol(item.VVT_CRS_DC, 0, delta, item.DocCode);
            }

            void RecalcLocalRemaindersCol()
            {
                foreach (var dateDC in allCurrencyRemainderCol.Where(_ => _.SD_101.VV_START_DATE > item.Date)
                             .Select(_ => _.DOC_CODE).Distinct())
                {
                    var thisDateOpWithInThisCur = allCurrencyRemainderCol.Where(_ => _.DOC_CODE == dateDC)
                        .Where(_ => _.VVU_CRS_DC == item.VVT_CRS_DC).ToList();
                    var valStart = Convert.ToDecimal(thisDateOpWithInThisCur
                        .FirstOrDefault(_ => _.VVU_REST_TYPE == 0)?.VVU_VAL_SUMMA);
                    var valStop = Convert.ToDecimal(thisDateOpWithInThisCur
                        .FirstOrDefault(_ => _.VVU_REST_TYPE == 1)?.VVU_VAL_SUMMA);
                    if (thisDateOpWithInThisCur.Count != 0)
                        AddItemInLocalRemaindedCol(item.VVT_CRS_DC, valStart + delta, valStop + delta, dateDC);
                    else
                        AddItemInLocalRemaindedCol(item.VVT_CRS_DC, delta, delta, dateDC);
                }
            }

            foreach (var del in remainderCol)
                ctx.UD_101.Remove(del);
            foreach (var s in localRemainderCol)
                ctx.UD_101.Add(s);

            void AddItemInLocalRemaindedCol(decimal crs, decimal valStart, decimal valEnd, decimal deateDC)
            {
                var newItemStart = new UD_101
                {
                    VVU_VAL_SUMMA = valStart,
                    DOC_CODE = deateDC,
                    VVU_CRS_DC = crs,
                    VVU_REST_TYPE = 0,
                    VVU_RUB_SUMMA = 0
                };
                var newItemEnd = new UD_101
                {
                    VVU_VAL_SUMMA = valEnd,
                    DOC_CODE = deateDC,
                    VVU_CRS_DC = crs,
                    VVU_REST_TYPE = 1,
                    VVU_RUB_SUMMA = 0
                };
                localRemainderCol.Add(newItemStart);
                localRemainderCol.Add(newItemEnd);
            }
        }
        catch (Exception e)
        {
            WindowManager.ShowError(e);
        }
    }

    public string CheckForNonzero(decimal bankDC, ALFAMEDIAEntities ctx)
    {
        var sql = "SELECT  AccountDC ,Date ,Start ," +
                  "SummaIn ,SummaOut ,[End] " +
                  $"FROM dbo.BankOperations (nolock) WHERE AccountDC = {CustomFormat.DecimalToSqlDecimal(bankDC)} " +
                  " order by 2";
        var data = ctx.Database.SqlQuery<BankOperations>(sql).ToList();
        var accountInfo = ctx.SD_114.SingleOrDefault(_ => _.DOC_CODE == bankDC);
        if (accountInfo == null) return $"Нет счета с кодом {bankDC}";
        if (accountInfo.BA_NEGATIVE_RESTS == 1)
        {
            var firstZero = data.FirstOrDefault(_ => _.Date >= accountInfo.DateNonZero
                                                     && _.End < 0);
            if (firstZero != null)
                // ReSharper disable once PossibleInvalidOperationException
                return $"Возникли отрицательные остатки на {firstZero.Date.Value.ToShortDateString()}" +
                       $" в размере {firstZero.End}";
        }

        return null;
    }


    private void recalcRest(List<BankOperations> opers)
    {
        var d = opers.First().Start ?? 0;
        foreach (var op in opers)
        {
            op.End = d + op.SummaIn - op.SummaOut;
            d = op.End ?? 0;
        }
    }

    public string CheckForNonzero(decimal bankDC, DateTime date, decimal sumOut)
    {
        using (var ctx = GlobalOptions.GetEntities())
        {
            var sql = "SELECT  AccountDC ,Date ,Start ," +
                      "SummaIn ,SummaOut ,[End] " +
                      $"FROM dbo.BankOperations (nolock) WHERE AccountDC = {CustomFormat.DecimalToSqlDecimal(bankDC)} " +
                      " order by 2";
            var data = ctx.Database.SqlQuery<BankOperations>(sql).ToList();
            var accountInfo = ctx.SD_114.SingleOrDefault(_ => _.DOC_CODE == bankDC);
            if (accountInfo == null) return $"Нет счета с кодом {bankDC}";
            if ((accountInfo.BA_NEGATIVE_RESTS ?? 0) == 0)
            {
                var old = data.FirstOrDefault(_ => _.Date == date);
                if (old != null)
                    old.SummaOut -= sumOut;
                else
                    data.Add(new BankOperations
                    {
                        Date = date,
                        Start = data.FirstOrDefault(_ => _.Date < date)?.End ?? 0,
                        SummaIn = 0,
                        SummaOut = sumOut,
                        End = (data.FirstOrDefault(_ => _.Date < date)?.End ?? 0) - sumOut
                    });

                recalcRest(data);
                var firstZero = data.FirstOrDefault(_ => _.Date >= (accountInfo.DateNonZero ?? DateTime.MinValue)
                                                         && _.End < 0);
                if (firstZero != null)
                    // ReSharper disable once PossibleInvalidOperationException
                    return $"Возникли отрицательные остатки на {firstZero.Date.Value.ToShortDateString()}" +
                           $" в размере {firstZero.End}. Сохранение не возможно";
            }
        }

        return null;
    }

    public void SaveBankOperations(BankOperationsViewModel item, decimal bankDc, decimal insertDelta)
    {
        if (item.State == RowStatus.NewRow)
        {
            if (item.VVT_VAL_RASHOD > 0)
            {
                var errStr = CheckForNonzero(bankDc, item.Date, item.VVT_VAL_RASHOD.Value);
                if (!string.IsNullOrEmpty(errStr))
                {
                    winManager.ShowWinUIMessageBox(errStr, "Предупреждение", MessageBoxButton.OK);
                    return;
                }
            }

            AddBankOperation(item, bankDc);
            return;
        }

        if (item.State == RowStatus.Edited)
            using (var ctx = GlobalOptions.GetEntities())
            {
                if (item.VVT_VAL_RASHOD > 0)
                {
                    var s = 0m;
                    var old = ctx.TD_101.FirstOrDefault(_ => _.CODE == item.Code);
                    if (old != null)
                        s = old.VVT_VAL_RASHOD ?? 0;
                    var errStr = CheckForNonzero(bankDc, item.Date, s - item.VVT_VAL_RASHOD ?? 0);
                    if (!string.IsNullOrEmpty(errStr))
                    {
                        winManager.ShowWinUIMessageBox(errStr, "Предупреждение", MessageBoxButton.OK);
                        return;
                    }
                }

                var tran = ctx.Database.BeginTransaction();
                try
                {
                    if (item.VVT_SFACT_CLIENT_DC != null && item.VVT_SFACT_POSTAV_DC != null)
                        if (item.VVT_SFACT_POSTAV_DC != null)
                        {
                            var sql =
                                "SELECT s26.DocCode as DocCode, s26.SF_CRS_SUMMA as Summa, " +
                                "SUM(ISNULL(s34.CRS_SUMMA,0)+ISNULL(t101.VVT_VAL_RASHOD,0) " +
                                "+ ISNULL(t110.VZT_CRS_SUMMA,0)) AS PaySumma " +
                                "FROM sd_26 s26 " +
                                "LEFT OUTER JOIN sd_34 s34 ON s34.SPOST_DC = s26.DocCode " +
                                "LEFT OUTER JOIN td_101 t101 " +
                                $"ON t101.VVT_SFACT_POSTAV_DC = s26.DocCode AND t101.CODE != {item.Code} " +
                                "LEFT OUTER JOIN td_110 t110 ON t110.VZT_SPOST_DC = s26.DocCode " +
                                $"WHERE s26.DocCode = {CustomFormat.DecimalToSqlDecimal(item.VVT_SFACT_POSTAV_DC)} " +
                                "GROUP BY s26.DocCode, s26.SF_CRS_SUMMA ";
                            var pays = ctx.Database.SqlQuery<InvoicesManager.InvoicePayment>(sql)
                                .FirstOrDefault();
                            if (pays != null)
                                if (pays.Summa < pays.PaySumma + item.VVT_VAL_RASHOD)
                                {
                                    var res = winManager.ShowWinUIMessageBox(Application.Current.MainWindow,
                                        $"Сумма счета {pays.Summa:n2} меньше сумм платежей {pays.PaySumma + item.VVT_VAL_RASHOD:n2}. Установить максимально возможную сумму {pays.Summa - pays.PaySumma}?",
                                        "Предупреждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                                    switch (res)
                                    {
                                        case MessageBoxResult.Yes:
                                            item.VVT_VAL_RASHOD = pays.Summa - pays.PaySumma;
                                            item.VVT_KONTR_CRS_SUMMA = pays.Summa - pays.PaySumma;
                                            break;
                                        case MessageBoxResult.No:
                                            return;
                                    }
                                }

                            ctx.Database.ExecuteSqlCommand(
                                $"EXEC dbo.GenerateSFProviderCash {CustomFormat.DecimalToSqlDecimal(item.VVT_SFACT_POSTAV_DC)}");
                        }
                        else
                        {
                            var sql =
                                "SELECT s84.DocCode as DocCode, s84.SF_CRS_SUMMA_K_OPLATE as Summa, " +
                                "SUM(ISNULL(s33.CRS_SUMMA,0)+ISNULL(t101.VVT_VAL_PRIHOD,0) " +
                                "+ ISNULL(t110.VZT_CRS_SUMMA,0)) AS PaySumma " +
                                "FROM sd_84 s84 " +
                                "LEFT OUTER JOIN sd_33 s33 ON s33.SFACT_DC = s84.DocCode " +
                                "LEFT OUTER JOIN td_101 t101 ON t101.VVT_SFACT_CLIENT_DC = s84.DocCode  " +
                                "AND t101.CODE != {item.Code} " +
                                "LEFT OUTER JOIN td_110 t110 ON t110.VZT_SFACT_DC = s84.DocCode " +
                                $"WHERE s84.DocCode = {CustomFormat.DecimalToSqlDecimal(item.VVT_SFACT_CLIENT_DC)} " +
                                "GROUP BY s84.DocCode, s84.SF_CRS_SUMMA_K_OPLATE ";
                            var pays = ctx.Database.SqlQuery<InvoicesManager.InvoicePayment>(sql)
                                .FirstOrDefault();
                            if (pays != null)
                                if (pays.Summa < pays.PaySumma + item.VVT_VAL_PRIHOD)
                                {
                                    var res = winManager.ShowWinUIMessageBox(Application.Current.MainWindow,
                                        $"Сумма счета {pays.Summa:n2} меньше сумм платежей {pays.PaySumma + item.VVT_VAL_PRIHOD:n2}. Установить максимально возможную сумму {pays.Summa - pays.PaySumma}?",
                                        "Предупреждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                                    switch (res)
                                    {
                                        case MessageBoxResult.Yes:
                                            item.VVT_VAL_PRIHOD = pays.Summa - pays.PaySumma;
                                            item.VVT_KONTR_CRS_SUMMA = pays.Summa - pays.PaySumma;
                                            break;
                                        case MessageBoxResult.No:
                                            return;
                                    }
                                }

                            ctx.Database.ExecuteSqlCommand(
                                $"EXEC dbo.GenerateSFClientCash {CustomFormat.DecimalToSqlDecimal(item.VVT_SFACT_CLIENT_DC)}");
                        }

                    var oldItem = ctx.TD_101.FirstOrDefault(_ => _.DOC_CODE == item.DocCode
                                                                 && _.CODE == item.Code);
                    if (oldItem == null) return;
                    var oldDC = ctx.SD_101.FirstOrDefault(_ => _.VV_START_DATE == item.Date);
                    if (oldDC == null)
                    {
                        var dc = ctx.SD_101.Max(_ => _.DOC_CODE) + 1;
                        oldDC = new SD_101
                        {
                            DOC_CODE = dc,
                            VV_ACC_DC = item.BankAccount.DocCode,
                            VV_START_DATE = item.Date,
                            VV_STOP_DATE = item.Date
                        };
                        ctx.SD_101.Add(oldDC);
                        oldItem.DOC_CODE = oldDC.DOC_CODE;
                    }

                    oldItem.VVT_DOC_NUM = item.VVT_DOC_NUM;
                    oldItem.VVT_VAL_PRIHOD = item.VVT_VAL_PRIHOD;
                    oldItem.VVT_VAL_RASHOD = item.VVT_VAL_RASHOD;
                    oldItem.VVT_KONTRAGENT = item.Kontragent?.DocCode;
                    oldItem.BankAccountDC = item.BankAccountIn?.DocCode;
                    oldItem.BankFromTransactionCode = item.BankFromTransactionCode;
                    oldItem.VVT_PLATEL_POLUCH_DC = item.Payment?.DocCode;
                    oldItem.VVT_CRS_DC = item.Currency.DocCode;
                    oldItem.VVT_KONTR_CRS_DC = item.Currency.DocCode;
                    oldItem.VVT_KONTR_CRS_RATE = 1;
                    oldItem.VVT_KONTR_CRS_SUMMA = item.VVT_VAL_PRIHOD + item.VVT_VAL_RASHOD;
                    oldItem.VVT_UCHET_VALUTA_DC = GlobalOptions.SystemProfile.MainCurrency.DocCode;
                    oldItem.VVT_UCHET_VALUTA_RATE = 1;
                    oldItem.VVT_SUMMA_V_UCHET_VALUTE = item.VVT_VAL_PRIHOD - item.VVT_VAL_RASHOD;
                    oldItem.VVT_SF_OPLACHENO = 0;
                    oldItem.VVT_SHPZ_DC = item.VVT_SHPZ_DC;
                    oldItem.VVT_RASH_KASS_ORDER_DC = item.CashOut?.DocCode;
                    oldItem.VVT_KASS_PRIH_ORDER_DC = item.CashIn?.DocCode;
                    oldItem.VVT_SFACT_CLIENT_DC = item.VVT_SFACT_CLIENT_DC;
                    oldItem.VVT_SFACT_POSTAV_DC = item.VVT_SFACT_POSTAV_DC;
                    oldItem.AccuredId = item.AccuredId;
                    oldItem.CurrencyRateForReference = item.CurrencyRateForReference;
                    oldItem.EmployeeDC = item.Employee?.DocCode;

                    if (item.VVT_SFACT_POSTAV_DC != null)
                    {
                        var oldPay = ctx.ProviderInvoicePay.FirstOrDefault(_ => _.BankCode == item.Code);
                        if (oldPay == null)
                        {
                            ctx.ProviderInvoicePay.Add(new ProviderInvoicePay
                            {
                                Id = Guid.NewGuid(),
                                BankCode = item.Code,
                                DocDC = item.VVT_SFACT_POSTAV_DC.Value,
                                Summa = item.VVT_VAL_RASHOD ?? 0,
                                Rate = item.CurrencyRateForReference
                            });
                        }
                        else
                        {
                            oldPay.Summa = item.VVT_VAL_RASHOD ?? 0;
                            oldPay.Rate = item.CurrencyRateForReference;
                        }
                    }

                    DocumentHistoryHelper.SaveHistory(CustomFormat.GetEnumName(DocumentType.Bank), null,
                        item.DocCode, item.Code, (string)item.ToJson());
                    ctx.SaveChanges();
                    var err = CheckForNonzero(bankDc, ctx);
                    if (!string.IsNullOrEmpty(err))
                    {
                        if (tran.UnderlyingTransaction?.Connection != null)
                            tran.Rollback();
                        winManager.ShowWinUIMessageBox(err, "Ошибка", MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        return;
                    }

                    tran.Commit();
                    item.State = RowStatus.NotEdited;
                    ConnectionMultiplexer redis;
                    ISubscriber mySubscriber = null;
                    try
                    {
                        redis = ConnectionMultiplexer.Connect(ConfigurationManager.AppSettings["redis.connection"]);
                        mySubscriber = redis.GetSubscriber();
                        var message = new RedisMessage
                        {
                            DocumentType = DocumentType.Bank,
                            DocCode = item.DocCode,
                            DocDate = item.Date,
                            IsDocument = true,
                            OperationType = RedisMessageDocumentOperationTypeEnum.Update,
                            Message =
                                $"Пользователь '{GlobalOptions.UserInfo.Name}' изменил банк.транзакцию {item.Description}"
                        };
                        message.ExternalValues.Add("RowCode", item.Code);
                        message.ExternalValues.Add("BankDC", bankDc);
                        message.ExternalValues.Add("KontragentDC", item.Kontragent?.DocCode);
                        message.ExternalValues.Add("InvoiceDC", item.VVT_SFACT_CLIENT_DC);
                        var jsonSerializerSettings = new JsonSerializerSettings
                        {
                            TypeNameHandling = TypeNameHandling.All
                        };
                        var json = JsonConvert.SerializeObject(message, jsonSerializerSettings);
                        mySubscriber.Publish(new RedisChannel(RedisMessageChannels.Bank, RedisChannel.PatternMode.Auto),
                            json);
                        mySubscriber?.UnsubscribeAll();
                    }
                    catch
                    {
                        Console.WriteLine(
                            $@"Redis {ConfigurationManager.AppSettings["redis.connection"]} не обнаружен");
                    }
                }
                catch (Exception ex)
                {
                    if (tran.UnderlyingTransaction?.Connection != null)
                        tran.Rollback();
                    WindowManager.ShowError(ex);
                }

                if (item.VVT_KONTRAGENT != null)
                    RecalcKontragentBalans.CalcBalans((decimal)item.VVT_KONTRAGENT, item.Date);
            }
    }

    private void AddBankOperation(BankOperationsViewModel item, decimal bankDc)
    {
        ConnectionMultiplexer redis;
        ISubscriber mySubscriber = null;

        try
        {
            redis = ConnectionMultiplexer.Connect(ConfigurationManager.AppSettings["redis.connection"]);
            mySubscriber = redis.GetSubscriber();
        }
        catch
        {
            Console.WriteLine($@"Redis {ConfigurationManager.AppSettings["redis.connection"]} не обнаружен");
        }

        using (var ctx = GlobalOptions.GetEntities())
        {
            var tran = ctx.Database.BeginTransaction();
            try
            {
                var thisDate =
                    ctx.SD_101.FirstOrDefault(_ => _.VV_START_DATE == item.Date && _.VV_ACC_DC == bankDc);
                var code = 0;
                if (ctx.TD_101.Any())
                    code = ctx.TD_101.Max(_ => _.CODE) + 1;
                if (thisDate != null && item.State == RowStatus.NewRow)
                {
                    ctx.TD_101.Add(new TD_101
                    {
                        DOC_CODE = thisDate.DOC_CODE,
                        CODE = code,
                        VVT_DOC_NUM = item.VVT_DOC_NUM,
                        VVT_VAL_PRIHOD = item.VVT_VAL_PRIHOD,
                        VVT_VAL_RASHOD = item.VVT_VAL_RASHOD,
                        VVT_KONTRAGENT = item.Kontragent?.DocCode,
                        BankAccountDC = item.BankAccountIn?.DocCode,
                        BankFromTransactionCode = item.BankFromTransactionCode,
                        VVT_PLATEL_POLUCH_DC = item.Payment?.DocCode,
                        VVT_CRS_DC = item.Currency.DocCode,
                        VVT_KONTR_CRS_DC = item.Currency.DocCode,
                        VVT_KONTR_CRS_RATE = 1,
                        VVT_KONTR_CRS_SUMMA = item.VVT_VAL_PRIHOD + item.VVT_VAL_RASHOD,
                        VVT_UCHET_VALUTA_DC = GlobalOptions.SystemProfile.MainCurrency.DocCode,
                        VVT_UCHET_VALUTA_RATE = 1,
                        VVT_SUMMA_V_UCHET_VALUTE =
                            Convert.ToDecimal(item.VVT_VAL_PRIHOD) - Convert.ToDecimal(item.VVT_VAL_RASHOD),
                        VVT_SF_OPLACHENO = 0,
                        VVT_SHPZ_DC = item.VVT_SHPZ_DC,
                        VVT_RASH_KASS_ORDER_DC = item.CashOut?.DocCode,
                        VVT_KASS_PRIH_ORDER_DC = item.CashIn?.DocCode,
                        VVT_SFACT_CLIENT_DC = item.VVT_SFACT_CLIENT_DC,
                        VVT_SFACT_POSTAV_DC = item.VVT_SFACT_POSTAV_DC,
                        AccuredId = item.AccuredId,
                        CurrencyRateForReference = item.CurrencyRateForReference,
                        EmployeeDC = item.Employee?.DocCode
                    });
                    item.DocCode = thisDate.DOC_CODE;
                    item.Code = code;
                    item.myState = RowStatus.NotEdited;
                }

                if (thisDate == null && item.State == RowStatus.NewRow)
                {
                    item.DocCode = ctx.SD_101.ToList().Any() ? ctx.SD_101.Max(_ => _.DOC_CODE) + 1 : 11010000001;
                    ctx.SD_101.Add(new SD_101
                    {
                        DOC_CODE = item.DocCode,
                        VV_START_DATE = item.Date,
                        VV_STOP_DATE = item.Date,
                        VV_ACC_DC = bankDc,
                        VV_RUB_MONEY_START = 0,
                        VV_RUB_MONEY_STOP = 0
                    });
                    ctx.TD_101.Add(new TD_101
                    {
                        DOC_CODE = item.DocCode,
                        CODE = code,
                        VVT_DOC_NUM = item.VVT_DOC_NUM,
                        VVT_VAL_PRIHOD = item.VVT_VAL_PRIHOD,
                        VVT_VAL_RASHOD = item.VVT_VAL_RASHOD,
                        VVT_KONTRAGENT = item.Kontragent?.DocCode,
                        BankAccountDC = item.BankAccountIn?.DocCode,
                        BankFromTransactionCode = item.BankFromTransactionCode,
                        VVT_PLATEL_POLUCH_DC = item.Payment?.DocCode,
                        VVT_CRS_DC = item.Currency.DocCode,
                        VVT_KONTR_CRS_DC = item.Currency.DocCode,
                        VVT_KONTR_CRS_RATE = 1,
                        VVT_KONTR_CRS_SUMMA = item.VVT_VAL_PRIHOD + item.VVT_VAL_RASHOD,
                        VVT_UCHET_VALUTA_DC = GlobalOptions.SystemProfile.MainCurrency.DocCode,
                        VVT_UCHET_VALUTA_RATE = 1,
                        VVT_SUMMA_V_UCHET_VALUTE = item.VVT_VAL_PRIHOD - item.VVT_VAL_RASHOD,
                        VVT_SF_OPLACHENO = 0,
                        VVT_SHPZ_DC = item.VVT_SHPZ_DC,
                        VVT_RASH_KASS_ORDER_DC = item.CashOut?.DocCode,
                        VVT_KASS_PRIH_ORDER_DC = item.CashIn?.DocCode,
                        VVT_SFACT_CLIENT_DC = item.VVT_SFACT_CLIENT_DC,
                        VVT_SFACT_POSTAV_DC = item.VVT_SFACT_POSTAV_DC,
                        AccuredId = item.AccuredId,
                        CurrencyRateForReference = item.CurrencyRateForReference,
                        EmployeeDC = item.Employee?.DocCode
                    });
                }

                if (item.VVT_SFACT_POSTAV_DC != null)
                    ctx.ProviderInvoicePay.Add(new ProviderInvoicePay
                    {
                        Id = Guid.NewGuid(),
                        BankCode = code,
                        DocDC = item.VVT_SFACT_POSTAV_DC.Value,
                        Summa = item.VVT_VAL_RASHOD ?? 0
                    });
                DocumentHistoryHelper.SaveHistory(CustomFormat.GetEnumName(DocumentType.Bank), null,
                    item.DocCode, item.Code, (string)item.ToJson());
                ctx.SaveChanges();
                var err = CheckForNonzero(bankDc, ctx);
                if (!string.IsNullOrEmpty(err))
                {
                    if (tran.UnderlyingTransaction?.Connection != null)
                        tran.Rollback();
                    winManager.ShowWinUIMessageBox(err, "Ошибка", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                tran.Commit();
                item.myState = RowStatus.NotEdited;

                var message = new RedisMessage
                {
                    DocumentType = DocumentType.Bank,
                    DocCode = item.DocCode,
                    DocDate = item.Date,
                    IsDocument = true,
                    OperationType = RedisMessageDocumentOperationTypeEnum.Create,
                    Message = $"Пользователь '{GlobalOptions.UserInfo.Name}' создал банк.транзакцию {item.Description}"
                };
                message.ExternalValues.Add("RowCode", code);
                message.ExternalValues.Add("BankDC", bankDc);
                message.ExternalValues.Add("KontragentDC", item.Kontragent?.DocCode);
                message.ExternalValues.Add("InvoiceDC", item.VVT_SFACT_CLIENT_DC);
                var jsonSerializerSettings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All
                };
                var json = JsonConvert.SerializeObject(message, jsonSerializerSettings);
                mySubscriber?.Publish(new RedisChannel(RedisMessageChannels.Bank, RedisChannel.PatternMode.Auto), json);
                mySubscriber?.UnsubscribeAll();
            }
            catch (Exception ex)
            {
                if (tran.UnderlyingTransaction?.Connection != null)
                    tran.Rollback();
                WindowManager.ShowError(ex);
            }
        }

        if (item.VVT_KONTRAGENT != null)
            RecalcKontragentBalans.CalcBalans((decimal)item.VVT_KONTRAGENT, item.Date);
    }

    private Tuple<decimal, int> AddBankOperation2(ALFAMEDIAEntities ctx, BankOperationsViewModel item,
        decimal bankDc)
    {
        var thisDate = ctx.SD_101.FirstOrDefault(_ => _.VV_START_DATE == item.Date && _.VV_ACC_DC == bankDc);
        var code = 0;
        if (ctx.TD_101.Any())
            code = ctx.TD_101.Max(_ => _.CODE) + 1;
        if (thisDate != null && item.State == RowStatus.NewRow)
        {
            ctx.TD_101.Add(new TD_101
            {
                DOC_CODE = thisDate.DOC_CODE,
                CODE = code,
                VVT_DOC_NUM = item.VVT_DOC_NUM,
                VVT_VAL_PRIHOD = item.VVT_VAL_PRIHOD,
                VVT_VAL_RASHOD = item.VVT_VAL_RASHOD,
                VVT_KONTRAGENT = item.Kontragent?.DocCode,
                BankAccountDC = item.BankAccountIn?.DocCode,
                BankFromTransactionCode = item.BankFromTransactionCode,
                VVT_PLATEL_POLUCH_DC = item.Payment?.DocCode,
                VVT_CRS_DC = item.Currency.DocCode,
                VVT_KONTR_CRS_DC = item.Currency.DocCode,
                VVT_KONTR_CRS_RATE = 1,
                VVT_KONTR_CRS_SUMMA = item.VVT_VAL_PRIHOD + item.VVT_VAL_RASHOD,
                VVT_UCHET_VALUTA_DC = GlobalOptions.SystemProfile.MainCurrency.DocCode,
                VVT_UCHET_VALUTA_RATE = 1,
                VVT_SUMMA_V_UCHET_VALUTE =
                    Convert.ToDecimal(item.VVT_VAL_PRIHOD) - Convert.ToDecimal(item.VVT_VAL_RASHOD),
                VVT_SF_OPLACHENO = 0,
                VVT_SHPZ_DC = item.VVT_SHPZ_DC,
                VVT_RASH_KASS_ORDER_DC = item.CashOut?.DocCode,
                VVT_KASS_PRIH_ORDER_DC = item.CashIn?.DocCode,
                VVT_SFACT_CLIENT_DC = item.VVT_SFACT_CLIENT_DC,
                VVT_SFACT_POSTAV_DC = item.VVT_SFACT_POSTAV_DC,
                IsCurrencyChange = true,
                AccuredId = item.AccuredId,
                CurrencyRateForReference = item.CurrencyRateForReference,
                EmployeeDC = item.Employee?.DocCode
            });
            item.DocCode = thisDate.DOC_CODE;
            ctx.SaveChanges();
            item.myState = RowStatus.NotEdited;
        }

        if (thisDate == null && item.State == RowStatus.NewRow)
        {
            item.DocCode = ctx.SD_101.ToList().Any() ? ctx.SD_101.Max(_ => _.DOC_CODE) + 1 : 11010000001;
            ctx.SD_101.Add(new SD_101
            {
                DOC_CODE = item.DocCode,
                VV_START_DATE = item.Date,
                VV_STOP_DATE = item.Date,
                VV_ACC_DC = bankDc,
                VV_RUB_MONEY_START = 0,
                VV_RUB_MONEY_STOP = 0
            });
            ctx.TD_101.Add(new TD_101
            {
                DOC_CODE = item.DocCode,
                CODE = code,
                VVT_DOC_NUM = item.VVT_DOC_NUM,
                VVT_VAL_PRIHOD = item.VVT_VAL_PRIHOD,
                VVT_VAL_RASHOD = item.VVT_VAL_RASHOD,
                VVT_KONTRAGENT = item.Kontragent?.DocCode,
                BankAccountDC = item.BankAccountIn?.DocCode,
                BankFromTransactionCode = item.BankFromTransactionCode,
                VVT_PLATEL_POLUCH_DC = item.Payment?.DocCode,
                VVT_CRS_DC = item.Currency.DocCode,
                VVT_KONTR_CRS_DC = item.Currency.DocCode,
                VVT_KONTR_CRS_RATE = 1,
                VVT_KONTR_CRS_SUMMA = item.VVT_VAL_PRIHOD + item.VVT_VAL_RASHOD,
                VVT_UCHET_VALUTA_DC = GlobalOptions.SystemProfile.MainCurrency.DocCode,
                VVT_UCHET_VALUTA_RATE = 1,
                VVT_SUMMA_V_UCHET_VALUTE = item.VVT_VAL_PRIHOD - item.VVT_VAL_RASHOD,
                VVT_SF_OPLACHENO = 0,
                VVT_SHPZ_DC = item.VVT_SHPZ_DC,
                VVT_RASH_KASS_ORDER_DC = item.CashOut?.DocCode,
                VVT_KASS_PRIH_ORDER_DC = item.CashIn?.DocCode,
                VVT_SFACT_CLIENT_DC = item.VVT_SFACT_CLIENT_DC,
                VVT_SFACT_POSTAV_DC = item.VVT_SFACT_POSTAV_DC,
                IsCurrencyChange = true,
                AccuredId = item.AccuredId,
                CurrencyRateForReference = item.CurrencyRateForReference,
                EmployeeDC = item.Employee?.DocCode
            });
            ctx.SaveChanges();
            item.myState = RowStatus.NotEdited;
        }

        return new Tuple<decimal, int>(item.DocCode, code);
    }

    public void DeleteBankOperations(BankOperationsViewModel item, decimal bankDc)
    {
        if (item.VVT_VAL_PRIHOD > 0)
        {
            var errStr = CheckForNonzero(bankDc, item.Date, item.VVT_VAL_PRIHOD.Value);
            if (!string.IsNullOrEmpty(errStr))
            {
                winManager.ShowWinUIMessageBox(errStr, "Предупреждение", MessageBoxButton.OK);
                return;
            }
        }

        using (var ctx = GlobalOptions.GetEntities())
        {
            var tran = ctx.Database.BeginTransaction();
            try
            {
                var delItem = ctx.TD_101.FirstOrDefault(_ => _.CODE == item.Code && _.DOC_CODE == item.DocCode);
                if (delItem != null)
                    ctx.TD_101.Remove(delItem);
                var remainderCol = ctx.UD_101
                    .Include(_ => _.SD_101)
                    .Where(_ => _.SD_101.VV_START_DATE >= item.Date && _.SD_101.VV_ACC_DC == bankDc &&
                                _.VVU_CRS_DC == item.VVT_CRS_DC);
                var delta = item.VVT_VAL_PRIHOD - item.VVT_VAL_RASHOD;
                var r = remainderCol.FirstOrDefault(_ => _.VVU_REST_TYPE == 1 && _.SD_101.VV_START_DATE == item.Date);
                if (r != null)
                    r.VVU_VAL_SUMMA -= delta;
                foreach (var i in remainderCol.Where(_ => _.SD_101.VV_START_DATE > item.Date))
                    i.VVU_VAL_SUMMA -= delta;
                var sd = ctx.SD_101.First(_ => _.DOC_CODE == item.DocCode);
                if (sd.TD_101.Count == 0)
                {
                    ctx.SD_101.Remove(sd);
                    var delUdCol = ctx.UD_101.Where(_ => _.DOC_CODE == sd.DOC_CODE);
                    foreach (var d in delUdCol)
                        ctx.UD_101.Remove(d);
                }

                var oldPay = ctx.ProviderInvoicePay.FirstOrDefault(_ => _.BankCode == item.Code);
                if (oldPay != null)
                    ctx.ProviderInvoicePay.Remove(oldPay);
                ctx.SaveChanges();
                var err = CheckForNonzero(bankDc, ctx);
                if (!string.IsNullOrEmpty(err))
                {
                    if (tran.UnderlyingTransaction?.Connection != null)
                        tran.Rollback();
                    winManager.ShowWinUIMessageBox(err, "Ошибка", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                tran.Commit();
                ConnectionMultiplexer redis;
                ISubscriber mySubscriber = null;
                try
                {
                    redis = ConnectionMultiplexer.Connect(ConfigurationManager.AppSettings["redis.connection"]);
                    mySubscriber = redis.GetSubscriber();
                    var message = new RedisMessage
                    {
                        DocumentType = DocumentType.Bank,
                        DocCode = item.DocCode,
                        DocDate = item.Date,
                        IsDocument = true,
                        OperationType = RedisMessageDocumentOperationTypeEnum.Delete,
                        Message =
                            $"Пользователь '{GlobalOptions.UserInfo.Name}' удалил банк.транзакцию {item.Description}"
                    };
                    message.ExternalValues.Add("RowCode", item.Code);
                    message.ExternalValues.Add("BankDC", bankDc);
                    message.ExternalValues.Add("KontragentDC", item.Kontragent?.DocCode);
                    message.ExternalValues.Add("InvoiceDC", item.VVT_SFACT_CLIENT_DC);
                    var jsonSerializerSettings = new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.All
                    };
                    var json = JsonConvert.SerializeObject(message, jsonSerializerSettings);
                    mySubscriber.Publish(new RedisChannel(RedisMessageChannels.Bank, RedisChannel.PatternMode.Auto),
                        json);
                    mySubscriber.UnsubscribeAll();
                }
                catch
                {
                    Console.WriteLine($@"Redis {ConfigurationManager.AppSettings["redis.connection"]} не обнаружен");
                }
            }
            catch (Exception e)
            {
                if (tran.UnderlyingTransaction?.Connection != null)
                    tran.Rollback();
                WindowManager.ShowError(e);
            }
        }
    }

    [Obsolete]
    public RemainderCurrenciesDatePeriod GetRemain(decimal bankDC, DateTime datestart, DateTime dateend)
    {
        var ret = new RemainderCurrenciesDatePeriod
        {
            Id = Guid.NewGuid(),
            DateStart = datestart,
            DateEnd = dateend
        };
        using (var ctx = GlobalOptions.GetEntities())
        {
            var startRemain = ctx.SD_114_StartRemain.Where(_ => _.AccountDC == bankDC).ToList();
            var datastart = ctx.TD_101.Include(_ => _.SD_101).Where(_ =>
                _.SD_101.VV_ACC_DC == bankDC &&
                _.SD_101.VV_START_DATE <= datestart).ToList();
            var dataend = ctx.TD_101.Include(_ => _.SD_101).Where(_ =>
                _.SD_101.VV_ACC_DC == bankDC &&
                _.SD_101.VV_START_DATE <= dateend).ToList();
            var dataoborot = ctx.TD_101.Include(_ => _.SD_101).Where(_ =>
                _.SD_101.VV_ACC_DC == bankDC && _.SD_101.VV_START_DATE >= datestart &&
                _.SD_101.VV_START_DATE <= dateend).ToList();
            ret.SummaStartCHF = (datastart.Where(_ => _.VVT_CRS_DC == CurrencyCode.CHF)
                                    .Sum(_ => _.VVT_VAL_PRIHOD - _.VVT_VAL_RASHOD) ?? 0)
                                + (startRemain.FirstOrDefault(_ =>
                                    _.AccountDC == bankDC && _.CrsDC == CurrencyCode.CHF)?.Summa ?? 0);
            ret.SummaStartEUR = (datastart.Where(_ => _.VVT_CRS_DC == CurrencyCode.EUR)
                                    .Sum(_ => _.VVT_VAL_PRIHOD - _.VVT_VAL_RASHOD) ?? 0)
                                + (startRemain.FirstOrDefault(_ =>
                                    _.AccountDC == bankDC && _.CrsDC == CurrencyCode.EUR)?.Summa ?? 0);
            ret.SummaStartRUB = (datastart.Where(_ => _.VVT_CRS_DC == CurrencyCode.RUB)
                                    .Sum(_ => _.VVT_VAL_PRIHOD - _.VVT_VAL_RASHOD) ?? 0)
                                + (startRemain.FirstOrDefault(_ =>
                                    _.AccountDC == bankDC && _.CrsDC == CurrencyCode.RUB)?.Summa ?? 0);
            ret.SummaStartUSD = (datastart.Where(_ => _.VVT_CRS_DC == CurrencyCode.USD)
                                    .Sum(_ => _.VVT_VAL_PRIHOD - _.VVT_VAL_RASHOD) ?? 0)
                                + (startRemain.FirstOrDefault(_ =>
                                    _.AccountDC == bankDC && _.CrsDC == CurrencyCode.USD)?.Summa ?? 0);
            ret.SummaStartGBP = (datastart.Where(_ => _.VVT_CRS_DC == CurrencyCode.GBP)
                                    .Sum(_ => _.VVT_VAL_PRIHOD - _.VVT_VAL_RASHOD) ?? 0)
                                + (startRemain.FirstOrDefault(_ =>
                                    _.AccountDC == bankDC && _.CrsDC == CurrencyCode.GBP)?.Summa ?? 0);
            ret.SummaStartSEK = (datastart.Where(_ => _.VVT_CRS_DC == CurrencyCode.SEK)
                                    .Sum(_ => _.VVT_VAL_PRIHOD - _.VVT_VAL_RASHOD) ?? 0)
                                + (startRemain.FirstOrDefault(_ =>
                                    _.AccountDC == bankDC && _.CrsDC == CurrencyCode.SEK)?.Summa ?? 0);
            if (datestart == dateend)
            {
                ret.SummaEndCHF = ret.SummaStartCHF;
                ret.SummaEndEUR = ret.SummaStartEUR;
                ret.SummaEndRUB = ret.SummaStartRUB;
                ret.SummaEndUSD = ret.SummaStartUSD;
                ret.SummaEndGBP = ret.SummaStartGBP;
                ret.SummaEndSEK = ret.SummaStartSEK;
            }
            else
            {
                ret.SummaEndCHF = (dataend.Where(_ => _.VVT_CRS_DC == CurrencyCode.CHF)
                                      .Sum(_ => _.VVT_VAL_PRIHOD - _.VVT_VAL_RASHOD) ?? 0)
                                  + (startRemain.FirstOrDefault(_ =>
                                      _.AccountDC == bankDC && _.CrsDC == CurrencyCode.CHF)?.Summa ?? 0);
                ret.SummaEndEUR = (dataend.Where(_ => _.VVT_CRS_DC == CurrencyCode.EUR)
                                      .Sum(_ => _.VVT_VAL_PRIHOD - _.VVT_VAL_RASHOD) ?? 0)
                                  + (startRemain.FirstOrDefault(_ =>
                                      _.AccountDC == bankDC && _.CrsDC == CurrencyCode.EUR)?.Summa ?? 0);
                ret.SummaEndRUB = (dataend.Where(_ => _.VVT_CRS_DC == CurrencyCode.RUB)
                                      .Sum(_ => _.VVT_VAL_PRIHOD - _.VVT_VAL_RASHOD) ?? 0)
                                  + (startRemain.FirstOrDefault(_ =>
                                      _.AccountDC == bankDC && _.CrsDC == CurrencyCode.RUB)?.Summa ?? 0);
                ret.SummaEndUSD = (dataend.Where(_ => _.VVT_CRS_DC == CurrencyCode.USD)
                                      .Sum(_ => _.VVT_VAL_PRIHOD - _.VVT_VAL_RASHOD) ?? 0)
                                  + (startRemain.FirstOrDefault(_ =>
                                      _.AccountDC == bankDC && _.CrsDC == CurrencyCode.USD)?.Summa ?? 0);
                ret.SummaEndGBP = (dataend.Where(_ => _.VVT_CRS_DC == CurrencyCode.GBP)
                                      .Sum(_ => _.VVT_VAL_PRIHOD - _.VVT_VAL_RASHOD) ?? 0)
                                  + (startRemain.FirstOrDefault(_ =>
                                      _.AccountDC == bankDC && _.CrsDC == CurrencyCode.GBP)?.Summa ?? 0);
                ret.SummaEndSEK = (dataend.Where(_ => _.VVT_CRS_DC == CurrencyCode.SEK)
                                      .Sum(_ => _.VVT_VAL_PRIHOD - _.VVT_VAL_RASHOD) ?? 0)
                                  + (startRemain.FirstOrDefault(_ =>
                                      _.AccountDC == bankDC && _.CrsDC == CurrencyCode.SEK)?.Summa ?? 0);
            }

            // ReSharper disable once InconsistentNaming
            if (dataoborot.Any(_ => _.VVT_CRS_DC == CurrencyCode.CHF))
            {
                ret.SummaInCHF = dataoborot.Where(_ => _.VVT_CRS_DC == CurrencyCode.CHF)
                    .Sum(_ => _.VVT_VAL_PRIHOD);
                ret.SummaOutCHF = dataoborot.Where(_ => _.VVT_CRS_DC == CurrencyCode.CHF)
                    .Sum(_ => _.VVT_VAL_RASHOD);
            }

            if (dataoborot.Any(_ => _.VVT_CRS_DC == CurrencyCode.USD))
            {
                ret.SummaInUSD = dataoborot.Where(_ => _.VVT_CRS_DC == CurrencyCode.USD)
                    .Sum(_ => _.VVT_VAL_PRIHOD);
                ret.SummaOutUSD = dataoborot.Where(_ => _.VVT_CRS_DC == CurrencyCode.USD)
                    .Sum(_ => _.VVT_VAL_RASHOD);
            }

            if (dataoborot.Any(_ => _.VVT_CRS_DC == CurrencyCode.EUR))
            {
                ret.SummaInEUR = dataoborot.Where(_ => _.VVT_CRS_DC == CurrencyCode.EUR)
                    .Sum(_ => _.VVT_VAL_PRIHOD);
                ret.SummaOutEUR = dataoborot.Where(_ => _.VVT_CRS_DC == CurrencyCode.EUR)
                    .Sum(_ => _.VVT_VAL_RASHOD);
            }

            if (dataoborot.Any(_ => _.VVT_CRS_DC == CurrencyCode.SEK))
            {
                ret.SummaInSEK = dataoborot.Where(_ => _.VVT_CRS_DC == CurrencyCode.SEK)
                    .Sum(_ => _.VVT_VAL_PRIHOD);
                ret.SummaOutSEK = dataoborot.Where(_ => _.VVT_CRS_DC == CurrencyCode.SEK)
                    .Sum(_ => _.VVT_VAL_RASHOD);
            }

            if (dataoborot.Any(_ => _.VVT_CRS_DC == CurrencyCode.GBP))
            {
                ret.SummaInGBP = dataoborot.Where(_ => _.VVT_CRS_DC == CurrencyCode.GBP)
                    .Sum(_ => _.VVT_VAL_PRIHOD);
                ret.SummaOutGBP = dataoborot.Where(_ => _.VVT_CRS_DC == CurrencyCode.GBP)
                    .Sum(_ => _.VVT_VAL_RASHOD);
            }

            if (dataoborot.Any(_ => _.VVT_CRS_DC == CurrencyCode.RUB))
            {
                ret.SummaInRUB = dataoborot.Where(_ => _.VVT_CRS_DC == CurrencyCode.RUB)
                    .Sum(_ => _.VVT_VAL_PRIHOD);
                ret.SummaOutRUB = dataoborot.Where(_ => _.VVT_CRS_DC == CurrencyCode.RUB)
                    .Sum(_ => _.VVT_VAL_RASHOD);
            }

            return ret;
        }
    }

    [Obsolete]
    public List<RemainderCurrenciesDatePeriod> GetRemains(decimal bankDC, DateTime datestart, DateTime dateend)
    {
        using (var ctx = GlobalOptions.GetEntities())
        {
            var startRemain = ctx.SD_114_StartRemain.Where(_ => _.AccountDC == bankDC).ToList();
            var dataoborot = ctx.TD_101.Include(_ => _.SD_101).Where(_ =>
                _.SD_101.VV_ACC_DC == bankDC && _.SD_101.VV_START_DATE >= datestart &&
                _.SD_101.VV_START_DATE <= dateend).ToList();
            var PeriodAdapter = DatePeriod
                .GenerateIerarhy(dataoborot.Select(_ => _.SD_101.VV_START_DATE).Distinct(),
                    PeriodIerarhy.YearMonthDay).Select(d => new RemainderCurrenciesDatePeriod(d)).ToList()
                .OrderByDescending(_ => _.DateEnd);
            // ReSharper disable once PossibleMultipleEnumeration
            foreach (var d in PeriodAdapter)
            {
                d.SummaStartCHF = (dataoborot.Where(_ => _.VVT_CRS_DC == CurrencyCode.CHF
                                                         && _.SD_101.VV_START_DATE < d.DateStart)
                                      .Sum(_ => _.VVT_VAL_PRIHOD - _.VVT_VAL_RASHOD) ?? 0)
                                  + (startRemain.FirstOrDefault(_ =>
                                      _.AccountDC == bankDC && _.CrsDC == CurrencyCode.CHF)?.Summa ?? 0);
                d.SummaStartEUR = (dataoborot.Where(_ => _.VVT_CRS_DC == CurrencyCode.EUR
                                                         && _.SD_101.VV_START_DATE < d.DateStart)
                                      .Sum(_ => _.VVT_VAL_PRIHOD - _.VVT_VAL_RASHOD) ?? 0)
                                  + (startRemain.FirstOrDefault(_ =>
                                      _.AccountDC == bankDC && _.CrsDC == CurrencyCode.EUR)?.Summa ?? 0);
                d.SummaStartRUB = (dataoborot.Where(_ => _.VVT_CRS_DC == CurrencyCode.RUB
                                                         && _.SD_101.VV_START_DATE < d.DateStart)
                                      .Sum(_ => _.VVT_VAL_PRIHOD - _.VVT_VAL_RASHOD) ?? 0)
                                  + (startRemain.FirstOrDefault(_ =>
                                      _.AccountDC == bankDC && _.CrsDC == CurrencyCode.RUB)?.Summa ?? 0);
                d.SummaStartUSD = dataoborot.Where(_ => _.VVT_CRS_DC == CurrencyCode.USD
                                                        && _.SD_101.VV_START_DATE < d.DateStart)
                    .Sum(_ => _.VVT_VAL_PRIHOD - _.VVT_VAL_RASHOD) ?? 0;
                d.SummaStartGBP = (dataoborot.Where(_ => _.VVT_CRS_DC == CurrencyCode.GBP
                                                         && _.SD_101.VV_START_DATE < d.DateStart)
                                      .Sum(_ => _.VVT_VAL_PRIHOD - _.VVT_VAL_RASHOD) ?? 0)
                                  + (startRemain.FirstOrDefault(_ =>
                                      _.AccountDC == bankDC && _.CrsDC == CurrencyCode.GBP)?.Summa ?? 0);
                d.SummaStartSEK = (dataoborot.Where(_ => _.VVT_CRS_DC == CurrencyCode.SEK
                                                         && _.SD_101.VV_START_DATE < d.DateStart)
                                      .Sum(_ => _.VVT_VAL_PRIHOD - _.VVT_VAL_RASHOD) ?? 0)
                                  + (startRemain.FirstOrDefault(_ =>
                                      _.AccountDC == bankDC && _.CrsDC == CurrencyCode.SEK)?.Summa ?? 0);
                d.SummaEndCHF = (dataoborot.Where(_ => _.VVT_CRS_DC == CurrencyCode.CHF
                                                       && _.SD_101.VV_START_DATE <= d.DateEnd)
                                    .Sum(_ => _.VVT_VAL_PRIHOD - _.VVT_VAL_RASHOD) ?? 0)
                                + (startRemain.FirstOrDefault(_ =>
                                    _.AccountDC == bankDC && _.CrsDC == CurrencyCode.SEK)?.Summa ?? 0);
                d.SummaEndEUR = (dataoborot.Where(_ => _.VVT_CRS_DC == CurrencyCode.EUR
                                                       && _.SD_101.VV_START_DATE <= d.DateEnd)
                                    .Sum(_ => _.VVT_VAL_PRIHOD - _.VVT_VAL_RASHOD) ?? 0)
                                + (startRemain.FirstOrDefault(_ =>
                                    _.AccountDC == bankDC && _.CrsDC == CurrencyCode.EUR)?.Summa ?? 0);
                d.SummaEndRUB = (dataoborot.Where(_ => _.VVT_CRS_DC == CurrencyCode.RUB
                                                       && _.SD_101.VV_START_DATE <= d.DateEnd)
                                    .Sum(_ => _.VVT_VAL_PRIHOD - _.VVT_VAL_RASHOD) ?? 0)
                                + (startRemain.FirstOrDefault(_ =>
                                    _.AccountDC == bankDC && _.CrsDC == CurrencyCode.RUB)?.Summa ?? 0);
                d.SummaEndUSD = (dataoborot.Where(_ => _.VVT_CRS_DC == CurrencyCode.USD
                                                       && _.SD_101.VV_START_DATE <= d.DateEnd)
                                    .Sum(_ => _.VVT_VAL_PRIHOD - _.VVT_VAL_RASHOD) ?? 0)
                                + (startRemain.FirstOrDefault(_ =>
                                    _.AccountDC == bankDC && _.CrsDC == CurrencyCode.USD)?.Summa ?? 0);
                d.SummaEndGBP = (dataoborot.Where(_ => _.VVT_CRS_DC == CurrencyCode.GBP
                                                       && _.SD_101.VV_START_DATE <= d.DateEnd)
                                    .Sum(_ => _.VVT_VAL_PRIHOD - _.VVT_VAL_RASHOD) ?? 0)
                                + (startRemain.FirstOrDefault(_ =>
                                    _.AccountDC == bankDC && _.CrsDC == CurrencyCode.GBP)?.Summa ?? 0);
                d.SummaEndSEK = (dataoborot.Where(_ => _.VVT_CRS_DC == CurrencyCode.SEK
                                                       && _.SD_101.VV_START_DATE <= d.DateEnd)
                                    .Sum(_ => _.VVT_VAL_PRIHOD - _.VVT_VAL_RASHOD) ?? 0)
                                + (startRemain.FirstOrDefault(_ =>
                                    _.AccountDC == bankDC && _.CrsDC == CurrencyCode.SEK)?.Summa ?? 0);

                // ReSharper disable once InconsistentNaming
                if (dataoborot.Any(_ => _.VVT_CRS_DC == CurrencyCode.CHF))
                {
                    d.SummaInCHF = dataoborot.Where(_ => _.VVT_CRS_DC == CurrencyCode.CHF
                                                         && _.SD_101.VV_START_DATE >= d.DateStart &&
                                                         _.SD_101.VV_STOP_DATE <= d.DateEnd)
                        .Sum(_ => _.VVT_VAL_PRIHOD);
                    d.SummaOutCHF = dataoborot.Where(_ => _.VVT_CRS_DC == CurrencyCode.CHF
                                                          && _.SD_101.VV_START_DATE >= d.DateStart &&
                                                          _.SD_101.VV_STOP_DATE <= d.DateEnd)
                        .Sum(_ => _.VVT_VAL_RASHOD);
                }

                if (dataoborot.Any(_ => _.VVT_CRS_DC == CurrencyCode.USD))
                {
                    d.SummaInUSD = dataoborot.Where(_ => _.VVT_CRS_DC == CurrencyCode.USD
                                                         && _.SD_101.VV_START_DATE >= d.DateStart &&
                                                         _.SD_101.VV_STOP_DATE <= d.DateEnd)
                        .Sum(_ => _.VVT_VAL_PRIHOD);
                    d.SummaOutUSD = dataoborot.Where(_ => _.VVT_CRS_DC == CurrencyCode.USD
                                                          && _.SD_101.VV_START_DATE >= d.DateStart &&
                                                          _.SD_101.VV_STOP_DATE <= d.DateEnd)
                        .Sum(_ => _.VVT_VAL_RASHOD);
                }

                if (dataoborot.Any(_ => _.VVT_CRS_DC == CurrencyCode.EUR))
                {
                    d.SummaInEUR = dataoborot.Where(_ => _.VVT_CRS_DC == CurrencyCode.EUR
                                                         && _.SD_101.VV_START_DATE >= d.DateStart &&
                                                         _.SD_101.VV_STOP_DATE <= d.DateEnd)
                        .Sum(_ => _.VVT_VAL_PRIHOD);
                    d.SummaOutEUR = dataoborot.Where(_ => _.VVT_CRS_DC == CurrencyCode.EUR
                                                          && _.SD_101.VV_START_DATE >= d.DateStart &&
                                                          _.SD_101.VV_STOP_DATE <= d.DateEnd)
                        .Sum(_ => _.VVT_VAL_RASHOD);
                }

                if (dataoborot.Any(_ => _.VVT_CRS_DC == CurrencyCode.SEK))
                {
                    d.SummaInSEK = dataoborot.Where(_ => _.VVT_CRS_DC == CurrencyCode.SEK
                                                         && _.SD_101.VV_START_DATE >= d.DateStart &&
                                                         _.SD_101.VV_STOP_DATE <= d.DateEnd)
                        .Sum(_ => _.VVT_VAL_PRIHOD);
                    d.SummaOutSEK = dataoborot.Where(_ => _.VVT_CRS_DC == CurrencyCode.SEK
                                                          && _.SD_101.VV_START_DATE >= d.DateStart &&
                                                          _.SD_101.VV_STOP_DATE <= d.DateEnd)
                        .Sum(_ => _.VVT_VAL_RASHOD);
                }

                if (dataoborot.Any(_ => _.VVT_CRS_DC == CurrencyCode.GBP))
                {
                    d.SummaInGBP = dataoborot.Where(_ => _.VVT_CRS_DC == CurrencyCode.GBP
                                                         && _.SD_101.VV_START_DATE >= d.DateStart &&
                                                         _.SD_101.VV_STOP_DATE <= d.DateEnd)
                        .Sum(_ => _.VVT_VAL_PRIHOD);
                    d.SummaOutGBP = dataoborot.Where(_ => _.VVT_CRS_DC == CurrencyCode.GBP
                                                          && _.SD_101.VV_START_DATE >= d.DateStart &&
                                                          _.SD_101.VV_STOP_DATE <= d.DateEnd)
                        .Sum(_ => _.VVT_VAL_RASHOD);
                }

                if (dataoborot.Any(_ => _.VVT_CRS_DC == CurrencyCode.RUB))
                {
                    d.SummaInRUB = dataoborot.Where(_ => _.VVT_CRS_DC == CurrencyCode.RUB
                                                         && _.SD_101.VV_START_DATE >= d.DateStart &&
                                                         _.SD_101.VV_STOP_DATE <= d.DateEnd)
                        .Sum(_ => _.VVT_VAL_PRIHOD);
                    d.SummaOutRUB = dataoborot.Where(_ => _.VVT_CRS_DC == CurrencyCode.RUB
                                                          && _.SD_101.VV_START_DATE >= d.DateStart &&
                                                          _.SD_101.VV_STOP_DATE <= d.DateEnd)
                        .Sum(_ => _.VVT_VAL_RASHOD);
                }
            }

            // ReSharper disable once PossibleMultipleEnumeration
            return new List<RemainderCurrenciesDatePeriod>(PeriodAdapter);
        }
    }

    public List<ReminderDatePeriod> GetRemains2(decimal bankDC)
    {
        using (var ctx = GlobalOptions.GetEntities())
        {
            var sql = "SELECT  AccountDC ,Date ,Start ," +
                      "SummaIn ,SummaOut ,[End] " +
                      $"FROM dbo.BankOperations WHERE AccountDC = {CustomFormat.DecimalToSqlDecimal(bankDC)} " +
                      " order by 2";
            var data = ctx.Database.SqlQuery<BankOperations>(sql).ToList();
            var PeriodAdapter = DatePeriod
                // ReSharper disable once PossibleInvalidOperationException
                .GenerateIerarhy(data.Select(_ => (DateTime)_.Date).ToList(),
                    PeriodIerarhy.YearMonthDay).Select(d => new ReminderDatePeriod(d)).ToList()
                .OrderByDescending(_ => _.DateEnd);
            // ReSharper disable once PossibleMultipleEnumeration
            // ReSharper disable PossibleMultipleEnumeration
            foreach (var p in PeriodAdapter)
            {
                var dstart = data.FirstOrDefault(_ => _.Date == p.DateStart);
                var dob = data.Where(_ => _.Date >= p.DateStart && _.Date <= p.DateEnd);
                // ReSharper disable PossibleNullReferenceException
                if (p.PeriodType == PeriodType.Day)
                {
                    p.SummaStart = dstart?.Start ?? 0;
                }
                else
                {
                    var dtstart = dob.Min(_ => _.Date);
                    var ds = dob.Single(_ => _.Date == dtstart);
                    p.SummaStart = ds.Start ?? 0;
                }

                p.SummaIn = dob.Sum(_ => _.SummaIn);
                p.SummaOut = dob.Sum(_ => _.SummaOut);
                p.SummaEnd = p.SummaStart + p.SummaIn - p.SummaOut;
                // ReSharper restore PossibleNullReferenceException
            }
            // ReSharper restore PossibleMultipleEnumeration

            // ReSharper disable once PossibleMultipleEnumeration
            return new List<ReminderDatePeriod>(PeriodAdapter);
        }
    }

    public ReminderDatePeriod GetRemains2(decimal bankDC, DateTime DateStart, DateTime DateEnd)
    {
        if (DateStart > DateEnd)
            return new ReminderDatePeriod
            {
                DateEnd = DateStart,
                DateStart = DateEnd,
                SummaStart = 0,
                SummaEnd = 0,
                SummaIn = 0,
                SummaOut = 0,
                Currency = GlobalOptions.ReferencesCache.GetBankAccount(bankDC).Currency as Currency
            };
        using (var ctx = GlobalOptions.GetEntities())
        {
            var sql = "SELECT  AccountDC ,Date ,Start ," +
                      "SummaIn ,SummaOut ,[End] " +
                      $"FROM dbo.BankOperations WHERE AccountDC = {CustomFormat.DecimalToSqlDecimal(bankDC)} " +
                      " order by 2";
            var data = ctx.Database.SqlQuery<BankOperations>(sql).ToList();
            if (data.Count == 0)
                return new ReminderDatePeriod
                {
                    DateEnd = DateEnd,
                    DateStart = DateStart,
                    SummaStart = 0,
                    SummaEnd = 0,
                    SummaIn = 0,
                    SummaOut = 0,
                    Currency = GlobalOptions.ReferencesCache.GetBankAccount(bankDC).Currency as Currency
                };
            var startDate = data.Where(_ => _.Date <= DateStart).Max(_ => _.Date);
            var endDate = data.Where(_ => _.Date <= DateEnd).Max(_ => _.Date);
            if (startDate == null || endDate == null)
                return new ReminderDatePeriod
                {
                    DateEnd = DateStart,
                    DateStart = DateEnd,
                    SummaStart = 0,
                    SummaEnd = 0,
                    SummaIn = 0,
                    SummaOut = 0,
                    Currency = GlobalOptions.ReferencesCache.GetBankAccount(bankDC).Currency as Currency
                };
            return new ReminderDatePeriod
            {
                DateEnd = DateEnd,
                DateStart = DateStart,
                SummaStart = data.First(_ => _.Date == startDate).Start,
                SummaEnd = data.First(_ => _.Date == endDate).End,
                SummaIn = data.Where(_ => _.Date >= DateStart && _.Date <= DateEnd).Sum(_ => _.SummaIn),
                SummaOut = data.Where(_ => _.Date >= DateStart && _.Date <= DateEnd).Sum(_ => _.SummaOut),
                Currency = GlobalOptions.ReferencesCache.GetBankAccount(bankDC).Currency as Currency
            };
        }
    }

    public void SetRemain(RemainderCurrenciesDatePeriod from, RemainderCurrenciesDatePeriod to)
    {
        to.SummaEndCHF = from.SummaEndCHF;
        to.SummaEndEUR = from.SummaEndEUR;
        to.SummaEndGBP = from.SummaEndGBP;
        to.SummaEndRUB = from.SummaEndRUB;
        to.SummaEndUSD = from.SummaEndUSD;
        to.SummaEndSEK = from.SummaEndSEK;
        to.SummaStartCHF = from.SummaStartCHF;
        to.SummaStartEUR = from.SummaStartEUR;
        to.SummaStartGBP = from.SummaStartGBP;
        to.SummaStartRUB = from.SummaStartRUB;
        to.SummaStartUSD = from.SummaStartUSD;
        to.SummaStartSEK = from.SummaStartSEK;
        to.SummaInCHF = from.SummaInCHF;
        to.SummaInEUR = from.SummaInEUR;
        to.SummaInRUB = from.SummaInRUB;
        to.SummaInGBP = from.SummaInGBP;
        to.SummaInSEK = from.SummaInSEK;
        to.SummaInUSD = from.SummaInUSD;
        to.SummaOutCHF = from.SummaOutCHF;
        to.SummaOutEUR = from.SummaOutEUR;
        to.SummaOutRUB = from.SummaOutRUB;
        to.SummaOutGBP = from.SummaOutGBP;
        to.SummaOutSEK = from.SummaOutSEK;
        to.SummaOutUSD = from.SummaOutUSD;
    }

    public BankCurrencyChangeViewModel NewBankCurrencyChange()
    {
        return new BankCurrencyChangeViewModel
        {
            Id = Guid.NewGuid(),
            DocDate = DateTime.Today,
            DocNum = -1,
            State = RowStatus.NewRow,
            CREATOR = GlobalOptions.UserInfo.NickName
        };
    }

    public BankCurrencyChangeViewModel NewCopyBankCurrencyChange(BankCurrencyChangeViewModel doc)
    {
        var newItem = new BankCurrencyChangeViewModel
        {
            Id = Guid.NewGuid(),
            DocDate = DateTime.Today,
            DocNum = -1,
            BankFromDC = doc.BankFromDC,
            BankToDC = doc.BankToDC,
            CrsFromDC = doc.CrsFromDC,
            CrsToDC = doc.CrsToDC,
            DocFromDC = 0,
            DocRowFromCode = 0,
            DocRowToCode = 0,
            DocToDC = 0,
            Note = "",
            Rate = doc.Rate,
            CREATOR = GlobalOptions.UserInfo.NickName,
            State = RowStatus.NewRow
        };
        return newItem;
    }

    public BankCurrencyChangeViewModel LoadBankCurrencyChange(Guid id)
    {
        BankCurrencyChangeViewModel document = null;
        try
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                var doc = ctx.BankCurrencyChange.FirstOrDefault(_ => _.Id == id);
                document = new BankCurrencyChangeViewModel(doc);
            }
        }
        catch (Exception e)
        {
            WindowManager.ShowError(e);
        }

        return document;
    }

    public void AddBankCurrencyChange(BankCurrencyChangeViewModel item)
    {
        using (var ctx = GlobalOptions.GetEntities())
        {
            var tran = ctx.Database.BeginTransaction();
            try
            {
                var bankItemFrom = new BankOperationsViewModel
                {
                    Date = item.DocDate,
                    VVT_DOC_NUM = "Обмен валюты",
                    VVT_VAL_PRIHOD = 0,
                    VVT_VAL_RASHOD = item.SummaFrom,
                    Kontragent = null,
                    BankAccountIn = item.BankTo,
                    BankFromTransactionCode = null,
                    Payment = null,
                    Currency = item.CurrencyFrom,
                    VVT_SHPZ_DC = null,
                    CashIn = null,
                    CashOut = null,
                    VVT_SFACT_CLIENT_DC = null,
                    VVT_SFACT_POSTAV_DC = null,
                    State = RowStatus.NewRow
                };
                var d = AddBankOperation2(ctx, bankItemFrom, item.BankFromDC);
                var bankItemTo = new BankOperationsViewModel
                {
                    Date = item.DocDate,
                    VVT_DOC_NUM = "Обмен валюты",
                    VVT_VAL_PRIHOD = item.SummaTo,
                    VVT_VAL_RASHOD = 0,
                    Kontragent = null,
                    BankAccountIn = null,
                    BankFromTransactionCode = d.Item2,
                    Payment = null,
                    Currency = item.CurrencyTo,
                    VVT_SHPZ_DC = null,
                    CashIn = null,
                    CashOut = null,
                    VVT_SFACT_CLIENT_DC = null,
                    VVT_SFACT_POSTAV_DC = null,
                    State = RowStatus.NewRow
                };
                var d2 = AddBankOperation2(ctx, bankItemTo, item.BankToDC);
                var num = ctx.BankCurrencyChange.ToList().Any() ? ctx.BankCurrencyChange.Max(_ => _.DocNum) + 1 : 1;
                ctx.BankCurrencyChange.Add(new BankCurrencyChange
                {
                    DocDate = item.DocDate,
                    SummaFrom = item.SummaFrom,
                    Id = Guid.NewGuid(),
                    CrsToDC = item.CrsToDC,
                    Note = item.Note,
                    Rate = item.Rate,
                    BankFromDC = item.BankFromDC,
                    CrsFromDC = item.CrsFromDC,
                    BankToDC = item.BankToDC,
                    CREATOR = item.CREATOR,
                    DocRowFromCode = d.Item2,
                    DocRowToCode = d2.Item2,
                    DocFromDC = d.Item1,
                    SummaTo = item.SummaTo,
                    DocNum = num,
                    DocToDC = d2.Item1
                });
                ctx.SaveChanges();
                var err = CheckForNonzero(item.BankToDC, ctx);
                if (!string.IsNullOrWhiteSpace(err))
                {
                    if (tran.UnderlyingTransaction?.Connection != null)
                        tran.Rollback();
                    winManager.ShowWinUIMessageBox(err, "Ошибка", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                var err2 = CheckForNonzero(item.BankToDC, ctx);
                if (!string.IsNullOrEmpty(err2))
                {
                    if (tran.UnderlyingTransaction?.Connection != null)
                        tran.Rollback();
                    winManager.ShowWinUIMessageBox(err2, "Ошибка", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                tran.Commit();
                item.myState = RowStatus.NotEdited;
            }
            catch (Exception ex)
            {
                if (tran.UnderlyingTransaction?.Connection != null)
                    tran.Rollback();
                WindowManager.ShowError(ex);
            }
        }
    }

    public void SaveBankCurrencyChange(BankCurrencyChangeViewModel item)
    {
        using (var ctx = GlobalOptions.GetEntities())
        {
            var tran = ctx.Database.BeginTransaction();
            try
            {
                var bc = ctx.BankCurrencyChange.FirstOrDefault(_ => _.Id == item.Id);
                if (bc == null) return;
                //bc.DocDate = item.DocDate;
                bc.SummaFrom = item.SummaFrom;
                bc.SummaTo = item.SummaTo;
                bc.Rate = item.Rate;
                bc.Note = item.Note;
                var bankto =
                    ctx.TD_101.FirstOrDefault(_ => _.DOC_CODE == item.DocToDC && _.CODE == item.DocRowToCode);
                if (bankto == null) return;
                bankto.VVT_VAL_PRIHOD = item.SummaTo;
                bankto.VVT_VAL_RASHOD = 0;
                var bankfrom =
                    ctx.TD_101.FirstOrDefault(_ => _.DOC_CODE == item.DocFromDC && _.CODE == item.DocRowFromCode);
                if (bankfrom == null) return;
                bankfrom.VVT_VAL_RASHOD = item.SummaFrom;
                bankfrom.VVT_VAL_PRIHOD = 0;
                ctx.SaveChanges();
                tran.Commit();
                item.myState = RowStatus.NotEdited;
            }
            catch (Exception ex)
            {
                if (tran.UnderlyingTransaction.Connection != null)
                    tran.Rollback();
                WindowManager.ShowError(ex);
            }
        }
    }

    public bool DeleteBankCurrencyChange(BankCurrencyChangeViewModel item)
    {
        using (var ctx = GlobalOptions.GetEntities())
        {
            var tran = ctx.Database.BeginTransaction();
            try
            {
                var d1 = ctx.TD_101.FirstOrDefault(_ => _.CODE == item.DocRowToCode);
                var d2 = ctx.TD_101.FirstOrDefault(_ => _.CODE == item.DocRowFromCode);
                var doc = ctx.BankCurrencyChange.FirstOrDefault(_ => _.Id == item.Id);
                if (d1 == null || d2 == null || doc == null)
                    return false;
                ctx.TD_101.Remove(d2);
                ctx.TD_101.Remove(d1);
                ctx.BankCurrencyChange.Remove(doc);
                ctx.SaveChanges();
                tran.Commit();
                return true;
            }
            catch (Exception ex)
            {
                if (tran.UnderlyingTransaction.Connection != null)
                    tran.Rollback();
                WindowManager.ShowError(ex);
                return false;
            }
        }
    }
}
