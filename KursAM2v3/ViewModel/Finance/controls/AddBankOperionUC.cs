using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Core;
using Core.EntityViewModel;
using Core.ViewModel.Base;
using Core.ViewModel.Common;
using KursAM2.View.Finance.UC;

namespace KursAM2.ViewModel.Finance.controls
{
    // ReSharper disable once InconsistentNaming
    public class AddBankOperionUC : RSWindowViewModelBase
    {
        private BankAccount myBankAccount;
#pragma warning disable 414
        private BankOperationType myBankOperationType = BankOperationType.NotChoice;
#pragma warning restore 414
        private BankOperationsViewModel myCurrentBankOperations;
        private BankOperationsComareRowView myDataUserControl;
        private string myOperType;

        public AddBankOperionUC()
        {
            myDataUserControl = new BankOperationsComareRowView();
            // ReSharper disable once VirtualMemberCallInConstructor
            WindowName = "Добавить операцию";
        }

        public AddBankOperionUC(decimal docCode) : this()
        {
            CurrentBankOperations = new BankOperationsViewModel {Date = DateTime.Today};
            BankOperationType = BankOperationType.NotChoice;
            // ReSharper disable once VirtualMemberCallInConstructor
            RefreshData(docCode);
        }

        // ReSharper disable once UnusedParameter.Local
        public AddBankOperionUC(decimal docCode, BankOperationsViewModel row, BankAccount bankAcc) : this()
        {
            BankAccount = bankAcc;
            CurrentBankOperations = new BankOperationsViewModel
            {
                Date = DateTime.Today,
                Currency = row.Currency ?? GlobalOptions.SystemProfile.MainCurrency,
                Kontragent = row.Kontragent,
                Payment = row.Payment,
                VVT_VAL_RASHOD = row.VVT_VAL_RASHOD,
                VVT_VAL_PRIHOD = row.VVT_VAL_PRIHOD,
                SHPZ = row.SHPZ,
                State = RowStatus.NewRow
            };
            BankOperationType = BankOperationType.NotChoice;

            //RefreshData(docCode);
        }

        public bool IsChangeTypeEnable => CurrentBankOperations?.State == RowStatus.NewRow;

        public BankAccount BankAccount
        {
            get => myBankAccount;
            set
            {
                if (myBankAccount != null && myBankAccount.Equals(value)) return;
                myBankAccount = value;
                RaisePropertyChanged();
            }
        }

        public bool NotAllowSummaPrihodChanged => BankOperationType == BankOperationType.BankIn ||
                                                  BankOperationType == BankOperationType.CashIn
                                                  && CurrentBankOperations?.VVT_SFACT_POSTAV_DC != null;

        public bool NotAllowSummaRashodChanged => BankOperationType == BankOperationType.BankOut ||
                                                  BankOperationType == BankOperationType.CashOut
                                                  && CurrentBankOperations?.VVT_SFACT_CLIENT_DC != null;

        public string KontragentName => CurrentBankOperations?.KontragentName;

        public ICommand SFNameRemoveCommand
        {
            get { return new Command(SFNameRemove, _ => !string.IsNullOrEmpty(SFName)); }
        }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string CurrentOperType
        {
            get => myOperType;
            set
            {
                if (myOperType == value) return;
                myOperType = value;
                VVT_VAL_PRIHOD = 0;
                VVT_VAL_RASHOD = 0;
                VVT_DOC_NUM = null;
                Kontragent = null;
                Payment = null;
                CurrentBankOperations.CashIn = null;
                CurrentBankOperations.CashOut = null;
                CurrentBankOperations.BankAccountIn = null;
                CurrentBankOperations.BankAccountOut = null;
                switch (myOperType)
                {
                    case "Контрагент":
                        myBankOperationType = BankOperationType.Kontragent;
                        break;
                    case "Приходный кассовый ордер":
                        myBankOperationType = BankOperationType.CashIn;
                        break;
                    case "Расходный кассовый ордер":
                        myBankOperationType = BankOperationType.CashOut;
                        break;
                    case "Банк получатель":
                        myBankOperationType = BankOperationType.BankIn;
                        break;
                    case "Банк отправитель":
                        myBankOperationType = BankOperationType.BankOut;
                        break;
                    default:
                        myBankOperationType = BankOperationType.NotChoice;
                        break;
                }

                RaisePropertyChanged();
                RaisePropertiesChanged(nameof(KontragentName));
                DataUserControl.Kontragent.Text = null;
                RaisePropertiesChanged(nameof(NotAllowSummaPrihodChanged));
                RaisePropertiesChanged(nameof(NotAllowSummaRashodChanged));
            }
        }

