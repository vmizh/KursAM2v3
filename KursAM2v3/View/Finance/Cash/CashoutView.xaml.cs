﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using KursDomain.WindowsManager.WindowsManager;
using Data;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.LayoutControl;
using Helper;
using KursAM2.Dialogs;
using KursAM2.Managers;
using KursAM2.View.DialogUserControl;
using KursAM2.ViewModel.Finance.Cash;
using KursDomain;
using KursDomain.Documents.Cash;
using KursDomain.Documents.CommonReferences;
using KursDomain.ICommon;
using KursDomain.References;
using LayoutManager;

namespace KursAM2.View.Finance.Cash;

/// <summary>
///     Interaction logic for CashOutView.xaml
/// </summary>
public partial class CashOutView
{
    public ButtonEdit AccuredInfoButton;
    public ButtonEdit KontrSelectButton = new ButtonEdit();

    public DataLayoutItem NCODEItem = new DataLayoutItem();
    public DataLayoutItem SFactNameItem = new DataLayoutItem();
    public PopupCalcEdit SumKontrcont = new PopupCalcEdit();
    public PopupCalcEdit Sumordcont = new PopupCalcEdit();

    public CashOutView()
    {
        InitializeComponent();

        //LayoutManager = new LayoutManager.LayoutManager(GlobalOptions.KursSystem(), GetType().Name, this, layoutItems);
        //Loaded += CashoutView_Loaded;
        //Unloaded += CashoutView_Unloaded;
        MinWidth = 1000;
    }

    public ObservableCollection<Currency> CurrencyList { set; get; } = new ObservableCollection<Currency>();

    public ComboBoxEdit CurrencyItem { get; set; }
    public LayoutManager.LayoutManager LayoutManager { get; set; }
    public string LayoutManagerName { get; set; }

    public void SaveLayout()
    {
        LayoutManager.Save();
    }

