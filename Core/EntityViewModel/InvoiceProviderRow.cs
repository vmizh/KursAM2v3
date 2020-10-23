using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using Core.Helper;
using Core.ViewModel.Base;
using Core.ViewModel.Common;
using Core.WindowsManager;
using Data;
using DevExpress.Mvvm.DataAnnotations;

// ReSharper disable InconsistentNaming
namespace Core.EntityViewModel
{
    /// <summary>
    ///     Счет-фактура поставщика - строка
    /// </summary>
    [MetadataType(typeof(td_26LayoutData_FluentAPI))]
    public class InvoiceProviderRow : RSViewModelBase, IEntity<TD_26>
    {
        private TD_26 myEntity;
        private Kontragent myKontragentForNaklad;
        private Nomenkl myNomenkl;
        private Unit myPostUnit;
        private SDRSchet mySDRSchet;
        private Unit myUchUnit;

        public InvoiceProviderRow()
        {
            Entity = DefaultValue();
        }

        public InvoiceProviderRow(TD_26 entity, bool isLoadAll = true)
        {
            Entity = entity ?? DefaultValue();
            LoadReference(isLoadAll);
        }

        public ObservableCollection<InvoiceProviderRowCurrencyConvertViewModel>
            CurrencyConvertRows { set; get; } = new ObservableCollection<InvoiceProviderRowCurrencyConvertViewModel>();

        public decimal DOC_CODE
        {
            get => Entity.DOC_CODE;
            set
            {
                if (Entity.DOC_CODE == value) return;
                Entity.DOC_CODE = value;
                RaisePropertyChanged();
            }
        }

        public override decimal DocCode
        {
            get => Entity.DOC_CODE;
            set
            {
                if (Entity.DOC_CODE == value) return;
                Entity.DOC_CODE = value;
                RaisePropertyChanged();
            }
        }

        public override int Code
        {
            get => Entity.CODE;
            set
            {
                if (Entity.CODE == value) return;
                Entity.CODE = value;
                RaisePropertyChanged();
            }
        }

        public override Guid Id
        {
            get => Entity.Id;
            set
            {
                if (Entity.Id == value) return;
                Entity.Id = value;
                RaisePropertyChanged();
            }
        }

        public string SFT_TEXT
        {
            get => Entity.SFT_TEXT;
            set
            {
                if (Entity.SFT_TEXT == value) return;
                Entity.SFT_TEXT = value;
                RaisePropertyChanged();
            }
        }

        public override string Note
        {
            get => SFT_TEXT;
            set
            {
                if (SFT_TEXT == value) return;
                SFT_TEXT = value;
                RaisePropertyChanged();
            }
        }

        public decimal SFT_POST_ED_IZM_DC
        {
            get => Entity.SFT_POST_ED_IZM_DC;
            set
            {
                if (Entity.SFT_POST_ED_IZM_DC == value) return;
                Entity.SFT_POST_ED_IZM_DC = value;
                if (MainReferences.Units.ContainsKey(Entity.SFT_POST_ED_IZM_DC))
                    myPostUnit = MainReferences.Units[Entity.SFT_POST_ED_IZM_DC];
                RaisePropertyChanged(nameof(PostUnit));
                RaisePropertyChanged();
            }
        }

        public Unit PostUnit
        {
            get => myPostUnit;
            set
            {
                if (myPostUnit != null && myPostUnit.Equals(value)) return;
                myPostUnit = value;
                if (myPostUnit != null)
                    Entity.SFT_POST_ED_IZM_DC = myPostUnit.DocCode;
                RaisePropertyChanged(nameof(SFT_POST_ED_IZM_DC));
                RaisePropertyChanged();
            }
        }

        public decimal? SFT_POST_ED_CENA
        {
            get => Entity.SFT_POST_ED_CENA;
            set
            {
                if (Entity.SFT_POST_ED_CENA == value) return;
                Entity.SFT_POST_ED_CENA = value;
                RaisePropertyChanged();
            }
        }

        public decimal SFT_POST_KOL
        {
            get => Entity.SFT_POST_KOL;
            set
            {
                if (Entity.SFT_POST_KOL == value) return;
                Entity.SFT_POST_KOL = value;
                RaisePropertyChanged();
            }
        }

        public decimal SFT_NEMENKL_DC
        {
            get => Entity.SFT_NEMENKL_DC;
            set
            {
                if (Entity.SFT_NEMENKL_DC == value) return;
                Entity.SFT_NEMENKL_DC = value;
                myNomenkl = MainReferences.GetNomenkl(Entity.SFT_NEMENKL_DC);
                RaisePropertyChanged(nameof(Nomenkl));
                RaisePropertyChanged();
            }
        }

        public Nomenkl Nomenkl
        {
            get => myNomenkl;
            set
            {
                if (myNomenkl != null && myNomenkl.Equals(value)) return;
                myNomenkl = value;
                if (myNomenkl != null)
                    Entity.SFT_NEMENKL_DC = myNomenkl.DOC_CODE;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(IsNaklad));
                RaisePropertyChanged(nameof(IsUsluga));
                RaisePropertyChanged(nameof(Unit));
            }
        }

        public decimal SFT_UCHET_ED_IZM_DC
        {
            get => Entity.SFT_UCHET_ED_IZM_DC;
            set
            {
                if (Entity.SFT_UCHET_ED_IZM_DC == value) return;
                Entity.SFT_UCHET_ED_IZM_DC = value;
                RaisePropertyChanged();
            }
        }

