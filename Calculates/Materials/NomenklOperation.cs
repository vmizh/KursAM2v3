using System;
using System.Collections.Generic;
using Core;
using Core.EntityViewModel;

namespace Calculates.Materials
{
    public class NomenklOperation
    {
        public Guid Id { set; get; }
        public Guid OperId { get; set; }
        public Guid? LinkId { set; get; }
        public NomenklMoveOperationType OperationType { set; get; }
        public DateTime Date { set; get; }
        public Nomenkl Nomenkl { set; get; }
        public decimal Quantity { set; get; }
        public Currency Currency { set; get; }
        public Warehouse Warehouse { set; get; }
        public decimal? Price { set; get; }

        public static List<NomenklOperation> Generate(LoaderOperation oper)
        {
            if (oper.Expense == 0 && oper.Receipt == 0) return null;
            var ret = new List<NomenklOperation>();
            try
            {
                switch (Convert.ToInt64(oper.OperType))
                {
                    case 2010000014:
                        var id = Guid.NewGuid();
                        ret.Add(new NomenklOperation
                        {
                            Id = id,
                            OperId = oper.Id,
                            LinkId = null,
                            OperationType = NomenklMoveOperationType.ExpenseInnerMove,
                            Date = oper.Date,
                            Quantity = oper.Expense,
                            Currency = MainReferences.GetNomenkl(oper.NomenklDC)?.Currency,
                            Nomenkl = MainReferences.GetNomenkl(oper.NomenklDC),
                            Warehouse = MainReferences.Warehouses[oper.SkladOtprDC.Value],
                            Price = null
                        });
                        ret.Add(new NomenklOperation
                        {
                            Id = id,
                            OperId = oper.Id,
                            LinkId = id,
                            OperationType = NomenklMoveOperationType.ReceiptInnerMove,
                            Date = oper.Date,
                            Quantity = oper.Receipt,
                            Currency = MainReferences.GetNomenkl(oper.NomenklDC)?.Currency,
                            Nomenkl = MainReferences.GetNomenkl(oper.NomenklDC),
                            Warehouse = MainReferences.Warehouses[oper.SkladPolDC.Value],
                            Price = null
                        });
                        break;
                    case 2010000001:
                        ret.Add(new NomenklOperation
                        {
                            Id = Guid.NewGuid(),
                            OperId = oper.Id,
                            LinkId = null,
                            OperationType =
                                MainReferences.GetKontragent(oper.KontrOtpravDC)?.IsBalans == true
                                    ? NomenklMoveOperationType.ReceiptKontragent
                                    : NomenklMoveOperationType.ReceiptOutBalansKontragent,
                            Date = oper.Date,
                            Quantity = oper.Receipt,
                            Currency = MainReferences.GetKontragent(oper.KontrOtpravDC)?.BalansCurrency,
                            Nomenkl = MainReferences.GetNomenkl(oper.NomenklDC),
                            Warehouse = MainReferences.Warehouses[oper.SkladPolDC.Value],
                            Price = oper.SummaIn / oper.Receipt
                        });
                        break;
                    case 2010000005:
                        ret.Add(new NomenklOperation
                        {
                            Id = Guid.NewGuid(),
                            OperId = oper.Id,
                            LinkId = null,
                            OperationType =
                                oper.Receipt > 0
                                    ? NomenklMoveOperationType.ReceiptInventory
                                    : NomenklMoveOperationType.ExpenseInventory,
                            Date = oper.Date,
                            Quantity = oper.Receipt > 0 ? oper.Receipt : oper.Expense,
                            Currency = MainReferences.GetNomenkl(oper.NomenklDC)?.Currency,
                            Nomenkl = MainReferences.GetNomenkl(oper.NomenklDC),
                            Warehouse = MainReferences.Warehouses[oper.SkladPolDC.Value],
                            Price = null
                        });
                        break;
                    case 2010000008:
                        ret.Add(new NomenklOperation
                        {
                            Id = Guid.NewGuid(),
                            OperId = oper.Id,
                            LinkId = null,
                            OperationType =
                                oper.Receipt > 0
                                    ? NomenklMoveOperationType.ReceiptAssemblyProduct
                                    : NomenklMoveOperationType.ExpenseAssemblyProduct,
                            Date = oper.Date,
                            Quantity = oper.Receipt > 0 ? oper.Receipt : oper.Expense,
                            Currency = MainReferences.GetNomenkl(oper.NomenklDC)?.Currency,
                            Nomenkl = MainReferences.GetNomenkl(oper.NomenklDC),
                            Warehouse = MainReferences.Warehouses[oper.SkladPolDC.Value],
                            Price = null
                        });
                        break;
                    case 2010000009:
                        ret.Add(new NomenklOperation
                        {
                            Id = Guid.NewGuid(),
                            OperId = oper.Id,
                            LinkId = null,
                            OperationType =
                                oper.Receipt > 0
                                    ? NomenklMoveOperationType.ReceiptDisassemblyProduct
                                    : NomenklMoveOperationType.ExpenseDisassemblyProduct,
                            Date = oper.Date,
                            Quantity = oper.Receipt > 0 ? oper.Receipt : oper.Expense,
                            Currency = MainReferences.GetNomenkl(oper.NomenklDC)?.Currency,
                            Nomenkl = MainReferences.GetNomenkl(oper.NomenklDC),
                            Warehouse = MainReferences.Warehouses[oper.SkladPolDC.Value],
                            Price = null
                        });
                        break;
                    case 2010000010:
                        ret.Add(new NomenklOperation
                        {
                            Id = Guid.NewGuid(),
                            OperId = oper.Id,
                            LinkId = null,
                            OperationType = NomenklMoveOperationType.ExpenseWriteOff,
                            Date = oper.Date,
                            Quantity = oper.Expense,
                            Currency = MainReferences.GetNomenkl(oper.NomenklDC)?.Currency,
                            Nomenkl = MainReferences.GetNomenkl(oper.NomenklDC),
                            Warehouse = MainReferences.Warehouses[oper.SkladOtprDC.Value],
                            Price = null
                        });
                        break;
                    case 2010000003:
                    case 2010000012:
                        ret.Add(new NomenklOperation
                        {
                            Id = Guid.NewGuid(),
                            OperId = oper.Id,
                            LinkId = null,
                            OperationType =
                                MainReferences.GetKontragent(oper.KontrPolDC).IsBalans
                                    ? NomenklMoveOperationType.ExpenseKontragent
                                    : NomenklMoveOperationType.ExpenseOutBalansKontragent,
                            Date = oper.Date,
                            Quantity = oper.Expense,
                            Currency = MainReferences.GetKontragent(oper.KontrPolDC)?.BalansCurrency,
                            Nomenkl = MainReferences.GetNomenkl(oper.NomenklDC),
                            Warehouse = MainReferences.Warehouses[oper.SkladOtprDC.Value],
                            Price = null
                        });
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return ret;
        }
    }
}