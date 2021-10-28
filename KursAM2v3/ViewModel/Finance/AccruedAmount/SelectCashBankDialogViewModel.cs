using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows.Controls;
using Core;
using Core.EntityViewModel.CommonReferences;
using Core.Menu;
using Core.ViewModel.Base;
using DevExpress.Mvvm;
using KursAM2.Managers;

namespace KursAM2.ViewModel.Finance.AccruedAmount
{
    public class SelectCashBankDialogViewModel : RSWindowViewModelBase
    {
        #region Constructors

        public SelectCashBankDialogViewModel(bool isCash, Currency crs)
        {
            IsCash = isCash;
            currency = crs;
            RightMenuBar = MenuGenerator.DialogStandartBar(this);
            RefreshData(null);
        }

        #endregion

        public class CashBankItem : ISimpleDialogItem
        { 
            [Display(AutoGenerateField = true, Name = "Наименование", Order = 1)]
            public string Name { get; set; }

            [Display(AutoGenerateField = true, Name = "Остаток",Order = 2)]
            [DisplayFormat(DataFormatString = "n2")]
            public decimal Summa { set; get; }

            [Display(AutoGenerateField = false)] public decimal DocCode { get; set; }

            [Display(AutoGenerateField = false)] public Guid Id { get; set; }

            [Display(AutoGenerateField = false)] public int Code { get; set; }

           

            public override string ToString()
            {
                return Name;
            }
        }

        #region Fields

        private ICurrentWindowService winCurrentService;
        private CashBankItem myCurrentObject;
        private readonly bool IsCash;
        private Currency currency;
        
        #endregion

        #region Properties

        public new MessageResult DialogResult = MessageResult.No;

        public UserControl CustomDataUserControl { set; get; } = new SelectCashBankDialogView();

        public override string LayoutName => "SelectCashBankDialogViewModel";

        public List<CashBankItem> ObjectList { set; get; } = new List<CashBankItem>();

        public CashBankItem CurrentObject
        {
            get => myCurrentObject;
            set
            {
                if (myCurrentObject == value) return;
                if (value?.Summa <= 0)
                {
                    return;
                }
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
            winCurrentService = GetService<ICurrentWindowService>();
            if (winCurrentService != null)
            {
                DialogResult = MessageResult.OK;
                winCurrentService.Close();
            }

        }

        public override void Cancel(object obj)
        {
            winCurrentService = GetService<ICurrentWindowService>();
            if (winCurrentService != null)
            {
                CurrentObject = null;
                DialogResult = MessageResult.Cancel;
                winCurrentService.Close();
            }
        }

        public sealed override void RefreshData(object obj)
        {
            if (IsCash)
                foreach (var c in MainReferences.Cashs.Values)
                    ObjectList.Add(new CashBankItem
                    {
                        DocCode = c.DocCode,
                        Name = c.Name,
                        Summa = CashManager.GetCashCurrencyRemains(c.DocCode,currency.DocCode, DateTime.Today)
                    });
            else
            {
                var manager = new BankOperationsManager();
                foreach (var c in MainReferences.BankAccounts.Values.Where(_ => _.Currency.DocCode == currency.DocCode))
                    ObjectList.Add(new CashBankItem
                    {
                        DocCode = c.DocCode,
                        Name = c.Name,
                        // ReSharper disable once PossibleInvalidOperationException
                        Summa = manager.GetRemains2(c.DocCode, DateTime.Today, DateTime.Today)?
                            .SummaEnd ?? 0 
                    });
            }
        }

        #endregion
    }
}