using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Core;
using Core.EntityViewModel;
using Core.ViewModel.Base;
using Core.ViewModel.Base.Column;
using Core.WindowsManager;

namespace KursAM2.ViewModel.Personal
{
    public class PayRollVedomostSearchViewModel : RSViewModelBase, IViewModel<PayRollVedomostSearch>
    {
        public PayRollVedomostSearchViewModel()
        {
            TableViewInfo = new GridTableViewInfo();
            TableViewInfo.Generate(typeof(PayRollVedomostSearch));
            Source = new ObservableCollection<PayRollVedomostSearch>();
            DeletedItems = new List<PayRollVedomostSearch>();
        }

        public void Load(bool isTemplate)
        {
            Source.Clear();
            using (var ent = GlobalOptions.GetEntities())
            {
                try
                {
                    if (isTemplate)
                    {
                        foreach (var item in from e in ent.EMP_PR_DOC.OrderByDescending(_ => _.Date).ToList()
                            where e.IS_TEMPLATE == 1
                            select
                                new PayRollVedomostSearch
                                {
                                    Id = e.ID,
                                    Name = e.Notes,
                                    Date = e.Date,
                                    Istemplate = e.IS_TEMPLATE == 1,
                                    Creator = e.Creator
                                })
                        {
                            Source.Add(item);
                            // ReSharper disable once UnusedVariable
                            foreach (var row in (from r in ent.EMP_PR_ROWS
                                where r.ID == item.Id
                                select new PayRollVedomostEmployeeRowViewModel {Crs = new Currency()}).ToList())
                            {
                            }
                        }
                    }
                    else
                    {
                        var docs = ent.EMP_PR_DOC.OrderByDescending(_ => _.Date).ToList();
                        foreach (var item in from e in docs
                            where e.IS_TEMPLATE == 0
                            select
                                new PayRollVedomostSearch
                                {
                                    Id = e.ID,
                                    Name = e.Notes,
                                    Date = e.Date,
                                    Istemplate = e.IS_TEMPLATE == 1,
                                    Creator = e.Creator
                                })
                        {
                            Source.Add(item);
                            // ReSharper disable once UnusedVariable
                            foreach (var row in (from r in ent.EMP_PR_ROWS
                                where r.ID == item.Id
                                select new PayRollVedomostEmployeeRowViewModel {Crs = new Currency()}).ToList())
                            {
                            }
                        }
                    }

                    foreach (var doc in Source)
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
                }
                catch (Exception ex)
                {
                    WindowManager.ShowError(null, ex);
                }
            }
        }

        public GridTableViewInfo TableViewInfo { get; set; }

        #region IViewModel<PayRollVedomostSearch> Members

        public ObservableCollection<PayRollVedomostSearch> Source { get; set; }
        public ObservableCollection<PayRollVedomostSearch> SourceAll { get; set; }
        public List<PayRollVedomostSearch> DeletedItems { get; set; }

        #endregion
    }
}