using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using Core;
using Core.WindowsManager;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.LayoutControl;
using Helper;
using KursAM2.Dialogs;
using KursAM2.ViewModel.Finance.Cash;
using KursDomain.Documents.Cash;
using KursDomain.References;
using LayoutManager;

namespace KursAM2.View.Finance.Cash
{
    /// <summary>
    ///     Interaction logic for CashCurrencyExchangeView.xaml
    /// </summary>
    public partial class CashCurrencyExchangeView : ILayout
    {
        public CashCurrencyExchangeView()
        {
            InitializeComponent();
            ApplicationThemeHelper.ApplicationThemeName = Theme.MetropolisLightName;
            LayoutManager = new LayoutManager.LayoutManager(GetType().Name, this, layoutItems);
            Loaded += CashCurrencyExchangeView_Loaded;
            Unloaded += CashCurrencyExchangeView_Unloaded;
            MinWidth = 1000;
            if (DataContext is CashCurrencyExchangeWindowViewModel ctx)
                ctx.WindowName = $"Обмен валюты в {ctx.Document?.Cash?.Name} для {ctx.Document?.Kontragent}";
        }

        public List<Currency> CurrencyList { set; get; } = new List<Currency>();
        public LayoutManager.LayoutManager LayoutManager { get; set; }
        public string LayoutManagerName { get; set; }

        public void SaveLayout()
        {
            LayoutManager.Save();
        }

        private void CashCurrencyExchangeView_Unloaded(object sender, RoutedEventArgs e)
        {
            LayoutManager.Save();
        }

