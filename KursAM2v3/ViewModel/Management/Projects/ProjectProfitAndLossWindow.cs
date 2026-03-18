using Core.ViewModel.Base;
using DevExpress.Xpf.Grid;
using Helper;
using KursAM2.Managers;
using KursAM2.View.Management;
using KursDomain;
using KursDomain.Menu;
using KursDomain.References;
using KursRepositories.Repositories.MutualAccounting;
using KursRepositories.Repositories.ProfitAndLoss;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace KursAM2.ViewModel.Management.Projects;

public class ProjectProfitAndLossWindow : RSWindowViewModelBase
{
    #region Fields

    private ProfitAndLossesMainRowViewModel myBalansFact;
    private Visibility myGridControlVzaimozachetVisible;
    private Visibility myGridControlCurrencyConvertVisible;
    private Visibility myGridControlBaseExtendVisible;

    #endregion

    #region Constructors

    public ProjectProfitAndLossWindow()
    {
        LeftMenuBar = MenuGenerator.BaseLeftBar(this, new Dictionary<MenuGeneratorItemVisibleEnum, bool>
        {
            [MenuGeneratorItemVisibleEnum.AddSearchlist] = true
        });
        RightMenuBar = MenuGenerator.StandartInfoRightBar(this);
        EndDate = DateTime.Today;
        var d = DateTime.Today.AddMonths(-1);
        StartDate = new DateTime(d.Year, d.Month, 1);
    }

    #endregion

    #region Properties

    public override string WindowName => "Прибыли и убытки по проектам";
    public override string LayoutName => "ProjectProfitAndLossWindow";

    public List<Project> ProjectList => GlobalOptions.ReferencesCache.GetProjectsAll().Cast<Project>()
        .Where(_ => _.ParentId == null && !_.IsExcludeFromProfitAndLoss).ToList();

    public ObservableCollection<ProfitAndLossesMainRowViewModel> Main { set; get; }
    public ObservableCollection<ProfitAndLossesExtendRowViewModel> Extend { set; get; }
    public ObservableCollection<ProfitAndLossesExtendRowViewModel> ExtendActual { set; get; }

    public ObservableCollection<CurrencyConvertRow> CurrencyConvertRows { set; get; }
        = new ObservableCollection<CurrencyConvertRow>();

    public Project CurrentProject
    {
        get;
        set
        {
            if (Equals(value, field)) return;
            field = value;
            RaisePropertyChanged();
        }
    }

    public DateTime StartDate
    {
        get;
        set
        {
            if (value.Equals(field)) return;
            field = value;
            RaisePropertyChanged();
        }
    }

    public DateTime EndDate
    {
        get;
        set
        {
            if (value.Equals(field)) return;
            field = value;
            RaisePropertyChanged();
        }
    }

    public Visibility GridControlVzaimozachetVisible
    {
        set
        {
            if (value == myGridControlVzaimozachetVisible) return;
            myGridControlVzaimozachetVisible = value;
            RaisePropertyChanged();
        }
        get => myGridControlVzaimozachetVisible;
    }

    public Visibility GridControlCurrencyConvertVisible
    {
        set
        {
            if (value == myGridControlCurrencyConvertVisible) return;
            myGridControlCurrencyConvertVisible = value;
            RaisePropertyChanged();
        }
        get => myGridControlCurrencyConvertVisible;
    }

    public Visibility GridControlBaseExtendVisible
    {
        set
        {
            if (value == myGridControlBaseExtendVisible) return;
            myGridControlBaseExtendVisible = value;
            RaisePropertyChanged();
        }
        get => myGridControlBaseExtendVisible;
    }


