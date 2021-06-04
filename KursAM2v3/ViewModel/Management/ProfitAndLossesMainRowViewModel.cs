using System;
using System.Collections.Generic;
using Core.EntityViewModel.CommonReferences;
using Core.Helper;
using Core.ViewModel.Base;
using Core.ViewModel.Common;

namespace KursAM2.ViewModel.Management
{
    public class ProfitAndLossesMainRowViewModel : RSViewModelBase
    {
        private TypeProfitAndLossCalc myCalcType;
        private bool myIsDiv;
        private decimal myLossCHF;
        private decimal myLossEUR;
        private decimal myLossGBP;
        private decimal myLossRUB;
        private decimal myLossSEK;
        private decimal myLossUSD;
        private ProfitAndLossesExtendRowViewModel myProfitAndLossesExtendRowViewModel;
        private decimal myProfitCHF;
        private decimal myProfitEUR;
        private decimal myProfitGBP;
        private decimal myProfitRUB;
        private decimal myProfitSEK;
        private decimal myProfitUSD;
        private decimal myRecalcLoss;
        private decimal myRecalcProfit;
        private decimal myRecalcResult;
        private decimal myResultCHF;
        private decimal myResultEUR;
        private decimal myResultGBP;
        private decimal myResultRUB;
        private decimal myResultSEK;
        private decimal myResultUSD;
        private decimal myLossCNY;
        private decimal myProfitCNY;
        private decimal myResultCNY;

        public ProfitAndLossesMainRowViewModel()
        {
            LossRUB = 0;
            ProfitRUB = 0;
            ResultRUB = 0;
            LossUSD = 0;
            ProfitUSD = 0;
            ResultUSD = 0;
            LossEUR = 0;
            ProfitEUR = 0;
            ResultEUR = 0;
            LossGBP = 0;
            ProfitGBP = 0;
            ResultGBP = 0;
            LossCHF = 0;
            ProfitCHF = 0;
            ResultCHF = 0;
            LossSEK = 0;
            ProfitSEK = 0;
            ResultSEK = 0;
        }

