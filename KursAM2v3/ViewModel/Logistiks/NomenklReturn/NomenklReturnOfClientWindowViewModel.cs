﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.LayoutControl;
using KursAM2.Dialogs;
using KursAM2.Repositories.InvoicesRepositories;
using KursAM2.Repositories.NomenklReturn;
using KursAM2.Repositories.WarehousesRepository;
using KursAM2.View.Logistiks.NomenklReturn;
using KursDomain;
using KursDomain.Documents.NomenklReturn;
using KursDomain.ICommon;
using KursDomain.Menu;
using KursDomain.References;
using StackExchange.Redis;

namespace KursAM2.ViewModel.Logistiks.NomenklReturn
{
    public class NomenklReturnOfClientWindowViewModel : RSWindowViewModelBase, IDataErrorInfo
    {
        #region Constructors

        public NomenklReturnOfClientWindowViewModel(Guid? id)
        {
            myRepository = new NomenklReturnOfClientRepository(myContext);
            redis = ConnectionMultiplexer.Connect(ConfigurationManager.AppSettings["redis.connection"]);
            mySubscriber = redis.GetSubscriber();
            LeftMenuBar = GlobalOptions.UserInfo.IsAdmin
                ? MenuGenerator.DocWithCustomizeFormDocumentLeftBar(this)
                : MenuGenerator.DocWithRowsLeftBar(this);
            RightMenuBar = MenuGenerator.StandartDocWithDeleteRightBar(this);
            if (id is null)
            {
                Document = myRepository.CrateNewEmpty() as NomenklReturnOfClientViewModel;
                // ReSharper disable once PossibleNullReferenceException
                Document.State = RowStatus.NewRow;
            }
            else
            {
                Document = myRepository.Get(id.Value) as NomenklReturnOfClientViewModel;
                LoadRefernces();
                setUnchangedStatus();
            }
        }

        #endregion

        #region Methods

        public void LoadRefernces()
        {
            var prices = myRepository.GetNomenklLastPrices(Document.Rows.Select(_ => _.NomenklDC).ToList(),
                Document.DocDate);
            foreach (var row in Document.Rows)
            {
                row.CalcCost = prices.FirstOrDefault(_ => _.Key == row.NomenklDC).Value;
                if (row.Entity.RashodNakladId != null)
                {
                    var s = myWaybillRepository.GetInfoByRowId(row.Entity.RashodNakladId.Value); 
                    row.WaybillText = s.IndexOf('№') > 0 ? s.Remove(0, s.IndexOf('№')) : s;
                }
                if (row.InvoiceRowId is not null)
                {
                    var s1 = myInvoiceClientRepository.GetInfoByRowId(row.InvoiceRowId.Value);
                    row.InvoiceText = s1.IndexOf('№') > 0 ? s1.Remove(0, s1.IndexOf('№')) : s1;
                }   
            }
        }

        private void setUnchangedStatus()
        {
            foreach(var r in Document.Rows)
                r.myState = RowStatus.NotEdited;
            Document.State = RowStatus.NotEdited;
        }

        #endregion

        #region Fields

        private readonly ALFAMEDIAEntities myContext = GlobalOptions.GetEntities();

        private readonly INomenklReturnOfClientRepository myRepository;

        private readonly IWaybillRepository myWaybillRepository = new WaybillRepository(GlobalOptions.GetEntities());

        private readonly IInvoiceClientRepository myInvoiceClientRepository =
            new InvoiceClientRepository(GlobalOptions.GetEntities());

        private NomenklReturnOfClientViewModel myDocument;

        private readonly ConnectionMultiplexer redis;
        private readonly ISubscriber mySubscriber;

        private readonly Guid HeadViewLayoutId = Guid.Parse("{2D67F49B-AF6B-4827-891F-0A732EF3E5FF}");
        private string myHeadViewLayoutBase;
        private NomenklReturnOfClientRowViewModel myCurrentRow;

        #endregion

        #region Properties

        public override bool IsCanSaveData => Document != null && Document.State != RowStatus.NotEdited; 

        public override string WindowName => "Возврат товара от клиента";
        public override string LayoutName => "NomenklReturnOfClientWindow";

        public readonly List<NomenklReturnOfClientRowViewModel> DeletedRows = new List<NomenklReturnOfClientRowViewModel>();

