using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using Core;
using Core.EntityViewModel;
using Core.ViewModel.Base;
using Core.ViewModel.Base.Column;
using Core.WindowsManager;

namespace KursAM2.ViewModel.Personal
{
    public class PersonaReferenceViewModelOld : KursViewModelBase, IViewModel<Employee>
    {
        public PersonaReferenceViewModelOld()
        {
            TableViewInfo = new GridTableViewInfo();
            TableViewInfo.Generate(typeof(Employee));
            Source = new ObservableCollection<Employee>();
            SourceAll = new ObservableCollection<Employee>();
            DeletedItems = new List<Employee>();
        }

        public ObservableCollection<Employee> Source { get; set; }
        public ObservableCollection<Employee> SourceAll { get; set; }
        public List<Employee> DeletedItems { get; set; }

        public void Load()
        {
            try
            {
                Source.Clear();
                DeletedItems.Clear();
                var crsList = GlobalOptions.GetEntities().SD_301.ToList();
                foreach (var pers in GlobalOptions.GetEntities()
                    .SD_2.Include(_ => _.SD_301)
                    .Select(s => new Employee
                    {
                        DocCode = s.DOC_CODE,
                        NameFirst = s.NAME_FIRST,
                        NameLast = s.NAME_LAST,
                        NameSecond = s.NAME_SECOND,
                        Name = s.NAME,
                        TabelNumber = s.TABELNUMBER,
                        StatusNotes = s.STATUS_NOTES,
                        Currency = new Currency
                        {
                            DocCode = (decimal) s.crs_dc
                        },
                        State = RowStatus.NotEdited,
                        IsDeleted = s.DOC_CODE == 1,
                        Photo = s.PHOTO
                    }))
                {
                    var firstOrDefault = crsList.FirstOrDefault(t => t.DOC_CODE == pers.Currency.DocCode);
                    if (firstOrDefault != null)
                        pers.Currency = new Currency
                        {
                            DocCode = pers.Currency.DocCode,
                            Name =
                                firstOrDefault.CRS_SHORTNAME
                        };
                    Source.Add(pers);
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(null, ex);
            }
        }
    }
}