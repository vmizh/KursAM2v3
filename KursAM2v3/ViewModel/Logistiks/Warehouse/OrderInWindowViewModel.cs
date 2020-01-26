using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Core;
using Core.EntityViewModel;
using Core.Menu;
using Core.ViewModel.Base;
using Core.WindowsManager;
using KursAM2.Dialogs;
using KursAM2.Managers;
using KursAM2.View.Logistiks.Warehouse;
using WarehouseManager = KursAM2.Managers.Invoices.WarehouseManager;

namespace KursAM2.ViewModel.Logistiks.Warehouse
{
    public sealed class OrderInWindowViewModel : RSWindowViewModelBase
    {
        private readonly WarehouseManager orderManager;
        private WarehouseOrderIn myDocument;

        [SuppressMessage("ReSharper", "VirtualMemberCallInConstructor")]
        public OrderInWindowViewModel(StandartErrorManager errManager)
        {
            IsDocNewCopyAllow = true;
            IsDocNewCopyRequisiteAllow = true;
            LeftMenuBar = MenuGenerator.DocWithRowsLeftBar(this);
            RightMenuBar = MenuGenerator.StandartDocWithDeleteRightBar(this);
            orderManager = new WarehouseManager(errManager);
            Document = orderManager.NewOrderIn();
        }

        public OrderInWindowViewModel(StandartErrorManager errManager, decimal dc)
        {
            IsDocNewCopyAllow = true;
            IsDocNewCopyRequisiteAllow = true;
            LeftMenuBar = MenuGenerator.DocWithRowsLeftBar(this);
            RightMenuBar = MenuGenerator.StandartDocWithDeleteRightBar(this);
            orderManager = new WarehouseManager(errManager);
            RefreshData(dc);
        }

        #region Properties

        public WarehouseOrderIn Document
        {
            set
            {
                if (myDocument != null && myDocument.Equals(value)) return;
                myDocument = value;
                Rows.Clear();
                foreach (var r in Document.Rows)
                {
                    Rows.Add(r);
                }
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(Rows));
            }
            get => myDocument;
        }

        public ObservableCollection<WarehouseOrderInRow> Rows { set; get; } = new ObservableCollection<WarehouseOrderInRow>();

        private WarehouseOrderInRow myCurrentRow;
        public WarehouseOrderInRow CurrentRow
        {
            get => myCurrentRow;
            set
            {
                if (myCurrentRow != null && myCurrentRow.Equals(value)) return;
                myCurrentRow = value;
                RaisePropertyChanged();
            }
        }
        public bool IsCanChangedWarehouseType => Document?.Sender == null;
        public override string WindowName =>
            $"Приходный складской ордер №{Document?.DD_IN_NUM}/{Document?.DD_EXT_NUM} от {Document?.DD_DATE}";
        
        #endregion

        #region Command

        public override bool IsDocDeleteAllow => Document != null && Document.State != RowStatus.NewRow;
        public override bool IsCanRefresh => Document != null && Document.State != RowStatus.NotEdited;
        public override bool IsCanSaveData => Document != null && (Document.State != RowStatus.NotEdited
                                                                   || Document.Rows.Any(_ =>
                                                                       _.State != RowStatus.NotEdited)
                                                                   || Document.DeletedRows.Count > 0);
        public override RowStatus State => Document?.State ?? RowStatus.NewRow;

        public override void DocNewEmpty(object form)
        {
            var frm = new OrderInView {Owner = Application.Current.MainWindow};
            var ctx = new OrderInWindowViewModel(new StandartErrorManager(GlobalOptions.GetEntities(),
                    "WarehouseOrderIn", true))
                {Form = frm};
            frm.Show();
            frm.DataContext = ctx;
        }

        public override void DocNewCopy(object form)
        {
            if (Document == null) return;
            var frm = new OrderInView {Owner = Application.Current.MainWindow};
            var ctx = new OrderInWindowViewModel(new StandartErrorManager(GlobalOptions.GetEntities(),
                    "WarehouseOrderIn", true))
                {Form = frm};
            ctx.Document = orderManager.NewOrderInCopy(Document);
            frm.Show();
            frm.DataContext = ctx;
        }

        public override void DocNewCopyRequisite(object form)
        {
            if (Document == null) return;
            var frm = new OrderInView {Owner = Application.Current.MainWindow};
            var ctx = new OrderInWindowViewModel(new StandartErrorManager(GlobalOptions.GetEntities(),
                "WarehouseOrderIn", true)) {Form = frm, Document = orderManager.NewOrderInRecuisite(Document)};
            frm.Show();
            frm.DataContext = ctx;
        }