        public NomenklReturnOfClientViewModel Document
        {
            get => myDocument;
            set
            {
                if (Equals(myDocument, value)) return;
                myDocument = value;
                RaisePropertyChanged();
            }
        }

        public NomenklReturnOfClientRowViewModel CurrentRow
        {
            get => myCurrentRow;
            set
            {
                if (Equals(value, myCurrentRow)) return;
                myCurrentRow = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region ErrirInfo

        public string this[string columnName] => throw new NotImplementedException();

        public string Error { get; }

        #endregion

        #region Command

        public ICommand KontragentSelectCommad
        {
            get { return new Command(KontragentSelect, _ => Document.Rows.Count == 0); }
        }

        private void KontragentSelect(object obj)
        {
            var kontr = StandartDialogs.SelectKontragent();
            if (kontr == null) return;
            Document.Kontragent = kontr;
        }

        public ICommand WarehouseSelectCommand
        {
            get { return new Command(WarehouseSelect, _ => true); }
        }

        private void WarehouseSelect(object obj)
        {
            var warehouse = StandartDialogs.SelectWarehouseDialog(new List<KursDomain.References.Warehouse>(
                GlobalOptions.ReferencesCache.GetWarehousesAll().Cast<KursDomain.References.Warehouse>()
                    .Where(_ => _.IsDeleted)));
            if (warehouse == null) return;
            Document.Warehouse = warehouse;
        }

        public ICommand InvoiceSelectCommad
        {
            get { return new Command(InvoiceSelect, _ => Document.Kontragent is not null); }
        }

        private void InvoiceSelect(object obj)
        {
        }

        public override void UpdateVisualObjects()
        {
            base.UpdateVisualObjects();
            if (Form is not NomenklReturnOfClientView frm) return;
            using (var ctx = GlobalOptions.KursSystem())
            {
                var layoutHead = ctx.FormLayout.FirstOrDefault(_ => _.Id == HeadViewLayoutId);
                if (layoutHead is null) return;
                var stream = new MemoryStream();
                var writer = new StreamWriter(stream);
                writer.Write(layoutHead.Layout);
                writer.Flush();
                stream.Position = 0;
                using (var reader = XmlReader.Create(stream))
                {
                    frm.DocumentHead.ReadFromXML(reader);
                }
            }
        }

        protected override void SaveCustomizedFormDocument(object obj)
        {
            if (Form is NomenklReturnOfClientView frm)
                using (var ctx = GlobalOptions.KursSystem())
                {
                    foreach (var o in LogicalTreeHelper.GetChildren(frm.DocumentHead))
                        if (o is LayoutGroup item)
                            if (item.Header is not null && ((string)item.Header).ToLower() == "group")
                                item.View = LayoutGroupView.Group;

                    string layoutXML;
                    var stream = new MemoryStream();

                    using (var writer = XmlWriter.Create(stream))
                    {
                        frm.DocumentHead.WriteToXML(writer);
                        writer.Close();
                        stream.Seek(0, SeekOrigin.Begin);
                        using (var reader = new StreamReader(stream, Encoding.UTF8))
                        {
                            layoutXML = reader.ReadToEnd();
                        }
                    }

                    stream.Flush();


                    var old = ctx.FormLayout.FirstOrDefault(_ => _.Id == HeadViewLayoutId);
                    if (old != null)
                    {
                        old.Layout = layoutXML;
                        old.UserId = GlobalOptions.UserInfo.KursId;
                        old.UpdateDate = DateTime.Now;
                    }
                    else
                    {
                        ctx.FormLayout.Add(new FormLayout
                        {
                            Id = HeadViewLayoutId,
                            ControlName = $"{LayoutName}.DocumentHead",
                            FormName = $"{LayoutName}.DocumentHead",
                            Layout = layoutXML,
                            UserId = GlobalOptions.UserInfo.KursId,
                            UpdateDate = DateTime.Now,
                            WindowState = string.Empty
                        });
                    }

                    ctx.SaveChanges();
                }

            base.SaveCustomizedFormDocument(obj);
        }

        public override void ResetLayout(object form)
        {
            base.ResetLayout(form);
            if (Form is NomenklReturnOfClientView frm)
            {
                var stream = new MemoryStream();
                var writer = new StreamWriter(stream);
                writer.Write(myHeadViewLayoutBase);
                writer.Flush();
                stream.Position = 0;
                using (var reader = XmlReader.Create(stream))
                {
                    frm.DocumentHead.ReadFromXML(reader);
                }
            }
        }

        protected override void OnLayoutInitial(object obj)
        {
            base.OnLayoutInitial(obj);
            if (Form is NomenklReturnOfClientView frm)
            {
                var stream = new MemoryStream();

                using (var writer = XmlWriter.Create(stream))
                {
                    frm.DocumentHead.WriteToXML(writer);
                    writer.Close();
                    stream.Seek(0, SeekOrigin.Begin);
                    using (var reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        myHeadViewLayoutBase = reader.ReadToEnd();
                    }
                }

                stream.Flush();
            }
        }

        public ICommand AddNomenklCommand
        {
            get
            {
                return new Command(AddNomenkl, _ => Document?.Kontragent is not null
                                                    && Document?.Warehouse is not null);
            }
        }

        private void AddNomenkl(object obj)
        {
            var ctx = new NomenklReturnSelectWaybillRowsDialog(Document.Kontragent);
            var service = this.GetService<IDialogService>("DialogServiceUI");
            if (service.ShowDialog(MessageButton.OKCancel, "Запрос", ctx) == MessageResult.Cancel) return;
            var prices = myRepository.GetNomenklLastPrices(ctx.SelectedWaybillRows.Select(_ => _.NomenklDC).ToList(),
                Document.DocDate);
            foreach (var item in ctx.SelectedWaybillRows)
            {
                var newRow = new NomenklReturnOfClientRowViewModel(new NomenklReturnOfClientRow
                {
                    Id = Guid.NewGuid(),
                    Price = item.Price,
                    Quantity = item.Quantity,
                    DocId = Document.Id,
                    InvoiceRowId = item.InvoiceRowId,
                    RashodNakladId = item.Id
                }, Document)
                {
                    State = RowStatus.NewRow,
                    Nomenkl = GlobalOptions.ReferencesCache.GetNomenkl(item.NomenklDC) as Nomenkl,
                    CalcCost = prices.FirstOrDefault(_ => _.Key == item.NomenklDC).Value
                };
                Document.Entity.NomenklReturnOfClientRow.Add(newRow.Entity);
                newRow.Cost = newRow.CalcCost;
                var s = myWaybillRepository.GetInfoByRowId(item.Id);
                newRow.WaybillText = s.IndexOf('№') > 0 ? s.Remove(0, s.IndexOf('№')) : s;
                if (newRow.InvoiceRowId is not null)
                {
                    var s1 = myInvoiceClientRepository.GetInfoByRowId(newRow.InvoiceRowId.Value);
                    newRow.InvoiceText = s1.IndexOf('№') > 0 ? s1.Remove(0, s1.IndexOf('№')) : s1;
                }

                Document.Rows.Add(newRow);
            }
        }

        public ICommand DeleteRowCommand
        {
            get { return new Command(DeleteRow, _ => CurrentRow is not null); }
        }

        private void DeleteRow(object obj)
        {  
            if(CurrentRow.State != RowStatus.NewRow)
                DeletedRows.Add(CurrentRow);
            Document.Rows.Remove(CurrentRow);
            //Document.Entity.NomenklReturnOfClientRow.Remove(CurrentRow.Entity);
            Document.State = RowStatus.Edited;
        }

        public override void SaveData(object data)
        {
            try
            {
                if (State == RowStatus.NewRow || Document.DocNum < 0)
                {
                    var n = myRepository.GetNewNumber();
                    Document.DocNum = n <= 0 ? 1 : n;
                }

                myRepository.BeginTransaction();
                myRepository.AddOrUpdate(Document, DeletedRows.Select(_ => _.Id).ToList());
                myRepository.CommitTransaction();
                DeletedRows.Clear();
                setUnchangedStatus();
            }
            catch (Exception ex)
            {
                myRepository.RollbackTransaction();
                WindowManager.ShowError(ex);
            }
        }

        public override void RefreshData(object obj)
        {
            foreach (var dRow in DeletedRows)
            {
                Document.Rows.Add(dRow);
            }
            myRepository.UndoChanges();
            Document.RaisePropertyAllChanged();
            foreach (var row in Document.Rows)
            {
                row.RaisePropertyAllChanged();
            }
            DeletedRows.Clear();
            setUnchangedStatus();
        }

        #endregion
    }
}
