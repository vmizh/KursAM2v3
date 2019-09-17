using Core.EntityViewModel;
using Data;

namespace Core.ViewModel.Common
{
    public class Country : CountriesViewModel
    {
        public Country()
        {
        }

        public Country(Countries entity) : base(entity)
        {
        }

        // ReSharper disable once CollectionNeverQueried.Global
        public override int Code
        {
            set
            {
                if (Entity.Iso == value) return;
                Entity.Iso = value;
                RaisePropertyChanged();
            }
            get => (int) Entity.Iso;
        }

        public override string ToString()
        {
            return Name;
        }

        // ReSharper disable once ConvertToAutoProperty
    }
}