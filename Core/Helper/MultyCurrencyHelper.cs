using System.Collections.Generic;
using System.Linq;
using Core.EntityViewModel.CommonReferences;
using Core.Invoices.EntityViewModel;
using Core.ViewModel.Base;
using DevExpress.Xpf.Grid;

namespace Core.Helper
{
    public class MultyCurrencyHelper
    {
        #region Methods

        public static void VisibilityCurrencyWithDilerColumns(TreeListControl treeList,
            IEnumerable<IMultyWithDilerCurrency> data)
        {
            if (treeList == null || data == null) return;
            if (!data.Any())
            {
                foreach (var band in treeList.Bands)
                    switch (band.Header)
                    {
                        case "CHF":
                            band.Visible = false;
                            foreach (var c in band.Columns)
                                c.Visible = false;
                            break;
                        case "SEK":
                            band.Visible = false;
                            foreach (var c in band.Columns)
                                c.Visible = false;
                            break;
                        case "EUR":
                            band.Visible = false;
                            foreach (var c in band.Columns)
                                c.Visible = false;
                            break;
                        case "RUB":
                            band.Visible = false;
                            foreach (var c in band.Columns)
                                c.Visible = false;
                            break;
                        case "GBP":
                            band.Visible = false;
                            foreach (var c in band.Columns)
                                c.Visible = false;
                            break;
                        case "USD":
                            band.Visible = false;
                            foreach (var c in band.Columns)
                                c.Visible = false;
                            break;
                    }

                return;
            }

            var IsResultCHF = data
                .Any(_ => _.ProfitCHF != 0 || _.LossCHF != 0 || _.DilerCHF != 0);
            var IsResultSEK = data
                .Any(_ => _.ProfitSEK != 0 || _.LossSEK != 0 || _.DilerSEK != 0);
            var IsResultEUR = data
                .Any(_ => _.ProfitEUR != 0 || _.LossEUR != 0 || _.DilerEUR != 0);
            var IsResultRUB = data
                .Any(_ => _.ProfitRUB != 0 || _.LossRUB != 0 || _.DilerRUB != 0);
            var IsResultGBP = data
                .Any(_ => _.ProfitGBP != 0 || _.LossGBP != 0 || _.DilerGBP != 0);
            var IsResultUSD = data
                .Any(_ => _.ProfitUSD != 0 || _.LossUSD != 0 || _.DilerUSD != 0);
            foreach (var band in treeList.Bands)
                switch (band.Header)
                {
                    case "CHF":
                        band.Visible = IsResultCHF;
                        foreach (var c in band.Columns)
                            c.Visible = IsResultCHF;
                        break;
                    case "SEK":
                        band.Visible = IsResultSEK;
                        foreach (var c in band.Columns)
                            c.Visible = IsResultSEK;
                        break;
                    case "EUR":
                        band.Visible = IsResultEUR;
                        foreach (var c in band.Columns)
                            c.Visible = IsResultEUR;
                        break;
                    case "RUB":
                        band.Visible = IsResultRUB;
                        foreach (var c in band.Columns)
                            c.Visible = IsResultRUB;
                        break;
                    case "GBP":
                        band.Visible = IsResultGBP;
                        foreach (var c in band.Columns)
                            c.Visible = IsResultGBP;
                        break;
                    case "USD":
                        band.Visible = IsResultUSD;
                        foreach (var c in band.Columns)
                            c.Visible = IsResultUSD;
                        break;
                }
        }

