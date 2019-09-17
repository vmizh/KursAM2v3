using System.Collections.ObjectModel;
using System.Linq;
using Core.ViewModel.Base;

namespace Core.ViewModel.Common
{
    public class CountryDialogViewModel : RSWindowDialogViewModelBase
    {
        public CountryDialogViewModel()
        {
            Data = new ObservableCollection<Country>();
        }

        public ObservableCollection<Country> Data { set; get; }

        public override void RefreshData(object data)
        {
            base.RefreshData(data);
            Data.Clear();
            foreach (var d in GlobalOptions.GetEntities().Countries.ToList())
                Data.Add(new Country
                {
                    Id = Id,
                    Entity = d,
                    Name = d.Name
                });
        }
    }
}