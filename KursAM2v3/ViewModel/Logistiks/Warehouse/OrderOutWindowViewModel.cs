using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Core.ViewModel.Base;
using Core.WindowsManager;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using KursAM2.Dialogs;
using KursAM2.Managers;
using KursAM2.ReportManagers;
using KursAM2.View.Logistiks.Warehouse;
using KursDomain;
using KursDomain.Documents.CommonReferences;
using KursDomain.Documents.NomenklManagement;
using KursDomain.ICommon;
using KursDomain.Managers;
using KursDomain.Menu;
using KursDomain.References;

namespace KursAM2.ViewModel.Logistiks.Warehouse
{
    public sealed class OrderOutWindowViewModel : RSWindowViewModelBase
    {
        private readonly NomenklManager2 nomenklManager = new NomenklManager2(GlobalOptions.GetEntities());

        //private readonly NomenklManager nomManager = new NomenklManager();
        private readonly WarehouseManager orderManager;
        private readonly WindowManager winManager = new WindowManager();
        private WarehouseOrderOutRow myCurrentRow;
        private WarehouseOrderOut myDocument;

        [SuppressMessage("ReSharper", "VirtualMemberCallInConstructor")]
        public OrderOutWindowViewModel(StandartErrorManager errManager)
        {
            IsDocNewCopyAllow = true;
            IsDocNewCopyRequisiteAllow = true;
            LeftMenuBar = MenuGenerator.DocWithRowsLeftBar(this);
            RightMenuBar = MenuGenerator.StandartDocWithDeleteRightBar(this);
            orderManager = new WarehouseManager(errManager);
            Document = orderManager.NewOrderOut();
            WindowName = "Расходный складской ордер (новый)";
            var prn = RightMenuBar.FirstOrDefault(_ => _.Name == "Print");
            prn?.SubMenu.Add(new MenuButtonInfo
            {
                Caption = "Ордер",
                Command = PrintOrderCommand
            });
        }

        public OrderOutWindowViewModel(StandartErrorManager errManager, decimal dc)
        {
            IsDocNewCopyAllow = true;
            IsDocNewCopyRequisiteAllow = true;
            LeftMenuBar = MenuGenerator.DocWithRowsLeftBar(this);
            RightMenuBar = MenuGenerator.StandartDocWithDeleteRightBar(this);
            orderManager = new WarehouseManager(errManager);
            RefreshData(dc);
            WindowName = Document.ToString();
            var prn = RightMenuBar.FirstOrDefault(_ => _.Name == "Print");
            prn?.SubMenu.Add(new MenuButtonInfo
            {
                Caption = "Ордер",
                Command = PrintOrderCommand
            });
            
        }

        public override string LayoutName => "OrderOutWindowViewModel";

        public WarehouseOrderOut Document
        {
            set
            {
                if (Equals(myDocument, value)) return;
                myDocument = value;
                Rows.Clear();
                if (myDocument != null)
                    foreach (var r in Document.Rows)
                        Rows.Add(r);

                RaisePropertyChanged();
                RaisePropertyChanged(nameof(Rows));
            }
            get => myDocument;
        }

        public WarehouseOrderOutRow CurrentRow
        {
            set
            {
                if (Equals(myCurrentRow, value)) return;
                myCurrentRow = value;
                RaisePropertyChanged();
            }
            get => myCurrentRow;
        }

        public ObservableCollection<WarehouseOrderOutRow> Rows { set; get; } =
            new ObservableCollection<WarehouseOrderOutRow>();

        #region Справочники

        public List<Kontragent> Kontragents => GlobalOptions.ReferencesCache.GetKontragentsAll()
            .Cast<Kontragent>().ToList();

        public List<KursDomain.References.Warehouse> StoreDictionary =>
            GlobalOptions.ReferencesCache.GetWarehousesAll().Cast<KursDomain.References.Warehouse>()
                .OrderBy(_ => _.Name).ToList();

        #endregion

        #region Commands

        public ICommand PrintOrderCommand
        {
            get { return new Command(PrintOrder, _ => State != RowStatus.NewRow); }
        }

        private void PrintOrder(object obj)
        {
            ReportManager.WarehouseOrderOutReport(Document.DocCode);
        }

        public override bool IsDocDeleteAllow => Document != null && Document.State != RowStatus.NewRow;
        public override bool IsCanRefresh => Document != null && Document.State != RowStatus.NotEdited;

        public override bool IsCanSaveData => Document != null && (Document.State != RowStatus.NotEdited
                                                                   || Document.Rows.Any(_ =>
                                                                       _.State != RowStatus.NotEdited)
                                                                   || Document.DeletedRows.Count > 0)
                                                               && Document.State != RowStatus.NotEdited &&
                                                               Document.Rows.All(_ => _.MaxQuantity > 0)
                                                               && Document.WarehouseOut != null &&
                                                               Document.WarehouseIn != null;

        public override RowStatus State => Document?.State ?? RowStatus.NewRow;

        public override void DocNewEmpty(object form)
        {
            var frm = new OrderOutView { Owner = Application.Current.MainWindow };
            var ctx = new OrderOutWindowViewModel(new StandartErrorManager(GlobalOptions.GetEntities(),
                "WarehouseOrderOut", true))
            {
                Form = frm,
                State = RowStatus.NewRow
            };
            frm.Show();
            frm.DataContext = ctx;
        }