    private void LayoutItems_OnAutoGeneratingItem(object sender, DataLayoutControlAutoGeneratingItemEventArgs e)
    {
        var ctx = DataContext as CashOutWindowViewModel;
        var doc = ctx?.Document;
        if (doc == null) return;
        ctx.WindowName = $"Расходный кассовый ордер в {doc.Cash?.Name}";
        var oldContent = e.Item.Content as BaseEdit;
        if (e.PropertyType == typeof(decimal) || e.PropertyType == typeof(decimal?))
        {
            var newContent = new PopupCalcEdit
            {
                DisplayFormatString = "n2",
                MaskUseAsDisplayFormat = true
            };
            BindingHelper.CopyBinding(oldContent, newContent, BaseEdit.EditValueProperty);
            e.Item.Content = newContent;
        }

        switch (e.PropertyName)
        {
            case nameof(doc.Cash):
                var cb = ViewFluentHelper.SetComboBoxEdit(e.Item, doc.Cash, "Cash",
                    GlobalOptions.ReferencesCache.GetCashBoxAll().ToList());
                e.Item.HorizontalAlignment = HorizontalAlignment.Left;
                e.Item.HorizontalContentAlignment = HorizontalAlignment.Left;
                cb.EditValueChanged += Cb_EditValueChanged;
                break;
            case nameof(doc.CREATOR):
                e.Item.IsReadOnly = true;
                e.Item.HorizontalAlignment = HorizontalAlignment.Right;
                break;
            case nameof(doc.DATE_ORD):
                e.Item.HorizontalAlignment = HorizontalAlignment.Left;
                break;
            case nameof(doc.Currency):
                try
                {
                    using (var datactx = GlobalOptions.GetEntities())
                    {
                        var crslst = datactx.TD_22.Where(_ => _.DOC_CODE == doc.CA_DC).Select(_ => _.CRS_DC)
                            .ToList();
                        ctx.CurrencyList.Clear();
                        foreach (var dc in crslst)
                            ctx.CurrencyList.Add(
                                GlobalOptions.ReferencesCache.GetCurrency(dc) as Currency);
                    }
                }
                catch (Exception ex)
                {
                    WindowManager.ShowError(ex);
                }

                CurrencyItem = ViewFluentHelper.SetComboBoxEdit(e.Item, doc.Currency, "Currency", ctx.CurrencyList,
                    width: 50);
                e.Item.HorizontalContentAlignment = HorizontalAlignment.Left;
                if (doc.BANK_RASCH_SCHET_DC != null && doc.SPOST_DC != null && doc.State != RowStatus.NewRow &&
                    string.IsNullOrWhiteSpace(doc.Kontragent))
                    CurrencyItem.IsEnabled = false;
                break;
            case "NOTES_ORD":
                ViewFluentHelper.SetDefaultMemoEdit(e.Item);
                break;
            case nameof(doc.NUM_ORD):
                e.Item.IsReadOnly = true;
                e.Item.HorizontalAlignment = HorizontalAlignment.Left;
                break;
            case "OSN_ORD":
                ViewFluentHelper.SetDefaultMemoEdit(e.Item);
                break;
            case "NAME_ORD":
                ViewFluentHelper.SetDefaultMemoEdit(e.Item);
                break;
            case nameof(doc.AccuredInfo):
                AccuredInfoButton = new ButtonEdit
                {
                    TextWrapping = TextWrapping.Wrap,
                    IsTextEditable = false
                };
                AccuredInfoButton.SetBinding(IsEnabledProperty,
                    new Binding { Path = new PropertyPath("IsAccuredOpenEnable") });
                AccuredInfoButton.DefaultButtonClick += AccuredOpen_DefaultButtonClick;
                BindingHelper.CopyBinding(oldContent, AccuredInfoButton, BaseEdit.EditValueProperty);
                e.Item.Content = AccuredInfoButton;
                break;
            case "State":
                e.Item.IsReadOnly = true;
                if (e.Item.Content is ComboBoxEdit cbState) cbState.IsEnabled = false;
                e.Item.HorizontalAlignment = HorizontalAlignment.Right;
                break;
            case "Kontragent":
                KontrSelectButton = new ButtonEdit
                {
                    TextWrapping = TextWrapping.Wrap,
                    IsTextEditable = false
                };
                KontrSelectButton.SetBinding(IsEnabledProperty,
                    new Binding { Path = new PropertyPath("IsKontrSelectEnable") });
                KontrSelectButton.DefaultButtonClick += KontrEdit_DefaultButtonClick;
                BindingHelper.CopyBinding(oldContent, KontrSelectButton, BaseEdit.EditValueProperty);
                e.Item.Content = KontrSelectButton;
                e.Item.Width = 400;
                break;
            case nameof(doc.SDRSchet):
                ViewFluentHelper.SetComboBoxEdit(e.Item, doc.SDRSchet, "SDRSchet",
                    GlobalOptions.ReferencesCache.GetSDRSchetAll().Cast<SDRSchet>().ToList().OrderBy(_ => _.Name));
                e.Item.HorizontalAlignment = HorizontalAlignment.Left;
                break;
            case nameof(doc.Project):
                var list = GlobalOptions.ReferencesCache.GetProjectsAll().Cast<Project>().ToList();
                list.Add(new Project()
                {
                    Id = Guid.Empty,
                    Name = " Не указан"
                });
                ViewFluentHelper.SetComboBoxEdit(e.Item, doc.Project, "Project",
                    list.OrderBy(_ => _.Name),
                    displayMember:"Name",displayValue:"Id");
                e.Item.HorizontalContentAlignment = HorizontalAlignment.Left;
                break;
            case nameof(doc.SPostName):
                var cancelBtn = new ButtonInfo
                {
                    ButtonKind = ButtonKind.Simple,
                    GlyphKind = GlyphKind.Cancel,
                    ToolTip = "Удалить связь со счетом"
                    // IsEnabled = doc.SPOST_DC != null,
                };
                cancelBtn.Click += CancelBtn_Click;
                var spostEdit = new ButtonEdit
                {
                    TextWrapping = TextWrapping.Wrap,
                    IsTextEditable = false,
                    Buttons = new ButtonInfoCollection
                    {
                        cancelBtn
                    }
                };
                spostEdit.DefaultButtonClick += SFPostEdit_DefaultButtonClick;
                BindingHelper.CopyBinding(oldContent, spostEdit, BaseEdit.EditValueProperty);
                e.Item.Content = spostEdit;
                SFactNameItem = e.Item;
                break;
            case nameof(doc.SUMM_ORD):
                Sumordcont = new PopupCalcEdit
                {
                    DisplayFormatString = "n2",
                    MaskUseAsDisplayFormat = true
                };
                Sumordcont.EditValueChanged += Sumordcont1_EditValueChanged;
                //Sumordcont.SetBinding(IsEnabledProperty,
                //    new Binding {Path = new PropertyPath("IsSummaEnabled")});
                BindingHelper.CopyBinding(oldContent, Sumordcont, BaseEdit.EditValueProperty);
                e.Item.Content = Sumordcont;
                e.Item.Width = 250;
                e.Item.HorizontalAlignment = HorizontalAlignment.Left;
                break;
            case nameof(doc.CRS_SUMMA):
                SumKontrcont = new PopupCalcEdit
                {
                    DisplayFormatString = "n2",
                    MaskUseAsDisplayFormat = true
                };
                SumKontrcont.SetBinding(IsEnabledProperty,
                    new Binding { Path = new PropertyPath("IsKontrSummaEnabled") });
                BindingHelper.CopyBinding(oldContent, SumKontrcont, BaseEdit.EditValueProperty);
                e.Item.Content = SumKontrcont;
                e.Item.Width = 250;
                e.Item.HorizontalAlignment = HorizontalAlignment.Left;
                break;
            case nameof(doc.OBRATNY_RASCHET):
                e.Item.VerticalAlignment = VerticalAlignment.Center;
                break;
            case nameof(doc.KONTR_CRS_SUM_CORRECT_PERCENT):
                e.Item.Width = 50;
                e.Item.VerticalAlignment = VerticalAlignment.Center;
                break;
            case nameof(doc.KontragentType):
                if (e.Item.Content is ComboBoxEdit cbKontragentType) cbKontragentType.Width = 100;
                e.Item.HorizontalAlignment = HorizontalAlignment.Left;
                e.Item.VerticalAlignment = VerticalAlignment.Center;
                break;
            case nameof(doc.NCODE):
                NCODEItem = e.Item;
                break;
        }

        ViewFluentHelper.SetModeUpdateProperties(doc, e.Item, e.PropertyName);
    }

