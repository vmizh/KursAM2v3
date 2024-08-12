using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using Core.ViewModel.Base;
using Core.WindowsManager;
using DevExpress.Xpf.Editors.Settings;
using DevExpress.Xpf.Grid;
using Helper;
using KursAM2.View.Logistiks.AktSpisaniya;
using KursAM2.ViewModel.Finance;
using KursDomain;
using KursDomain.Documents.NomenklManagement;
using KursDomain.ICommon;
using KursDomain.Managers;
using KursDomain.References;
using KursDomain.RepositoryHelper;

namespace KursAM2.ViewModel.Logistiks.AktSpisaniya
{
    public sealed class DialogSelectExistNomOnSkaldViewModel : RSWindowViewModelBase
    {
        private readonly Currency myCurrency;
        private readonly List<decimal> myExclude;
        private readonly NomenklManager2 nomenklManager = new NomenklManager2(GlobalOptions.GetEntities());
        private GridColumn otgruzColumn;

        #region Constructors

        public DialogSelectExistNomOnSkaldViewModel(KursDomain.References.Warehouse sklad,
            DateTime date, List<decimal> exclude = null, Currency currency = null)
        {
            warehouse = sklad;
            Date = date;
            myCurrency = currency;
            myExclude = exclude;
            RefreshData(null);
        }

        #endregion

        #region Fields

        private NomenklRemainsOnSkladWithPrice myCurrentNomenkl;
        private NomenklRemainsOnSkladWithPrice myCurrentSelectedNomenkl;
        private readonly KursDomain.References.Warehouse warehouse;
        private readonly DateTime Date;
        private GridColumn mySelectedColumn;

        #endregion

        #region Properties

        public override string LayoutName => "DialogSelectExistNomOnSkaldViewModel";

        public ObservableCollection<NomenklRemainsOnSkladWithPrice> NomenklList { set; get; } =
            new ObservableCollection<NomenklRemainsOnSkladWithPrice>();

        // ReSharper disable once CollectionNeverUpdated.Global
        public ObservableCollection<NomenklRemainsOnSkladWithPrice> NomenklSkladRows { set; get; } =
            new ObservableCollection<NomenklRemainsOnSkladWithPrice>();

        public UserControl CustomDataUserControl { set; get; } = new SelectExistNomenklOnSkladView();