        public override void DocNewCopy(object form)
        {
            if (Document == null) return;
            var frm = new OrderOutView { Owner = Application.Current.MainWindow };
            var ctx = new OrderOutWindowViewModel(new StandartErrorManager(GlobalOptions.GetEntities(),
                "WarehouseOrderOut", true))
            {
                Form = frm,
                Document = orderManager.NewOrderOutCopy(Document.DocCode)
            };
            //ctx.Document.UpdateMaxQuantity();
            frm.Show();
            frm.DataContext = ctx;
        }

        public override void DocNewCopyRequisite(object form)
        {
            if (Document == null) return;
            var frm = new OrderOutView { Owner = Application.Current.MainWindow };
            var ctx = new OrderOutWindowViewModel(new StandartErrorManager(GlobalOptions.GetEntities(),
                "WarehouseOrderOut", true))
            {
                Form = frm,
                Document = orderManager.NewOrderOutRecuisite(Document),
                State = RowStatus.NewRow
            };

            frm.Show();
            frm.DataContext = ctx;
        }

        public override void RefreshData(object obj)
        {
            if (IsCanSaveData)
            {
                var service = this.GetService<IDialogService>("WinUIDialogService");
                dialogServiceText = "В документ внесены изменения, сохранить?";
                if (service.ShowDialog(MessageButton.YesNoCancel, "Запрос", this) == MessageResult.Yes)
                {
                    SaveData(null);
                    return;
                }
            }

            WarehouseOrderOut tempDoc = null;
            if (Document != null && Document.DocCode > 0)
            {
                tempDoc = orderManager.GetOrderOut(Document.DocCode);

            }
            else
            {
                var dc = obj as decimal? ?? 0;
                if (dc != 0)
                {
                    Document = orderManager.GetOrderOut(dc);
                    Document.UpdateMaxQuantity(Document.Date);
                }
            }

            if (tempDoc != null)
            {
                Document.Date = tempDoc.Date;
                Document.Note = tempDoc.Note;
                Document.WarehouseOut = tempDoc.WarehouseOut;
                Document.WarehouseIn = tempDoc.WarehouseIn;
                Document.KontragentSender = tempDoc.KontragentSender;
                Document.WarehouseSenderType = Document.KontragentSender != null
                    ? WarehouseSenderType.Kontragent
                    : WarehouseSenderType.Store;
                Document.DD_OT_KOGO_POLUCHENO = tempDoc.DD_OT_KOGO_POLUCHENO;
                foreach (var r in tempDoc.Rows)
                {
                    var oldRow = Rows.FirstOrDefault(_ => _.DocCode == r.DocCode && _.Code == r.Code);
                    if (oldRow == null) continue;
                    var q = nomenklManager.GetNomenklQuantity(Document.WarehouseOut.DocCode, r.DDT_NOMENKL_DC,
                        Document.Date, Document.Date);
                    var quan = q.Count == 0 ? 0 : q.First().OstatokQuantity;
                    oldRow.MaxQuantity = quan + oldRow.DDT_KOL_RASHOD;
                    oldRow.myState = RowStatus.NotEdited;
                    oldRow.Quantity = r.Quantity;
                    oldRow.Note = r.Note;
                }

                Document.myState = RowStatus.NotEdited;
                LastDocumentManager.SaveLastOpenInfo(DocumentType.StoreOrderOut, null, Document.DocCode,
                    Document.CREATOR, GlobalOptions.UserInfo.NickName, Document.Description);
            }

            Document.UpdateMaxQuantity(Document.Date);
            Document.myState = RowStatus.NotEdited;
        }

        public override void SaveData(object data)
        {
            var dc = orderManager.SaveOrderOut(Document);
            LastDocumentManager.SaveLastOpenInfo(DocumentType.StoreOrderOut, null, Document.DocCode,
                Document.CREATOR, GlobalOptions.UserInfo.NickName, Document.Description);
            //if (dc > 0) RefreshData(dc);
        }

        public ICommand AddNomenklCommand
        {
            get { return new Command(AddNomenkl, _ => Document != null && Document.WarehouseOut != null); }
        }

        private void AddNomenkl(object obj)
        {
            var nomenkls = StandartDialogs.SelectNomenkls(null, true);
            if (nomenkls == null || nomenkls.Count <= 0) return;
            foreach (var n in nomenkls)
                if (Document.Rows.All(_ => _.Nomenkl.DocCode != n.DocCode && !n.IsUsluga))
                {
                    var q = nomenklManager.GetNomenklQuantity(Document.WarehouseOut.DocCode, n.DocCode,
                        Document.Date, Document.Date);
                    var m = q.Count == 0 ? 0 : q.First().OstatokQuantity;
                    if (m <= 0)
                    {
                        winManager.ShowWinUIMessageBox($"Остатки номенклатуры {n.NomenklNumber} {n.Name} на складе " +
                                                       $"{GlobalOptions.ReferencesCache.GetWarehouse(Document.WarehouseOut.DocCode)}" +
                                                       $"кол-во {m}. Операция по номенклатуре не может быть проведена.",
                            "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                        continue;
                    }

                    Document.Rows.Add(new WarehouseOrderOutRow
                    {
                        DocCode = -1,
                        Nomenkl = n,
                        DDT_KOL_RASHOD = Math.Min(1, m),
                        Unit = n.Unit as Unit,
                        Currency = n.Currency as Currency,
                        MaxQuantity = m,
                        State = RowStatus.NewRow
                    });
                }
        }

        public ICommand DeleteNomenklCommand
        {
            get { return new Command(DeleteNomenkl, _ => Document.SelectedRows.Count > 0); }
        }

        private void DeleteNomenkl(object obj)
        {
            var delList = new List<WarehouseOrderOutRow>(Document.SelectedRows.ToList());
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
                    orderManager.DeleteOrderOut(Document);
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