    private void Sumordcont1_EditValueChanged(object sender, EditValueChangedEventArgs e)
    {
        if (!(DataContext is CashOutWindowViewModel ctx)) return;
        var doc = ctx.Document;
        if ((decimal)e.NewValue < 0)
        {
            WindowManager.ShowMessage(this, "Сумма ордера не может быть меньше 0!", "Ошибка",
                MessageBoxImage.Stop);
            doc.SUMM_ORD = 0;
            return;
        }

        if ((decimal)e.NewValue > doc.CRS_SUMMA + doc.MaxSumma && doc.SPOST_DC != null)
        {
            WindowManager.ShowMessage(this, "Сумма ордера не может быть больше сумму оплаты по счету!", "Ошибка",
                MessageBoxImage.Stop);
            doc.SUMM_ORD = ctx.OldSumma;
        }
    }

    private void CancelBtn_Click(object sender, RoutedEventArgs e)
    {
        var dtx = DataContext as CashOutWindowViewModel;
        if (dtx == null) return;
        var doc = dtx.Document;
        doc.SPOST_DC = null;
        doc.SPostName = null;
    }

    private void AccuredOpen_DefaultButtonClick(object sender, RoutedEventArgs e)
    {
        var ctx = DataContext as CashOutWindowViewModel;
        var doc = ctx?.Document;
        if (doc == null)
            return;
        if (doc.AccuredAmountOfSupplier != null)
            DocumentsOpenManager.Open(
                DocumentType.AccruedAmountOfSupplier, 0, doc.AccuredAmountOfSupplier.DocId, this);
    }