    public ProfitAndLossesMainRowViewModel BalansFact
    {
        get => myBalansFact;
        set
        {
            if (myBalansFact == value) return;
            myBalansFact = value;
            if (myBalansFact != null)
            {
                setColumnVisible(Form as ProfitAndLoss);
                switch (myBalansFact.Id.ToString())
                {
                    case "459937df-085f-4825-9ae9-810b054d0276":
                    case "30e9bd73-9bda-4d75-b897-332f9210b9b1":
                        setControldVisible(isVzaimozachetVisible: true);
                        UpdateExtend(myBalansFact.Id);
                        setCurrencyColumnVisible(((ProfitAndLoss)Form).GridControlVzaimozachetExtend);
                        break;
                    case "b6f2540a-9593-42e3-b34f-8c0983bc39a2":
                    case "35ebabec-eac3-4c3c-8383-6326c5d64c8c":
                        setControldVisible(isCurrencyConvertVisible: true);
                        UpdateCurrencyConvert(StartDate, EndDate);
                        setCurrencyColumnVisible(((ProfitAndLoss)Form).GridControlCurrencyConvert, true);
                        break;
                    case "35c9783e-e19f-452b-8479-d6f022444552":
                        setControldVisible(isCurrencyConvertVisible: true);
                        UpdateBalansOper(StartDate, EndDate);
                        setCurrencyColumnVisible(((ProfitAndLoss)Form).GridControlCurrencyConvert, true);
                        break;
                    default:
                        setControldVisible(true);
                        UpdateExtend(myBalansFact.Id);
                        setCurrencyColumnVisible(((ProfitAndLoss)Form).GridControlBaseExtend);
                        break;
                }
            }

            RaisePropertyChanged();
        }
    }

    #endregion

    #region Methods

    private void setControldVisible(bool isBaseVisible = false, bool isVzaimozachetVisible = false,
        bool isCurrencyConvertVisible = false)
    {
        GridControlVzaimozachetVisible = isVzaimozachetVisible ? Visibility.Visible : Visibility.Hidden;
        GridControlBaseExtendVisible = isBaseVisible ? Visibility.Visible : Visibility.Hidden;
        GridControlCurrencyConvertVisible = isCurrencyConvertVisible ? Visibility.Visible : Visibility.Hidden;
    }

    private void setColumnVisible(ProfitAndLoss frm)
    {
        foreach (var col in frm.GridControlBaseExtend.Columns)
        {




            if (col.FieldName == "DocTypeName")
                col.Visible = true;

            if (BalansFact?.Id == Guid.Parse("{2FA1DD9F-6842-4209-B0CC-DDEF3B920496}") ||
                BalansFact?.ParentId == Guid.Parse("{2FA1DD9F-6842-4209-B0CC-DDEF3B920496}") ||
                BalansFact?.Id == Guid.Parse("{E47EF726-3BEA-4B18-9773-E564D624FDF6}") ||
                BalansFact?.ParentId == Guid.Parse("{E47EF726-3BEA-4B18-9773-E564D624FDF6}"))
            {
                if (col.FieldName == "KontragentName")
                {
                    col.Visible = true;
                }
            }
            else
            {
                if (col.FieldName == "KontragentName" || col.FieldName == "DirectPaySumma")
                {
                    col.Visible = false;
                }
            }
        }
    }