        public static void VisibilityCurrencyColumns(TreeListControl treeList,
            IEnumerable<IMultyCurrency> data)
        {
            if (treeList == null || data == null) return;
            if (!data.Any())
            {
                foreach (var band in treeList.Bands)
                    switch (band.Header)
                    {
                        case "CHF":
                            band.Visible = false;
                            foreach (var c in band.Columns)
                                c.Visible = false;
                            break;
                        case "SEK":
                            band.Visible = false;
                            foreach (var c in band.Columns)
                                c.Visible = false;
                            break;
                        case "EUR":
                            band.Visible = false;
                            foreach (var c in band.Columns)
                                c.Visible = false;
                            break;
                        case "RUB":
                            band.Visible = false;
                            foreach (var c in band.Columns)
                                c.Visible = false;
                            break;
                        case "GBP":
                            band.Visible = false;
                            foreach (var c in band.Columns)
                                c.Visible = false;
                            break;
                        case "USD":
                            band.Visible = false;
                            foreach (var c in band.Columns)
                                c.Visible = false;
                            break;
                    }

                return;
            }

            var IsResultCHF = data
                .Any(_ => _.ProfitCHF != 0 || _.LossCHF != 0);
            var IsResultSEK = data
                .Any(_ => _.ProfitSEK != 0 || _.LossSEK != 0);
            var IsResultEUR = data
                .Any(_ => _.ProfitEUR != 0 || _.LossEUR != 0);
            var IsResultRUB = data
                .Any(_ => _.ProfitRUB != 0 || _.LossRUB != 0);
            var IsResultGBP = data
                .Any(_ => _.ProfitGBP != 0 || _.LossGBP != 0);
            var IsResultUSD = data
                .Any(_ => _.ProfitUSD != 0 || _.LossUSD != 0);
            foreach (var band in treeList.Bands)
                switch (band.Header)
                {
                    case "CHF":
                        band.Visible = IsResultCHF;
                        foreach (var c in band.Columns)
                            c.Visible = IsResultCHF;
                        break;
                    case "SEK":
                        band.Visible = IsResultSEK;
                        foreach (var c in band.Columns)
                            c.Visible = IsResultSEK;
                        break;
                    case "EUR":
                        band.Visible = IsResultEUR;
                        foreach (var c in band.Columns)
                            c.Visible = IsResultEUR;
                        break;
                    case "RUB":
                        band.Visible = IsResultRUB;
                        foreach (var c in band.Columns)
                            c.Visible = IsResultRUB;
                        break;
                    case "GBP":
                        band.Visible = IsResultGBP;
                        foreach (var c in band.Columns)
                            c.Visible = IsResultGBP;
                        break;
                    case "USD":
                        band.Visible = IsResultUSD;
                        foreach (var c in band.Columns)
                            c.Visible = IsResultUSD;
                        break;
                }
        }

        public static void VisibilityCurrencyWithDilerColumns(GridControl grid,
            IEnumerable<IMultyWithDilerCurrency> data)
        {
            if (grid == null || data == null) return;
            if (!data.Any())
            {
                foreach (var band in grid.Bands)
                    switch (band.Header)
                    {
                        case "CHF":
                            band.Visible = false;
                            foreach (var c in band.Columns)
                                c.Visible = false;
                            break;
                        case "SEK":
                            band.Visible = false;
                            foreach (var c in band.Columns)
                                c.Visible = false;
                            break;
                        case "EUR":
                            band.Visible = false;
                            foreach (var c in band.Columns)
                                c.Visible = false;
                            break;
                        case "RUB":
                            band.Visible = false;
                            foreach (var c in band.Columns)
                                c.Visible = false;
                            break;
                        case "GBP":
                            band.Visible = false;
                            foreach (var c in band.Columns)
                                c.Visible = false;
                            break;
                        case "USD":
                            band.Visible = false;
                            foreach (var c in band.Columns)
                                c.Visible = false;
                            break;
                    }

                return;
            }

            var IsResultCHF = data
                .Any(_ => _.ProfitCHF != 0 || _.LossCHF != 0 || _.DilerCHF != 0);
            var IsResultSEK = data
                .Any(_ => _.ProfitSEK != 0 || _.LossSEK != 0 || _.DilerSEK != 0);
            var IsResultEUR = data
                .Any(_ => _.ProfitEUR != 0 || _.LossEUR != 0 || _.DilerEUR != 0);
            var IsResultRUB = data
                .Any(_ => _.ProfitRUB != 0 || _.LossRUB != 0 || _.DilerRUB != 0);
            var IsResultGBP = data
                .Any(_ => _.ProfitGBP != 0 || _.LossGBP != 0 || _.DilerGBP != 0);
            var IsResultUSD = data
                .Any(_ => _.ProfitUSD != 0 || _.LossUSD != 0 || _.DilerUSD != 0);
            foreach (var band in grid.Bands)
                switch (band.Header)
                {
                    case "CHF":
                        band.Visible = IsResultCHF;
                        foreach (var c in band.Columns)
                            c.Visible = IsResultCHF;
                        break;
                    case "SEK":
                        band.Visible = IsResultSEK;
                        foreach (var c in band.Columns)
                            c.Visible = IsResultSEK;
                        break;
                    case "EUR":
                        band.Visible = IsResultEUR;
                        foreach (var c in band.Columns)
                            c.Visible = IsResultEUR;
                        break;
                    case "RUB":
                        band.Visible = IsResultRUB;
                        foreach (var c in band.Columns)
                            c.Visible = IsResultRUB;
                        break;
                    case "GBP":
                        band.Visible = IsResultGBP;
                        foreach (var c in band.Columns)
                            c.Visible = IsResultGBP;
                        break;
                    case "USD":
                        band.Visible = IsResultUSD;
                        foreach (var c in band.Columns)
                            c.Visible = IsResultUSD;
                        break;
                }
        }

