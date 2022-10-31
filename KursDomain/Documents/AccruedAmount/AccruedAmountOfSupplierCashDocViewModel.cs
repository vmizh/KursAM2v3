using System;
using System.ComponentModel.DataAnnotations;
using Core.ViewModel.Base;

namespace KursDomain.Documents.AccruedAmount
{
    public class AccruedAmountOfSupplierCashDocViewModel : RSViewModelBase
    {
        #region Fields

        private string myDocumentType;
        private DateTime myDocDate;
        private decimal mySumma;
        private string myDocNumber;
        private string myCreator;

        #endregion

        #region Properties

        [Display(AutoGenerateField = true, Name = "Тип документа")]
        public string DocumentType
        {
            get => myDocumentType;
            set
            {
                if (myDocumentType == value) return;
                myDocumentType = value;
                RaisePropertyChanged();
            }
        }

        [Display(AutoGenerateField = true, Name = "Дата")]
        public DateTime DocDate
        {
            get => myDocDate;
            set
            {
                if (myDocDate == value) return;
                myDocDate = value;
                RaisePropertyChanged();
            }
        }

        [Display(AutoGenerateField = true, Name = "Сумма")]
        public decimal Summa
        {
            get => mySumma;
            set
            {
                if (mySumma == value) return;
                mySumma = value;
                RaisePropertyChanged();
            }
        }

        [Display(AutoGenerateField = true, Name = "Номер")]
        public string DocNumber
        {
            get => myDocNumber;
            set
            {
                if (myDocNumber == value) return;
                myDocNumber = value;
                RaisePropertyChanged();
            }
        }

        [Display(AutoGenerateField = true, Name = "Создатель")]
        public string Creator
        {
            get => myCreator;
            set
            {
                if (myCreator == value) return;
                myCreator = value;
                RaisePropertyChanged();
            }
        }

        [Display(AutoGenerateField = true, Name = "Примечания")]
        public override string Note
        {
            get => base.Note;
            set
            {
                if (base.Note == value) return;
                base.Note = value;
                RaisePropertyChanged();
            }
        }

        #endregion
    }
}