        public NomenklRemainsOnSkladWithPrice CurrentNomenkl
        {
            get => myCurrentNomenkl;
            set
            {
                if (myCurrentNomenkl == value) return;
                myCurrentNomenkl = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<NomenklRemainsOnSkladWithPrice> NomenklSelectedList { set; get; } =
            new ObservableCollection<NomenklRemainsOnSkladWithPrice>();

        public ObservableCollection<NomenklRemainsOnSkladWithPrice> SelectedRows { set; get; } =
            new ObservableCollection<NomenklRemainsOnSkladWithPrice>();

        public GridColumn SelectedColumn
        {
            set
            {
                if (Equals(value, mySelectedColumn)) return;
                mySelectedColumn = value;
                RaisePropertyChanged();
            }
            get => mySelectedColumn;
        }

        public NomenklRemainsOnSkladWithPrice CurrentSelectedNomenkl
        {
            get => myCurrentSelectedNomenkl;
            set
            {
                if (myCurrentSelectedNomenkl == value) return;
                myCurrentSelectedNomenkl = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region Commands

        public ICommand RemoveNomenklFromSelectedCommand
        {
            get
            {
                return new Command(RemoveNomenklFromSelected,
                    _ => CurrentSelectedNomenkl != null);
            }
        }

        private void RemoveNomenklFromSelected(object obj)
        {
            //if (SelectedColumn?.FieldName != "FactOtgruz") return;
            var removeList = new List<decimal>();
            if (SelectedRows.Count > 0)
            {
                foreach (var item in SelectedRows)
                {
                    removeList.Add(item.Nomenkl.DocCode);
                    NomenklList.Add(item);
                }

                foreach (var nomDC in removeList)
                {
                    var r = NomenklSelectedList.FirstOrDefault(_ => _.Nomenkl.DocCode == nomDC);
                    if (r != null)
                        NomenklSelectedList.Remove(r);
                }
            }
        }


        public ICommand AddNomenklToSelectedCommand
        {
            get { return new Command(AddNomenklToSelected, _ => CurrentNomenkl != null || NomenklSkladRows.Count > 1); }
        }

        private void AddNomenklToSelected(object obj)
        {
            List<decimal> removeDCs = new List<decimal>();
            if (NomenklSkladRows.Count > 1)
            {
                foreach (var ns in NomenklSkladRows)
                    if (NomenklSelectedList.All(_ => _.Nomenkl.DocCode != ns.Nomenkl.DocCode))
                    {
                        ns.FactOtgruz = ns.MaxOtgruz;
                        NomenklSelectedList.Add(ns);
                        removeDCs.Add(ns.Nomenkl.DocCode);
                    }
            }
            else
            {
                if (NomenklSelectedList.All(_ => _.Nomenkl.DocCode != CurrentNomenkl.Nomenkl.DocCode))
                {
                    CurrentNomenkl.FactOtgruz = CurrentNomenkl.MaxOtgruz;
                    removeDCs.Add(CurrentNomenkl.Nomenkl.DocCode);
                    NomenklSelectedList.Add(CurrentNomenkl);
                }
            }

            foreach (var old in removeDCs.Select(dc => NomenklList.FirstOrDefault(_ => _.Nomenkl.DocCode == dc))
                         .Where(old => old != null))
            {
                NomenklList.Remove(old);
            }
        }

        public override void RefreshData(object obj)
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                var sql = "DECLARE @StartPrice NUMERIC(18, 4) " +
                          ",@StartPriceWithNaklad NUMERIC(18, 4) " +
                          ",@StartKol NUMERIC(18,4) " +
                          ",@nomDC NUMERIC(18, 0) " +
                          ",@startDate DATETIME " +
                          $",@DateStart DATETIME = '{CustomFormat.DateToString(new DateTime(2000, 1, 1))}' " +
                          $",@DateEnd DATETIME = '{CustomFormat.DateToString(Date)}' " +
                          $",@StoreDC NUMERIC(18, 0) = {CustomFormat.DecimalToSqlDecimal(warehouse.DocCode)}; " +
                          " " +
                          "DROP TABLE IF EXISTS #startprices; " +
                          "DROP TABLE IF EXISTS #tab; " +
                          " " +
                          "CREATE TABLE #startprices ( " +
                          "  NomDC NUMERIC(18, 0) " +
                          " ,Price NUMERIC(18, 4) " +
                          " ,PriceWithnaklad NUMERIC(18, 4) " +
                          " ,Ostatok NUMERIC(18,4) " +
                          "); " +
                          "CREATE NONCLUSTERED INDEX ix_tempstartprices ON #startprices (NomDC); " +
                          " " +
                          "CREATE TABLE #tab ( " +
                          "NomDC NUMERIC(18, 0) " +
                          ",Date DATETIME " +
                          ",StartQuantity NUMERIC(18,4) " +
                          ",PriceStart NUMERIC(18, 4) " +
                          ",PiceStartWithPrice NUMERIC(18, 4) " +
                          ",Prihod NUMERIC(18, 4) " +
                          ",MoneyPrihod NUMERIC(18, 4) " +
                          ",MoneyPrihodWithNaklad NUMERIC(18, 4) " +
                          ",Rashod NUMERIC(18, 4) " +
                          ",MoneyRashod NUMERIC(18, 4) " +
                          ",MoneyRashodWithNaklad NUMERIC(18, 4) " +
                          ",OrderBy INT " +
                          ",SumPrihod NUMERIC(18, 4) " +
                          ",SumRashod NUMERIC(18, 4) " +
                          "); " +
                          "CREATE NONCLUSTERED INDEX ix_temptab ON #tab (NomDC,Date); " +
                          "INSERT INTO #tab " +
                          "SELECT " +
                          "NomDC " +
                          ",Date " +
                          ",0 " +
                          ",0 " +
                          ",0 " +
                          ",Prihod " +
                          ",CASE WHEN Prihod > 0 THEN isnull(SUM(Prihod * Price) OVER (PARTITION BY NomDC ORDER BY Date ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW),0)" +
                          "ELSE 0 END AS MoneyPrihod " +
                          ",CASE WHEN Prihod > 0 THEN isnull(SUM(Prihod * PriceWithNaklad) OVER (PARTITION BY NomDC ORDER BY Date ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW),0) " +
                          "ELSE 0 END AS MoneyPrihodWithNaklad " +
                          ",Rashod " +
                          ",isnull(SUM(Rashod * Price) OVER (PARTITION BY NomDC ORDER BY Date ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW),0) AS MoneyRashod " +
                          ",isnull(SUM(Rashod * PriceWithNaklad) OVER (PARTITION BY NomDC ORDER BY Date ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW),0) AS MoneyRashodWithNaklad " +
                          ",CASE WHEN Prihod > 0 THEN 0 ELSE 1 END OrderBy " +
                          ",SUM(Prihod) OVER (PARTITION BY NomDC ORDER BY Date ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS SumPrihod " +
                          ",SUM(Rashod) OVER (PARTITION BY NomDC ORDER BY Date ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS SumRashod " +
                          "FROM NomenklMoveForCalc " +
                          "WHERE Date <= @DateEnd --AND OperType != 'Накладная на внутренее перемещение' \n " +
                          "AND StoreDC = @StoreDC " +
                          "ORDER BY NomDC, OrderBy; " +
                          "INSERT INTO #startprices " +
                          "SELECT " +
                          "np.NOM_DC " +
                          ",np.PRICE_WO_NAKLAD " +
                          ",np.PRICE " +
                          ",ISNULL((SELECT SUM(Prihod-Rashod) FROM #tab tt WHERE tt.NomDC = np.NOM_DC AND tt.Date < @DateStart),0) " +
                          "FROM NOM_PRICE np " +
                          "WHERE np.NOM_DC IN (SELECT DISTINCT " +
                          "t.NomDC " +
                          "FROM #tab t) " +
                          "AND np.Date = (SELECT " +
                          "MAX(np1.DATE) " +
                          "FROM NOM_PRICE np1 " +
                          "WHERE np1.NOM_DC = np.NOM_DC " +
                          "AND np1.Date < @DateStart); " +
                          " " +
                          "SELECT * FROM ( " +
                          "SELECT " +
                          "t.NomDC " +
                          ",NOM_NOMENKL AS NomNumber " +
                          ",NOM_NAME AS NomName " +
                          ",t.Date " +
                          ",Prihod " +
                          ",t.MoneyPrihod MoneyPrihod " +
                          ",MoneyPrihodWithNaklad MoneyPrihodWithNaklad " +
                          ",Rashod " +
                          ",t.Rashod * np.PRICE_WO_NAKLAD MoneyRashod " +
                          ",t.Rashod * np.PRICE MoneyRashodWithNaklad " +
                          ",SUM(Prihod - Rashod) OVER (PARTITION BY t.NomDC ORDER BY t.Date, OrderBy ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS Ostatok " +
                          ",np.PRICE_WO_NAKLAD AS Price " +
                          ",np.PRICE AS PriceWithNaklad " +
                          ",ISNULL(s.Price, 0) StartPrice " +
                          ",ISNULL(s.PriceWithnaklad, 0) StartPriceWithNaklad " +
                          ",ISNULL(s.Ostatok,0) Start " +
                          "FROM #tab t " +
                          "INNER JOIN sd_83 " +
                          "  ON NomDC = SD_83.DOC_CODE " +
                          "INNER JOIN NOM_PRICE np " +
                          "  ON SD_83.DOC_CODE = np.NOM_DC " +
                          "    AND np.DATE = (SELECT " +
                          " MAX(np1.DATE) " +
                          "      FROM NOM_PRICE np1 " +
                          "      WHERE np1.NOM_DC = SD_83.DOC_CODE " +
                          "      AND np1.Date <= t.Date) " +
                          "LEFT OUTER JOIN #startprices s " +
                          "  ON t.NomDC = s.NomDC) tab " +
                          "WHERE tab.Date >= @DateStart " +
                          "ORDER BY tab.NomDC, tab.Date " +
                          " " +
                          "DROP TABLE #startprices; " +
                          "DROP TABLE #tab; ";
                var data = ctx.Database.SqlQuery<NomenklCalcMove>(sql).ToList();
                var nnomlist = (from item in data
                    group item by item.NomDC
                    into g
                    select new
                    {
                        NomDC = g.Key,
                        SummaIn = g.Sum(item => item.MoneyPrihod),
                        SummaOut = g.Sum(item => item.MoneyRashod),
                        Prihod = g.Sum(item => item.Prihod),
                        Rashod = g.Sum(item => item.Rashod),
                        LastOp = g.Last()
                    }).ToList();
                var listTemp = new List<NomenklMoveOnSkladViewModel>();
                foreach (var item in nnomlist)
                {
                    if (myExclude != null && myExclude.Count > 0 && myExclude.Contains(item.NomDC)) continue;
                    var summaIn = item.SummaIn;
                    var summaOut = item.SummaOut;
                    var newitem = new NomenklMoveOnSkladViewModel
                    {
                        Nomenkl = GlobalOptions.ReferencesCache.GetNomenkl(item.NomDC) as Nomenkl,
                        PriceStart = item.LastOp.StartPrice,
                        PriceEnd = item.LastOp.Price,
                        QuantityEnd = item.LastOp.Ostatok,
                        QuantityStart = item.LastOp.Start,
                        QuantityIn = item.Prihod,
                        QuantityOut = item.Rashod
                    };
                    listTemp.Add(newitem);
                    switch (newitem.CurrencyName)
                    {
                        case CurrencyCode.RUBName:
                        case CurrencyCode.RURName:
                            newitem.SummaRUBStart = newitem.PriceStart * newitem.QuantityStart;
                            newitem.SummaRUBEnd = newitem.PriceEnd * newitem.QuantityEnd;
                            newitem.SummaRUBIn = summaIn;
                            newitem.SummaRUBOut = summaOut;
                            continue;
                        case CurrencyCode.USDName:
                            newitem.SummaUSDStart = newitem.PriceStart * newitem.QuantityStart;
                            newitem.SummaUSDEnd = newitem.PriceEnd * newitem.QuantityEnd;
                            newitem.SummaUSDIn = summaIn;
                            newitem.SummaUSDOut = summaOut;
                            continue;
                        case CurrencyCode.EURName:
                            newitem.SummaEURStart = newitem.PriceStart * newitem.QuantityStart;
                            newitem.SummaEUREnd = newitem.PriceEnd * newitem.QuantityEnd;
                            newitem.SummaEURIn = summaIn;
                            newitem.SummaEUROut = summaOut;
                            continue;
                        default:
                            newitem.SummaAllStart = newitem.PriceStart * newitem.QuantityStart;
                            newitem.SummaAllEnd = newitem.PriceEnd * newitem.QuantityEnd;
                            newitem.SummaAllIn = summaIn;
                            newitem.SummaAllOut = summaOut;
                            continue;
                    }
                }

                var delList = new List<NomenklMoveOnSkladViewModel>(listTemp.Where(nl => nl.QuantityStart == 0
                    && nl.QuantityIn == 0 && nl.QuantityOut == 0 && nl.QuantityEnd == 0));
                foreach (var nl in delList) listTemp.Remove(nl);
                NomenklList.Clear();
                if (myCurrency == null)
                    foreach (var n in listTemp.Where(_ => _.QuantityEnd > 0))
                        NomenklList.Add(new NomenklRemainsOnSkladWithPrice
                        {
                            Nomenkl = n.Nomenkl,
                            Quantity = n.QuantityEnd,
                            Prices = nomenklManager.GetNomenklPrice(n.Nomenkl.DocCode, DateTime.Today)
                        });
                else
                    foreach (var n in listTemp.Where(_ =>
                                 _.QuantityEnd > 0 && ((IDocCode)_.Nomenkl.Currency).DocCode == myCurrency.DocCode))
                        NomenklList.Add(new NomenklRemainsOnSkladWithPrice
                        {
                            Nomenkl = n.Nomenkl,
                            Quantity = n.QuantityEnd,
                            Prices = nomenklManager.GetNomenklPrice(n.Nomenkl.DocCode, DateTime.Today)
                        });
            }

            UpdateMaxQuantity(Date);
        }

        public void UpdateMaxQuantity(DateTime newDate)
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                foreach (var r in NomenklList)
                {
                    var nq = nomenklManager.GetNomenklQuantity(warehouse.DocCode, r.Nomenkl.DocCode,
                        newDate, newDate > DateTime.Today ? newDate : DateTime.Today);
                    r.MaxOtgruz = nq.Count > 0 ? nq.Min(_ => _.OstatokQuantity) : 0;
                }
            }
        }