    public void UpdateExtend(Guid id)
    {
        ExtendActual.Clear();
        if (myBalansFact?.Id == Guid.Parse("{459937df-085f-4825-9ae9-810b054d0276}")
            || myBalansFact?.Id == Guid.Parse("{30e9bd73-9bda-4d75-b897-332f9210b9b1}"))
        {
            foreach (var d in Extend.Where(d => d.GroupId == id))
            {
                var f = ExtendActual.FirstOrDefault(_ => _.DocCode == d.DocCode);
                if (f == null)
                {
                    d.VzaimozachetInfo =
                    [
                        new VzaimozachetRow
                        {
                            Kontragent = d.KontragentBase?.Name,
                            CurrencyName = d.CurrencyName,
                            Note = d.Note,
                            SDRSchet = d.SDRSchet,
                            Summa = d.ResultCHF + d.ResultEUR + d.ResultGBP + d.ResultRUB + d.ResultSEK +
                                    d.ResultUSD + d.ResultCNY,
                            ProfitCHF = d.ProfitCHF,
                            ProfitEUR = d.ProfitEUR,
                            ProfitGBP = d.ProfitGBP,
                            ProfitRUB = d.ProfitRUB,
                            ProfitSEK = d.ProfitSEK,
                            ProfitUSD = d.ProfitUSD,
                            ProfitCNY = d.ProfitCNY,
                            LossCHF = d.LossCHF,
                            LossEUR = d.LossEUR,
                            LossGBP = d.LossGBP,
                            LossRUB = d.LossRUB,
                            LossSEK = d.LossSEK,
                            LossUSD = d.LossUSD,
                            LossCNY = d.LossCNY,
                            ResultCHF = d.ResultCHF,
                            ResultEUR = d.ResultEUR,
                            ResultGBP = d.ResultGBP,
                            ResultRUB = d.ResultRUB,
                            ResultSEK = d.ResultSEK,
                            ResultUSD = d.ResultUSD,
                            ResultCNY = d.ResultCNY
                        }
                    ];
                    ExtendActual.Add(ProfitAndLossesExtendRowViewModel.GetCopy(d));
                }
                else
                {
                    f.VzaimozachetInfo.Add(new VzaimozachetRow
                    {
                        Kontragent = d.KontragentBase?.Name,
                        CurrencyName = d.CurrencyName,
                        Note = d.Note,
                        SDRSchet = d.SDRSchet,
                        Summa = d.ResultCHF + d.ResultEUR + d.ResultGBP + d.ResultRUB + d.ResultSEK +
                                d.ResultUSD + d.ResultCNY,
                        ProfitCHF = d.ProfitCHF,
                        ProfitEUR = d.ProfitEUR,
                        ProfitGBP = d.ProfitGBP,
                        ProfitRUB = d.ProfitRUB,
                        ProfitSEK = d.ProfitSEK,
                        ProfitUSD = d.ProfitUSD,
                        ProfitCNY = d.ProfitCNY,
                        LossCHF = d.LossCHF,
                        LossEUR = d.LossEUR,
                        LossGBP = d.LossGBP,
                        LossRUB = d.LossRUB,
                        LossSEK = d.LossSEK,
                        LossUSD = d.LossUSD,
                        LossCNY = d.LossCNY,
                        ResultCHF = d.ResultCHF,
                        ResultEUR = d.ResultEUR,
                        ResultGBP = d.ResultGBP,
                        ResultRUB = d.ResultRUB,
                        ResultSEK = d.ResultSEK,
                        ResultUSD = d.ResultUSD,
                        ResultCNY = d.ResultCNY
                    });
                    f.ProfitCHF += d.ProfitCHF;
                    f.ProfitEUR += d.ProfitEUR;
                    f.ProfitGBP += d.ProfitGBP;
                    f.ProfitRUB += d.ProfitRUB;
                    f.ProfitSEK += d.ProfitSEK;
                    f.ProfitUSD += d.ProfitUSD;
                    f.ProfitCNY += d.ProfitCNY;
                    f.LossCHF += d.LossCHF;
                    f.LossEUR += d.LossEUR;
                    f.LossGBP += d.LossGBP;
                    f.LossRUB += d.LossRUB;
                    f.LossSEK += d.LossSEK;
                    f.LossUSD += d.LossUSD;
                    f.LossCNY += d.LossCNY;
                    f.ResultCHF += d.ResultCHF;
                    f.ResultEUR += d.ResultEUR;
                    f.ResultGBP += d.ResultGBP;
                    f.ResultRUB += d.ResultRUB;
                    f.ResultSEK += d.ResultSEK;
                    f.ResultUSD += d.ResultUSD;
                    f.ResultCNY += d.ResultCNY;
                }
            }
        }
        else
        {
            if (id == Guid.Parse("{334973B4-1652-4473-9DED-FD4B31B31FC1}") ||
                id == Guid.Parse("{D89B1E18-074E-4A7D-A0EE-9537DC1585D8}") ||
                id == Guid.Parse("{2FA1DD9F-6842-4209-B0CC-DDEF3B920496}") ||
                id == Guid.Parse("{E47EF726-3BEA-4B18-9773-E564D624FDF6}") ||
                id == Guid.Parse("{B96B2906-C5AA-4566-B77F-F3E4B912E72E}")
               )
                if (id == Guid.Parse("{B96B2906-C5AA-4566-B77F-F3E4B912E72E}"))
                {

                    foreach (var d in Extend.Where(d => d.GroupId == id))
                        ExtendActual.Add(d);

                }

                else
                    foreach (var d in Extend.Where(d => d.GroupId == id))
                        ExtendActual.Add(d);
        }

    }