        public BankOperationType BankOperationType
        {
            set
            {
                if (CurrentBankOperations.BankOperationType == value) return;
                Kontragent = null;
                Payment = null;
                CurrentBankOperations.BankAccountIn = null;
                CurrentBankOperations.BankAccountOut = null;
                CurrentBankOperations.CashIn = null;
                CurrentBankOperations.CashOut = null;
                CurrentBankOperations.BankOperationType = value;
                if (BankOperationType == BankOperationType.BankIn || BankOperationType == BankOperationType.BankOut)
                    Payment = GlobalOptions.SystemProfile.OwnerKontragent;
                RaisePropertyChanged();
                RaisePropertiesChanged(nameof(NotAllowSummaPrihodChanged));
                RaisePropertiesChanged(nameof(NotAllowSummaRashodChanged));
                RaisePropertiesChanged(nameof(KontragentName));
            }
            get => myCurrentBankOperations.BankOperationType;
        }

        public BankOperationsViewModel CurrentBankOperations
        {
            set
            {
                if (myCurrentBankOperations != null && myCurrentBankOperations.Equals(value)) return;
                myCurrentBankOperations = value;
                RaisePropertyChanged();
            }
            get => myCurrentBankOperations;
        }

        public BankAccount BankAccountOut
        {
            get => myCurrentBankOperations.myBankAccountOut;
            set
            {
                if (myCurrentBankOperations.myBankAccountOut != null &&
                    myCurrentBankOperations.myBankAccountOut.Equals(value)) return;
                myCurrentBankOperations.myBankAccountOut = value;
                RaisePropertyChanged();
            }
        }

        public string SFName
        {
            get => myCurrentBankOperations.SFName;
            set
            {
                if (myCurrentBankOperations.SFName == value) return;
                myCurrentBankOperations.SFName = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(IsPrihodSummaEnabled));
                RaisePropertyChanged(nameof(IsRashodSummaEnabled));
                RaisePropertyChanged(nameof(IsKontragentEnabled));
            }
        }

        public BankAccount BankAccountIn
        {
            get => myCurrentBankOperations.myBankAccountIn;
            set
            {
                if (myCurrentBankOperations.myBankAccountIn != null &&
                    myCurrentBankOperations.myBankAccountIn.Equals(value)) return;
                myCurrentBankOperations.myBankAccountIn = value;
                RaisePropertyChanged();
            }
        }

        public BankOperationsComareRowView DataUserControl
        {
            set
            {
                if (Equals(myDataUserControl, value)) return;
                myDataUserControl = value;
                RaisePropertiesChanged();
            }
            get => myDataUserControl;
        }

        public Currency Currency
        {
            set
            {
                if (Equals(CurrentBankOperations.Currency, value)) return;
                CurrentBankOperations.Currency = value;
                RaisePropertyChanged();
            }
            get => CurrentBankOperations.Currency;
        }

        public SDRSchet SHPZ
        {
            set
            {
                if (Equals(CurrentBankOperations.SHPZ, value)) return;
                CurrentBankOperations.SHPZ = value;
                RaisePropertyChanged();
            }
            get => CurrentBankOperations.SHPZ;
        }

