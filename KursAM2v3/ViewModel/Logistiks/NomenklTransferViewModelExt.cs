using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Transactions;
using Core;
using Core.EntityViewModel;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;

namespace KursAM2.ViewModel.Logistiks
{
    public class NomenklTransferViewModelExt : NomenklTransferViewModel
    {
        private string mySchetFactura;
        private SD_26 mySchetFacturaBase;
        private Core.EntityViewModel.Warehouse myWarehouse;

        public NomenklTransferViewModelExt(NomenklTransfer entity) : base(entity)
        {
            if (Entity.NomenklTransferRow?.Count > 0)
            {
                foreach (var r in Entity.NomenklTransferRow)
                {
                    var n = new NomenklTransferRowViewModelExt(r)
                    {
                        IsCalcConrol = false,
                        MaxQuantity = GetMaxQuantity(Entity.SkladDC, r.NomenklOutDC, Entity.Date),
                        PriceIn = r.PriceIn,
                        PriceOut = r.PriceOut,
                        Quantity = r.Quantity,
                        SummaOut = r.PriceOut * r.Quantity,
                        SummaIn = r.PriceIn * r.Quantity,
                        OldPrice = Math.Round(Nomenkl.Price(r.NomenklOutDC, Entity.Date.AddDays(-1)), 2),
                        IsPriceAccepted = r.IsPriceAcepted ?? false,
                        NakladEdSumma = r.NakladEdSumma ?? 0,
                        NakladRate = r.NakladRate ?? 0,
                        NakladNewEdSumma = r.NakladNewEdSumma ?? 0,
                        State = RowStatus.NotEdited
                    };
                    Rows.Add(n);
                }

                foreach (var r in Rows)
                    r.IsCalcConrol = true;
            }

            if (entity.SD_27 != null)
                myWarehouse = new Core.EntityViewModel.Warehouse(entity.SD_27);
        }

        private NomenklTransferViewModelExt()
        {
        }

        public ObservableCollection<NomenklTransferRowViewModelExt> Rows { set; get; } =
            new ObservableCollection<NomenklTransferRowViewModelExt>();

        public Core.EntityViewModel.Warehouse Warehouse
        {
            get => myWarehouse;
            set
            {
                if (myWarehouse != null && myWarehouse.Equals(value)) return;
                myWarehouse = value;
                Entity.SkladDC = myWarehouse?.DocCode ?? decimal.MinusOne;
                RaisePropertyChanged();
            }
        }

        public SD_26 SchetFacturaBase
        {
            get => mySchetFacturaBase;
            set
            {
                if (mySchetFacturaBase == value) return;
                mySchetFacturaBase = value;
                RaisePropertyChanged();
            }
        }

        public string SchetFactura
        {
            get => mySchetFactura;
            set
            {
                if (mySchetFactura == value) return;
                mySchetFactura = value;
                RaisePropertyChanged();
            }
        }

        private decimal GetMaxQuantity(decimal skladDC, decimal nomDC, DateTime date)
        {
            var q = Nomenkl.Quantity(skladDC, nomDC, date);
            return q < 0 ? 0 : q;
        }

        private decimal GetMaxQuantity(decimal? skladDC, decimal nomDC, DateTime date)
        {
            var q = Nomenkl.Quantity(skladDC, nomDC, date);
            return q < 0 ? 0 : q;
        }

        public static NomenklTransferViewModelExt Load(Guid id, DbContext dbContext)
        {
            try
            {
                var ctx = dbContext as ALFAMEDIAEntities;
                var d =
                    ctx?.NomenklTransfer
                        .Include(_ => _.NomenklTransferRow)
                        .Include(_ => _.SD_27)
                        .AsNoTracking()
                        .SingleOrDefault(_ => _.Id == id);
                return d == null
                    ? null
                    : new NomenklTransferViewModelExt(d)
                    {
                        State = RowStatus.NotEdited
                    };
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(null, ex);
                return null;
            }
        }

        public static NomenklTransferViewModelExt New()
        {
            return new NomenklTransferViewModelExt
            {
                Id = Guid.NewGuid(),
                Date = DateTime.Today,
                Creator = GlobalOptions.UserInfo.NickName,
                Rows = new ObservableCollection<NomenklTransferRowViewModelExt>(),
                State = RowStatus.NewRow
            };
        }

        public NomenklTransferViewModelExt CopyRequisite()
        {
            return new NomenklTransferViewModelExt
            {
                Id = Guid.NewGuid(),
                Date = Date,
                Creator = GlobalOptions.UserInfo.NickName,
                Rows = new ObservableCollection<NomenklTransferRowViewModelExt>(),
                State = RowStatus.NewRow
            };
        }

        public NomenklTransferViewModelExt Copy()
        {
            var ret = CopyRequisite();
            if (Rows.Count <= 0) return ret;
            foreach (var r in Rows)
                ret.Rows.Add(r.Copy());
            return ret;
        }

