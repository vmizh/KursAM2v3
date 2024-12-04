using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using Core.WindowsManager;
using DevExpress.Xpf.Core;
using KursAM2.View.Management;
using KursAM2.ViewModel.Finance;
using KursAM2.ViewModel.Management;
using KursDomain;
using LayoutManager;

namespace KursAM2.View.Finance
{
    /// <summary>
    ///     Interaction logic for SaleTaxNomenklView.xaml
    /// </summary>
    public partial class SaleTaxNomenklView : ILayout
    {
        public SaleTaxNomenklView()
        {
            InitializeComponent(); 
            
            LayoutManager = new LayoutManager.LayoutManager(GlobalOptions.KursSystem(),GetType().Name, this, mainLayoutControl);
            Loaded += SaleTaxNomenklView_Loaded;
            Closing += SaleTaxNomenklView_Closing;
        }

        // ReSharper disable once InconsistentNaming
        private SaleTaxNomenklWindowViewModel dataContext => DataContext as SaleTaxNomenklWindowViewModel;
        public LayoutManager.LayoutManager LayoutManager { get; set; }
        public string LayoutManagerName { get; set; }
        public void SaveLayout()
        {
            LayoutManager.Save();
        }

        private void SaleTaxNomenklView_Closing(object sender, CancelEventArgs e)
        {
            //var ctx = DataContext as SaleTaxNomenklWindowViewModel;
            if (dataContext != null)
                if (dataContext.IsCanSaveData)
                    switch (
                        dataContext.WindowManager.ShowWinUIMessageBox("В форме есть не сохраненные данные, сохранить?",
                            "Предупреждение", MessageBoxButton.YesNoCancel,
                            MessageBoxImage.Question, MessageBoxResult.Cancel, MessageBoxOptions.None))
                    {
                        case MessageBoxResult.Cancel:
                            e.Cancel = true;
                            return;
                        case MessageBoxResult.Yes:
                            dataContext.SaveData(null);
                            break;
                        case MessageBoxResult.No:
                            break;
                    }
            LayoutManager.Save();
        }

        private void SaleTaxNomenklView_Loaded(object sender, RoutedEventArgs e)
        {
            LayoutManager.Load();
        }

        private void MenuOpenBalansKontragent_Click(object sender, RoutedEventArgs e)
        {
            if (dataContext?.CurrentPurchase.SD_24.DD_KONTR_OTPR_DC == null) return;
            var ctxk = new KontragentBalansWindowViewModel(dataContext.CurrentPurchase.SD_24.DD_KONTR_OTPR_DC.Value);
            var frm = new KontragentBalansView {Owner = Application.Current.MainWindow, DataContext = ctxk};
            frm.Show();
        }

        private void MenuOpenPurchaseDocument_Click(object sender, RoutedEventArgs e)
        {
            WindowManager.ShowFunctionNotReleased();
        }

        private void MenuOpenPaymentDocument_Click(object sender, RoutedEventArgs e)
        {
            WindowManager.ShowFunctionNotReleased();
        }
    }

    public class IntoToColorConverter : MarkupExtension, IValueConverter
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        #region IValueConverter Members

        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            if (value != null && (bool) value)
                return Brushes.Blue;
            return Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
