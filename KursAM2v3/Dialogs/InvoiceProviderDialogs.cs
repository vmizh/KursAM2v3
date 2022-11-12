using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using Core.ViewModel.Base;
using Data;
using KursAM2.View.Base;
using KursAM2.View.DialogUserControl;
using KursDomain;
using KursDomain.Menu;
using KursDomain.References;

namespace KursAM2.Dialogs
{
    public class AccruedAmountSelectRow
    {
        [Display(AutoGenerateField = false)] public Guid Id { set; get; }

        [Display(AutoGenerateField = false)] public Guid DocId { set; get; }

        [Display(AutoGenerateField = false)] public AccuredAmountOfSupplierRow Entity { set; get; }

        [Display(AutoGenerateField = true, Name = "Выбран")]
        public bool IsSelected { set; get; }

        [Display(AutoGenerateField = true, Name = "№")]
        [ReadOnly(true)]
        public string DocNum { set; get; }

        [Display(AutoGenerateField = true, Name = "Дата")]
        [ReadOnly(true)]
        public DateTime DocDate { set; get; }

        [Display(AutoGenerateField = true, Name = "Контрагент")]
        [ReadOnly(true)]
        public Kontragent Kongtragent { set; get; }

        [Display(AutoGenerateField = true, Name = "Начисление")]
        [ReadOnly(true)]
        public Nomenkl Nomenkl { set; get; }

        [Display(AutoGenerateField = true, Name = "Сумма")]
        [ReadOnly(true)]
        public decimal Summa { set; get; }

        [Display(AutoGenerateField = true, Name = "Оплачено")]
        [ReadOnly(true)]
        public decimal PaySumma { set; get; }

        [Display(AutoGenerateField = true, Name = "Распределено")]
        [ReadOnly(true)]
        public decimal DistributeSumm { set; get; }

        [Display(AutoGenerateField = true, Name = "Счет дох/расх")]
        [ReadOnly(true)]
        public SDRSchet SDRSchet { set; get; }

        [Display(AutoGenerateField = true, Name = "Примечание")]
        [ReadOnly(true)]
        public string Note { set; get; }

        [Display(AutoGenerateField = true, Name = "Создатель")]
        [ReadOnly(true)]
        public string Creator { set; get; }

        [Display(AutoGenerateField = true, Name = "Валюта")]
        [ReadOnly(true)]
        public Currency Currency { set; get; }
    }

    public sealed class AccrualAmountDialogs : KursBaseControlViewModel
    {
        public AccrualAmountDialogs()
        {
            ModelView = new StandartDialogSelectUC3(GetType().Name);
            RightMenuBar = MenuGenerator.RefreshOnlyRightBar(this);
        }

        public override string WindowName => "Распределние накладных расходов > Выбор прямых расходов";

        public ObservableCollection<AccruedAmountSelectRow> ItemsCollection { set; get; } =
            new ObservableCollection<AccruedAmountSelectRow>();

        public bool? ShowDialog()
        {
            ItemsCollection.Clear();
            var dsForm = new KursBaseDialog
            {
                Owner = Application.Current.MainWindow
            };
            Form = dsForm;
            dsForm.DataContext = this;
            ((AccrualAmountDialogs) dsForm.DataContext).RefreshData(null);
            return dsForm.ShowDialog();
        }

        public override void RefreshData(object obj)
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                ItemsCollection.Clear();
                var data = ctx.AccuredAmountOfSupplierRow
                    .Include(_ => _.SD_34)
                    .Include(_ => _.TD_101)
                    .Include(_ => _.AccruedAmountOfSupplier)
                    .Include(_ => _.DistributeNakladInfo).OrderByDescending(v => v.AccruedAmountOfSupplier.DocDate)
                    .ToList();
                foreach (var d in data)
                    if (d.Summa - d.DistributeNakladInfo.Sum(x => x.DistributeSumma) > 1)
                        ItemsCollection.Add(new AccruedAmountSelectRow
                        {
                            Id = d.Id,
                            DocId = d.DocId,
                            DocNum = d.AccruedAmountOfSupplier.DocInNum +
                                     (string.IsNullOrWhiteSpace(d.AccruedAmountOfSupplier.DocExtNum)
                                         ? string.Empty
                                         : $"/{d.AccruedAmountOfSupplier.DocExtNum}"),
                            DocDate = d.AccruedAmountOfSupplier.DocDate,
                            Kongtragent =
                                GlobalOptions.ReferencesCache.GetKontragent(d.AccruedAmountOfSupplier.KontrDC) as
                                    Kontragent,
                            Nomenkl = GlobalOptions.ReferencesCache.GetNomenkl(d.NomenklDC) as Nomenkl,
                            Summa = d.Summa,
                            Currency = GlobalOptions.ReferencesCache.GetKontragent(d.AccruedAmountOfSupplier.KontrDC)
                                .Currency as Currency,
                            DistributeSumm = d.DistributeNakladInfo.Sum(x => x.DistributeSumma),
                            // ReSharper disable once MergeConditionalExpression
                            // ReSharper disable once PossibleInvalidOperationException
                            PaySumma = (decimal) ((d.SD_34 != null ? d.SD_34.Sum(x => x.SUMM_ORD ?? 0m) : 0)
                                                  + (d.TD_101 != null ? d.TD_101.Sum(x => x.VVT_VAL_RASHOD) : 0)),
                            IsSelected = false,
                            SDRSchet = GlobalOptions.ReferencesCache.GetSDRSchet(d.SHPZ_DC) as SDRSchet,
                            Creator = d.AccruedAmountOfSupplier.Creator,
                            Note = d.AccruedAmountOfSupplier.Note + " " + d.Note,
                            Entity = d
                        });
            }
        }
    }
}
