using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Calculates.Materials;
using Core;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using Helper;
using KursAM2.Managers;
using KursDomain.Documents.CommonReferences;
using KursDomain.Documents.NomenklManagement;
using KursDomain.Menu;
using KursDomain.References;

namespace KursAM2.ViewModel.Logistiks
{
    public class NomenklCostCalculatorWindowViewModel : RSWindowViewModelBase
    {
        private NomenklCalcCostOperation myCurrentOperation;
        private NomenklCost myNomenklCost;
        private Nomenkl mySelectedNomenkl;

        public NomenklCostCalculatorWindowViewModel(decimal nomDC)
        {
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.StandartInfoRightBar(this);
            if (nomDC != 0)
                SelectedNomenkl = MainReferences.GetNomenkl(nomDC);
            RefreshReferences();
        }

        public NomenklCostCalculatorWindowViewModel(Nomenkl nom)
        {
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.StandartInfoRightBar(this);
            SelectedNomenkl = nom;
            RefreshReferences();
        }

        public ObservableCollection<Nomenkl> Nomenkls { set; get; } = new ObservableCollection<Nomenkl>();

        public Nomenkl SelectedNomenkl
        {
            get => mySelectedNomenkl;
            set
            {
                if (mySelectedNomenkl != null && mySelectedNomenkl.Equals(value)) return;
                SkladOstatki.Clear();
                RaisePropertyChanged(nameof(SkladOstatki));
                mySelectedNomenkl = value;
                if (mySelectedNomenkl != null)
                    using (var ctx = GlobalOptions.GetEntities())
                    {
                        var clc = new NomenklCostMediumSliding(ctx);
                        myNomenklCost = new NomenklCost
                        {
                            Nomenkl = mySelectedNomenkl,
                            Operations = clc.GetOperations(mySelectedNomenkl.DocCode, false)
                        };
                    }

                RaisePropertyChanged();
                RaisePropertyChanged(nameof(Operations));
            }
        }