        public Unit UchUnit
        {
            get => myUchUnit;
            set
            {
                if (myUchUnit != null && myUchUnit.Equals(value)) return;
                myUchUnit = value;
                if (myUchUnit != null)
                    Entity.SFT_UCHET_ED_IZM_DC = myUchUnit.DocCode;
                RaisePropertyChanged(nameof(SFT_UCHET_ED_IZM_DC));
                RaisePropertyChanged();
            }
        }

        public decimal? SFT_ED_CENA
        {
            get => Entity.SFT_ED_CENA;
            set
            {
                if (Entity.SFT_ED_CENA == value) return;
                Entity.SFT_ED_CENA = value;
                Calc();
                if (CurrencyConvertRows != null && CurrencyConvertRows.Count > 0)
                    foreach (var r in CurrencyConvertRows)
                    {
                        r.Price = (decimal) SFT_ED_CENA;
                        r.PriceWithNaklad = (decimal) (SFT_ED_CENA + SFT_SUMMA_NAKLAD / SFT_KOL);
                        r.CalcRow();
                    }

                RaisePropertyChanged();
            }
        }

        public decimal SFT_KOL
        {
            get => Entity.SFT_KOL;
            set
            {
                if (Entity.SFT_KOL == value) return;
                if (Parent is InvoiceProvider p && p.Facts.Count > 0)
                {
                    var s = p.Facts.Where(_ => _.DDT_SPOST_DC == DOC_CODE
                                               && _.DDT_SPOST_ROW_CODE == Code).Sum(_ => _.DDT_KOL_PRIHOD);
                    if (value < s)
                    {
                        WindowManager.ShowMessage($"Новая сумма {value} меньше отфактурированной {s}",
                            "Предупреждение", MessageBoxImage.Stop);
                        return;
                    }
                }

                Entity.SFT_KOL = value;
                Calc();
                RaisePropertyChanged();
            }
        }

        public decimal? SFT_SUMMA_CBOROV
        {
            get => Entity.SFT_SUMMA_CBOROV;
            set
            {
                if (Entity.SFT_SUMMA_CBOROV == value) return;
                Entity.SFT_SUMMA_CBOROV = value;
                RaisePropertyChanged();
            }
        }

        public decimal SFT_NDS_PERCENT
        {
            get => Entity.SFT_NDS_PERCENT;
            set
            {
                if (Entity.SFT_NDS_PERCENT == value) return;
                Entity.SFT_NDS_PERCENT = value;
                Calc();
                RaisePropertyChanged();
            }
        }

        public decimal? SFT_SUMMA_NAKLAD
        {
            get => Entity.SFT_SUMMA_NAKLAD;
            set
            {
                if (Entity.SFT_SUMMA_NAKLAD == value) return;
                Entity.SFT_SUMMA_NAKLAD = value;
                RaisePropertyChanged();
            }
        }

        public decimal? SFT_SUMMA_NDS
        {
            get => Entity.SFT_SUMMA_NDS;
            set
            {
                if (Entity.SFT_SUMMA_NDS == value) return;
                Entity.SFT_SUMMA_NDS = value;
                RaisePropertyChanged();
            }
        }

        public decimal? SFT_SUMMA_K_OPLATE
        {
            get => Entity.SFT_SUMMA_K_OPLATE;
            set
            {
                if (Entity.SFT_SUMMA_K_OPLATE == value) return;
                Entity.SFT_SUMMA_K_OPLATE = value;
                Entity.SFT_SUMMA_K_OPLATE_KONTR_CRS = Entity.SFT_SUMMA_K_OPLATE;
                RaisePropertyChanged();
            }
        }

        public decimal? SFT_ED_CENA_PRIHOD
        {
            get => Entity.SFT_ED_CENA_PRIHOD;
            set
            {
                if (Entity.SFT_ED_CENA_PRIHOD == value) return;
                Entity.SFT_ED_CENA_PRIHOD = value;
                RaisePropertyChanged();
            }
        }

        public short SFT_IS_NAKLAD
        {
            get => Entity.SFT_IS_NAKLAD;
            set
            {
                if (Entity.SFT_IS_NAKLAD == value) return;
                Entity.SFT_IS_NAKLAD = value;
                RaisePropertyChanged(nameof(IsNaklad));
                RaisePropertyChanged();
            }
        }

        public bool IsUsluga => Nomenkl?.IsUsluga ?? false;
        public bool IsNaklad => Nomenkl?.IsNaklRashod ?? false;

        public short SFT_VKLUCH_V_CENU
        {
            get => Entity.SFT_VKLUCH_V_CENU;
            set
            {
                if (Entity.SFT_VKLUCH_V_CENU == value) return;
                Entity.SFT_VKLUCH_V_CENU = value;
                RaisePropertyChanged();
            }
        }

        public bool IsIncludeInPrice
        {
            get => SFT_VKLUCH_V_CENU == 1;
            set
            {
                if (Entity.SFT_VKLUCH_V_CENU == (value ? 1 : 0)) return;
                Entity.SFT_VKLUCH_V_CENU = (short) (value ? 1 : 0);
                Calc();
                RaisePropertyChanged(nameof(SFT_VKLUCH_V_CENU));
                RaisePropertyChanged();
            }
        }

