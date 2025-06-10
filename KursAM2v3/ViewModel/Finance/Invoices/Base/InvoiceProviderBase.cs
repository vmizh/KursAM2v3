using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using Data;
using KursDomain;
using KursDomain.Documents.Vzaimozachet;
using KursDomain.IDocuments.Finance;
using KursDomain.References;

namespace KursAM2.ViewModel.Finance.Invoices.Base
{
    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public class InvoiceProviderBase : IInvoiceProvider, INotifyPropertyChanged
    {
        private int? mySFInNum;
        private string mySFPostavNum;
        private DateTime myDocDate;
        private Kontragent myKontragent;
        private decimal mySumma;
        private decimal mySummaFact;
        private bool myIsPay;
        private decimal myPaySumma;
        private PayCondition myPayCondition;
        private bool myIsAccepted;
        private string myNote;
        private string myCreator;
        private PayForm myFormRaschet;
        private bool myIsNDSInPrice;
        private CentrResponsibility myCo;
        private Kontragent myKontrReceiver;
        private Employee myPersonaResponsible;

        public InvoiceProviderBase()
        {
        }

        public InvoiceProviderBase(SD_26 entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
        }

        public InvoiceProviderBase(IEnumerable<InvoicePostQuery> invList, bool isLoadDetails = false)
        {
            if (invList == null || !invList.Any()) throw new ArgumentNullException(nameof(invList));
            var doc = invList.First();
            DocCode = doc.DocCode;
            Id = doc.Id;
            NakladDistributedSumma = doc.NakladDistributedSumma;
            if(doc.PersonalResponsibleDC != null)
                PersonaResponsible = GlobalOptions.ReferencesCache.GetEmployee(doc.PersonalResponsibleDC) as Employee;
            else
            {
                PersonaResponsible = GlobalOptions.ReferencesCache.GetEmployee(doc.EmployeeTabelNumber) as Employee;
            }
            SF_IN_NUM = doc.InNum;
            SF_POSTAV_NUM = doc.PostavNum;
            DocDate = doc.Date;
            Kontragent = GlobalOptions.ReferencesCache.GetKontragent(doc.PostDC) as Kontragent;
            Summa = doc.Summa ?? 0;
            SummaFact = 0;
            foreach (var s in invList)
            {
                if (s.IsUsluga ?? false)
                    SummaFact += (s.Price ?? 0) * s.Quantity;
                else
                    SummaFact += s.ShippedSumma;
            }
            Currency = GlobalOptions.ReferencesCache.GetCurrency(doc.CurrencyDC) as Currency;
            PaySumma = doc.PaySumma;
            IsPay = Summa <= PaySumma;
            PayCondition = GlobalOptions.ReferencesCache.GetPayCondition(doc.SF_PAY_COND_DC) as PayCondition;
            IsAccepted = doc.IsAccepted ?? false;
            Note = doc.Note;
            CREATOR = doc.Creator;
            FormRaschet = GlobalOptions.ReferencesCache.GetPayForm(doc.FormRaschetDC) as PayForm;
            IsNDSInPrice = doc.IsNDSInPrice ?? false;
            CO = GlobalOptions.ReferencesCache.GetCentrResponsibility(doc.CO_DC) as CentrResponsibility;
            KontrReceiver = GlobalOptions.ReferencesCache.GetKontragent(doc.PoluchatDC) as Kontragent;
            VzaimoraschetTypeDC = doc.VzaimoraschetTypeDC;
            if (!isLoadDetails) return;
            Rows = new ObservableCollection<IInvoiceProviderRow>();
            foreach (var r in invList) Rows.Add(new InvoiceProviderRowBase(r));
        }

        public Currency Currency { get; set; }


        public decimal DocCode { get; set; }
        public Guid Id { get; set; }
        public decimal? NakladDistributedSumma { get; set; }

        public Employee PersonaResponsible
        {
            get => myPersonaResponsible;
            set
            {
                if (Equals(value, myPersonaResponsible)) return;
                myPersonaResponsible = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(EmployeeTabelNumber));
            }
        }

        public int? EmployeeTabelNumber => PersonaResponsible?.TabelNumber;

        public int? SF_IN_NUM
        {
            get => mySFInNum;
            set
            {
                if (value == mySFInNum) return;
                mySFInNum = value;
                OnPropertyChanged();
            }
        }

        public string SF_POSTAV_NUM
        {
            get => mySFPostavNum;
            set
            {
                if (value == mySFPostavNum) return;
                mySFPostavNum = value;
                OnPropertyChanged();
            }
        }

        public DateTime DocDate
        {
            get => myDocDate;
            set
            {
                if (value.Equals(myDocDate)) return;
                myDocDate = value;
                OnPropertyChanged();
            }
        }

        public Kontragent Kontragent
        {
            get => myKontragent;
            set
            {
                if (Equals(value, myKontragent)) return;
                myKontragent = value;
                OnPropertyChanged();
            }
        }

