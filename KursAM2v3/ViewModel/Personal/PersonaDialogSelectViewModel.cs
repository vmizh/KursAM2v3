using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Core;
using Core.ViewModel.Base;
using Core.ViewModel.Base.Column;
using KursDomain.WindowsManager.WindowsManager;
using DevExpress.Xpf.Grid;
using KursDomain;
using KursDomain.Documents.Employee;
using KursDomain.ICommon;
using KursDomain.References;

namespace KursAM2.ViewModel.Personal
{
    public class PersonaDialogSelectViewModel : RSViewModelBase, IViewModel<PersonaDialogSelect>
    {
        public PersonaDialogSelectViewModel()
        {
            TableViewInfo = new GridTableViewInfo();
            TableViewInfo.Generate(typeof(PersonaDialogSelect));
            Source = new ObservableCollection<PersonaDialogSelect>();
            DeletedItems = new List<PersonaDialogSelect>();
        }

        public GridTableViewInfo TableViewInfo { get; set; }
        public GridControl Grid { get; set; }
        public ObservableCollection<PersonaDialogSelect> Source { get; set; }
        public ObservableCollection<PersonaDialogSelect> SourceAll { get; set; }
        public List<PersonaDialogSelect> DeletedItems { get; set; }

        public void ResetSummary()
        {
            throw new NotImplementedException();
        }

        public void Load()
        {
            try
            {
                var crs = GlobalOptions.GetEntities().SD_301.ToList();
                foreach (var item in GlobalOptions.GetEntities()
                             .SD_2.Select(s => new Employee()
                             ))
                {
                    ((IName)item.Currency).Name =
                        crs.Where(t => t.DOC_CODE == ((IDocCode)item.Currency).DocCode)
                            .Select(s => s.CRS_SHORTNAME)
                            .FirstOrDefault();
                    Source.Add(new PersonaDialogSelect { Persona = item });
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(null, ex);
            }
        }
    }
}