    private void setCurrencyColumnVisible(GridControl grid, bool isCurrency = false)
    {
        foreach (var col in grid.Columns)
        {
            GridControlBand b;
            switch (col.FieldName)
            {
                case "ProfitEUR":
                    b =
                        grid.Bands.FirstOrDefault(
                            _ => _.Columns.Any(c => c.FieldName == "ProfitEUR"));

                    if (b != null)
                        if (!isCurrency)
                        {
                            b.Visible = ExtendActual.Sum(_ => _.ProfitEUR) != 0 ||
                                        ExtendActual.Sum(_ => _.LossEUR) != 0;
                        }
                        else
                        {
                            b.Visible = CurrencyConvertRows.Sum(_ => _.ProfitEUR) != 0 ||
                                        CurrencyConvertRows.Sum(_ => _.LossEUR) != 0;
                        }

                    break;
                case "ProfitRUB":
                    b =
                        grid.Bands.FirstOrDefault(
                            _ => _.Columns.Any(c => c.FieldName == "ProfitRUB"));

                    if (b != null)
                        if (!isCurrency)
                            b.Visible = ExtendActual.Sum(_ => _.ProfitRUB) != 0 ||
                                        ExtendActual.Sum(_ => _.LossRUB) != 0;
                        else
                            b.Visible = CurrencyConvertRows.Sum(_ => _.ProfitRUB) != 0 ||
                                        CurrencyConvertRows.Sum(_ => _.LossRUB) != 0;

                    break;
                case "LossUSD":
                    b =
                        grid.Bands.FirstOrDefault(
                            _ => _.Columns.Any(c => c.FieldName == "LossUSD"));

                    if (b != null)
                        if (!isCurrency)
                            b.Visible = ExtendActual.Sum(_ => _.ProfitUSD) != 0 ||
                                        ExtendActual.Sum(_ => _.LossUSD) != 0;
                        else
                            b.Visible = CurrencyConvertRows.Sum(_ => _.ProfitUSD) != 0 ||
                                        CurrencyConvertRows.Sum(_ => _.LossUSD) != 0;

                    break;
                case "ProfitGBP":
                    b =
                        grid.Bands.FirstOrDefault(
                            _ => _.Columns.Any(c => c.FieldName == "ProfitGBP"));
                    if (b != null)
                        if (!isCurrency)
                            b.Visible = ExtendActual.Sum(_ => _.ProfitGBP) != 0 ||
                                        ExtendActual.Sum(_ => _.LossGBP) != 0;
                        else
                            b.Visible = CurrencyConvertRows.Sum(_ => _.ProfitGBP) != 0 ||
                                        CurrencyConvertRows.Sum(_ => _.LossGBP) != 0;
                    break;
                case "ProfitCHF":
                    b =
                        grid.Bands.FirstOrDefault(
                            _ => _.Columns.Any(c => c.FieldName == "ProfitCHF"));
                    if (b != null)
                        if (!isCurrency)
                            b.Visible = ExtendActual.Sum(_ => _.ProfitCHF) != 0 ||
                                        ExtendActual.Sum(_ => _.LossCHF) != 0;
                        else
                            b.Visible = CurrencyConvertRows.Sum(_ => _.ProfitCHF) != 0 ||
                                        CurrencyConvertRows.Sum(_ => _.LossCHF) != 0;
                    break;
                case "ProfitSEK":
                    b =
                        grid.Bands.FirstOrDefault(
                            _ => _.Columns.Any(c => c.FieldName == "ProfitSEK"));
                    if (b != null)
                        if (!isCurrency)
                            b.Visible = ExtendActual.Sum(_ => _.ProfitSEK) != 0 ||
                                        ExtendActual.Sum(_ => _.LossSEK) != 0;
                        else
                            b.Visible = CurrencyConvertRows.Sum(_ => _.ProfitSEK) != 0 ||
                                        CurrencyConvertRows.Sum(_ => _.LossSEK) != 0;
                    break;
                case "ProfitCNY":
                    b =
                        grid.Bands.FirstOrDefault(
                            _ => _.Columns.Any(c => c.FieldName == "ProfitCNY"));
                    if (b != null)
                        if (!isCurrency)
                            b.Visible = ExtendActual.Sum(_ => _.ProfitCNY) != 0 ||
                                        ExtendActual.Sum(_ => _.LossCNY) != 0;
                        else
                            b.Visible = CurrencyConvertRows.Sum(_ => _.ProfitCNY) != 0 ||
                                        CurrencyConvertRows.Sum(_ => _.LossCNY) != 0;
                    break;
            }
        }
    }
    private void UpdateCurrencyConvert(DateTime dateStart, DateTime dateEnd, bool isCurrencyOnly = true)
    {
        if (isCurrencyOnly)
            CurrencyConvertRows.Clear();
        using (var ctx = GlobalOptions.GetEntities())
        {
            var sql = $@"SELECT
                      s110.DOC_CODE AS DocCode
                      ,'Акт конвертации' AS DocName
                     ,CAST(s110.VZ_NUM AS VARCHAR) AS DocNum
                     ,VZ_DATE AS DocDate
                     ,CREATOR AS Creator
                     ,s110.CurrencyFromDC AS CurrencyFromDC
                     ,s110.CurrencyToDC AS CurrencyToDC
                     ,SUM(CASE
                        WHEN t110.VZT_1MYDOLZH_0NAMDOLZH = 0 THEN t110.VZT_CRS_SUMMA
                        ELSE 0
                      END) AS FromSumma
                     ,SUM(CASE
                        WHEN t110.VZT_1MYDOLZH_0NAMDOLZH = 1 THEN t110.VZT_CRS_SUMMA
                        ELSE 0
                      END) AS ToSumma,
                      NULL AS BankRowCode
                    FROM TD_110 t110
                    INNER JOIN SD_110 s110
                      ON t110.DOC_CODE = s110.DOC_CODE
                    INNER JOIN SD_111 s111
                      ON s110.VZ_TYPE_DC = s111.DOC_CODE
                        AND s111.IsCurrencyConvert = 1
                    WHERE s110.VZ_DATE >= '{CustomFormat.DateToString(dateStart)}'
                    AND s110.VZ_DATE <= '{CustomFormat.DateToString(dateEnd)}'
                    GROUP BY s110.DOC_CODE
                            ,s110.VZ_NUM
                            ,VZ_DATE
                            ,s110.CREATOR
                            ,s110.CurrencyFromDC
                            ,s110.CurrencyToDC
                    union all
                    SELECT cast(0 AS numeric(18,0)) AS DocCode
                    ,'Банковская конвертация' AS DocName,
                    '' AS DocNum,
                    bcc.DocDate AS DocDate
                    ,bcc.CREATOR AS Creator
                    ,bcc.CrsFromDC AS CurrencyFromDC
                    ,bcc.CrsToDC AS CurrencyToDC
                    ,bcc.SummaFrom AS FromSumma
                    ,bcc.SummaTo AS ToSumma
                    ,bcc.DocRowToCode 
                    FROM BankCurrencyChange bcc
                        WHERE bcc.DocDate >= '{CustomFormat.DateToString(dateStart)}' 
                            AND bcc.DocDate <= '{CustomFormat.DateToString(dateEnd)}' 
                    UNION all
                    SELECT DOC_CODE,
                    'Касса'
                    ,cast(CH_NUM_ORD AS varchar)
                    ,CH_DATE
                    ,CREATOR
                    ,CH_CRS_OUT_DC
                    ,CH_CRS_IN_DC
                    ,CH_CRS_OUT_SUM
                    ,CH_CRS_IN_SUM
                    ,null
                    from sd_251
                        WHERE CH_DATE >= '{CustomFormat.DateToString(dateStart)}' 
                            AND CH_date <= '{CustomFormat.DateToString(dateEnd)}' ";
            var data = ctx.Database.SqlQuery<CurrencyConvertRow>(sql);
            foreach (var d in data)
            {
                d.Operation = $"{GlobalOptions.ReferencesCache.GetCurrency(d.CurrencyFromDC)} -> " +
                              $"{GlobalOptions.ReferencesCache.GetCurrency(d.CurrencyToDC)}";
                if (d.ToSumma > 0)
                {
                    if (d.CurrencyFromDC == GlobalOptions.SystemProfile.NationalCurrency.DocCode)
                    {
                        d.Rate = d.FromSumma / d.ToSumma;
                    }
                    else
                    {
                        if (d.CurrencyToDC == GlobalOptions.SystemProfile.NationalCurrency.DocCode)
                            d.Rate = d.ToSumma / d.FromSumma;
                        else
                            d.Rate = d.FromSumma / d.ToSumma;
                    }
                }
                else
                {
                    d.Rate = 1;
                }

                ProfitAndLossesManager.SetCurrenciesValue(d, d.CurrencyToDC, d.ToSumma, 0);
                ProfitAndLossesManager.SetCurrenciesValue(d, d.CurrencyFromDC, 0, d.FromSumma);
                CurrencyConvertRows.Add(d);
            }
        }
    }

    private void UpdateBalansOper(DateTime dateStart, DateTime dateEnd)
    {
        CurrencyConvertRows.Clear();
        UpdateCurrencyConvert(dateStart, dateEnd, false);
    }

    #endregion
}