        public short SFT_AUTO_FLAG
        {
            get => Entity.SFT_AUTO_FLAG;
            set
            {
                if (Entity.SFT_AUTO_FLAG == value) return;
                Entity.SFT_AUTO_FLAG = value;
                RaisePropertyChanged();
            }
        }

        public bool IsAutoFlag
        {
            get => SFT_AUTO_FLAG == 1;
            set
            {
                if (Entity.SFT_AUTO_FLAG == (value ? 1 : 0)) return;
                Entity.SFT_AUTO_FLAG = (short) (value ? 1 : 0);
                RaisePropertyChanged(nameof(SFT_AUTO_FLAG));
                RaisePropertyChanged();
            }
        }

        public decimal? SFT_STDP_DC
        {
            get => Entity.SFT_STDP_DC;
            set
            {
                if (Entity.SFT_STDP_DC == value) return;
                Entity.SFT_STDP_DC = value;
                RaisePropertyChanged();
            }
        }

        public Unit Unit => Nomenkl?.Unit;

        public decimal? SFT_NOM_CRS_DC
        {
            get => Entity.SFT_NOM_CRS_DC;
            set
            {
                if (Entity.SFT_NOM_CRS_DC == value) return;
                Entity.SFT_NOM_CRS_DC = value;
                RaisePropertyChanged();
            }
        }

        public decimal? SFT_NOM_CRS_RATE
        {
            get => Entity.SFT_NOM_CRS_RATE;
            set
            {
                if (Entity.SFT_NOM_CRS_RATE == value) return;
                Entity.SFT_NOM_CRS_RATE = value;
                RaisePropertyChanged();
            }
        }

        public decimal? SFT_NOM_CRS_CENA
        {
            get => Entity.SFT_NOM_CRS_CENA;
            set
            {
                if (Entity.SFT_NOM_CRS_CENA == value) return;
                Entity.SFT_NOM_CRS_CENA = value;
                RaisePropertyChanged();
            }
        }

        public decimal? SFT_CENA_V_UCHET_VALUTE
        {
            get => Entity.SFT_CENA_V_UCHET_VALUTE;
            set
            {
                if (Entity.SFT_CENA_V_UCHET_VALUTE == value) return;
                Entity.SFT_CENA_V_UCHET_VALUTE = value;
                RaisePropertyChanged();
            }
        }

        public decimal? SFT_SUMMA_V_UCHET_VALUTE
        {
            get => Entity.SFT_SUMMA_V_UCHET_VALUTE;
            set
            {
                if (Entity.SFT_SUMMA_V_UCHET_VALUTE == value) return;
                Entity.SFT_SUMMA_V_UCHET_VALUTE = value;
                RaisePropertyChanged();
            }
        }

        public decimal? SFT_DOG_POKUP_DC
        {
            get => Entity.SFT_DOG_POKUP_DC;
            set
            {
                if (Entity.SFT_DOG_POKUP_DC == value) return;
                Entity.SFT_DOG_POKUP_DC = value;
                RaisePropertyChanged();
            }
        }

        public int? SFT_DOG_POKUP_PLAN_ROW_CODE
        {
            get => Entity.SFT_DOG_POKUP_PLAN_ROW_CODE;
            set
            {
                if (Entity.SFT_DOG_POKUP_PLAN_ROW_CODE == value) return;
                Entity.SFT_DOG_POKUP_PLAN_ROW_CODE = value;
                RaisePropertyChanged();
            }
        }

        public decimal? SFT_SUMMA_K_OPLATE_KONTR_CRS
        {
            get => Entity.SFT_SUMMA_K_OPLATE_KONTR_CRS;
            set
            {
                if (Entity.SFT_SUMMA_K_OPLATE_KONTR_CRS == value) return;
                Entity.SFT_SUMMA_K_OPLATE_KONTR_CRS = value;
                RaisePropertyChanged();
            }
        }

        public decimal? SFT_SHPZ_DC
        {
            get => Entity.SFT_SHPZ_DC;
            set
            {
                if (Entity.SFT_SHPZ_DC == value) return;
                Entity.SFT_SHPZ_DC = value;
                if (Entity.SFT_SHPZ_DC != null && MainReferences.SDRSchets.ContainsKey(Entity.SFT_SHPZ_DC.Value))
                    mySDRSchet = MainReferences.SDRSchets[Entity.SFT_SHPZ_DC.Value];
                RaisePropertyChanged(nameof(SFT_SHPZ_DC.Value));
                RaisePropertyChanged();
            }
        }

        public SDRSchet SDRSchet
        {
            get => mySDRSchet;
            set
            {
                if (mySDRSchet != null && mySDRSchet.Equals(value)) return;
                mySDRSchet = value;
                if (mySDRSchet != null)
                    Entity.SFT_SHPZ_DC = mySDRSchet.DocCode;
                RaisePropertyChanged(nameof(SFT_SHPZ_DC));
                RaisePropertyChanged();
            }
        }

        public string SFT_STRANA_PROIS
        {
            get => Entity.SFT_STRANA_PROIS;
            set
            {
                if (Entity.SFT_STRANA_PROIS == value) return;
                Entity.SFT_STRANA_PROIS = value;
                RaisePropertyChanged();
            }
        }

        public string SFT_N_GRUZ_DECLAR
        {
            get => Entity.SFT_N_GRUZ_DECLAR;
            set
            {
                if (Entity.SFT_N_GRUZ_DECLAR == value) return;
                Entity.SFT_N_GRUZ_DECLAR = value;
                RaisePropertyChanged();
            }
        }