        /// <summary>
        ///     Шведская Крона
        /// </summary>
        public decimal LossSEK
        {
            get => myLossSEK;
            set
            {
                if (myLossSEK == value) return;
                myLossSEK = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        ///     Шведская Крона
        /// </summary>
        public decimal ProfitSEK
        {
            get => myProfitSEK;
            set
            {
                if (myProfitSEK == value) return;
                myProfitSEK = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        ///     Шведская Крона
        /// </summary>
        public decimal ResultSEK
        {
            get => myResultSEK;
            set
            {
                if (myResultSEK == value) return;
                myResultSEK = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        ///     Швейцарский франк
        /// </summary>
        public decimal LossCHF
        {
            get => myLossCHF;
            set
            {
                if (myLossCHF == value) return;
                myLossCHF = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        ///     Швейцарский франк
        /// </summary>
        public decimal ProfitCHF
        {
            get => myProfitCHF;
            set
            {
                if (myProfitCHF == value) return;
                myProfitCHF = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        ///     Швейцарский франк
        /// </summary>
        public decimal ResultCHF
        {
            get => myResultCHF;
            set
            {
                if (myResultCHF == value) return;
                myResultCHF = value;
                RaisePropertyChanged();
            }
        }

        public decimal LossGBP
        {
            get => myLossGBP;
            set
            {
                if (myLossGBP == value) return;
                myLossGBP = value;
                RaisePropertyChanged();
            }
        }

        public decimal ProfitGBP
        {
            get => myProfitGBP;
            set
            {
                if (myProfitGBP == value) return;
                myProfitGBP = value;
                RaisePropertyChanged();
            }
        }

        public decimal ResultGBP
        {
            get => myResultGBP;
            set
            {
                if (myResultGBP == value) return;
                myResultGBP = value;
                RaisePropertyChanged();
            }
        }

        public TypeProfitAndLossCalc CalcType
        {
            set
            {
                if (Equals(value, myCalcType)) return;
                myCalcType = value;
                RaisePropertyChanged();
            }
            get => myCalcType;
        }

        public decimal ProfitRUB
        {
            set
            {
                if (Equals(value, myProfitRUB)) return;
                myProfitRUB = value;
                RaisePropertyChanged();
            }
            get => myProfitRUB;
        }

        public decimal LossRUB
        {
            set
            {
                if (Equals(value, myLossRUB)) return;
                myLossRUB = value;
                RaisePropertyChanged();
            }
            get => myLossRUB;
        }

        public decimal ResultRUB
        {
            set
            {
                if (Equals(value, myResultRUB)) return;
                myResultRUB = value;
                RaisePropertyChanged();
            }
            get => myResultRUB;
        }

        public decimal ProfitUSD
        {
            set
            {
                if (Equals(value, myProfitUSD)) return;
                myProfitUSD = value;
                RaisePropertyChanged();
            }
            get => myProfitUSD;
        }

        public decimal LossUSD
        {
            set
            {
                if (Equals(value, myLossUSD)) return;
                myLossUSD = value;
                RaisePropertyChanged();
            }
            get => myLossUSD;
        }

        public decimal ResultUSD
        {
            set
            {
                if (Equals(value, myResultUSD)) return;
                myResultUSD = value;
                RaisePropertyChanged();
            }
            get => myResultUSD;
        }

        public decimal ProfitEUR
        {
            set
            {
                if (Equals(value, myProfitEUR)) return;
                myProfitEUR = value;
                RaisePropertyChanged();
            }
            get => myProfitEUR;
        }

        public decimal LossEUR
        {
            set
            {
                if (Equals(value, myLossEUR)) return;
                myLossEUR = value;
                RaisePropertyChanged();
            }
            get => myLossEUR;
        }

        public decimal ResultEUR
        {
            set
            {
                if (Equals(value, myResultEUR)) return;
                myResultEUR = value;
                RaisePropertyChanged();
            }
            get => myResultEUR;
        }

        /// <summary>
        ///     Китайский юань
        /// </summary>
        public decimal LossCNY
        {
            get => myLossCNY;
            set
            {
                if (myLossCNY == value) return;
                myLossCNY = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        ///     Китайский юань
        /// </summary>
        public decimal ProfitCNY
        {
            get => myProfitCNY;
            set
            {
                if (myProfitCNY == value) return;
                myProfitCNY = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        ///     Китайский юань
        /// </summary>
        public decimal ResultCNY
        {
            get => myResultCNY;
            set
            {
                if (myResultCNY == value) return;
                myResultCNY = value;
                RaisePropertyChanged();
            }
        }

        public decimal RecalcProfit
        {
            set
            {
                if (Equals(value, myRecalcProfit)) return;
                myRecalcProfit = value;
                RaisePropertyChanged();
            }
            get => myRecalcProfit;
        }

        public decimal RecalcLoss
        {
            set
            {
                if (Equals(value, myRecalcLoss)) return;
                myRecalcLoss = value;
                RaisePropertyChanged();
            }
            get => myRecalcLoss;
        }

        public decimal RecalcResult
        {
            set
            {
                if (Equals(value, myRecalcResult)) return;
                myRecalcResult = value;
                RaisePropertyChanged();
            }
            get => myRecalcResult;
        }

        public ProfitAndLossesExtendRowViewModel ProfitAndLossesExtendRowViewModel
        {
            get => myProfitAndLossesExtendRowViewModel;
            set
            {
                if (myProfitAndLossesExtendRowViewModel != null &&
                    myProfitAndLossesExtendRowViewModel.Equals(value)) return;
                myProfitAndLossesExtendRowViewModel = value;
                RaisePropertyChanged();
            }
        }

        public bool IsDiv
        {
            get => myIsDiv;
            set
            {
                if (myIsDiv == value) return;
                myIsDiv = value;
                RaisePropertyChanged();
            }
        }

        public SDRSchet SDRSchet { get; set; }
        public SDRState SDRState { get; set; }
        
        #region Структура

        public static readonly Guid Result = Guid.Parse("{E7DA6232-CAA0-4358-9BAE-5D96C2EE248A}");
        
        public static readonly Guid Dohod = Guid.Parse("{A938446C-AB19-45C3-8DFD-0F24AB08DF49}");
        public static readonly Guid Rashod = Guid.Parse("{F59D3232-DDF2-4F15-B7AE-E3691F385DDD}");
        
        public static readonly Guid BalansOperations = Guid.Parse("{35C9783E-E19F-452B-8479-D6F022444552}");
        public static readonly Guid NomenklCurrencyChanges = Guid.Parse("{564DB69C-6DAD-4B16-8BF5-118F5AF2D07F}");

        #region Начальные остатки

        public static readonly Guid StartDohodRemains = Guid.Parse("{16524D17-451C-4149-8813-BDB9875F759A}");
        public static readonly Guid StartRashodRemains = Guid.Parse("{2F1FD81F-A7BE-4830-93EB-9B3A9A362B2B}");
        
        public static readonly Guid StartDohodBank = Guid.Parse("{0AD95635-A46D-49F2-AE78-CBDF52BD6E27}");
        public static readonly Guid StartRashodBank = Guid.Parse("{920B2AB6-1984-49A9-B5E7-18407221F620}");
        public static readonly Guid StartDohodCash = Guid.Parse("{A084B37A-D942-4B7F-9AE9-3C3AAA0F4475}");
        public static readonly Guid StartRashodCash = Guid.Parse("{C0AEDEC3-15CD-4D5B-8E37-0FE1EDB5C6FF}");
        public static readonly Guid StartBalansKontragentDohod = Guid.Parse("{2D07127B-72A8-4018-B9A8-62C7A78CB9C3}");
        public static readonly Guid StartBalansKontragentRashod = Guid.Parse("{15DF4D79-D608-412A-87A8-1560714A706A}");

        #endregion

        public static readonly Guid PodOtchetRashod = Guid.Parse("{52EA160E-27DC-47E1-9006-70DF349943F6}");
        public static readonly Guid PodOtchetDohod = Guid.Parse("{7550849B-1D51-445B-B692-CE3FF7AB11B0}");

        #region Товары

        public static readonly Guid NomenklDohod = Guid.Parse("{334973B4-1652-4473-9DED-FD4B31B31FC1}");
        public static readonly Guid NomenklRashod = Guid.Parse("{D89B1E18-074E-4A7D-A0EE-9537DC1585D8}");

        #endregion
        
        #endregion

        public static List<ProfitAndLossesMainRowViewModel> GetStructure()
        {
            var res = new List<ProfitAndLossesMainRowViewModel>
            {
                new ProfitAndLossesMainRowViewModel
                {
                    Id = Result,
                    ParentId = null,
                    Name = "Результат"
                },
                new ProfitAndLossesMainRowViewModel
                {
                    Id = Dohod,
                    ParentId = Result,
                    Name = "Доходы",
                    CalcType = TypeProfitAndLossCalc.IsProfit
                },
                new ProfitAndLossesMainRowViewModel
                {
                    Id = Rashod,
                    ParentId = Result,
                    Name = @"Расходы",
                    CalcType = TypeProfitAndLossCalc.IsLoss
                },
                new ProfitAndLossesMainRowViewModel
                {
                    Id = BalansOperations,
                    ParentId = Result,
                    Name = @"Балансовые операции",
                    CalcType = TypeProfitAndLossCalc.IsAll
                },

                #region Начальные остатки

                new ProfitAndLossesMainRowViewModel
                {
                    Id = StartDohodRemains,
                    ParentId = Dohod,
                    Name = @"Начальные остатки",
                    CalcType = TypeProfitAndLossCalc.IsProfit
                },
                new ProfitAndLossesMainRowViewModel
                {
                    Id = StartRashodRemains,
                    ParentId = Rashod,
                    Name = @"Начальные остатки",
                    CalcType = TypeProfitAndLossCalc.IsLoss
                },
                new ProfitAndLossesMainRowViewModel
                {
                    Id = StartDohodBank,
                    ParentId = StartDohodRemains,
                    Name = @"Банковские счета",
                    CalcType = TypeProfitAndLossCalc.IsProfit
                },
                new ProfitAndLossesMainRowViewModel
                {
                    Id = StartRashodBank,
                    ParentId = StartRashodRemains,
                    Name = @"Банковские счета",
                    CalcType = TypeProfitAndLossCalc.IsLoss
                },
                new ProfitAndLossesMainRowViewModel
                {
                    Id = StartDohodCash,
                    ParentId = StartDohodRemains,
                    Name = @"Касса",
                    CalcType = TypeProfitAndLossCalc.IsProfit
                },
                new ProfitAndLossesMainRowViewModel
                {
                    Id = StartRashodCash,
                    ParentId = StartRashodRemains,
                    Name = @"Касса",
                    CalcType = TypeProfitAndLossCalc.IsLoss
                },
                new ProfitAndLossesMainRowViewModel
                {
                    Id = StartBalansKontragentDohod,
                    ParentId = StartDohodRemains,
                    Name = @"Балансы контрагентов",
                    CalcType = TypeProfitAndLossCalc.IsProfit
                },
                new ProfitAndLossesMainRowViewModel
                {
                    Id = StartBalansKontragentRashod,
                    ParentId = StartRashodRemains,
                    Name = @"Балансы контрагентов",
                    CalcType = TypeProfitAndLossCalc.IsLoss
                },

                #endregion

                new ProfitAndLossesMainRowViewModel
                {
                    Id = PodOtchetRashod,
                    ParentId = Rashod,
                    Name = @"Выдано под отчет",
                    CalcType = TypeProfitAndLossCalc.IsLoss
                },
                new ProfitAndLossesMainRowViewModel
                {
                    Id = PodOtchetDohod,
                    ParentId = Dohod,
                    Name = @"Возврат с под отчета",
                    CalcType = TypeProfitAndLossCalc.IsProfit
                },
                new ProfitAndLossesMainRowViewModel
                {
                    Id = NomenklDohod,
                    ParentId = Dohod,
                    Name = @"Товары",
                    CalcType = TypeProfitAndLossCalc.IsProfit
                },
                new ProfitAndLossesMainRowViewModel
                {
                    Id = NomenklRashod,
                    ParentId = Rashod,
                    Name = @"Товары",
                    CalcType = TypeProfitAndLossCalc.IsLoss
                },
                new ProfitAndLossesMainRowViewModel
                {
                    Id = NomenklCurrencyChanges,
                    ParentId = BalansOperations,
                    Name = @"Валютный перевод товаров",
                    CalcType = TypeProfitAndLossCalc.IsAll
                },
                new ProfitAndLossesMainRowViewModel
                {
                    Id = Guid.Parse("{BA628F86-6AE4-4CF3-832B-C6F7388DD01B}"),
                    ParentId = Guid.Parse("{F59D3232-DDF2-4F15-B7AE-E3691F385DDD}"),
                    Name = @"Дилерские",
                    CalcType = TypeProfitAndLossCalc.IsLoss
                },
                new ProfitAndLossesMainRowViewModel
                {
                    Id = Guid.Parse("{C5C36299-FDEF-4251-B525-3DF10C0E8CB9}"),
                    ParentId = Guid.Parse("{F59D3232-DDF2-4F15-B7AE-E3691F385DDD}"),
                    Name = @"Возврат товара",
                    CalcType = TypeProfitAndLossCalc.IsLoss
                },
                new ProfitAndLossesMainRowViewModel
                {
                    Id = Guid.Parse("{04A7B6BB-7B3C-49F1-8E10-F1AE5F5582E4}"),
                    ParentId = Guid.Parse("{A938446C-AB19-45C3-8DFD-0F24AB08DF49}"),
                    Name = @"Возврат товара",
                    CalcType = TypeProfitAndLossCalc.IsProfit
                },
                new ProfitAndLossesMainRowViewModel
                {
                    Id = Guid.Parse("{66E0F763-7362-488D-B367-50AC84A72AD4}"),
                    ParentId = Guid.Parse("{D89B1E18-074E-4A7D-A0EE-9537DC1585D8}"),
                    Name = @"Списание товара",
                    CalcType = TypeProfitAndLossCalc.IsLoss
                },
                new ProfitAndLossesMainRowViewModel
                {
                    Id = Guid.Parse("{2FA1DD9F-6842-4209-B0CC-DDEF3B920496}"),
                    ParentId = Guid.Parse("{A938446C-AB19-45C3-8DFD-0F24AB08DF49}"),
                    Name = @"Услуги",
                    CalcType = TypeProfitAndLossCalc.IsProfit
                },
                new ProfitAndLossesMainRowViewModel
                {
                    Id = Guid.Parse("{E47EF726-3BEA-4B18-9773-E564D624FDF6}"),
                    ParentId = Guid.Parse("{F59D3232-DDF2-4F15-B7AE-E3691F385DDD}"),
                    Name = @"Услуги",
                    CalcType = TypeProfitAndLossCalc.IsLoss
                },
                new ProfitAndLossesMainRowViewModel
                {
                    Id = Guid.Parse("{30E9BD73-9BDA-4D75-B897-332F9210B9B1}"),
                    ParentId = Guid.Parse("{A938446C-AB19-45C3-8DFD-0F24AB08DF49}"),
                    Name = @"Финансовые операции",
                    CalcType = TypeProfitAndLossCalc.IsProfit
                },
                new ProfitAndLossesMainRowViewModel
                {
                    Id = Guid.Parse("{459937DF-085F-4825-9AE9-810B054D0276}"),
                    ParentId = Guid.Parse("{F59D3232-DDF2-4F15-B7AE-E3691F385DDD}"),
                    Name = @"Финансовые операции",
                    CalcType = TypeProfitAndLossCalc.IsLoss
                },
                new ProfitAndLossesMainRowViewModel
                {
                    Id = Guid.Parse("{B6F2540A-9593-42E3-B34F-8C0983BC39A2}"),
                    ParentId = Guid.Parse("{A938446C-AB19-45C3-8DFD-0F24AB08DF49}"),
                    Name = @"Акты конвертации",
                    CalcType = TypeProfitAndLossCalc.IsProfit
                },
                new ProfitAndLossesMainRowViewModel
                {
                    Id = Guid.Parse("{35EBABEC-EAC3-4C3C-8383-6326C5D64C8C}"),
                    ParentId = Guid.Parse("{F59D3232-DDF2-4F15-B7AE-E3691F385DDD}"),
                    Name = @"Акты конвертации",
                    CalcType = TypeProfitAndLossCalc.IsLoss
                },
                new ProfitAndLossesMainRowViewModel
                {
                    Id = Guid.Parse("{297D2727-5161-48ED-969E-651811906526}"),
                    ParentId = Guid.Parse("{A938446C-AB19-45C3-8DFD-0F24AB08DF49}"),
                    Name = @"Операции с внебалансовыми контрагентами",
                    CalcType = TypeProfitAndLossCalc.IsProfit
                },
                new ProfitAndLossesMainRowViewModel
                {
                    Id = Guid.Parse("{E47B2338-C42F-4B2A-8865-1024FC84F020}"),
                    ParentId = Guid.Parse("{F59D3232-DDF2-4F15-B7AE-E3691F385DDD}"),
                    Name = @"Операции с внебалансовыми контрагентами",
                    CalcType = TypeProfitAndLossCalc.IsLoss
                },
                new ProfitAndLossesMainRowViewModel
                {
                    Id = Guid.Parse("{10AAC4B7-ECD1-4649-B372-256BA03C27FC}"),
                    ParentId = Guid.Parse("{A938446C-AB19-45C3-8DFD-0F24AB08DF49}"),
                    Name = @"Обмен валюты",
                    CalcType = TypeProfitAndLossCalc.IsProfit
                },
                new ProfitAndLossesMainRowViewModel
                {
                    Id = Guid.Parse("{04FB505A-651E-4F65-903D-80CB4B6C74D0}"),
                    ParentId = Guid.Parse("{F59D3232-DDF2-4F15-B7AE-E3691F385DDD}"),
                    Name = @"Обмен валюты",
                    CalcType = TypeProfitAndLossCalc.IsLoss
                },
                new ProfitAndLossesMainRowViewModel
                {
                    Id = Guid.Parse("{B96B2906-C5AA-4566-B77F-F3E4B912E72E}"),
                    ParentId = Guid.Parse("{F59D3232-DDF2-4F15-B7AE-E3691F385DDD}"),
                    Name = @"Заработная плата",
                    CalcType = TypeProfitAndLossCalc.IsLoss
                },
                new ProfitAndLossesMainRowViewModel
                {
                    Id = Guid.Parse("{836CC414-BEF4-4371-A253-47D2E8F4535F}"),
                    ParentId = Guid.Parse("{A938446C-AB19-45C3-8DFD-0F24AB08DF49}"),
                    Name = @"Проценты по кассовым ордерам",
                    CalcType = TypeProfitAndLossCalc.IsProfit
                },
                new ProfitAndLossesMainRowViewModel
                {
                    Id = Guid.Parse("{E9D63300-829B-4CB1-AA05-68EC3A73C459}"),
                    ParentId = Guid.Parse("{F59D3232-DDF2-4F15-B7AE-E3691F385DDD}"),
                    Name = @"Проценты по кассовым ордерам",
                    CalcType = TypeProfitAndLossCalc.IsLoss
                }
            };
            return res;
        }
    }
}