        private void CashCurrencyExchangeView_Loaded(object sender, RoutedEventArgs e)
        {
            LayoutManager.Load();
            var ctx = DataContext as CashCurrencyExchangeWindowViewModel;
            var doc = ctx?.Document;
            if (doc == null) return;
            ctx.WindowName = $"Обмен валюты в {doc.Cash?.Name}";
            try
            {
                using (var datactx = GlobalOptions.GetEntities())
                {
                    var crslst = datactx.TD_22.Where(_ => _.DOC_CODE == doc.CH_CASH_DC).Select(_ => _.CRS_DC).ToList();
                    foreach (var dc in crslst)
                        CurrencyList.Add(MainReferences.Currencies.Values.FirstOrDefault(_ => _.DocCode == dc));
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }
        }

        private void LayoutItems_OnAutoGeneratingItem(object sender, DataLayoutControlAutoGeneratingItemEventArgs e)
        {
            var ctx = DataContext as CashCurrencyExchangeWindowViewModel;
            var doc = ctx?.Document;
            if (doc == null) return;

            // ReSharper disable once CollectionNeverQueried.Local
            var oldContent = e.Item.Content as BaseEdit;
            if (e.PropertyType == typeof(decimal) || e.PropertyType == typeof(decimal?)
                                                  || e.PropertyType == typeof(double) ||
                                                  e.PropertyType == typeof(double?)
                                                  || e.PropertyType == typeof(float) ||
                                                  e.PropertyType == typeof(float?))
            {
                var newContent = new PopupCalcEdit
                {
                    DisplayFormatString = "n2",
                    MaskUseAsDisplayFormat = true,
                    Width = 150
                };
                BindingHelper.CopyBinding(oldContent, newContent, BaseEdit.EditValueProperty);
                e.Item.Content = newContent;
            }

            switch (e.PropertyName)
            {
                case nameof(doc.CREATOR):
                    e.Item.IsReadOnly = true;
                    e.Item.HorizontalAlignment = HorizontalAlignment.Right;
                    break;
                case nameof(doc.Cash):
                    var cb = ViewFluentHelper.SetComboBoxEdit(e.Item, doc.Cash, "Cash",
                        MainReferences.Cashs.Values.Where(_ => _.IsAccessRight).ToList());
                    e.Item.HorizontalAlignment = HorizontalAlignment.Left;
                    e.Item.HorizontalContentAlignment = HorizontalAlignment.Left;
                    cb.EditValueChanged += Cb_EditValueChanged;
                    break;
                case nameof(doc.KontragentType):
                    if (e.Item.Content is ComboBoxEdit cbKontragentType) cbKontragentType.Width = 100;
                    e.Item.HorizontalAlignment = HorizontalAlignment.Left;
                    break;
                case nameof(doc.KontragentName):
                    var kontrEdit = new ButtonEdit
                    {
                        TextWrapping = TextWrapping.Wrap,
                        IsTextEditable = false
                    };
                    kontrEdit.SetBinding(IsEnabledProperty,
                        new Binding { Path = new PropertyPath("IsKontrSelectEnable") });
                    kontrEdit.DefaultButtonClick += KontrEdit_DefaultButtonClick;
                    BindingHelper.CopyBinding(oldContent, kontrEdit, BaseEdit.EditValueProperty);
                    e.Item.Content = kontrEdit;
                    e.Item.Width = 400;
                    e.Item.HorizontalAlignment = HorizontalAlignment.Left;
                    break;
                case nameof(doc.CurrencyIn):
                    try
                    {
                        using (var datactx = GlobalOptions.GetEntities())
                        {
                            var crslst = datactx.TD_22.Where(_ => _.DOC_CODE == doc.CH_CASH_DC).Select(_ => _.CRS_DC)
                                .ToList();
                            ctx.CurrencyList.Clear();
                            foreach (var dc in crslst)
                                ctx.CurrencyList.Add(
                                    MainReferences.Currencies.Values.FirstOrDefault(_ => _.DocCode == dc));
                        }
                    }
                    catch (Exception ex)
                    {
                        WindowManager.ShowError(ex);
                    }

                    ViewFluentHelper.SetComboBoxEdit(e.Item, doc.CurrencyIn, "CurrencyIn", ctx.CurrencyList, width: 50);
                    e.Item.HorizontalContentAlignment = HorizontalAlignment.Left;
                    break;
                case nameof(doc.CurrencyOut):
                    try
                    {
                        using (var datactx = GlobalOptions.GetEntities())
                        {
                            var crslst = datactx.TD_22.Where(_ => _.DOC_CODE == doc.CH_CASH_DC).Select(_ => _.CRS_DC)
                                .ToList();
                            ctx.CurrencyList.Clear();
                            foreach (var dc in crslst)
                                ctx.CurrencyList.Add(
                                    MainReferences.Currencies.Values.FirstOrDefault(_ => _.DocCode == dc));
                        }
                    }
                    catch (Exception ex)
                    {
                        WindowManager.ShowError(ex);
                    }

                    ViewFluentHelper.SetComboBoxEdit(e.Item, doc.CurrencyOut, "CurrencyOut", ctx.CurrencyList,
                        width: 50);
                    e.Item.HorizontalContentAlignment = HorizontalAlignment.Left;
                    break;
                case nameof(doc.CrossRate):
                    e.Item.HorizontalAlignment = HorizontalAlignment.Left;
                    break;
                case nameof(doc.CH_CRS_IN_SUM):
                    var sumordcont = new PopupCalcEdit
                    {
                        DisplayFormatString = "n2",
                        MaskUseAsDisplayFormat = true
                    };
                    sumordcont.SetBinding(IsEnabledProperty,
                        new Binding { Path = new PropertyPath("IsSummaInEnabled") });
                    BindingHelper.CopyBinding(oldContent, sumordcont, BaseEdit.EditValueProperty);
                    e.Item.Content = sumordcont;
                    e.Item.Width = 250;
                    e.Item.HorizontalAlignment = HorizontalAlignment.Left;
                    break;
                case nameof(doc.CH_CRS_OUT_SUM):
                    var sumordcont1 = new PopupCalcEdit
                    {
                        DisplayFormatString = "n2",
                        MaskUseAsDisplayFormat = true
                    };
                    sumordcont1.SetBinding(IsEnabledProperty,
                        new Binding { Path = new PropertyPath("IsSummaOutEnabled") });
                    BindingHelper.CopyBinding(oldContent, sumordcont1, BaseEdit.EditValueProperty);
                    e.Item.Content = sumordcont1;
                    e.Item.Width = 250;
                    e.Item.HorizontalAlignment = HorizontalAlignment.Left;
                    break;
                case nameof(doc.CH_NOTE):
                    ViewFluentHelper.SetDefaultMemoEdit(e.Item);
                    break;
                case "State":
                    e.Item.IsReadOnly = true;
                    if (e.Item.Content is ComboBoxEdit cbState) cbState.IsEnabled = false;
                    e.Item.HorizontalAlignment = HorizontalAlignment.Right;
                    break;
                case nameof(doc.SDRSchet):
                    ViewFluentHelper.SetComboBoxEdit(e.Item, doc.SDRSchet, "SDRSchet",
                        MainReferences.SDRSchets.Values.ToList().OrderBy(_ => _.Name));
                    e.Item.HorizontalContentAlignment = HorizontalAlignment.Left;
                    break;
                case nameof(doc.CH_NUM_ORD):
                    e.Item.Width = 100;
                    e.Item.HorizontalContentAlignment = HorizontalAlignment.Left;
                    break;
                case nameof(doc.CH_DATE):
                    e.Item.HorizontalContentAlignment = HorizontalAlignment.Left;
                    break;
            }

            //((BaseEdit) e.Item.Content).EditValueChanging += OldContent_EditValueChanging;
            if (!(bool)ctx.Document.GetType().GetProperties().FirstOrDefault(_ => _.Name == e.PropertyName)
                    ?.CanWrite) return;
            {
                if (!(e.Item.Content is BaseEdit editor)) return;
                // ReSharper disable once PossibleNullReferenceException
                var binding = editor.GetBindingExpression(BaseEdit.EditValueProperty).ParentBinding;
                editor.SetBinding(BaseEdit.EditValueProperty,
                    new Binding
                    {
                        Path = binding.Path,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                    });
            }
        }

        private void KontrEdit_DefaultButtonClick(object sender, RoutedEventArgs e)
        {
            var ctx = DataContext as CashCurrencyExchangeWindowViewModel;
            var doc = ctx?.Document;
            if (doc == null)
                return;
            switch (ctx.Document.KontragentType)
            {
                case CashCurrencyExchangeKontragentType.Kontragent:
                    var kontr = StandartDialogs.SelectKontragent();
                    if (kontr != null) ctx.Document.Kontragent = kontr;
                    break;
                case CashCurrencyExchangeKontragentType.Employee:
                    var emp = StandartDialogs.SelectEmployee();
                    if (emp != null) ctx.Document.Employee = emp;
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
            var ctx = DataContext as CashInWindowViewModel;
            var doc = ctx?.Document;
            if (doc == null || doc.Cash == null) return;
            try
            {
                using (var datactx = GlobalOptions.GetEntities())
                {
                    var crslst = datactx.TD_22.Where(_ => _.DOC_CODE == doc.CA_DC).Select(_ => _.CRS_DC)
                        .ToList();
                    ctx.CurrencyList.Clear();
                    foreach (var dc in crslst)
                        ctx.CurrencyList.Add(MainReferences.Currencies.Values.FirstOrDefault(_ => _.DocCode == dc));
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }
        }
    }
}