        public short? SFT_PEREVOZCHIK_POZITION
        {
            get => Entity.SFT_PEREVOZCHIK_POZITION;
            set
            {
                if (Entity.SFT_PEREVOZCHIK_POZITION == value) return;
                Entity.SFT_PEREVOZCHIK_POZITION = value;
                RaisePropertyChanged();
            }
        }

        public decimal? SFT_NAKLAD_KONTR_DC
        {
            get => Entity.SFT_NAKLAD_KONTR_DC;
            set
            {
                if (Entity.SFT_NAKLAD_KONTR_DC == value) return;
                Entity.SFT_NAKLAD_KONTR_DC = value;
                if (Entity.SFT_NAKLAD_KONTR_DC != null)
                    myKontragentForNaklad = MainReferences.GetKontragent(Entity.SFT_NAKLAD_KONTR_DC);
                RaisePropertyChanged(nameof(KontragentForNaklad));
                RaisePropertyChanged();
            }
        }

        public Kontragent KontragentForNaklad
        {
            get => myKontragentForNaklad;
            set
            {
                if (myKontragentForNaklad != null && myKontragentForNaklad.Equals(value)) return;
                myKontragentForNaklad = value;
                if (myKontragentForNaklad != null)
                    Entity.SFT_NAKLAD_KONTR_DC = myKontragentForNaklad.DocCode;
                RaisePropertyChanged(nameof(SFT_NAKLAD_KONTR_DC));
                RaisePropertyChanged();
            }
        }

        public decimal? SFT_SALE_PRICE_IN_UCH_VAL
        {
            get => Entity.SFT_SALE_PRICE_IN_UCH_VAL;
            set
            {
                if (Entity.SFT_SALE_PRICE_IN_UCH_VAL == value) return;
                Entity.SFT_SALE_PRICE_IN_UCH_VAL = value;
                RaisePropertyChanged();
            }
        }

        public decimal? RUBRate
        {
            get => Entity.RUBRate ?? 1;
            set
            {
                if (Entity.RUBRate == value) return;
                Entity.RUBRate = value;
                RaisePropertyChanged();
            }
        }

        public decimal? RUBSumma
        {
            get => Entity.RUBSumma ?? 0;
            set
            {
                if (Entity.RUBSumma == value) return;
                Entity.RUBSumma = value;
                RaisePropertyChanged();
            }
        }

        public decimal? USDRate
        {
            get => Entity.USDRate ?? 1;
            set
            {
                if (Entity.USDRate == value) return;
                Entity.USDRate = value;
                RaisePropertyChanged();
            }
        }

        public decimal? USDSumma
        {
            get => Entity.USDSumma ?? 0;
            set
            {
                if (Entity.USDSumma == value) return;
                Entity.USDSumma = value;
                RaisePropertyChanged();
            }
        }

        public decimal? EURRate
        {
            get => Entity.EURRate ?? 1;
            set
            {
                if (Entity.EURRate == value) return;
                Entity.EURRate = value;
                RaisePropertyChanged();
            }
        }

        public decimal? EURSumma
        {
            get => Entity.EURSumma ?? 0;
            set
            {
                if (Entity.EURSumma == value) return;
                Entity.EURSumma = value;
                RaisePropertyChanged();
            }
        }


        public decimal? GBPRate
        {
            get => Entity.GBPRate ?? 1;
            set
            {
                if (Entity.GBPRate == value) return;
                Entity.GBPRate = value;
                RaisePropertyChanged();
            }
        }

        public decimal? GBPSumma
        {
            get => Entity.GBPSumma ?? 0;
            set
            {
                if (Entity.GBPSumma == value) return;
                Entity.GBPSumma = value;
                RaisePropertyChanged();
            }
        }

        public decimal? SFT_PERCENT
        {
            get => Entity.SFT_PERCENT;
            set
            {
                if (Entity.SFT_PERCENT == value) return;
                Entity.SFT_PERCENT = value;
                RaisePropertyChanged();
            }
        }

        public byte[] TSTAMP
        {
            get => Entity.TSTAMP;
            set
            {
                if (Entity.TSTAMP == value) return;
                Entity.TSTAMP = value;
                RaisePropertyChanged();
            }
        }

        public Guid? DocId
        {
            get => Entity.DocId;
            set
            {
                if (Entity.DocId == value) return;
                Entity.DocId = value;
                RaisePropertyChanged();
            }
        }

        public Guid? SchetRowNakladRashodId
        {
            get => Entity.SchetRowNakladRashodId;
            set
            {
                if (Entity.SchetRowNakladRashodId == value) return;
                Entity.SchetRowNakladRashodId = value;
                RaisePropertyChanged();
            }
        }

        public decimal? SchetRowNakladSumma
        {
            get => Entity.SchetRowNakladSumma;
            set
            {
                if (Entity.SchetRowNakladSumma == value) return;
                Entity.SchetRowNakladSumma = value;
                RaisePropertyChanged();
            }
        }

        public decimal? SchetRowNakladRate
        {
            get => Entity.SchetRowNakladRate;
            set
            {
                if (Entity.SchetRowNakladRate == value) return;
                Entity.SchetRowNakladRate = value;
                RaisePropertyChanged();
            }
        }

        public SD_165 SD_165
        {
            get => Entity.SD_165;
            set
            {
                if (Entity.SD_165 == value) return;
                Entity.SD_165 = value;
                RaisePropertyChanged();
            }
        }

