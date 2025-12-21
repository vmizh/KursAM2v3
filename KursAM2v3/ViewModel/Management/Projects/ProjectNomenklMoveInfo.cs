using System;
using System.ComponentModel.DataAnnotations;

namespace KursAM2.ViewModel.Management.Projects;

public class ProjectNomenklMoveInfo
{
    [Display(AutoGenerateField = false)] public Guid NomId { get; set; }

    [Display(AutoGenerateField = false)] public decimal NomDC { get; set; }

    [Display(AutoGenerateField = true, GroupName = "Основные", Name = "Ном.№")]
    public string NomNomenkl { get; set; }

    [Display(AutoGenerateField = true, GroupName = "Основные", Name = "Номенклатура")]
    public string NomName { get; set; }

    [Display(AutoGenerateField = true, GroupName = "Основные", Name = "Услуга")]
    public bool IsService { get; set; }


    [Display(AutoGenerateField = true, Name = "Есть исключенные")]
    public bool HasExcluded { set; get; }

    #region Документы

    [Display(AutoGenerateField = true, GroupName = "Документы", Name = "Кол-во(приход)")]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal DocQuantityIn { get; set; }

    [Display(AutoGenerateField = true, GroupName = "Документы", Name = "Сумма(приход)")]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal DocSummaIn { get; set; }


    [Display(AutoGenerateField = true, GroupName = "Документы", Name = "Кол-во(расход)")]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal DocQuantityOut { get; set; }

    [Display(AutoGenerateField = true, GroupName = "Документы", Name = "Сумма(расход)")]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal DocSummaOut { get; set; }

    [Display(AutoGenerateField = true, GroupName = "Документы", Name = "Результат (кол-во)")]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal DocQuantityResult => DocQuantityIn - DocQuantityOut;

    [Display(AutoGenerateField = true, GroupName = "Документы", Name = "Результат (сумма)")]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal DocSummaResult => DocSummaOut - DocSummaIn;

    #endregion

    #region Фактически

    [Display(AutoGenerateField = true, GroupName = "Фактические", Name = "Кол-во(приход)")]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal FactQuantityIn { get; set; }

    [Display(AutoGenerateField = true, GroupName = "Фактические", Name = "Кол-во(расход)")]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal FactQuantityOut { get; set; }

    [Display(AutoGenerateField = true, GroupName = "Фактические", Name = "Сумма(приход)")]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal FactSummaIn { get; set; }

    [Display(AutoGenerateField = true, GroupName = "Фактические", Name = "Сумма(расход)")]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal FactSummaOut { get; set; }

    [Display(AutoGenerateField = true, GroupName = "Фактические", Name = "Результат (кол-во)")]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal FactQuantityResult => FactQuantityIn - FactQuantityOut;

    [Display(AutoGenerateField = true, GroupName = "Фактические", Name = "Результат (сумма)")]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal FactSummaResult => FactSummaOut - FactSummaIn;

    #endregion

    #region Услуги и прочее

    [Display(AutoGenerateField = true, GroupName = "Услуги и затраты", Name = "Сумма дилер")]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal DilerSumma { get; set; }

    [Display(AutoGenerateField = true, GroupName = "Услуги и затраты", Name = "Сумма накл.")]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal NakladSumma { get; set; }

    [Display(AutoGenerateField = true, GroupName = "Услуги и затраты", Name = "Услуги поставщиков")]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal ServiceProviderSumma { get; set; }

    [Display(AutoGenerateField = true, GroupName = "Услуги и затраты", Name = "Услуги клиентам")]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal ServiceClientSumma { get; set; }

    #endregion

    #region Сводные результаты

    [Display(AutoGenerateField = true, GroupName = "Сводные результаты", Name = "Цена закупки")]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal ResultPriceIn { get; set; }

    [Display(AutoGenerateField = true, GroupName = "Сводные результаты", Name = "Сумма закупки")]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal ResultSummaIn { get; set; }

    [Display(AutoGenerateField = true, GroupName = "Сводные результаты", Name = "Цена продажи")]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal ResultPriceOut { get; set; }

    [Display(AutoGenerateField = true, GroupName = "Сводные результаты", Name = "Продано (кол-во)")]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal ResultQuantityOut { get; set; }

    [Display(AutoGenerateField = true, GroupName = "Сводные результаты", Name = "Сумма продажи")]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal ResultSummaOut { get; set; }

    [Display(AutoGenerateField = true, GroupName = "Сводные результаты", Name = "Результат")]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal Result { get; set; }

    [Display(AutoGenerateField = true, GroupName = "Сводные результаты", Name = "Остаток не реализованных")]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal ResultOstatok { get; set; }

    [Display(AutoGenerateField = true, GroupName = "Сводные результаты", Name = "Остаток (сумма)")]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal ResultOstatokSumma { get; set; }

    [Display(AutoGenerateField = true, GroupName = "Сводные результаты", Name = "Предполагаемый доход")]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal ExpectedIncomeSumma { get; set; }

    [Display(AutoGenerateField = true, GroupName = "Сводные результаты", Name = "Предполагаемая маржа")]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal ExpectedIncomeProfit { get; set; }

    #endregion
}
