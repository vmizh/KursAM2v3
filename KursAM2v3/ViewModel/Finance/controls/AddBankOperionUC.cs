using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using Core;
using Core.EntityViewModel.Bank;
using Core.EntityViewModel.CommonReferences;
using Core.EntityViewModel.CommonReferences.Kontragent;
using Core.ViewModel.Base;
using KursAM2.View.Base;
using KursAM2.View.Finance.UC;

namespace KursAM2.ViewModel.Finance.controls
{
    // ReSharper disable once InconsistentNaming
    public sealed class AddBankOperionUC : RSWindowViewModelBase
    {
        private readonly Brush ColorCanEdit = new SolidColorBrush(Colors.Blue);
        private readonly Brush colorCanEditButZero = new SolidColorBrush(Colors.Orange);
        private readonly Brush colorNon = new SolidColorBrush(Colors.Gray);
        private readonly Brush colorNonCanEdit = new SolidColorBrush(Colors.LightGray);
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
            CurrentBankOperations = new BankOperationsViewModel { Date = DateTime.Today };
            BankOperationType = BankOperationType.NotChoice;
            // ReSharper disable once VirtualMemberCallInConstructor
            RefreshData(docCode);
        }

        // ReSharper disable once UnusedParameter.Local
        public AddBankOperionUC(decimal docCode, BankOperationsViewModel row, BankAccount bankAcc, bool isNew) : this()
        {
            BankAccount = bankAcc;
            CurrentBankOperations = new BankOperationsViewModel
            {
                DocCode = row.DOC_CODE,
                Code = row.Code,
                BankAccount = bankAcc,
                Date = isNew ? DateTime.Today : row.Date,
                Currency = bankAcc.Currency,
                Kontragent = row.Kontragent,
                BankOperationType = row.BankOperationType,
                Payment = row.Payment,
                VVT_VAL_RASHOD = row.VVT_VAL_RASHOD,
                VVT_VAL_PRIHOD = row.VVT_VAL_PRIHOD,
                SHPZ = row.SHPZ,
                VVT_SFACT_CLIENT_DC = row.VVT_SFACT_CLIENT_DC,
                VVT_SFACT_POSTAV_DC = row.VVT_SFACT_POSTAV_DC,
                SFName = row.SFName,
                VVT_DOC_NUM = row.VVT_DOC_NUM,
                State = isNew ? RowStatus.NewRow : RowStatus.NotEdited
            };
        }

        // ReSharper disable once InconsistentNaming
        public List<SDRSchet> SHPZList => MainReferences.SDRSchets.Values.ToList();

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
                                                  && CurrentBankOperations?.VVT_SFACT_POSTAV_DC != null
                                                  || IsNotCurrencyChange;

        public bool NotAllowSummaRashodChanged => BankOperationType == BankOperationType.BankOut ||
                                                  BankOperationType == BankOperationType.CashOut
                                                  && CurrentBankOperations?.VVT_SFACT_CLIENT_DC != null
                                                  || IsNotCurrencyChange;

        public string KontragentName => CurrentBankOperations?.KontragentName;

