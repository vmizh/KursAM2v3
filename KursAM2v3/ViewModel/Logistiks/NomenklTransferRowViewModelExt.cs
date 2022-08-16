using System;
using System.Collections.Generic;
using System.Windows;
using Core;
using Core.EntityViewModel.CommonReferences;
using Core.EntityViewModel.NomenklManagement;
using Core.Invoices.EntityViewModel;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using JetBrains.Annotations;
using static System.Math;

namespace KursAM2.ViewModel.Logistiks
{
    public class NomenklTransferRowViewModelExt : NomenklTransferRowViewModel
    {
        public bool IsCalcConrol = true;
        private bool myIsPriceAccepted;
        private List<Nomenkl> myLinkedNomenkls;
        private decimal myMaxQuantity;
        private decimal myNakladEdSumma;
        private decimal myNakladNewEdSumma;
        private decimal myNakladRate;
        private Nomenkl myNomenklIn;
        private Nomenkl myNomenklOut;

        /// <summary>
        ///     занесеная руками
        /// </summary>
        private decimal myOldPrice;

        private decimal myPriceIn;
        private decimal myPriceOut;
        private string mySchetInfo;
        private decimal mySummaIn;
        private decimal mySummaOut;
        private Core.EntityViewModel.NomenklManagement.Warehouse myWarehouse;

        public NomenklTransferRowViewModelExt(NomenklTransferRow entity) : base(entity)
        {
            myNomenklIn = MainReferences.GetNomenkl(Entity.NomenklInDC);
            myNomenklOut = MainReferences.GetNomenkl(Entity.NomenklOutDC);
        }

        public NomenklTransferRowViewModelExt()
        {
        }

        public bool IsPriceAccepted
        {
            get => myIsPriceAccepted;
            set
            {
                if (myIsPriceAccepted == value) return;
                myIsPriceAccepted = value;
                RaisePropertyChanged();
            }
        }

        public Core.EntityViewModel.NomenklManagement.Warehouse Warehouse
        {
            get => myWarehouse;
            set
            {
                if (myWarehouse != null && myWarehouse.Equals(value)) return;
                myWarehouse = value;
                if (myWarehouse != null)
                    Entity.StoreDC = myWarehouse.DocCode;
                RaisePropertyChanged();
            }
        }

        public string SchetInfo
        {
            get => mySchetInfo;
            set
            {
                if (mySchetInfo == value) return;
                mySchetInfo = value;
                RaisePropertyChanged();
            }
        }

        public Nomenkl NomenklOut
        {
            get => myNomenklOut;
            set
            {
                if (myNomenklOut != null && myNomenklOut.Equals(value)) return;
                myNomenklOut = value;
                if (myNomenklOut != null)
                    Entity.NomenklOutDC = myNomenklOut.DocCode;
                RaisePropertyChanged();
            }
        }

        public Nomenkl NomenklIn
        {
            get => myNomenklIn;
            set
            {
                if (myNomenklIn != null && myNomenklIn.Equals(value)) return;
                myNomenklIn = value;
                if (myNomenklIn != null)
                    Entity.NomenklInDC = myNomenklIn.DocCode;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(NomenklNumberIn));
                RaisePropertyChanged(nameof(CurrencyIn));
            }
        }

        public decimal MaxQuantity
        {
            get => myMaxQuantity;
            set
            {
                if (myMaxQuantity == value) return;
                myMaxQuantity = value;
                RaisePropertyChanged();
            }
        }

        public List<Nomenkl> LinkedNomenkls
        {
            get => myLinkedNomenkls;
            set
            {
                myLinkedNomenkls = value;
                RaisePropertyChanged();
            }
        }

        public decimal SummaOut
        {
            get => Round(mySummaOut, 2);
            set
            {
                if (mySummaOut == value) return;
                mySummaOut = value;
                RaisePropertyChanged();
            }
        }

        public decimal SummaIn
        {
            get => Round(mySummaIn, 2);
            set
            {
                if (mySummaIn == value) return;
                mySummaIn = value;
                if (IsCalcConrol && !IsAccepted)
                {
                    myPriceIn = mySummaIn / Quantity;
                    Entity.Rate = mySummaIn / SummaOut;
                    RaisePropertyChanged(nameof(Rate));
                    RaisePropertyChanged(nameof(PriceIn));
                }

                RaisePropertyChanged();
            }
        }

        /* <dxg:GridColumn x:Name="col16" Header="Накладные расходы (ед)" FieldName="NakladEdSumma" ReadOnly="True">
                            <dxg:GridColumn.EditSettings>
                                <dxe:CalcEditSettings DisplayFormat="n2"
                                                      MaskUseAsDisplayFormat="True"
                                                      AllowDefaultButton="False" />
                            </dxg:GridColumn.EditSettings>
                        </dxg:GridColumn>
                        <dxg:GridColumn x:Name="col17" Header="Курс (накл)" FieldName="NakladRate">
                            <dxg:GridColumn.EditSettings>
                                <dxe:CalcEditSettings DisplayFormat="n4"
                                                      MaskUseAsDisplayFormat="True"
                                                      AllowDefaultButton="False" />
                            </dxg:GridColumn.EditSettings>
                        </dxg:GridColumn>
                        <dxg:GridColumn x:Name="col18" Header="Накладные расходы (новые)" FieldName="NakladNewEdSumma" >*/
        public decimal OldPrice
        {
            get => myOldPrice;
            set
            {
                if (myOldPrice == value) return;
                myOldPrice = value;
                RaisePropertyChanged();
            }
        }

