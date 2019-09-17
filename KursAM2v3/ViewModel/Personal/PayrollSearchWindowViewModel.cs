using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Core;
using Core.Menu;
using Core.ViewModel.Base;
using Core.WindowsManager;
using KursAM2.View.Personal;

namespace KursAM2.ViewModel.Personal
{
    public class PayrollSearchWindowViewModel : RSWindowViewModelBase
    {
        private PayRollVedomostSearch myCurrentTemplate;
        private PayRollVedomostSearch myCurrentVedomost;
        private bool myIsTemplate;

        private ObservableCollection<PayRollVedomostSearch> myTemplate =
            new ObservableCollection<PayRollVedomostSearch>();

        private ObservableCollection<PayRollVedomostSearch> myVedomost =
            new ObservableCollection<PayRollVedomostSearch>();

        public PayrollSearchWindowViewModel()
        {
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.StandartSearchRightBar(this);
        }

        public ObservableCollection<PayRollVedomostSearch> Template
        {
            set
            {
                myTemplate = value;
                RaisePropertyChanged();
            }
            get => myTemplate;
        }

        public ObservableCollection<PayRollVedomostSearch> Vedomost
        {
            set
            {
                myVedomost = value;
                RaisePropertyChanged();
            }
            get => myVedomost;
        }

        public bool IsTemplate
        {
            get => myIsTemplate;
            set
            {
                if (myIsTemplate == value) return;
                myIsTemplate = value;
                RaisePropertyChanged();
            }
        }

        public PayRollVedomostSearch CurrentVedomost
        {
            get => myCurrentVedomost;
            set
            {
                // ReSharper disable once PossibleUnintendedReferenceComparison
                if (myCurrentVedomost == value) return;
                myCurrentVedomost = value;
                RaisePropertyChanged();
            }
        }

        public PayRollVedomostSearch CurrentTemplate
        {
            get => myCurrentTemplate;
            set
            {
                // ReSharper disable once PossibleUnintendedReferenceComparison
                if (myCurrentTemplate == value) return;
                myCurrentTemplate = value;
                RaisePropertyChanged();
            }
        }

        public override bool IsDocumentOpenAllow => CurrentTemplate != null || CurrentVedomost != null;

