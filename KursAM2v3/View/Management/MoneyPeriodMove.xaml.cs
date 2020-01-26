using System.ComponentModel;
using System.Timers;
using System.Windows;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Editors;
using KursAM2.ViewModel.Management;
using LayoutManager;

namespace KursAM2.View.Management
{
    /// <summary>
    ///     Форма "Движение денежных средств"
    /// </summary>
    public partial class MoneyPeriodMove : ILayout
    {
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly Timer myTimer;

        public MoneyPeriodMove()
        {
            InitializeComponent();
            LayoutManager = new LayoutManager.LayoutManager(GetType().Name, this, mainLayoutControl);
            Loaded += MoneyPeriodMove_Loaded;
            Closing += MoneyPeriodMove_Closing;
            myTimer = new Timer();
            myTimer.Elapsed += DisplayTimeEvent;
            myTimer.Interval = int.MaxValue;
            myTimer.Start();
        }

        public LayoutManager.LayoutManager LayoutManager { get; set; }
        public string LayoutManagerName { get; set; }
        public void ResetLayot()
        {
            throw new System.NotImplementedException();
        }

        private void DisplayTimeEvent(object sender, ElapsedEventArgs e)
        {
        }

        private void MoneyPeriodMove_Closing(object sender, CancelEventArgs e)
        {
            LayoutManager.Save();
        }

        private void MoneyPeriodMove_Loaded(object sender, RoutedEventArgs e)
        {
            LayoutManager.Load();
        }

        private void cmbTimeOut_EditValueChanged(object sender, EditValueChangedEventArgs e)
        {
        }

        private void tabsLayoutGroup_SelectedTabChildChanged(object sender, ValueChangedEventArgs<FrameworkElement> e)
        {
            if (!(DataContext is MoneyPeriodWindowViewModel ctx)) return;
            ctx.CurrentItem = null;
        }
    }
}