        public override void RefreshData(object obj)
        {
            if (Document != null && Document.DocCode > 0)
            {
                Document = orderManager.GetOrderIn(Document.DocCode);
            }
            else
            {
                var dc = obj as decimal? ?? 0;
                if (dc != 0)
                    Document = orderManager.GetOrderIn(dc);
            }
            RaisePropertyChanged(nameof(Document));
            RaisePropertyChanged(nameof(Document.Sender));
            RaisePropertyChanged(nameof(Document.WarehouseIn));
        }

        public override void SaveData(object data)
        {
            Document.DD_POLUCH_NAME = Document.WarehouseIn.Name;
            Document.DD_OTRPAV_NAME = Document.Sender;
            Document.DD_TYPE_DC = 2010000001;
            var dc = orderManager.SaveOrderIn(Document);
            if (dc > 0)
            {
                RefreshData(dc);
            }
        }

        public ICommand DeleteLinkDocumentCommand
        {
            get { return new Command(DeleteLinkDocument, _ => CurrentRow != null && CurrentRow.LinkDocument != null); }
        }

        private void DeleteLinkDocument(object obj)
        {
            foreach (var r in Document.SelectedRows)
            {
                r.LinkInvoice = null;
                r.LinkOrder = null;
                r.DDT_SPOST_DC = null;
                r.DDT_SPOST_ROW_CODE = null;
                r.InvoiceProvider = null;
                r.InvoiceProviderRow = null;
                r.DDT_TAX_EXECUTED = 0;
                r.DDT_FACT_EXECUTED = 0;
            }
        }

        public ICommand OpenLinkDocumentCommand
        {
            get { return new Command(OpenLinkDocument, _ => CurrentRow != null && CurrentRow.LinkDocument != null); }
        }

        private void OpenLinkDocument(object obj)
        {
            if (CurrentRow.LinkInvoice != null)
            {
                DocumentsOpenManager.Open(DocumentType.InvoiceProvider, (decimal)CurrentRow.DDT_SPOST_DC);
            }
        }

        public ICommand AddFromDocumentCommand
        {
            get { return new Command(AddFromDocument, _ => true); }
        }

        //TODO создать диалог для вставки в приходный ордер строк из счетов фактур,ордеров и внутренних перемещений
        private void AddFromDocument(object obj)
        {
            
        }

        public ICommand AddNomenklCommand
        {
            get { return new Command(AddNomenkl, _ => true); }
        }

        private void AddNomenkl(object obj)
        {
            var nomenkls = StandartDialogs.SelectNomenkls();
            if (nomenkls == null || nomenkls.Count <= 0) return;
            foreach (var n in nomenkls)
                if (Document.Rows.All(_ => _.Nomenkl.DocCode != n.DocCode))
                    Document.Rows.Add(new WarehouseOrderInRow
                    {
                        DocCode = -1,
                        Nomenkl = n,
                        DDT_KOL_PRIHOD = 1,
                        Unit = n.Unit,
                        Currency = n.Currency,
                        State = RowStatus.NewRow
                    });
        }

        public ICommand DeleteNomenklCommand
        {
            get { return new Command(DeleteNomenkl, _ => Document.SelectedRows.Count > 0); }
        }

        private void DeleteNomenkl(object obj)
        {
            var delList = new List<WarehouseOrderInRow>(Document.SelectedRows.ToList());
            foreach (var row in delList)
                if (row.State == RowStatus.NewRow)
                {
                    Document.Rows.Remove(row);
                }
                else
                {
                    Document.DeletedRows.Add(row);
                    Document.Rows.Remove(row);
                }
        }

        public override void DocDelete(object form)
        {
            var res = MessageBox.Show("Вы уверены, что хотите удалить данный документ?", "Запрос",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            switch (res)
            {
                case MessageBoxResult.Yes:
                    orderManager.DeleteOrderIn(Document);
                    CloseWindow(Form);
                    break;
                case MessageBoxResult.No:
                    break;
            }
        }

        public override void CloseWindow(object form)
        {
            if (IsCanSaveData && Document?.State != RowStatus.Deleted)
            {
                var res = MessageBox.Show("В документ были внесены изменения, сохранить?", "Запрос",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);
                switch (res)
                {
                    case MessageBoxResult.Yes:
                        SaveData(null);
                        break;
                    case MessageBoxResult.No:
                        break;
                    case MessageBoxResult.Cancel:
                        return;
                }
            }
            if (Form != null)
            {
                Form.Close();
                return;
            }
            var frm = form as Window;
            frm?.Close();
        }

        #endregion
    }
}