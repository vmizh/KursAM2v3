using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using Core;
using Core.EntityViewModel.StockHolder;
using Core.ViewModel.Base;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using KursAM2.View.DialogUserControl;

namespace KursAM2.ViewModel.StockHolder
{
    public sealed class SelectStockHolderDialogViewModel : RSWindowViewModelBase
    {
        #region Fields

        private StockHolderViewModel myCurrentObject;
        private ICurrentWindowService winCurrentService;

        #endregion

        #region Constructors

        public SelectStockHolderDialogViewModel()
        {
            RefreshData(null);
        }

        #endregion

        #region Commands

        public override bool IsOkAllow()
        {
            return CurrentItem != null;
        }

        public override void Ok(object obj)
        {
            winCurrentService = this.GetService<ICurrentWindowService>();
            if (winCurrentService != null)
            {
                DialogResult = MessageResult.OK;
                winCurrentService.Close();
            }

        }

        public override void Cancel(object obj)
        {
            winCurrentService = this.GetService<ICurrentWindowService>();
            if (winCurrentService != null)
            {
                CurrentItem = null;
                DialogResult = MessageResult.Cancel;
                winCurrentService.Close();
            }
        }

        public override void RefreshData(object obj)
        {
            ItemsCollection.Clear();
            using (var ctx = GlobalOptions.GetEntities())
            {
                var data = ctx.StockHolders.ToList();
                foreach(var d in data)
                    ItemsCollection.Add(new StockHolderViewModel(d));
            }
        }

        #endregion

        #region Properties

        public new MessageResult DialogResult = MessageResult.No;


        public UserControl CustomDataUserControl { set; get; } = new DefaultTableSelectUC();

        public override string LayoutName => "SelectStockHolderDialogViewModel";

        public List<StockHolderViewModel> ItemsCollection { set; get; } = new List<StockHolderViewModel>();

        public StockHolderViewModel CurrentItem
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
    }
}