        public static void VisibilityCurrencyColumns(GridControl grid,
            IEnumerable<IMultyCurrency> data)
        {
            if (grid == null || data == null) return;
            if (!data.Any())
            {
                foreach (var band in grid.Bands)
                    switch (band.Header)
                    {
                        case "CHF":
                            band.Visible = false;
                            foreach (var c in band.Columns)
                                c.Visible = false;
                            break;
                        case "SEK":
                            band.Visible = false;
                            foreach (var c in band.Columns)
                                c.Visible = false;
                            break;
                        case "EUR":
                            band.Visible = false;
                            foreach (var c in band.Columns)
                                c.Visible = false;
                            break;
                        case "RUB":
                            band.Visible = false;
                            foreach (var c in band.Columns)
                                c.Visible = false;
                            break;
                        case "GBP":
                            band.Visible = false;
                            foreach (var c in band.Columns)
                                c.Visible = false;
                            break;
                        case "USD":
                            band.Visible = false;
                            foreach (var c in band.Columns)
                                c.Visible = false;
                            break;
                    }

                return;
            }

            var IsResultCHF = data
                .Any(_ => _.ProfitCHF != 0 || _.LossCHF != 0);
            var IsResultSEK = data
                .Any(_ => _.ProfitSEK != 0 || _.LossSEK != 0);
            var IsResultEUR = data
                .Any(_ => _.ProfitEUR != 0 || _.LossEUR != 0);
            var IsResultRUB = data
                .Any(_ => _.ProfitRUB != 0 || _.LossRUB != 0);
            var IsResultGBP = data
                .Any(_ => _.ProfitGBP != 0 || _.LossGBP != 0);
            var IsResultUSD = data
                .Any(_ => _.ProfitUSD != 0 || _.LossUSD != 0);
            foreach (var band in grid.Bands)
                switch (band.Header)
                {
                    case "CHF":
                        band.Visible = IsResultCHF;
                        foreach (var c in band.Columns)
                            c.Visible = IsResultCHF;
                        break;
                    case "SEK":
                        band.Visible = IsResultSEK;
                        foreach (var c in band.Columns)
                            c.Visible = IsResultSEK;
                        break;
                    case "EUR":
                        band.Visible = IsResultEUR;
                        foreach (var c in band.Columns)
                            c.Visible = IsResultEUR;
                        break;
                    case "RUB":
                        band.Visible = IsResultRUB;
                        foreach (var c in band.Columns)
                            c.Visible = IsResultRUB;
                        break;
                    case "GBP":
                        band.Visible = IsResultGBP;
                        foreach (var c in band.Columns)
                            c.Visible = IsResultGBP;
                        break;
                    case "USD":
                        band.Visible = IsResultUSD;
                        foreach (var c in band.Columns)
                            c.Visible = IsResultUSD;
                        break;
                }
        }