        public SD_175 SD_175
        {
            get => Entity.SD_175;
            set
            {
                if (Entity.SD_175 == value) return;
                Entity.SD_175 = value;
                RaisePropertyChanged();
            }
        }

        public SD_175 SD_1751
        {
            get => Entity.SD_1751;
            set
            {
                if (Entity.SD_1751 == value) return;
                Entity.SD_1751 = value;
                RaisePropertyChanged();
            }
        }

        public SD_26 SD_26
        {
            get => Entity.SD_26;
            set
            {
                if (Entity.SD_26 == value) return;
                Entity.SD_26 = value;
                RaisePropertyChanged();
            }
        }

        public SD_26 SD_261
        {
            get => Entity.SD_261;
            set
            {
                if (Entity.SD_261 == value) return;
                Entity.SD_261 = value;
                RaisePropertyChanged();
            }
        }

        public SD_301 SD_301
        {
            get => Entity.SD_301;
            set
            {
                if (Entity.SD_301 == value) return;
                Entity.SD_301 = value;
                RaisePropertyChanged();
            }
        }

        public SD_303 SD_303
        {
            get => Entity.SD_303;
            set
            {
                if (Entity.SD_303 == value) return;
                Entity.SD_303 = value;
                RaisePropertyChanged();
            }
        }

        public SD_43 SD_43
        {
            get => Entity.SD_43;
            set
            {
                if (Entity.SD_43 == value) return;
                Entity.SD_43 = value;
                RaisePropertyChanged();
            }
        }

        public SD_83 SD_83
        {
            get => Entity.SD_83;
            set
            {
                if (Entity.SD_83 == value) return;
                Entity.SD_83 = value;
                RaisePropertyChanged();
            }
        }

        public UD_112 UD_112
        {
            get => Entity.UD_112;
            set
            {
                if (Entity.UD_112 == value) return;
                Entity.UD_112 = value;
                RaisePropertyChanged();
            }
        }

        public TD_26 Entity
        {
            get => myEntity;
            set
            {
                if (myEntity == value) return;
                myEntity = value;
                RaisePropertyChanged();
            }
        }

        public EntityLoadCodition LoadCondition { get; set; }
        public string NomenklNumber => Nomenkl?.NomenklNumber;

        public List<TD_26> LoadList()
        {
            throw new NotImplementedException();
        }

        public bool IsAccessRight { get; set; }

        [SuppressMessage("ReSharper", "PossibleInvalidOperationException")]
        public void Calc()
        {
            if ((SFT_ED_CENA ?? 0) == 0 || SFT_KOL == 0)
            {
                Entity.SFT_SUMMA_K_OPLATE = 0;
                Entity.SFT_SUMMA_NDS = 0;

            }
            else
            {
                if (IsIncludeInPrice)
                {
                    Entity.SFT_SUMMA_K_OPLATE = Math.Round((decimal) (SFT_ED_CENA * SFT_KOL), 2);
                    Entity.SFT_SUMMA_NDS = Math.Round((decimal) (SFT_SUMMA_K_OPLATE / (1 + SFT_NDS_PERCENT)), 2);
                }
                else
                {
                    Entity.SFT_SUMMA_K_OPLATE =
                        Math.Round((decimal) (SFT_ED_CENA * SFT_KOL * (1 + SFT_NDS_PERCENT / 100)), 2);
                    Entity.SFT_SUMMA_NDS = Math.Round((decimal) (SFT_ED_CENA * SFT_KOL * SFT_NDS_PERCENT / 100), 2);
                }
            }

            if (Parent is InvoiceProvider p)
            {
                // ReSharper disable once PossibleInvalidOperationException
                p.SF_CRS_SUMMA = (decimal) p.Rows.Sum(_ => _.SFT_SUMMA_K_OPLATE);
            }

            Entity.EURSumma = Entity.SFT_SUMMA_K_OPLATE * Entity.EURRate;
            Entity.USDSumma = Entity.SFT_SUMMA_K_OPLATE * Entity.USDRate;
            Entity.GBPSumma = Entity.SFT_SUMMA_K_OPLATE * Entity.GBPRate;
            Entity.RUBSumma = Entity.SFT_SUMMA_K_OPLATE * Entity.RUBRate;
            RaisePropertyChanged(nameof(EURSumma));
            RaisePropertyChanged(nameof(USDSumma));
            RaisePropertyChanged(nameof(GBPSumma));
            RaisePropertyChanged(nameof(RUBSumma));
            RaisePropertyChanged(nameof(SFT_SUMMA_K_OPLATE));
            RaisePropertyChanged(nameof(SFT_SUMMA_NDS));
        }