        public decimal NakladNewEdSumma
        {
            get => myNakladNewEdSumma;
            set
            {
                if (myNakladNewEdSumma == value) return;
                myNakladNewEdSumma = value;
                myNakladRate = Round(NakladEdSumma != 0 ? myNakladNewEdSumma / NakladEdSumma : 0, 4);
                RaisePropertyChanged();
            }
        }

        public new decimal NakladRate
        {
            get => myNakladRate;
            set
            {
                if (myNakladRate == value) return;
                myNakladRate = value;
                myNakladNewEdSumma = Round(NakladEdSumma * NakladRate, 2);
                RaisePropertyChanged(nameof(NakladNewEdSumma));
                RaisePropertyChanged();
            }
        }

        public new decimal NakladEdSumma
        {
            get => myNakladEdSumma;
            set
            {
                if (myNakladEdSumma == value) return;
                myNakladEdSumma = value;
                myNakladNewEdSumma = Round(NakladEdSumma * NakladRate, 2);
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(NakladNewEdSumma));
            }
        }

        /// <summary>
        ///     расчетная
        /// </summary>
        public new decimal PriceOut
        {
            get => Round(myPriceOut, 2);
            set
            {
                if (myPriceOut == value) return;
                myPriceOut = value;
                if (IsCalcConrol && !IsAccepted)
                {
                    myPriceIn = PriceOut * Rate;
                    mySummaIn = Round(myPriceOut * Quantity * Rate, 2);
                    mySummaOut = Round(myPriceOut * Quantity, 2);
                    RaisePropertyChanged(nameof(PriceIn));
                    RaisePropertyChanged(nameof(SummaIn));
                    RaisePropertyChanged(nameof(PriceOut));
                    RaisePropertyChanged(nameof(SummaOut));
                }

                RaisePropertyChanged();
            }
        }

        public new decimal PriceIn
        {
            get => Round(myPriceIn, 2);
            set
            {
                if (myPriceIn == value) return;
                myPriceIn = value;
                if (IsCalcConrol && !IsAccepted)
                {
                    Entity.Rate = Round(PriceIn / PriceOut, 2);
                    mySummaIn = Round(PriceIn * Quantity, 2);
                    RaisePropertyChanged(nameof(SummaIn));
                    RaisePropertyChanged(nameof(Rate));
                }

                RaisePropertyChanged();
            }
        }

        public new decimal Quantity
        {
            get => Entity.Quantity;
            set
            {
                if (Entity.Quantity == value) return;
                if (IsCalcConrol)
                    if (value > MaxQuantity)
                    {
                        WindowManager.ShowMessage(null, $"Кол-во не может превосходить максимальное {MaxQuantity}",
                            "Ошибка", MessageBoxImage.Error);
                        return;
                    }

                Entity.Quantity = value;
                if (IsCalcConrol && !IsAccepted)
                {
                    mySummaIn = Round(Quantity * PriceIn * Rate, 2);
                    mySummaOut = Round(Quantity * PriceOut, 2);
                    RaisePropertyChanged(nameof(SummaIn));
                    RaisePropertyChanged(nameof(SummaOut));
                }

                RaisePropertyChanged();
            }
        }

        public new decimal Rate
        {
            get => Round(Entity.Rate, 4);
            set
            {
                if (Entity.Rate == value) return;
                Entity.Rate = value;
                if (IsCalcConrol && !IsAccepted)
                {
                    myPriceIn = PriceOut * Rate;
                    mySummaIn = Round(myPriceOut * Quantity * Rate, 2);
                    RaisePropertyChanged(nameof(PriceOut));
                    RaisePropertyChanged(nameof(SummaOut));
                }

                if (NakladRate == 0)
                    NakladRate = Entity.Rate;
                RaisePropertyChanged();
            }
        }

        public Currency CurrencyOut => NomenklOut?.Currency;
        public Currency CurrencyIn => NomenklIn?.Currency;
        public string NomenklNumberOut => NomenklOut?.NomenklNumber;
        public string NomenklNumberIn => NomenklIn?.NomenklNumber;

        public static NomenklTransferRowViewModelExt New([NotNull] NomenklTransferViewModelExt parent)
        {
            var ret = New(parent.Id);
            ret.Parent = parent;
            return ret;
        }

        public static NomenklTransferRowViewModelExt New(Guid parentId)
        {
            return new NomenklTransferRowViewModelExt
            {
                Id = Guid.NewGuid(),
                DocId = parentId,
                State = RowStatus.NewRow
            };
        }

        public NomenklTransferRowViewModelExt Copy()
        {
            return new NomenklTransferRowViewModelExt
            {
                Id = Guid.NewGuid(),
                DocId = Guid.Empty,
                State = RowStatus.NewRow,
                NomenklIn = NomenklIn,
                NomenklOut = NomenklOut,
                Rate = Rate,
                MaxQuantity = MaxQuantity - Quantity,
                IsAccepted = false,
                Quantity = MaxQuantity - Quantity,
                Note = Note
            };
        }
    }
}