        public static void SetCurrencyValueToZero(IMultyCurrency row)
        {
            row.ProfitCHF = 0;
            row.ProfitEUR = 0;
            row.ProfitGBP = 0;
            row.ProfitRUB = 0;
            row.ProfitSEK = 0;
            row.ProfitUSD = 0;
            row.LossCHF = 0;
            row.LossEUR = 0;
            row.LossGBP = 0;
            row.LossRUB = 0;
            row.LossSEK = 0;
            row.LossUSD = 0;
        }

        public static void SetCurrencyValueToZero(IMultyWithDilerCurrency row)
        {
            row.ProfitCHF = 0;
            row.ProfitEUR = 0;
            row.ProfitGBP = 0;
            row.ProfitRUB = 0;
            row.ProfitSEK = 0;
            row.ProfitUSD = 0;
            row.LossCHF = 0;
            row.LossEUR = 0;
            row.LossGBP = 0;
            row.LossRUB = 0;
            row.LossSEK = 0;
            row.LossUSD = 0;
            row.DilerCHF = 0;
            row.DilerEUR = 0;
            row.DilerGBP = 0;
            row.DilerRUB = 0;
            row.DilerSEK = 0;
            row.DilerUSD = 0;
        }

        public static void SetCurrencyValue(decimal summa, Currency crs, IMultyCurrency row,
            TypeProfitAndLossCalc calcType)
        {
            if (row == null || crs == null || calcType == TypeProfitAndLossCalc.None) return;
            SetCurrencyValueToZero(row);
            switch (crs.Name)
            {
                case CurrencyCode.RUBName:
                case CurrencyCode.RURName:
                    switch (calcType)
                    {
                        case TypeProfitAndLossCalc.IsProfit:
                            row.ProfitRUB = summa;
                            break;
                        case TypeProfitAndLossCalc.IsLoss:
                            row.LossRUB = summa;
                            break;
                        case TypeProfitAndLossCalc.IsAll:
                            row.ProfitRUB = summa;
                            row.LossRUB = summa;
                            break;
                    }

                    break;
                case CurrencyCode.USDName:
                    switch (calcType)
                    {
                        case TypeProfitAndLossCalc.IsProfit:
                            row.ProfitUSD = summa;
                            break;
                        case TypeProfitAndLossCalc.IsLoss:
                            row.LossUSD = summa;
                            break;
                        case TypeProfitAndLossCalc.IsAll:
                            row.ProfitUSD = summa;
                            row.LossUSD = summa;
                            break;
                    }

                    break;
                case CurrencyCode.EURName:
                    switch (calcType)
                    {
                        case TypeProfitAndLossCalc.IsProfit:
                            row.ProfitEUR = summa;
                            break;
                        case TypeProfitAndLossCalc.IsLoss:
                            row.LossEUR = summa;
                            break;
                        case TypeProfitAndLossCalc.IsAll:
                            row.ProfitEUR = summa;
                            row.LossEUR = summa;
                            break;
                    }

                    break;
                case CurrencyCode.GBPName:
                    switch (calcType)
                    {
                        case TypeProfitAndLossCalc.IsProfit:
                            row.ProfitGBP = summa;
                            break;
                        case TypeProfitAndLossCalc.IsLoss:
                            row.LossGBP = summa;
                            break;
                        case TypeProfitAndLossCalc.IsAll:
                            row.ProfitGBP = summa;
                            row.LossGBP = summa;
                            break;
                    }

                    break;
                case CurrencyCode.CHFName:
                    switch (calcType)
                    {
                        case TypeProfitAndLossCalc.IsProfit:
                            row.ProfitCHF = summa;
                            break;
                        case TypeProfitAndLossCalc.IsLoss:
                            row.LossCHF = summa;
                            break;
                        case TypeProfitAndLossCalc.IsAll:
                            row.ProfitCHF = summa;
                            row.LossCHF = summa;
                            break;
                    }

                    break;
                case CurrencyCode.SEKName:
                    switch (calcType)
                    {
                        case TypeProfitAndLossCalc.IsProfit:
                            row.ProfitSEK = summa;
                            break;
                        case TypeProfitAndLossCalc.IsLoss:
                            row.LossSEK = summa;
                            break;
                        case TypeProfitAndLossCalc.IsAll:
                            row.ProfitSEK = summa;
                            row.LossSEK = summa;
                            break;
                    }

                    break;
            }
        }