        private void LoadReference(bool isLoadAll)
        {
            myNomenkl = MainReferences.GetNomenkl(Entity.SFT_NEMENKL_DC);
            RaisePropertyChanged(nameof(Nomenkl));
            if (MainReferences.Units.ContainsKey(Entity.SFT_POST_ED_IZM_DC))
            {
                myPostUnit = MainReferences.Units[Entity.SFT_POST_ED_IZM_DC];
                RaisePropertyChanged(nameof(PostUnit));
            }

            if (MainReferences.Units.ContainsKey(Entity.SFT_UCHET_ED_IZM_DC))
            {
                myUchUnit = MainReferences.Units[Entity.SFT_UCHET_ED_IZM_DC];
                RaisePropertyChanged(nameof(UchUnit));
            }

            if (Entity.SFT_SHPZ_DC != null && MainReferences.SDRSchets.ContainsKey(Entity.SFT_SHPZ_DC.Value))
            {
                mySDRSchet = MainReferences.SDRSchets[Entity.SFT_SHPZ_DC.Value];
                RaisePropertyChanged(nameof(SDRSchet));
            }

            if (Entity.SFT_NAKLAD_KONTR_DC != null)
            {
                myKontragentForNaklad = MainReferences.GetKontragent(Entity.SFT_NAKLAD_KONTR_DC);
                RaisePropertyChanged(nameof(KontragentForNaklad));
            }
            if (Entity.TD_26_CurrencyConvert?.Count > 0)
                foreach (var d in Entity.TD_26_CurrencyConvert)
                {
                    var newItem = new InvoiceProviderRowCurrencyConvertViewModel(d);
                    newItem.OLdPrice = (decimal) SFT_ED_CENA;
                    newItem.OLdNakladPrice = (SFT_SUMMA_NAKLAD ?? 0) != 0
                        ? Math.Round((decimal) SFT_ED_CENA +
                                     // ReSharper disable once PossibleInvalidOperationException
                                     (decimal) SFT_ED_CENA / (SFT_SUMMA_NAKLAD ?? 0), 2)
                        : Math.Round((decimal) SFT_ED_CENA, 2);
                    CurrencyConvertRows.Add(newItem);
                }
        }

        public virtual void Save(TD_26 doc)
        {
            throw new NotImplementedException();
        }

        public void Save()
        {
            throw new NotImplementedException();
        }

        public void Delete()
        {
            throw new NotImplementedException();
        }

        public void Delete(Guid id)
        {
            throw new NotImplementedException();
        }

        public void Delete(decimal dc)
        {
            throw new NotImplementedException();
        }

        public void UpdateFrom(TD_26 ent)
        {
            Code = ent.CODE;
            SFT_TEXT = ent.SFT_TEXT;
            SFT_POST_ED_IZM_DC = ent.SFT_POST_ED_IZM_DC;
            SFT_POST_ED_CENA = ent.SFT_POST_ED_CENA;
            SFT_POST_KOL = ent.SFT_POST_KOL;
            SFT_NEMENKL_DC = ent.SFT_NEMENKL_DC;
            SFT_UCHET_ED_IZM_DC = ent.SFT_UCHET_ED_IZM_DC;
            SFT_ED_CENA = ent.SFT_ED_CENA;
            SFT_KOL = ent.SFT_KOL;
            SFT_SUMMA_CBOROV = ent.SFT_SUMMA_CBOROV;
            SFT_NDS_PERCENT = ent.SFT_NDS_PERCENT;
            SFT_SUMMA_NAKLAD = ent.SFT_SUMMA_NAKLAD;
            SFT_SUMMA_NDS = ent.SFT_SUMMA_NDS;
            SFT_SUMMA_K_OPLATE = ent.SFT_SUMMA_K_OPLATE;
            SFT_ED_CENA_PRIHOD = ent.SFT_ED_CENA_PRIHOD;
            SFT_IS_NAKLAD = ent.SFT_IS_NAKLAD;
            SFT_VKLUCH_V_CENU = ent.SFT_VKLUCH_V_CENU;
            SFT_AUTO_FLAG = ent.SFT_AUTO_FLAG;
            SFT_STDP_DC = ent.SFT_STDP_DC;
            SFT_NOM_CRS_DC = ent.SFT_NOM_CRS_DC;
            SFT_NOM_CRS_RATE = ent.SFT_NOM_CRS_RATE;
            SFT_NOM_CRS_CENA = ent.SFT_NOM_CRS_CENA;
            SFT_CENA_V_UCHET_VALUTE = ent.SFT_CENA_V_UCHET_VALUTE;
            SFT_SUMMA_V_UCHET_VALUTE = ent.SFT_SUMMA_V_UCHET_VALUTE;
            SFT_DOG_POKUP_DC = ent.SFT_DOG_POKUP_DC;
            SFT_DOG_POKUP_PLAN_ROW_CODE = ent.SFT_DOG_POKUP_PLAN_ROW_CODE;
            SFT_SUMMA_K_OPLATE_KONTR_CRS = ent.SFT_SUMMA_K_OPLATE_KONTR_CRS;
            SFT_SHPZ_DC = ent.SFT_SHPZ_DC;
            SFT_STRANA_PROIS = ent.SFT_STRANA_PROIS;
            SFT_N_GRUZ_DECLAR = ent.SFT_N_GRUZ_DECLAR;
            SFT_PEREVOZCHIK_POZITION = ent.SFT_PEREVOZCHIK_POZITION;
            SFT_NAKLAD_KONTR_DC = ent.SFT_NAKLAD_KONTR_DC;
            SFT_SALE_PRICE_IN_UCH_VAL = ent.SFT_SALE_PRICE_IN_UCH_VAL;
            SFT_PERCENT = ent.SFT_PERCENT;
            TSTAMP = ent.TSTAMP;
            Id = ent.Id;
            DocId = ent.DocId;
            SchetRowNakladRashodId = ent.SchetRowNakladRashodId;
            SchetRowNakladSumma = ent.SchetRowNakladSumma;
            SchetRowNakladRate = ent.SchetRowNakladRate;
            SD_165 = ent.SD_165;
            SD_175 = ent.SD_175;
            SD_1751 = ent.SD_1751;
            SD_26 = ent.SD_26;
            SD_261 = ent.SD_261;
            SD_301 = ent.SD_301;
            SD_303 = ent.SD_303;
            SD_43 = ent.SD_43;
            SD_83 = ent.SD_83;
        }