        public Kontragent Payment
        {
            set
            {
                // ReSharper disable once PossibleUnintendedReferenceComparison
                if (CurrentBankOperations != null && CurrentBankOperations.Payment == value) return;
                if (CurrentBankOperations != null) CurrentBankOperations.Payment = value;
                RaisePropertyChanged();
            }
            get => CurrentBankOperations.Payment;
        }

        public Kontragent Kontragent
        {
            set
            {
                // ReSharper disable once PossibleUnintendedReferenceComparison
                if (CurrentBankOperations != null && CurrentBankOperations.Kontragent == value) return;
                if (CurrentBankOperations != null) CurrentBankOperations.Kontragent = value;
                RaisePropertyChanged();
            }
            get => CurrentBankOperations.Kontragent;
        }

        public string VVT_DOC_NUM
        {
            set
            {
                if (CurrentBankOperations.VVT_DOC_NUM == value) return;
                CurrentBankOperations.VVT_DOC_NUM = value;
                RaisePropertyChanged();
            }
            get => CurrentBankOperations.VVT_DOC_NUM;
        }

        public decimal VVT_VAL_RASHOD
        {
            set
            {
                if (CurrentBankOperations.VVT_VAL_RASHOD == value) return;
                CurrentBankOperations.VVT_VAL_RASHOD = value;
                RaisePropertyChanged();
            }
            get => Convert.ToDecimal(CurrentBankOperations.VVT_VAL_RASHOD);
        }

        public decimal VVT_VAL_PRIHOD
        {
            set
            {
                if (CurrentBankOperations.VVT_VAL_PRIHOD == value) return;
                CurrentBankOperations.VVT_VAL_PRIHOD = value;
                RaisePropertyChanged();
            }
            get => Convert.ToDecimal(CurrentBankOperations.VVT_VAL_PRIHOD);
        }

        public DateTime Date
        {
            set
            {
                if (CurrentBankOperations.Date == value) return;
                CurrentBankOperations.Date = value;
                RaisePropertyChanged();
            }
            get => CurrentBankOperations.Date;
        }

        // ReSharper disable once IdentifierTypo
        public List<Currency> CurrencysCompendium => MainReferences.Currencies.Values.ToList();
        // ReSharper disable once InconsistentNaming
        public List<SDRSchet> SHPZList => MainReferences.SDRSchets.Values.ToList();

        public bool IsRashodSummaEnabled => CurrentBankOperations.VVT_SFACT_CLIENT_DC == null &&
                                            CurrentBankOperations.VVT_RASH_KASS_ORDER_DC == null;

        public bool IsPrihodSummaEnabled => CurrentBankOperations.VVT_SFACT_POSTAV_DC == null &&
                                            CurrentBankOperations.VVT_KASS_PRIH_ORDER_DC == null;

        public bool IsKontragentEnabled => CurrentBankOperations.VVT_SFACT_CLIENT_DC == null &&
                                           CurrentBankOperations.VVT_RASH_KASS_ORDER_DC == null
                                           && CurrentBankOperations.VVT_SFACT_POSTAV_DC == null &&
                                           CurrentBankOperations.VVT_KASS_PRIH_ORDER_DC == null;

        private void SFNameRemove(object obj)
        {
            CurrentBankOperations.VVT_SFACT_CLIENT_DC = null;
            CurrentBankOperations.VVT_SFACT_POSTAV_DC = null;
            SFName = null;
        }

        public override void RefreshData(object o)
        {
            base.RefreshData(o);
            CurrentBankOperations.Code = -1;
            CurrentBankOperations.DOC_CODE = (decimal) o;
        }

        public override bool IsOkAllow()
        {
            return (CurrentBankOperations.VVT_VAL_PRIHOD != 0 ||
                    CurrentBankOperations.VVT_VAL_RASHOD != 0) &&
                   !string.IsNullOrEmpty(CurrentBankOperations.KontragentName) &&
                   CurrentBankOperations.Currency != null
                   && BankOperationType != BankOperationType.NotChoice;
        }
    }
}