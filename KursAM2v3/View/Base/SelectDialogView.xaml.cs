using System.ComponentModel;
using System.Linq;
using System.Windows;
using Core.ViewModel.Base;
using Core.WindowsManager;
using DevExpress.Xpf.Core;
using KursAM2.Managers;
using KursAM2.ViewModel.Finance.controls;
using KursDomain;
using LayoutManager;

namespace KursAM2.View.Base
{
    /// <summary>
    ///     Interaction logic for SelectDialogView.xaml
    /// </summary>
    public partial class SelectDialogView : ILayout
    {
        /// <summary>
        ///     Используется kayout из view, false - из viewmodel
        /// </summary>
        public bool IsUsedViewLayout = true;

        public SelectDialogView()
        {
            InitializeComponent();
            ApplicationThemeHelper.ApplicationThemeName = Theme.MetropolisLightName;
            DataContextChanged += SelectDialogView_DataContextChanged;
            Loaded += SelectDialogView_Loaded;
            Closing += SelectDialogView_Closing;
        }

        public LayoutManager.LayoutManager LayoutManager { get; set; }
        public string LayoutManagerName { get; set; }

        public void SaveLayout()
        {
            LayoutManager.Save();
        }

        private void SelectDialogView_Loaded(object sender, RoutedEventArgs e)
        {
            LayoutManager?.Load();
        }

        private void SelectDialogView_Closing(object sender, CancelEventArgs e)
        {
            LayoutManager?.Save();
        }

        private void SelectDialogView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!IsUsedViewLayout) return;   
            if (LayoutManager == null)
            {
                var ctrl = DataContext as IDataUserControl;
                LayoutManager = new LayoutManager.LayoutManager(GlobalOptions.KursSystem(),
                    $"{GetType().Name}.{DataContext.GetType()}", this,
                    ctrl?.LayoutControl);
            }
            //LayoutManager.Load();
        }

        private void Ok_OnClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is RSWindowViewModelBase ctx)
            {
                if (DataContext is AddBankOperionUC bank)
                {
                    var manager = new BankOperationsManager();
                    var winManager = new WindowManager();
                    using (var context = GlobalOptions.GetEntities())
                    {
                        if (bank.VVT_VAL_RASHOD > 0)
                        {
                            var s = 0m;
                            var old = context.TD_101.FirstOrDefault(_ => _.CODE == bank.Code);
                            if (old != null)
                                s = old.VVT_VAL_RASHOD ?? 0;
                            var errStr = manager.CheckForNonzero(bank.BankAccount.DocCode,
                                bank.Date, s > 0 ? s - bank.VVT_VAL_RASHOD : 0);
                            if (!string.IsNullOrEmpty(errStr))
                            {
                                winManager.ShowWinUIMessageBox(errStr, "Предупреждение", MessageBoxButton.OK);
                                return;
                            }
                        }
                    }
                }

                DialogResult = true;
                ctx.DialogResult = true;
                Close();
            }
            else
            {
                DialogResult = false;
                Close();
            }
        }

        private void Cancel_OnClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is RSWindowViewModelBase ctx)
            {
                DialogResult = false;
                ctx.DialogResult = false;
                Close();
            }
            else
            {
                DialogResult = false;
                Close();
            }
        }

        private void Reset_OnClick(object sender, RoutedEventArgs e)
        {
            LayoutManager.ResetLayout();
        }
    }
}
