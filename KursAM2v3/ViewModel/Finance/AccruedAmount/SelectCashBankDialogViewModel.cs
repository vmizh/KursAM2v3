using System;
using System.Collections.Generic;
using Core;
using Core.Menu;
using Core.ViewModel.Base;
using Core.WindowsManager;

namespace KursAM2.ViewModel.Finance.AccruedAmount
{
    public class SelectCashBankDialogViewModel : RSWindowViewModelBase
    {
        #region Constructors

        public SelectCashBankDialogViewModel(bool isCash)
        {
            IsCash = isCash;
            RightMenuBar = MenuGenerator.DialogStandartBar(this);
            RefreshData(null);
        }

        #endregion

        #region Fields

        private CashBankItem myCurrentObject;
        private readonly bool IsCash;

        #endregion

        #region Properties

        public override string LayoutName => "SelectCashBankDialogViewModel";

        public List<CashBankItem> ObjectList { set; get; } = new List<CashBankItem>();
        
        public CashBankItem CurrentObject
        {
            get => myCurrentObject;
            set
            {
                if (myCurrentObject == value) return;
                myCurrentObject = value;
                RaisePropertyChanged();
            }
        }
            

        #endregion

        #region Command

        public override bool IsOkAllow() 
        {
            return CurrentObject != null;
        }

        public override void Ok(object obj)
        {
            if (Form == null) return;
            Form.DialogResult = true;
            Form.Close();
        }

        public override void Cancel(object obj)
        {
            if (Form == null) return;
            DialogResult = false;
            Form?.Close();
        }

        public sealed override void RefreshData(object obj)
        {
            if (IsCash)
            {
                foreach (var c in MainReferences.Cashs.Values)
                {
                    ObjectList.Add(new CashBankItem
                    {
                        DocCode = c.DocCode,
                        Name = c.Name
                    });
                }
            }
            else
            {
                foreach (var c in MainReferences.BankAccounts.Values)
                {
                    ObjectList.Add(new CashBankItem
                    {
                        DocCode = c.DocCode,
                        Name = c.Name
                    });
                }
            }
        }

        #endregion

        public class CashBankItem : ISimpleDialogItem
        {
            
            public override string ToString()
            {
                return Name;
            }

            public decimal DocCode { get; set; }
            public Guid Id { get; set; }
            public int Code { get; set; }
            public string Name { get; set; }
        }
    }
}