        public override void Delete()
        {
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    using (var tn = new TransactionScope())
                    {
                        var old = ctx.NomenklTransfer
                            .Include(_ => _.NomenklTransferRow)
                            .FirstOrDefault(_ => _.Id == Id);
                        if (old == null) return;
                        foreach (var r in old.NomenklTransferRow.ToList())
                            ctx.NomenklTransferRow.Remove(r);
                        ctx.NomenklTransfer.Remove(old);
                        ctx.SaveChanges();
                        tn.Complete();
                    }
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(null, ex);
            }
        }

        public override void Save()
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                using (var tn = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        switch (State)
                        {
                            case RowStatus.Edited:
                                var old = ctx.NomenklTransfer
                                    .Include(_ => _.NomenklTransferRow)
                                    .FirstOrDefault(_ => _.Id == Id);
                                if (old == null) return;
                                old.Date = Date;
                                old.Note = Note;
                                old.SkladDC = Warehouse.DocCode;
                                old.Creator = GlobalOptions.UserInfo.NickName;
                                old.LastUpdate = DateTime.Now;
                                old.LastUpdater = GlobalOptions.UserInfo.NickName;
                                foreach (var nr in Rows)
                                {
                                    var or = old.NomenklTransferRow.SingleOrDefault(_ => _.Id == nr.Id);
                                    if (or == null)
                                    {
                                        old.NomenklTransferRow.Add(new NomenklTransferRow
                                        {
                                            Id = nr.Id,
                                            DocId = Id,
                                            IsAccepted = nr.IsAccepted,
                                            LastUpdate = nr.LastUpdate,
                                            LastUpdater = nr.LastUpdater,
                                            NomenklInDC = nr.NomenklInDC,
                                            NomenklOutDC = nr.NomenklOutDC,
                                            PriceIn = nr.PriceIn,
                                            PriceOut = nr.PriceOut,
                                            Note = nr.Note,
                                            Quantity = nr.Quantity,
                                            Rate = nr.Rate,
                                            IsPriceAcepted = nr.IsPriceAccepted,
                                            NakladEdSumma = nr.NakladEdSumma,
                                            NakladRate = nr.NakladRate,
                                            NakladNewEdSumma = nr.NakladNewEdSumma
                                        });
                                    }
                                    else
                                    {
                                        or.IsAccepted = nr.IsAccepted;
                                        or.LastUpdate = nr.LastUpdate;
                                        or.LastUpdater = nr.LastUpdater;
                                        or.NomenklInDC = nr.NomenklInDC;
                                        or.NomenklOutDC = nr.NomenklOutDC;
                                        or.Note = nr.Note;
                                        or.Quantity = nr.Quantity;
                                        or.Rate = nr.Rate;
                                        or.PriceIn = nr.PriceIn;
                                        or.PriceOut = nr.PriceOut;
                                        or.IsPriceAcepted = nr.IsPriceAccepted;
                                        or.NakladEdSumma = nr.NakladEdSumma;
                                        or.NakladRate = nr.NakladRate;
                                        or.NakladNewEdSumma = nr.NakladNewEdSumma;
                                    }
                                }

                                var removeIds =
                                    old.NomenklTransferRow.Where(r => Rows.All(_ => _.Id != r.Id)).ToList().ToList();
                                foreach (var r in removeIds)
                                    ctx.Entry(r).State = EntityState.Deleted;
                                break;
                            case RowStatus.NewRow:
                                var newItem = new NomenklTransfer
                                {
                                    Id = Id,
                                    Date = Date,
                                    Note = Note,
                                    SkladDC = Warehouse.DocCode,
                                    Creator = GlobalOptions.UserInfo.NickName,
                                    LastUpdate = DateTime.Now,
                                    LastUpdater = GlobalOptions.UserInfo.NickName,
                                    NomenklTransferRow = new List<NomenklTransferRow>()
                                };
                                foreach (var nr in Rows)
                                {
                                    var or = new NomenklTransferRow
                                    {
                                        Id = nr.Id,
                                        DocId = Id,
                                        IsAccepted = nr.IsAccepted,
                                        LastUpdate = nr.LastUpdate,
                                        LastUpdater = nr.LastUpdater,
                                        NomenklInDC = nr.NomenklInDC,
                                        NomenklOutDC = nr.NomenklOutDC,
                                        Note = nr.Note,
                                        Quantity = nr.Quantity,
                                        Rate = nr.Rate,
                                        PriceIn = nr.PriceIn,
                                        PriceOut = nr.PriceOut,
                                        IsPriceAcepted = nr.IsPriceAccepted,
                                        NakladEdSumma = nr.NakladEdSumma,
                                        NakladRate = nr.NakladRate,
                                        NakladNewEdSumma = nr.NakladNewEdSumma
                                    };
                                    newItem.NomenklTransferRow.Add(or);
                                }

                                ctx.NomenklTransfer.Add(newItem);
                                break;
                        }

                        ctx.SaveChanges();
                        tn.Commit();
                        myState = RowStatus.NotEdited;
                        foreach (var r in Rows)
                            r.myState = RowStatus.NotEdited;
                    }
                    catch (Exception ex)
                    {
                        tn.Rollback();
                        WindowManager.ShowError(ex);
                    }
                }
            }
        }
    }
}