        public void UpdateTo(TD_26 ent)
        {
            ent.CODE = Code;
            ent.SFT_TEXT = SFT_TEXT;
            ent.SFT_POST_ED_IZM_DC = SFT_POST_ED_IZM_DC;
            ent.SFT_POST_ED_CENA = SFT_POST_ED_CENA;
            ent.SFT_POST_KOL = SFT_POST_KOL;
            ent.SFT_NEMENKL_DC = SFT_NEMENKL_DC;
            ent.SFT_UCHET_ED_IZM_DC = SFT_UCHET_ED_IZM_DC;
            ent.SFT_ED_CENA = SFT_ED_CENA;
            ent.SFT_KOL = SFT_KOL;
            ent.SFT_SUMMA_CBOROV = SFT_SUMMA_CBOROV;
            ent.SFT_NDS_PERCENT = SFT_NDS_PERCENT;
            ent.SFT_SUMMA_NAKLAD = SFT_SUMMA_NAKLAD;
            ent.SFT_SUMMA_NDS = SFT_SUMMA_NDS;
            ent.SFT_SUMMA_K_OPLATE = SFT_SUMMA_K_OPLATE;
            ent.SFT_ED_CENA_PRIHOD = SFT_ED_CENA_PRIHOD;
            ent.SFT_IS_NAKLAD = SFT_IS_NAKLAD;
            ent.SFT_VKLUCH_V_CENU = SFT_VKLUCH_V_CENU;
            ent.SFT_AUTO_FLAG = SFT_AUTO_FLAG;
            ent.SFT_STDP_DC = SFT_STDP_DC;
            ent.SFT_NOM_CRS_DC = SFT_NOM_CRS_DC;
            ent.SFT_NOM_CRS_RATE = SFT_NOM_CRS_RATE;
            ent.SFT_NOM_CRS_CENA = SFT_NOM_CRS_CENA;
            ent.SFT_CENA_V_UCHET_VALUTE = SFT_CENA_V_UCHET_VALUTE;
            ent.SFT_SUMMA_V_UCHET_VALUTE = SFT_SUMMA_V_UCHET_VALUTE;
            ent.SFT_DOG_POKUP_DC = SFT_DOG_POKUP_DC;
            ent.SFT_DOG_POKUP_PLAN_ROW_CODE = SFT_DOG_POKUP_PLAN_ROW_CODE;
            ent.SFT_SUMMA_K_OPLATE_KONTR_CRS = SFT_SUMMA_K_OPLATE_KONTR_CRS;
            ent.SFT_SHPZ_DC = SFT_SHPZ_DC;
            ent.SFT_STRANA_PROIS = SFT_STRANA_PROIS;
            ent.SFT_N_GRUZ_DECLAR = SFT_N_GRUZ_DECLAR;
            ent.SFT_PEREVOZCHIK_POZITION = SFT_PEREVOZCHIK_POZITION;
            ent.SFT_NAKLAD_KONTR_DC = SFT_NAKLAD_KONTR_DC;
            ent.SFT_SALE_PRICE_IN_UCH_VAL = SFT_SALE_PRICE_IN_UCH_VAL;
            ent.SFT_PERCENT = SFT_PERCENT;
            ent.TSTAMP = TSTAMP;
            ent.Id = Id;
            ent.DocId = DocId;
            ent.SchetRowNakladRashodId = SchetRowNakladRashodId;
            ent.SchetRowNakladSumma = SchetRowNakladSumma;
            ent.SchetRowNakladRate = SchetRowNakladRate;
            ent.SD_165 = SD_165;
            ent.SD_175 = SD_175;
            ent.SD_1751 = SD_1751;
            ent.SD_26 = SD_26;
            ent.SD_261 = SD_261;
            ent.SD_301 = SD_301;
            ent.SD_303 = SD_303;
            ent.SD_43 = SD_43;
            ent.SD_83 = SD_83;
        }

        public override string ToString()
        {
            return Entity.SD_26 != null
                ? $"С/ф постащика № {Entity.SD_26.SF_IN_NUM}/{Entity.SD_26.SF_POSTAV_NUM} " +
                  $"от {Entity.SD_26.SF_POSTAV_DATE.ToShortDateString()}"
                : base.ToString();
        }

        public TD_26 DefaultValue()
        {
            return new TD_26
            {
                DOC_CODE = -1,
                CODE = -1,
                Id = Guid.NewGuid(),
                DocId = Guid.Empty
            };
        }

        public virtual TD_26 Load(decimal dc)
        {
            throw new NotImplementedException();
        }

        public virtual TD_26 Load(Guid id)
        {
            throw new NotImplementedException();
        }

        // ReSharper disable once MethodOverloadWithOptionalParameter
        public TD_26 Load(decimal dc, bool isShort = true)
        {
            throw new NotImplementedException();
        }