        public NomenklCost NomenklCost
        {
            get => myNomenklCost;
            set
            {
                if (myNomenklCost != null && myNomenklCost.Equals(value)) return;
                myNomenklCost = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<NomenklCalcCostOperation> Operations => NomenklCost?.Operations;

        public ICommand FinanceDocumentOpenCommand
        {
            get { return new Command(FinanceDocumentOpen, param => CurrentOperation.FinDocumentDC != null); }
        }

        public ICommand TovarDocumentOpenCommand
        {
            get { return new Command(TovarDocumentOpen, param => CurrentOperation.FinDocumentDC != null); }
        }

        public string FinanceDocumentName
        {
            get
            {
                if (CurrentOperation?.FinDocumentDC == null) return null;
                switch (CurrentOperation.OperCode)
                {
                    case 1:
                        return "Открыть счет/фактура поставщика";
                    case 2:
                        return "Открыть счет/фактура клиенту";
                    case 5:
                        return "Открыть инвентаризационную ведомость";
                    case 12:
                        return "Открыть счет/фактура клиенту";
                    case 18:
                        return "Открыть Продажа за наличный расчет";
                }

                return "Финансовый документ отсутствует";
            }
        }

        public string TovarDocumentName
        {
            get
            {
                if (CurrentOperation.TovarDocument == null) return null;
                switch (CurrentOperation.OperCode)
                {
                    case 1:
                        return "Открыть приходный складской ордер";
                    case 2:
                        return "Открыть расходный складской ордер";
                    case 5:
                        return "Открыть инвентаризационную ведомость";
                    case 12:
                        return "Открыть расходную накладную";
                    case 18:
                        return "Открыть Продажа за наличный расчет";
                }

                return "Товарный документ отсутствует";
            }
        }

        public NomenklCalcCostOperation CurrentOperation
        {
            get => myCurrentOperation;
            set
            {
                if (myCurrentOperation != null && myCurrentOperation == value) return;
                myCurrentOperation = value;
                if (myCurrentOperation != null)
                    CalcOstatki(myCurrentOperation);
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(SkladOstatki));
                RaisePropertyChanged(nameof(FinanceDocumentName));
                RaisePropertyChanged(nameof(TovarDocumentName));
            }
        }

        public ObservableCollection<NomenklOstatkiWithPrice> SkladOstatki { set; get; } =
            new ObservableCollection<NomenklOstatkiWithPrice>();

        private void TovarDocumentOpen(object obj)
        {
            switch (CurrentOperation.OperCode)
            {
                case 1:
                    DocumentsOpenManager.Open(DocumentType.StoreOrderIn, (decimal)CurrentOperation.TovarDocDC);
                    //return $"Открыть счет/фактура поставщика";
                    break;
                case 2:
                    DocumentsOpenManager.Open(DocumentType.Waybill, (decimal)CurrentOperation.TovarDocDC);
                    //return $"Открыть счет/фактура клиенту";
                    break;
                case 5:
                    DocumentsOpenManager.Open(DocumentType.InventoryList, (decimal)CurrentOperation.TovarDocDC);
                    //return $"Открыть инвентаризационную ведомость";
                    break;
                case 12:
                    DocumentsOpenManager.Open(DocumentType.Waybill, (decimal)CurrentOperation.TovarDocDC);
                    //return $"Открыть счет/фактура клиенту";
                    break;
                case 18:
                    WindowManager.ShowFunctionNotReleased();
                    //return $"Открыть Продажа за наличный расчет";
                    break;
            }
        }

        private void RefreshReferences()
        {
            if (SelectedNomenkl == null)
                foreach (var n in MainReferences.ALLNomenkls.Values.Where(_ => _.IsUsluga == false))
                    Nomenkls.Add(n);
            else
                Nomenkls.Add(SelectedNomenkl);
        }

        private void CalcOstatki(NomenklCalcCostOperation op)
        {
            SkladOstatki.Clear();
            using (var ctx = GlobalOptions.GetEntities())
            {
                var sql = "SELECT  NomDC ,Date ,StoreDC ,MAX(Start) Start ," +
                          "SUM(Prihod) Prihod ,SUM(Rashod) Rashod ,MAX(Nakopit) Nakopit ," +
                          "SUM(SummaIn) SummaIn ,SUM(SummaNakladIn) SummaNakladIn ,SUM(SummaOut) SummaOut ," +
                          "SUM(SummaNakladOut) SummaNakladOut ,MAX(Price) Price ," +
                          "MAX(PriceWithNaklad) PriceWithNaklad " +
                          "FROM dbo.NomenklMoveStore " +
                          $"WHERE nomdc = {CustomFormat.DecimalToSqlDecimal(op.NomenklDC)} " +
                          "GROUP BY nomdc,storedc,date " +
                          "ORDER BY nomdc, storedc, date";

                var data = ctx.Database.SqlQuery<NomenklMoveStore>(sql).ToList();
                var skladsDC = data.Select(_ => _.StoreDC).Distinct();
                foreach (var sdc in skladsDC)
                {
                    var d = data.Where(_ => _.StoreDC == sdc && _.Date <= op.DocDate).ToList();
                    if (d.Count == 0) continue;
                    var dt = d.Max(_ => _.Date);
                    var s = data.Single(_ => _.StoreDC == sdc && _.Date == dt);
                    if (s.Nakopit == 0) continue;
                    var newSklad = new NomenklOstatkiWithPrice
                    {
                        Quantity = (decimal)s.Nakopit,
                        PriceWONaklad = s.Price,
                        Price = s.PriceWithNaklad,
                        SummaWONaklad = (decimal)(s.Price * s.Nakopit),
                        Summa = (decimal)(s.Price * s.Nakopit)
                    };
                    SkladOstatki.Add(newSklad);
                }
            }
            //var skladsIn =
            //    Operations.Where(_ => _.DocDate <= op.DocDate)
            //        .Select(_ => _.SkladInName)
            //        .Where(_ => _ != null)
            //        .ToList();
            //var skladsOut =
            //    Operations.Where(_ => _.DocDate <= op.DocDate)
            //        .Select(_ => _.SkladOutName)
            //        .Where(_ => _ != null)
            //        .ToList();
            //var sklads = skladsIn.Concat(skladsOut).Distinct().ToList();

            //var prcRow = Operations.Where(_ => _.DocDate <= op.DocDate).Max(_ => _.RowNumber);
            //var prc = Operations.Single(_ => _.RowNumber == prcRow);
            //foreach (var s in sklads)
            //{
            //    var q =
            //        Operations.Where(_ => _.SkladInName == s && _.DocDate <= op.DocDate)
            //            .Sum(_ => _.QuantityIn) -
            //        Operations.Where(_ => _.SkladOutName == s && _.DocDate <= op.DocDate)
            //            .Sum(_ => _.QuantityOut);
            //    var newSklad = new NomenklOstatkiWithPrice
            //    {
            //        Quantity = q,
            //        StoreName = s,
            //        PriceWONaklad = prc.CalcPrice,
            //        Price = prc.CalcPriceNaklad,
            //        SummaWONaklad = prc.CalcPrice * q,
            //        Summa = prc.CalcPriceNaklad * q
            //    };
            //    if (newSklad.Quantity != 0)
            //        SkladOstatki.Add(newSklad);
            //}
            RaisePropertyChanged(nameof(SkladOstatki));
        }

        private void FinanceDocumentOpen(object obj)
        {
            switch (CurrentOperation.OperCode)
            {
                case 1:
                    DocumentsOpenManager.Open(DocumentType.InvoiceProvider, (decimal)CurrentOperation.FinDocumentDC);
                    //return $"Открыть счет/фактура поставщика";
                    break;
                case 2:
                    DocumentsOpenManager.Open(DocumentType.InvoiceClient, (decimal)CurrentOperation.FinDocumentDC);
                    //return $"Открыть счет/фактура клиенту";
                    break;
                case 5:
                    DocumentsOpenManager.Open(DocumentType.InventoryList, (decimal)CurrentOperation.TovarDocDC);
                    //return $"Открыть инвентаризационную ведомость";
                    break;
                case 12:
                    DocumentsOpenManager.Open(DocumentType.InvoiceClient, (decimal)CurrentOperation.FinDocumentDC);
                    //return $"Открыть счет/фактура клиенту";
                    break;
                case 18:
                    WindowManager.ShowFunctionNotReleased();
                    //return $"Открыть Продажа за наличный расчет";
                    break;
            }
        }
    }
}
