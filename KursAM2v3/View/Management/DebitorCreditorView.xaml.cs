using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using Core;
using Core.WindowsManager;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.LayoutControl;
using KursAM2.ViewModel.Management;
using KursAM2.ViewModel.Management.Calculations;
using KursAM2.ViewModel.Management.DebitorCreditor;
using LayoutManager;

namespace KursAM2.View.Management
{
    /// <summary>
    ///     Interaction logic for DebitorCreditorView.xaml
    /// </summary>
    public partial class DebitorCreditorView : ILayout
    {
        public DebitorCreditorView()
        {
            InitializeComponent();
            LayoutManager = new LayoutManager.LayoutManager(GetType().Name, this, mainLayoutControl);
            Closing += DebitorCreditorView_Closing;
            Loaded += DebitorCreditorView_Loaded;
            DebitorCreditorGrid.FilterChanged += DebitorCreditorGrid_FilterChanged;
            DebitorGrid.FilterChanged += DebitorGrid_FilterChanged;
            CreditorGrid.FilterChanged += CreditorGrid_FilterChanged;
            var column = DebitorCreditorGrid.Columns[0];
            var colStyle = column.CellStyle;
            FilteredColumnStyle = new Style(typeof(LightweightCellEditor), colStyle);
            FilteredColumnStyle.Setters.Add(new Setter(ForegroundProperty, Brushes.Blue));
            UnFilteredColumnStyle = new Style(typeof(LightweightCellEditor), colStyle);
            UnFilteredColumnStyle.Setters.Add(new Setter(ForegroundProperty, Brushes.Black));
            Title = "Дебиторы/кредиторы. База данных " + GlobalOptions.DataBaseName;
        }

        private Style FilteredColumnStyle { get; }
        private Style UnFilteredColumnStyle { get; }
        public LayoutManager.LayoutManager LayoutManager { get; set; }
        public string LayoutManagerName { get; set; }
        public void SaveLayout()
        {
            LayoutManager.Save();
        }

        private void DebitorCreditorView_Loaded(object sender, RoutedEventArgs e)
        {
            LayoutManager.Load();
        }

        private void DebitorCreditorView_Closing(object sender, CancelEventArgs e)
        {
            LayoutManager.Save();
        }

        private void CreditorGrid_FilterChanged(object sender, RoutedEventArgs e)
        {
            foreach (var col in CreditorGrid.Columns)
                col.CellStyle = col.IsFiltered ? FilteredColumnStyle : UnFilteredColumnStyle;
        }

        private void DebitorGrid_FilterChanged(object sender, RoutedEventArgs e)
        {
            foreach (var col in DebitorGrid.Columns)
                col.CellStyle = col.IsFiltered ? FilteredColumnStyle : UnFilteredColumnStyle;
        }

        private void DebitorCreditorGrid_FilterChanged(object sender, RoutedEventArgs e)
        {
            foreach (var col in DebitorCreditorGrid.Columns)
                col.CellStyle = col.IsFiltered ? FilteredColumnStyle : UnFilteredColumnStyle;
        }

        private void DebitorOpenKontragent_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!(DataContext is DebitorCreditorWindowViewModel ctx)) return;
                var ctxk = new KontragentBalansWindowViewModel(ctx.CurrentDebitor.KontrInfo.DOC_CODE);
                var frm = new KontragentBalansForm {Owner = Application.Current.MainWindow, DataContext = ctxk};
                frm.Show();
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(null, ex);
            }
        }

        private void CreditorOpenKontragent_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!(DataContext is DebitorCreditorWindowViewModel ctx)) return;
                var ctxk = new KontragentBalansWindowViewModel(ctx.CurrentCreditor.KontrInfo.DOC_CODE);
                var frm = new KontragentBalansForm {Owner = Application.Current.MainWindow, DataContext = ctxk};
                frm.Show();
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(null, ex);
            }
        }
        private void DebitorCreditorOpenKontragent_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!(DataContext is DebitorCreditorWindowViewModel ctx)) return;
                var ctxk = new KontragentBalansWindowViewModel(ctx.CurrentDebitorCreditor.KontrInfo.DOC_CODE);
                var frm = new KontragentBalansForm { Owner = Application.Current.MainWindow, DataContext = ctxk };
                frm.Show();
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(null, ex);
            }
        }

        //DebitorCreditorOpenKontragent_OnClick

        private void LayoutGroup_SelectedTabChildChanged(object sender, ValueChangedEventArgs<FrameworkElement> e)
        {
            if (!(e.NewValue is LayoutGroup n)) return;
            if (!(DataContext is DebitorCreditorWindowViewModel ctx)) return;
            switch (n.Header.ToString())
            {
                case "Дебиторы":
                    ctx.ChangedKontr(1);
                    break;
                case "Кредиторы":
                    ctx.ChangedKontr(2);
                    break;
                case "Все":
                    ctx.ChangedKontr(3);
                    break;
            }
        }

        private void RecalcBalansDebitor_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var ctx = DataContext as DebitorCreditorWindowViewModel;
                if (ctx == null) return;
                RecalcKontragentBalans.CalcBalans(ctx.CurrentDebitor.KontrInfo.DOC_CODE, new DateTime(2000, 1, 1));
                ctx.RefreshData(null);
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(null, ex);
            }
        }

        private void RecalcBalansCreditor_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!(DataContext is DebitorCreditorWindowViewModel ctx)) return;
                RecalcKontragentBalans.CalcBalans(ctx.CurrentCreditor.KontrInfo.DOC_CODE, new DateTime(2000, 1, 1));
                ctx.RefreshData(null);
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(null, ex);
            }
        }

        private void RecalcBalansAll_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!(DataContext is DebitorCreditorWindowViewModel ctx)) return;
                if (ctx.CurrentDebitorCreditor == null) return;
                RecalcKontragentBalans.CalcBalans(ctx.CurrentDebitorCreditor.KontrInfo.DOC_CODE,
                    new DateTime(2000, 1, 1));
                ctx.RefreshData(null);
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(null, ex);
            }
        }

        private void DebitorCreditorTableView_FocusedColumnChanged(object sender, CurrentColumnChangedEventArgs e)
        {
            if (!(DataContext is DebitorCreditorWindowViewModel ctx)) return;
            if (e.NewColumn != null)
            {
                if (e.NewColumn.FieldName != "IsSelected") return;
                if (ctx.CurrentCreditor != null)
                    ctx.CurrentCreditor.IsSelected = !ctx.CurrentCreditor.IsSelected;
                if (ctx.CurrentDebitor != null)
                    ctx.CurrentDebitor.IsSelected = !ctx.CurrentDebitor.IsSelected;
                if (ctx.CurrentDebitorCreditor != null)
                    ctx.CurrentDebitorCreditor.IsSelected = !ctx.CurrentDebitorCreditor.IsSelected;
            }
        }
    }
}