        protected override void OnWindowLoaded(object obj)
        {
            //base.OnWindowLoaded(obj);
            var grids = CustomDataUserControl.FindVisualChildren<GridControl>().ToList();
            var trees = CustomDataUserControl.FindVisualChildren<TreeListControl>().ToList();
            try
            {
                foreach (var col in grids.SelectMany(grid => grid.Columns))
                {
                    col.Name = col.FieldName;
                }
                foreach (var col in trees.SelectMany(grid => grid.Columns))
                {
                    col.Name = col.FieldName;
                }
                if (LayoutManager == null) OnLayoutInitial(null); 
                LayoutManager?.Load();
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }
            ((SelectExistNomenklOnSkladView)CustomDataUserControl).gridNomenklRows.SelectionMode =
                MultiSelectMode.Row;
            ((SelectExistNomenklOnSkladView)CustomDataUserControl).tableViewRows.NavigationStyle =
                GridViewNavigationStyle.Cell;

            ((SelectExistNomenklOnSkladView)CustomDataUserControl).gridSelectNomenklRows.SelectionMode =
                MultiSelectMode.Row;
            ((SelectExistNomenklOnSkladView)CustomDataUserControl).tableSelectNomenklViewRows.NavigationStyle =
                GridViewNavigationStyle.Cell;
            foreach (var col in ((SelectExistNomenklOnSkladView)CustomDataUserControl).gridSelectNomenklRows.Columns)
                switch (col.FieldName)
                {
                    case "FactOtgruz":
                        otgruzColumn = col;
                        col.ReadOnly = false;
                        break;
                }
        }

        #endregion

        public ICommand FactOtgruzChangedCommand
        {
            get { return new Command(FactOtgruzChanged, _ => true); }
        }

        private void FactOtgruzChanged(object obj)
        {
            if (obj is CellValueChangedEventArgs args && CurrentSelectedNomenkl != null)
            {
                if ((decimal)args.Value > CurrentSelectedNomenkl.MaxOtgruz)
                {
                    CurrentSelectedNomenkl.FactOtgruz = CurrentSelectedNomenkl.MaxOtgruz;
                }
            }
        }
    }
}