        public static void SetCurrencyValue(decimal? summa, Currency crs, IMultyCurrency row,
            TypeProfitAndLossCalc calcType)
        {
            SetCurrencyValue(summa ?? 0, crs, row, calcType);
        }

        public static void SetCurrencyValue(decimal summa, decimal dilerSumma, Currency crs,
            IMultyWithDilerCurrency row,
            TypeProfitAndLossCalc calcType)
        {
            if (row == null) return;
            SetCurrencyValueToZero(row);
            switch (crs.Name)
            {
                case CurrencyCode.RUBName:
                case CurrencyCode.RURName:
                    switch (calcType)
                    {
                        case TypeProfitAndLossCalc.IsProfit:
                            row.ProfitRUB = summa;
                            break;
                        case TypeProfitAndLossCalc.IsLoss:
                            row.LossRUB = summa;
                            break;
                        case TypeProfitAndLossCalc.IsAll:
                            row.ProfitRUB = summa;
                            row.LossRUB = summa;
                            break;
                    }

                    row.DilerRUB = dilerSumma;
                    break;
                case CurrencyCode.USDName:
                    switch (calcType)
                    {
                        case TypeProfitAndLossCalc.IsProfit:
                            row.ProfitUSD = summa;
                            break;
                        case TypeProfitAndLossCalc.IsLoss:
                            row.LossUSD = summa;
                            break;
                        case TypeProfitAndLossCalc.IsAll:
                            row.ProfitUSD = summa;
                            row.LossUSD = summa;
                            break;
                    }

                    row.DilerUSD = dilerSumma;
                    break;
                case CurrencyCode.EURName:
                    switch (calcType)
                    {
                        case TypeProfitAndLossCalc.IsProfit:
                            row.ProfitEUR = summa;
                            break;
                        case TypeProfitAndLossCalc.IsLoss:
                            row.LossEUR = summa;
                            break;
                        case TypeProfitAndLossCalc.IsAll:
                            row.ProfitEUR = summa;
                            row.LossEUR = summa;
                            break;
                    }

                    row.DilerEUR = dilerSumma;
                    break;
                case CurrencyCode.GBPName:
                    switch (calcType)
                    {
                        case TypeProfitAndLossCalc.IsProfit:
                            row.ProfitGBP = summa;
                            break;
                        case TypeProfitAndLossCalc.IsLoss:
                            row.LossGBP = summa;
                            break;
                        case TypeProfitAndLossCalc.IsAll:
                            row.ProfitGBP = summa;
                            row.LossGBP = summa;
                            break;
                    }

                    row.DilerGBP = dilerSumma;
                    break;
                case CurrencyCode.CHFName:
                    switch (calcType)
                    {
                        case TypeProfitAndLossCalc.IsProfit:
                            row.ProfitCHF = summa;
                            break;
                        case TypeProfitAndLossCalc.IsLoss:
                            row.LossCHF = summa;
                            break;
                        case TypeProfitAndLossCalc.IsAll:
                            row.ProfitCHF = summa;
                            row.LossCHF = summa;
                            break;
                    }

                    row.DilerCHF = dilerSumma;
                    break;
                case CurrencyCode.SEKName:
                    switch (calcType)
                    {
                        case TypeProfitAndLossCalc.IsProfit:
                            row.ProfitSEK = summa;
                            break;
                        case TypeProfitAndLossCalc.IsLoss:
                            row.LossSEK = summa;
                            break;
                        case TypeProfitAndLossCalc.IsAll:
                            row.ProfitSEK = summa;
                            row.LossSEK = summa;
                            break;
                    }

                    row.DilerSEK = dilerSumma;
                    break;
            }
        }

        public static void SetCurrencyValue(decimal? summa, decimal dilerSumma, Currency crs,
            IMultyWithDilerCurrency row,
            TypeProfitAndLossCalc calcType)
        {
            SetCurrencyValue(summa ?? 0, dilerSumma, crs, row, calcType);
        }

        #endregion
    }
}