        // ReSharper disable once MethodOverloadWithOptionalParameter
        public TD_26 Load(Guid id, bool isShort = true)
        {
            throw new NotImplementedException();
        }
    }

    [MetadataType(typeof(DataAnnotationsInvoiceProviderRowShort))]
    public class InvoiceProviderRowShort : InvoiceProviderRow
    {
        // ReSharper disable once RedundantBaseConstructorCall
        public InvoiceProviderRowShort() : base()
        {
        }

        public InvoiceProviderRowShort(TD_26 entity) : base(entity)
        {
        }
    }

    public class DataAnnotationsInvoiceProviderRowShort : DataAnnotationForFluentApiBase,
        IMetadataProvider<InvoiceProviderRowShort>
    {
        void IMetadataProvider<InvoiceProviderRowShort>.BuildMetadata(
            MetadataBuilder<InvoiceProviderRowShort> builder)
        {
            SetNotAutoGenerated(builder);
            builder.Property(_ => _.UchUnit).NotAutoGenerated();
            builder.Property(_ => _.PostUnit).NotAutoGenerated();
            builder.Property(_ => _.Unit).AutoGenerated().DisplayName("Ед.изм.");
            builder.Property(_ => _.Nomenkl).AutoGenerated().DisplayName("Наименование").ReadOnly();
            builder.Property(_ => _.SFT_KOL).AutoGenerated().DisplayName("Кол-во").DisplayFormatString("n4");
            builder.Property(_ => _.SFT_ED_CENA).AutoGenerated().DisplayName("Цена").DisplayFormatString("n2");
            builder.Property(_ => _.SFT_SUMMA_K_OPLATE).AutoGenerated().DisplayName("Сумма").DisplayFormatString("n2");
            builder.Property(_ => _.Note).AutoGenerated().DisplayName("Примечание");
            builder.Property(_ => _.SFT_NDS_PERCENT).AutoGenerated().DisplayName("НДС %").DisplayFormatString("n2");
            builder.Property(_ => _.IsNaklad).AutoGenerated().DisplayName("Накладные").DisplayFormatString("n2");
            builder.Property(_ => _.IsUsluga).AutoGenerated().DisplayName("Услуга");
            builder.Property(_ => _.NomenklNumber).AutoGenerated().DisplayName("Ном.№").ReadOnly();
        }
    }


    public class td_26LayoutData_FluentAPI : DataAnnotationForFluentApiBase,
        IMetadataProvider<InvoiceProviderRow>
    {
        void IMetadataProvider<InvoiceProviderRow>.BuildMetadata(
            MetadataBuilder<InvoiceProviderRow> builder)
        {
            SetNotAutoGenerated(builder);
            builder.Property(_ => _.UchUnit).NotAutoGenerated();
            builder.Property(_ => _.PostUnit).NotAutoGenerated();
            builder.Property(_ => _.Unit).AutoGenerated().DisplayName("Ед.изм.");
            builder.Property(_ => _.Nomenkl).AutoGenerated().DisplayName("Наименование").ReadOnly();
            builder.Property(_ => _.SFT_KOL).AutoGenerated().DisplayName("Кол-во").DisplayFormatString("n4");
            builder.Property(_ => _.SFT_ED_CENA).AutoGenerated().DisplayName("Цена").DisplayFormatString("n2");
            builder.Property(_ => _.SFT_SUMMA_K_OPLATE).AutoGenerated().DisplayName("Сумма").DisplayFormatString("n2");
            builder.Property(_ => _.SFT_SUMMA_NAKLAD).AutoGenerated().DisplayName("Сумма накл")
                .DisplayFormatString("n2").ReadOnly();
            builder.Property(_ => _.Note).AutoGenerated().DisplayName("Примечание");
            builder.Property(_ => _.SFT_NDS_PERCENT).AutoGenerated().DisplayName("НДС %").DisplayFormatString("n2");
            builder.Property(_ => _.SFT_SUMMA_NDS).AutoGenerated().DisplayName("НДС сумма").ReadOnly()
                .DisplayFormatString("n2");
            builder.Property(_ => _.IsIncludeInPrice).AutoGenerated()
                .DisplayName("НДС включен в цену");
            builder.Property(_ => _.SDRSchet).AutoGenerated().DisplayName("Счет дох./расх.");
            builder.Property(_ => _.IsNaklad).AutoGenerated().DisplayName("Накладные").DisplayFormatString("n2");
            builder.Property(_ => _.IsUsluga).AutoGenerated().DisplayName("Услуга");
            builder.Property(_ => _.KontragentForNaklad).NotAutoGenerated().DisplayName("Контрагент(накладные)");
            builder.Property(_ => _.NomenklNumber).AutoGenerated().DisplayName("Ном.№").ReadOnly();

            builder.Property(_ => _.RUBRate).NotAutoGenerated().DisplayName("RUB курс").DisplayFormatString("n4");
            builder.Property(_ => _.RUBSumma).NotAutoGenerated().DisplayName("RUB сумма").DisplayFormatString("n2");
            builder.Property(_ => _.USDRate).NotAutoGenerated().DisplayName("USD курс").DisplayFormatString("n4");
            builder.Property(_ => _.USDSumma).NotAutoGenerated().DisplayName("USD сумма").DisplayFormatString("n2");
            builder.Property(_ => _.EURRate).NotAutoGenerated().DisplayName("EUR курс").DisplayFormatString("n4");
            builder.Property(_ => _.EURSumma).NotAutoGenerated().DisplayName("EUR сумма").DisplayFormatString("n2");
            builder.Property(_ => _.GBPRate).NotAutoGenerated().DisplayName("EUR курс").DisplayFormatString("n4");
            builder.Property(_ => _.GBPSumma).NotAutoGenerated().DisplayName("EUR сумма").DisplayFormatString("n2");
        }
    }
}