        public bool IsNotCurrencyChange => CurrentBankOperations?.IsCurrencyChange == false;

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
                    case "Обмен валюты":
                        myBankOperationType = BankOperationType.CurrencyChange;
                        break;
                    default:
                        myBankOperationType = BankOperationType.NotChoice;
                        break;
                }

                RaisePropertyChanged();
                RaisePropertyChanged(nameof(KontragentName));
                DataUserControl.Kontragent.Text = null;
                RaisePropertyChanged(nameof(NotAllowSummaPrihodChanged));
                RaisePropertyChanged(nameof(NotAllowSummaRashodChanged));
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

                RaisePropertyChanged();
                RaisePropertyChanged(nameof(IsPrihodSummaEnabled));
                RaisePropertyChanged(nameof(IsRashodSummaEnabled));
                RaisePropertyChanged(nameof(NotAllowSummaPrihodChanged));
                RaisePropertyChanged(nameof(NotAllowSummaRashodChanged));
                RaisePropertyChanged(nameof(KontragentName));
                RaisePropertyChanged(nameof(PrihodEditMode));
                RaisePropertyChanged(nameof(RashodEditMode));

                SetBrushForPrihodRashod();
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
                SetBrushForPrihodRashod();
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
                RaisePropertyChanged();
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
                SetBrushForPrihodRashod();
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
                SetBrushForPrihodRashod();
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

        public bool IsPrihodSummaEnabled => CurrentBankOperations.BankOperationType == BankOperationType.BankOut
                                            || CurrentBankOperations.BankOperationType == BankOperationType.CashOut
                                            || CurrentBankOperations.BankOperationType == BankOperationType.Kontragent;

        public bool IsRashodSummaEnabled => CurrentBankOperations.BankOperationType == BankOperationType.BankIn
                                            || CurrentBankOperations.BankOperationType == BankOperationType.CashIn
                                            || CurrentBankOperations.BankOperationType == BankOperationType.Kontragent;


        public bool IsKontragentEnabled => CurrentBankOperations.VVT_SFACT_CLIENT_DC == null &&
                                           CurrentBankOperations.VVT_RASH_KASS_ORDER_DC == null
                                           && CurrentBankOperations.VVT_SFACT_POSTAV_DC == null &&
                                           CurrentBankOperations.VVT_KASS_PRIH_ORDER_DC == null;

        public EditModeEnum PrihodEditMode
        {
            get
            {
                switch (BankOperationType)
                {
                    case BankOperationType.BankIn:
                    case BankOperationType.CashIn:
                        return EditModeEnum.NotCanEdit;
                    case BankOperationType.Kontragent:
                        if (VVT_VAL_RASHOD > 0)
                            return EditModeEnum.CanEditButZero;
                        else
                            return EditModeEnum.CanEdit;
                    case BankOperationType.BankOut:
                    case BankOperationType.CashOut:
                        return EditModeEnum.CanEdit;
                }

                return EditModeEnum.None;
            }
        }

        public EditModeEnum RashodEditMode
        {
            get
            {
                switch (BankOperationType)
                {
                    case BankOperationType.BankIn:
                    case BankOperationType.CashIn:
                        return EditModeEnum.CanEdit;
                    case BankOperationType.Kontragent:
                        if (VVT_VAL_PRIHOD > 0)
                            return EditModeEnum.CanEditButZero;
                        else
                            return EditModeEnum.CanEdit;
                    case BankOperationType.BankOut:
                    case BankOperationType.CashOut:
                        return EditModeEnum.NotCanEdit;
                }

                return EditModeEnum.None;
            }
        }

        public void SetBrushForPrihodRashod()
        {
            if (Form is SelectDialogView frm)
                if (frm.contentControl.Content is BankOperationsComareRowView ctrl)
                {
                    var bInc = ctrl.borderIncoming;
                    var bCons = ctrl.borderIConsumption;
                    switch (PrihodEditMode)
                    {
                        case EditModeEnum.CanEdit:
                            bInc.BorderBrush = ColorCanEdit;
                            break;
                        case EditModeEnum.None:
                            bInc.BorderBrush = colorNon;
                            break;
                        case EditModeEnum.NotCanEdit:
                            bInc.BorderBrush = colorNonCanEdit;
                            break;
                        case EditModeEnum.CanEditButZero:
                            bInc.BorderBrush = colorCanEditButZero;
                            break;
                    }

                    switch (RashodEditMode)
                    {
                        case EditModeEnum.CanEdit:
                            bCons.BorderBrush = ColorCanEdit;
                            break;
                        case EditModeEnum.None:
                            bCons.BorderBrush = colorNon;
                            break;
                        case EditModeEnum.NotCanEdit:
                            bCons.BorderBrush = colorNonCanEdit;
                            break;
                        case EditModeEnum.CanEditButZero:
                            bCons.BorderBrush = colorCanEditButZero;
                            break;
                    }
                }
        }

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
            CurrentBankOperations.DOC_CODE = (decimal)o;
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


    public enum EditModeEnum
    {
        CanEdit = 1,
        NotCanEdit = 2,
        CanEditButZero = 3,
        None = 0
    }
}