        public decimal Summa
        {
            get => mySumma;
            set
            {
                if (value == mySumma) return;
                mySumma = value;
                OnPropertyChanged();
            }
        }

        public decimal SummaFact
        {
            get => mySummaFact;
            set
            {
                if (value == mySummaFact) return;
                mySummaFact = value;
                OnPropertyChanged();
            }
        }

        public bool IsPay
        {
            get => myIsPay;
            set
            {
                if (value == myIsPay) return;
                myIsPay = value;
                OnPropertyChanged();
            }
        }

        public decimal PaySumma
        {
            get => myPaySumma;
            set
            {
                if (value == myPaySumma) return;
                myPaySumma = value;
                OnPropertyChanged();
            }
        }

        public PayCondition PayCondition
        {
            get => myPayCondition;
            set
            {
                if (Equals(value, myPayCondition)) return;
                myPayCondition = value;
                OnPropertyChanged();
            }
        }

        public bool IsAccepted
        {
            get => myIsAccepted;
            set
            {
                if (value == myIsAccepted) return;
                myIsAccepted = value;
                OnPropertyChanged();
            }
        }

        public string Note
        {
            get => myNote;
            set
            {
                if (value == myNote) return;
                myNote = value;
                OnPropertyChanged();
            }
        }

        public string CREATOR
        {
            get => myCreator;
            set
            {
                if (value == myCreator) return;
                myCreator = value;
                OnPropertyChanged();
            }
        }

        public PayForm FormRaschet
        {
            get => myFormRaschet;
            set
            {
                if (Equals(value, myFormRaschet)) return;
                myFormRaschet = value;
                OnPropertyChanged();
            }
        }

        public bool IsNDSInPrice
        {
            get => myIsNDSInPrice;
            set
            {
                if (value == myIsNDSInPrice) return;
                myIsNDSInPrice = value;
                OnPropertyChanged();
            }
        }

        public CentrResponsibility CO
        {
            get => myCo;
            set
            {
                if (Equals(value, myCo)) return;
                myCo = value;
                OnPropertyChanged();
            }
        }

        public Kontragent KontrReceiver
        {
            get => myKontrReceiver;
            set
            {
                if (Equals(value, myKontrReceiver)) return;
                myKontrReceiver = value;
                OnPropertyChanged();
            }
        }

        public bool? IsExcludeFromPays { get; set; }

        public decimal? VzaimoraschetTypeDC { get; set; }
        public ObservableCollection<IInvoiceProviderRow> Rows { get; set; }
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

        public string LastChanger { get; set; }
        public DateTime LastChangerDate { get; set; }
    }

    public class InvoiceProviderRowBase : IInvoiceProviderRow
    {
        public InvoiceProviderRowBase(InvoicePostQuery row)
        {
            DocCode = row.DocCode;
            Code = row.CODE;
            Id = row.RowId;
            DocId = row.Id;
            var Nomenkl = GlobalOptions.ReferencesCache.GetNomenkl(row.NomenklDC);
            var Unit = Nomenkl?.Unit;
            Price = row.Price ?? 0;
            Quantity = row.Quantity;
            NDSPercent = row.NDSPercent;
            SummaNaklad = row.NakladSumma;
            NDSSumma = row.NDSSumma;
            SFT_SUMMA_K_OPLATE = row.Summa;
            Summa = row.Summa ?? 0;
            IsUsluga = Nomenkl.IsUsluga;
            IsNaklad = Nomenkl.IsNakladExpense;
            IsIncludeInPrice = row.IsNDSInPrice ?? false;
            SFT_SUMMA_K_OPLATE_KONTR_CRS = row.Summa;
        }

        public InvoiceProviderRowBase()
        {
        }

        public decimal? SFT_SUMMA_K_OPLATE { get; set; }
        public decimal? SFT_SUMMA_K_OPLATE_KONTR_CRS { get; set; }

        public decimal DocCode { get; set; }
        public int Code { get; set; }
        public Guid Id { get; set; }
        public Guid DocId { get; set; }
        public string Note { get; set; }
        public Unit PostUnit { get; set; }
        public Unit UchUnit { get; set; }
        public decimal SFT_POST_KOL { get; set; }
        public Nomenkl Nomenkl { get; set; }
        public string NomenklNumber { get; set; }
        public Unit Unit { get; set; }
        public decimal Price { get; set; }
        public decimal PriceWithNDS { get; }
        public decimal Quantity { get; set; }
        public decimal NDSPercent { get; set; }
        public decimal? SummaNaklad { get; set; }
        public decimal? NDSSumma { get; set; }
        public decimal Summa { get; set; }
        public bool IsUsluga { get; }
        public bool IsNaklad { get; }
        public bool IsIncludeInPrice { get; set; }
        public SDRSchet SDRSchet { get; set; }
        public decimal Shipped { get; set; }
    }
}
