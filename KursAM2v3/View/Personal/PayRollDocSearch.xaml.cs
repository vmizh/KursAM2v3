using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Core;
using Core.ViewModel.Base;
using Core.WindowsManager;
using DevExpress.Xpf.Core;
using KursAM2.ViewModel.Personal;
using LayoutManager;

namespace KursAM2.View.Personal
{
    /// <summary>
    ///     Interaction logic for PayRollDocSearch.xaml
    /// </summary>
    public partial class PayRollDocSearch : ILayout
    {
        private bool isTemplate;

        public PayRollDocSearch()
        {
            InitializeComponent(); ApplicationThemeHelper.ApplicationThemeName = Theme.MetropolisLightName;
            LayoutManager = new LayoutManager.LayoutManager(GetType().Name, this, mainLayoutControl);
            Loaded += PayRollDocSearch_Loaded;
            Closing += PayRollDocSearch_Closing;
        }

        public LayoutManager.LayoutManager LayoutManager { get; set; }
        public string LayoutManagerName { get; set; }
        public void SaveLayout()
        {
            LayoutManager.Save();
        }

        private void PayRollDocSearch_Closing(object sender, CancelEventArgs e)
        {
            LayoutManager.Save();
        }

        private void PayRollDocSearch_Loaded(object sender, RoutedEventArgs e)
        {
            LayoutManager.Load();
        }

        private void VedomostOpen_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!(gridVedomost.CurrentItem is PayRollVedomostSearch row)) return;
                var ctx = new PayRollVedomostWindowViewModel(row.Id);
                var form = new PayRollVedomost {Owner = this};
                form.Show();
                form.DataContext = ctx;
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(null, ex);
            }
        }

        private void TemplateOpen_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                PayRollVedomostSearch row;
                if (isTemplate)
                {
                    var r = gridTemplate.CurrentItem as PayRollVedomostSearch;
                    if (r == null) return;
                    row = r;
                }
                else
                {
                    var r = gridVedomost.CurrentItem as PayRollVedomostSearch;
                    if (r == null) return;
                    row = r;
                }
                var pr = new PayRollVedomostWindowViewModel(row.Id);
                var form = new PayRollVedomost {Owner = Application.Current.MainWindow, DataContext = pr};
                form.Show();
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(null, ex);
            }
        }

        private void MenuItemCopy_OnClick(object sender, RoutedEventArgs e)
        {
            PayRollVedomostSearch row;
            if (isTemplate)
            {
                var r = gridTemplate.CurrentItem as PayRollVedomostSearch;
                if (r == null) return;
                row = r;
            }
            else
            {
                var r = gridVedomost.CurrentItem as PayRollVedomostSearch;
                if (r == null) return;
                row = r;
            }
            var pr = new PayRollVedomostWindowViewModel(row.Id);
            var newVed = pr.Copy();
            var form = new PayRollVedomost {Owner = Application.Current.MainWindow, DataContext = newVed};
            form.Show();
        }

        private void LayoutGroup_SelectedTabChildChanged(object sender, ValueChangedEventArgs<FrameworkElement> e)
        {
            isTemplate = e.NewValue.Name == "tabTemplate";
            if (DataContext is PayrollSearchWindowViewModel ctx)
                ctx.IsTemplate = isTemplate;
        }

        private void NewRequisite_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is PayrollSearchWindowViewModel ctx)
            {
                if (ctx.CurrentVedomost == null)
                    return;
                var row = gridVedomost.CurrentItem as PayRollVedomostSearch;
                if (row == null) return;
                var pr = new PayRollVedomostWindowViewModel(row.Id);
                var dtx = pr.Copy();
                var frm = new PayRollVedomost {Owner = Application.Current.MainWindow, DataContext = dtx};
                frm.Show();
                foreach (var emp in dtx.Employees)
                {
                    emp.Rows.Clear();
                    var nach = new PayRollVedomostEmployeeRowViewModel
                    {
                        Id = dtx.Id,
                        RowId = Guid.NewGuid(),
                        Name = emp.Name,
                        State = RowStatus.NewRow,
                        Employee = emp.Employee,
                        Crs = MainReferences.Currencies[GlobalOptions.SystemProfile.MainCurrency.DocCode],
                        PRType = dtx.PayrollTypeCollection.Single(_ =>
                            _.DocCode == GlobalOptions.SystemProfile.DafaultPayRollType.DocCode),
                        Summa = 0,
                        Rate = 0
                    };
                    emp.Rows.Add(nach);
                    emp.RaisePropertyChanged("Rows");
                    emp.CalcSumma();
                }
            }
        }
    }
}