    private void SFPostEdit_DefaultButtonClick(object sender, RoutedEventArgs e)
    {
        if (!(DataContext is CashOutWindowViewModel dtx)) return;
        
        var doc = dtx.Document;
        var item = StandartDialogs.SelectInvoiceProvider(dtx, true, true, true);
        if (item == null) return;

        var winManager = new WindowManager();
        doc.MaxSumma = item.Summa - item.PaySumma;
        if (item.DocDate != doc.DATE_ORD)
        {
            var res = winManager.ShowWinUIMessageBox(
                $"Дата счета {item.DocDate.ToShortDateString()} не совпадает с датой ордера {doc.DATE_ORD}." +
                "Установить дату ордера равной дате счета?", "Запрос", MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            if (res == MessageBoxResult.Yes)
            {
                doc.DATE_ORD = item.DocDate;
                doc.SPostName = item.ToString();
                doc.SPOST_DC = item.DocCode;
                doc.SUMM_ORD = item.Summa - item.PaySumma;
                doc.Currency = item.Currency;
                doc.KontragentType = CashKontragentType.Kontragent;
                doc.KONTRAGENT_DC = item.Kontragent.DocCode;
                doc.NOTES_ORD = item.Note;
            }
            else
            {
                doc.SPostName = item.ToString();
                doc.SPOST_DC = item.DocCode;
                doc.SUMM_ORD = item.Summa - item.PaySumma;
                doc.Currency = item.Currency;
                doc.KontragentType = CashKontragentType.Kontragent;
                doc.KONTRAGENT_DC = item.Kontragent.DocCode;
                doc.NOTES_ORD = item.Note;
            }
        }
        else
        {
            doc.SPostName = item.ToString();
            doc.SPOST_DC = item.DocCode;
            doc.SUMM_ORD = item.Summa - item.PaySumma;
            doc.Currency = item.Currency;
            doc.KontragentType = CashKontragentType.Kontragent;
            doc.KONTRAGENT_DC = item.Kontragent.DocCode;
            doc.NOTES_ORD = item.Note;
        }

        if (dtx.Document.DocCode > 0)
            using (var ctx = GlobalOptions.GetEntities())
            {
                try
                {
                    var old = ctx.ProviderInvoicePay.FirstOrDefault(_ => _.CashDC == dtx.Document.DocCode
                                                                         && _.DocDC == dtx.Document.SPOST_DC);
                    if (old == null)
                        ctx.ProviderInvoicePay.Add(new ProviderInvoicePay
                        {
                            Id = Guid.NewGuid(),
                            Rate = 1,
                            // ReSharper disable once PossibleInvalidOperationException
                            Summa = (decimal)dtx.Document.SUMM_ORD,
                            CashDC = dtx.Document.DocCode,
                            // ReSharper disable once PossibleInvalidOperationException
                            DocDC = (decimal)dtx.Document.SPOST_DC
                        });
                    else
                        // ReSharper disable once PossibleInvalidOperationException
                        old.Summa = (decimal)dtx.Document.SUMM_ORD;

                    ctx.SaveChanges();
                }
                catch (Exception ex)
                {
                    WindowManager.ShowError(ex);
                }
            }
    }

    private void KontrEdit_DefaultButtonClick(object sender, RoutedEventArgs e)
    {
        var ctx = DataContext as CashOutWindowViewModel;
        var doc = ctx?.Document;
        if (doc == null)
            return;
        if (doc.BANK_RASCH_SCHET_DC != null && doc.SPOST_DC != null && doc.State != RowStatus.NewRow)
        {
            WindowManager.ShowMessage("Ордер уже проведен. Изменить контрагента нельзя",
                "Предупреждение", MessageBoxImage.Information);
            return;
        }

        switch (ctx.Document.KontragentType)
        {
            case CashKontragentType.Kontragent:
                var kontr = StandartDialogs.SelectKontragent(new KontragentSelectDialogOptions
                {
                    Currency = ctx.Document.Currency
                });
                if (kontr == null) return;
                ctx.Document.BankAccount = null;
                ctx.Document.CashTo = null;
                ctx.Document.Employee = null;
                ctx.Document.StockHolder = null;
                ctx.Document.KONTRAGENT_DC = kontr.DocCode;
                ctx.Document.NAME_ORD = kontr?.Name;
                ctx.Document.KONTR_CRS_DC = ((IDocCode)kontr?.Currency).DocCode;
                ctx.Document.SPostName = null;
                ctx.Document.SPOST_DC = null;
                ctx.Document.Currency = kontr.Currency as Currency;
                CurrencyItem.IsEnabled = false;
                break;
            case CashKontragentType.Employee:
                var emp = StandartDialogs.SelectEmployee();
                if (emp == null) return;
                ctx.Document.BankAccount = null;
                ctx.Document.CashTo = null;
                ctx.Document.StockHolder = null;
                ctx.Document.KONTRAGENT_DC = null;
                ctx.Document.Employee = emp;
                ctx.Document.NAME_ORD = emp?.Name;
                ctx.Document.SPostName = null;
                ctx.Document.SPOST_DC = null;
                ctx.Document.Currency = emp.Currency as Currency;
                CurrencyItem.IsEnabled = false;
                break;
            case CashKontragentType.Bank:
                var bank = StandartDialogs.SelectBankAccount(ctx.Document.Currency);
                if (bank == null) return;
                ctx.Document.KONTRAGENT_DC = null;
                ctx.Document.BankAccount = null;
                ctx.Document.CashTo = null;
                ctx.Document.Employee = null;
                ctx.Document.StockHolder = null;
                ctx.Document.NAME_ORD = bank.Name;
                ctx.Document.BankAccount = bank;
                ctx.Document.SPostName = null;
                ctx.Document.SPOST_DC = null;
                ctx.Document.Currency = bank.Currency as Currency;
                CurrencyItem.IsEnabled = false;
                break;
            case CashKontragentType.Cash:
                var ch = StandartDialogs.SelectCash(new List<CashBox> { ctx.Document.Cash });
                if (ch != null) ctx.Document.CashTo = ch;
                ctx.Document.KONTRAGENT_DC = null;
                ctx.Document.BankAccount = null;
                ctx.Document.Employee = null;
                ctx.Document.StockHolder = null;
                ctx.Document.NAME_ORD = ch?.Name;
                ctx.Document.SPostName = null;
                ctx.Document.SPOST_DC = null;
                break;
            case CashKontragentType.StockHolder:
                ctx.SelectStockHolder();
                break;
        }
    }

    private void LayoutItems_OnAutoGeneratedUI(object sender, EventArgs e)
    {
        foreach (var item in WindowHelper.GetLogicalChildCollection<MemoEdit>(layoutItems))
            if (item.EditValue != null && !string.IsNullOrWhiteSpace((string)item.EditValue))
                item.Height = 80;
    }

    private void Cb_EditValueChanged(object sender, EditValueChangedEventArgs e)
    {
        if (!(DataContext is CashOutWindowViewModel ctx)) return;
        var doc = ctx.Document;
        ctx.WindowName = $"Расходный кассовый ордер в {doc?.Cash?.Name}";
        if (doc == null || doc.Cash == null) return;
        try
        {
            using (var datactx = GlobalOptions.GetEntities())
            {
                var crslst = datactx.TD_22.Where(_ => _.DOC_CODE == doc.CA_DC).Select(_ => _.CRS_DC)
                    .ToList();
                ctx.CurrencyList.Clear();
                foreach (var dc in crslst)
                    ctx.CurrencyList.Add(GlobalOptions.ReferencesCache.GetCurrency(dc) as Currency);
            }
        }
        catch (Exception ex)
        {
            WindowManager.ShowError(ex);
        }
    }
}