        public override void RefreshData(object obj)
        {
            Template.Clear();
            Vedomost.Clear();
            try
            {
                using (var ent = GlobalOptions.GetEntities())
                {
                    foreach (var item in ent.EMP_PR_DOC.OrderByDescending(_ => _.Date).ToList())
                        switch (item.IS_TEMPLATE)
                        {
                            case 1:
                                Template.Add(new PayRollVedomostSearch
                                {
                                    Id = item.ID,
                                    Name = item.Notes,
                                    Date = item.Date,
                                    Istemplate = item.IS_TEMPLATE == 1,
                                    Creator = item.Creator
                                });
                                break;
                            case 0:
                                Vedomost.Add(new PayRollVedomostSearch
                                {
                                    Id = item.ID,
                                    Name = item.Notes,
                                    Date = item.Date,
                                    Istemplate = item.IS_TEMPLATE == 1,
                                    Creator = item.Creator
                                });
                                break;
                        }

                    foreach (var doc in Template)
                    {
                        var doc1 = doc;
                        var nachList = ent.EMP_PR_ROWS.Where(t => t.ID == doc1.Id);
                        foreach (var nach in nachList)
                            switch (nach.CRS_DC)
                            {
                                case CurrencyCode.USD:
                                    doc.USD += nach.SUMMA;
                                    break;
                                case CurrencyCode.RUB:
                                    doc.RUB += nach.SUMMA;
                                    break;
                                case CurrencyCode.EUR:
                                    doc.EUR += nach.SUMMA;
                                    break;
                            }
                    }

                    foreach (var doc in Vedomost)
                    {
                        var doc1 = doc;
                        var nachList = ent.EMP_PR_ROWS.Where(t => t.ID == doc1.Id);
                        foreach (var nach in nachList)
                            switch (nach.CRS_DC)
                            {
                                case CurrencyCode.USD:
                                    doc.USD += nach.SUMMA;
                                    break;
                                case CurrencyCode.RUB:
                                    doc.RUB += nach.SUMMA;
                                    break;
                                case CurrencyCode.EUR:
                                    doc.EUR += nach.SUMMA;
                                    break;
                            }

                        RaisePropertyChanged(nameof(Template));
                        RaisePropertyChanged(nameof(Vedomost));
                    }
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }
        }

        public override void DocumentOpen(object obj)
        {
            PayRollVedomostSearch row;
            row = IsTemplate ? CurrentTemplate : CurrentVedomost;
            if (row == null) return;
            var pr = new PayRollVedomostWindowViewModel(row.Id);
            //var newVed = pr.Copy();
            //var form = new PayRollVedomost {Owner = Application.Current.MainWindow, DataContext = newVed};
            var form = new PayRollVedomost {Owner = Application.Current.MainWindow, DataContext = pr};
            form.Show();
            pr.RefreshData(null);
        }

        public override void DocNewEmpty(object form)
        {
            var ved = PayRollVedomostWindowViewModel.CreateNew();
            var frm = new PayRollVedomost {Owner = Application.Current.MainWindow, DataContext = ved};
            frm.Show();
        }

        public override void DocNewCopy(object form)
        {
            //PayRollVedomostSearch row;
            //row = IsTemplate ? CurrentTemplate : CurrentVedomost;
            //if (row == null) return;
            //// ReSharper disable once UseObjectOrCollectionInitializer
            //var pr = new PayRollVedomostWindowViewModel(row.Id);
            //pr.Id = Guid.NewGuid();
            //pr.Date = DateTime.Today;
            //foreach (var emp in pr.Employees)
            //{
            //    foreach (var r in emp.Rows)
            //    {
            //        r.Id = pr.Id;
            //        r.RowId = Guid.NewGuid();
            //        r.NachDate = pr.Date;
            //    }
            //}
            if (CurrentVedomost == null) return;
            var pr = new PayRollVedomostWindowViewModel(Guid.Parse(CurrentVedomost.Id));
            pr.Id = Guid.NewGuid();
            pr.Date = DateTime.Today;
            pr.Name = string.Empty;
            foreach (var emp in pr.Employees)
            {
                emp.State = RowStatus.NewRow;
                foreach (var r in emp.Rows)
                {
                    r.State = RowStatus.NewRow;
                    r.Id = pr.Id;
                    r.RowId = Guid.NewGuid();
                    r.NachDate = DateTime.Today;
                }
            }
            pr.myState = RowStatus.NewRow;
            var frm = new PayRollVedomost {Owner = Application.Current.MainWindow,
                DataContext = pr};
            frm.Show();
        }

        public override void DocNewCopyRequisite(object form)
        {
            var frmSearch = Form as PayRollDocSearch;
            if (frmSearch == null) return;
            PayRollVedomostWindowViewModel pr;
            pr = IsTemplate
                ? new PayRollVedomostWindowViewModel(CurrentTemplate.Id)
                : new PayRollVedomostWindowViewModel(CurrentVedomost.Id);
            var dtx = pr.Copy();
            pr.myState = RowStatus.NewRow;
            var frm = new PayRollVedomost {Owner = Application.Current.MainWindow, DataContext = dtx};
            frm.Show();
            foreach (var e in dtx.Employees)
            {
                e.Rows.Clear();
                var nach = new PayRollVedomostEmployeeRowViewModel
                {
                    Id = dtx.Id,
                    RowId = Guid.NewGuid(),
                    Name = e.Name,
                    State = RowStatus.NewRow,
                    Employee = e.Employee,
                    Crs = MainReferences.Currencies[GlobalOptions.SystemProfile.MainCurrency.DocCode],
                    PRType = dtx.PayrollTypeCollection.Single(_ =>
                        _.DocCode == GlobalOptions.SystemProfile.DafaultPayRollType.DocCode),
                    Summa = 0,
                    Rate = 0
                };
                e.Rows.Add(nach);
                e.RaisePropertyChanged("Rows");
                e.CalcSumma();
            }
        }
    }
}