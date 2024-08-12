using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using KursDomain.Documents.Currency;
using KursDomain.References;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace KursAM2.ViewModel.Logistiks.AktSpisaniya
{
    [POCOViewModel(ImplementIDataErrorInfo = true)]
    public class NomenklRemainsOnSkladWithPrice : IDataErrorInfo, INotifyPropertyChanged
    {
        [Display(Name = "Номенклатура")]
        [ReadOnly(true)]
        public Nomenkl Nomenkl { set; get; }

        [Display(Name = "Ном.№")]
        [ReadOnly(true)]
        public string NomNomenkl => Nomenkl?.NomenklNumber;

        [Display(Name = "Валюта")]
        [ReadOnly(true)]
        public Currency Currency => (Currency)Nomenkl?.Currency;

        [Display(Name = "Кол-во на складе")]
        [ReadOnly(true)]
        [DisplayFormat(DataFormatString = "n2")]
        public decimal Quantity { set; get; }

        [Display(AutoGenerateField = false)] public Prices Prices { set; get; }

        [Display(Name = "Цена")]
        [ReadOnly(true)]
        [DisplayFormat(DataFormatString = "n2")]
        public decimal Price => Prices?.Price ?? 0;

        [Display(Name = "Сумма")]
        [ReadOnly(true)]
        [DisplayFormat(DataFormatString = "n2")]
        public decimal Summa => (Prices?.Price ?? 0) * Quantity;

        [Display(Name = "Цена с накл.")]
        [ReadOnly(true)]
        [DisplayFormat(DataFormatString = "n2")]
        public decimal PriceWithNaklad => (Prices?.PriceWithNaklad ?? 0);

        [Display(Name = "Сумма с накл.")]
        [ReadOnly(true)]
        [DisplayFormat(DataFormatString = "n2")]
        public decimal SummaWithPrices => (Prices?.PriceWithNaklad ?? 0) * Quantity;

        [Display(Name = "Макс. отгруз.")]
        [ReadOnly(true)]
        [DisplayFormat(DataFormatString = "n2")]
        public decimal MaxOtgruz { set; get; }


        [Display(Name = "Отгрузить")]
        [DisplayFormat(DataFormatString = "n2")]
        public decimal FactOtgruz { set; get; }

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case "FactOtgruz":
                        if (FactOtgruz > MaxOtgruz) return "Кол-во не может превышать максимальное.";
                        return string.Empty;

                    default:
                        return string.Empty;
                }
            }
        }

        [Display(AutoGenerateField = false)]
        public string Error { get; private set; }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
