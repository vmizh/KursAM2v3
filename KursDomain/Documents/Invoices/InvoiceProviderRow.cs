using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Core.Helper;
using Core.ViewModel.Base;
using Data;
using DevExpress.Mvvm.DataAnnotations;
using KursDomain.ICommon;
using KursDomain.IDocuments.Finance;
using KursDomain.References;

// ReSharper disable InconsistentNaming
namespace KursDomain.Documents.Invoices;

/// <summary>
///     Счет-фактура поставщика - строка
/// </summary>
//[MetadataType(typeof(td_26LayoutData_FluentAPI))]
public class InvoiceProviderRow : RSViewModelBase, IEntity<TD_26>, IInvoiceProviderRow
{
    private TD_26 myEntity;
    private Kontragent myKontragentForNaklad;
    private Nomenkl myNomenkl;
    private Unit myPostUnit;
    private SDRSchet mySDRSchet;
    private Unit myUchUnit;
    private decimal myShipped;

    public InvoiceProviderRow()
    {
        Entity = DefaultValue();
    }

    // ReSharper disable once UnusedParameter.Local
    public InvoiceProviderRow(TD_26 entity, bool isLoadAll = true)
    {
        Entity = entity ?? DefaultValue();
        LoadReference();
    }

    public ObservableCollection<InvoiceProviderRowCurrencyConvertViewModel>
        CurrencyConvertRows { set; get; } = new ObservableCollection<InvoiceProviderRowCurrencyConvertViewModel>();

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
            Entity.SFT_AUTO_FLAG = (short)(value ? 1 : 0);
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

    public string GruzoDeclaration
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
                myKontragentForNaklad =
                    GlobalOptions.ReferencesCache.GetKontragent(Entity.SFT_NAKLAD_KONTR_DC) as Kontragent;
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

    public EntityLoadCodition LoadCondition { get; set; }

    public bool IsAccessRight { get; set; }

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

    public override string Note
    {
        get => Entity.SFT_TEXT;
        set
        {
            if (Entity.SFT_TEXT == value) return;
            Entity.SFT_TEXT = value;
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

    public Nomenkl Nomenkl
    {
        get => myNomenkl;
        set
        {
            if (Entity.SFT_NEMENKL_DC != default &&
                GlobalOptions.ReferencesCache.GetNomenkl(Entity.SFT_NEMENKL_DC) == value) return;
            myNomenkl = value;
            if (myNomenkl != null)
            {
                Entity.SFT_NEMENKL_DC = myNomenkl.DocCode;
                Entity.SFT_POST_ED_IZM_DC = ((IDocCode)myNomenkl.Unit).DocCode;
                Entity.SFT_UCHET_ED_IZM_DC = ((IDocCode)myNomenkl.Unit).DocCode;
            }

            RaisePropertyChanged();
            RaisePropertyChanged(nameof(IsNaklad));
            RaisePropertyChanged(nameof(IsUsluga));
            RaisePropertyChanged(nameof(Unit));
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
            {
                Entity.SFT_UCHET_ED_IZM_DC = myUchUnit.DocCode;
                Entity.SFT_POST_ED_IZM_DC = myUchUnit.DocCode;
            }

            RaisePropertyChanged();
        }
    }

    public decimal Price
    {
        get => Entity.SFT_ED_CENA ?? 0;
        set
        {
            if (value < 0)
                //WindowManager.ShowMessage("Цена должна быть больше нуля", "Ошибка",
                //    MessageBoxImage.Error);
                return;
            Entity.SFT_ED_CENA = value;
            Entity.SFT_ED_CENA_PRIHOD = value;
            CalcRow();
            if (CurrencyConvertRows != null && CurrencyConvertRows.Count > 0)
                foreach (var r in CurrencyConvertRows)
                {
                    r.Price = (decimal)Entity.SFT_ED_CENA;
                    r.PriceWithNaklad = (decimal)(Entity.SFT_ED_CENA + (SummaNaklad ?? 0) / Entity.SFT_KOL);
                    r.CalcRow();
                }

            RaisePropertyChanged();
        }
    }

    public decimal PriceWithNDS
    {
        get
        {
            if (!IsIncludeInPrice)
            {
                var n = Price * NDSPercent / 100;
                Entity.SFT_SUMMA_K_OPLATE = Quantity * (Price + n);
                return Price + n;
            }

            if (Quantity <= 0)
            {
                Entity.SFT_SUMMA_K_OPLATE = 0;
                return 0;
            }

            Entity.SFT_SUMMA_K_OPLATE = Quantity * Price;
            return Price;
        }
    }

    public decimal Quantity
    {
        get => Entity.SFT_KOL;
        set
        {
            if (value < 0)
                //WindowManager.ShowMessage("Количество должно быть больше нуля", "Ошибка",
                //    MessageBoxImage.Error);
                return;
            if (Entity.SFT_KOL == value) return;
            if (Parent is InvoiceProvider p && p.Facts.Count > 0)
            {
                var s = p.Facts.Where(_ => _.DDT_SPOST_DC == DocCode
                                           && _.DDT_SPOST_ROW_CODE == Code).Sum(_ => _.DDT_KOL_PRIHOD);
                if (value < s)
                    //WindowManager.ShowMessage($"Новая сумма {value} меньше отфактурированной {s}",
                    //    "Предупреждение", MessageBoxImage.Stop);
                    return;
            }

            Entity.SFT_KOL = value;
            Entity.SFT_POST_KOL = value;
            CalcRow();
            RaisePropertyChanged();
        }
    }

    public decimal NDSPercent
    {
        get => Entity.SFT_NDS_PERCENT;
        set
        {
            if (value < 0)
                //WindowManager.ShowMessage("НДС должен быть больше нуля", "Ошибка",
                //    MessageBoxImage.Error);
                return;
            if (Entity.SFT_NDS_PERCENT == value) return;
            Entity.SFT_NDS_PERCENT = value;
            CalcRow();
            RaisePropertyChanged();
        }
    }

    public decimal? SummaNaklad
    {
        get => Entity.SFT_SUMMA_NAKLAD;
        set
        {
            if (Entity.SFT_SUMMA_NAKLAD == value) return;
            Entity.SFT_SUMMA_NAKLAD = value;
            RaisePropertyChanged();
        }
    }

    public decimal? NDSSumma
    {
        get => Entity.SFT_SUMMA_NDS;
        set
        {
            if (Entity.SFT_SUMMA_NDS == value) return;
            Entity.SFT_SUMMA_NDS = value;
            RaisePropertyChanged();
        }
    }

    public decimal Summa
    {
        set
        {
            if (Entity.SFT_SUMMA_K_OPLATE == value) return;
            Entity.SFT_SUMMA_K_OPLATE = value;
            RaisePropertyChanged();
        }

        get => Entity.SFT_SUMMA_K_OPLATE ?? 0;
    }

    public decimal? SFT_SUMMA_K_OPLATE
    {
        get => Entity.SFT_SUMMA_K_OPLATE;
        set
        {
            Entity.SFT_SUMMA_K_OPLATE = value;
            Entity.SFT_SUMMA_K_OPLATE_KONTR_CRS = Entity.SFT_SUMMA_K_OPLATE;
            RaisePropertyChanged();
        }
    }

    public bool IsUsluga => Nomenkl?.IsUsluga ?? false;
    public bool IsNaklad => Nomenkl?.IsNakladExpense ?? false;

    public bool IsIncludeInPrice
    {
        get => SFT_VKLUCH_V_CENU == 1;
        set
        {
            if (Entity.SFT_VKLUCH_V_CENU == (value ? 1 : 0)) return;
            Entity.SFT_VKLUCH_V_CENU = (short)(value ? 1 : 0);
            RaisePropertyChanged(nameof(SFT_VKLUCH_V_CENU));
            RaisePropertyChanged();
        }
    }

    public Unit Unit
    {
        set { }
        get => Nomenkl?.Unit as Unit;
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
    public SDRSchet SDRSchet
    {
        get => mySDRSchet;
        set
        {
            if (mySDRSchet != null && mySDRSchet.Equals(value)) return;
            mySDRSchet = value;
            if (mySDRSchet != null)
                Entity.SFT_SHPZ_DC = mySDRSchet.DocCode;
            RaisePropertyChanged(nameof(Entity.SFT_SHPZ_DC));
            RaisePropertyChanged();
        }
    }

    public decimal Shipped
    {
        get => myShipped;
        set
        {
            if (myShipped == value) return;
            myShipped = value;
            RaisePropertyChanged();
        }
    }

    public Guid DocId
    {
        get => Entity.DocId ?? Guid.Empty;
        set
        {
            if (Entity.DocId == value) return;
            Entity.DocId = value;
            RaisePropertyChanged();
        }
    }

    public string NomenklNumber
    {
        set { }
        get => Nomenkl?.NomenklNumber;
    }

    public List<TD_26> LoadList()
    {
        throw new NotImplementedException();
    }

    public void LoadReference()
    {
        myNomenkl = GlobalOptions.ReferencesCache.GetNomenkl(Entity.SFT_NEMENKL_DC) as Nomenkl;
        myPostUnit = GlobalOptions.ReferencesCache.GetUnit(Entity.SFT_POST_ED_IZM_DC) as Unit;
        myUchUnit = GlobalOptions.ReferencesCache.GetUnit(Entity.SFT_UCHET_ED_IZM_DC) as Unit;
        mySDRSchet = GlobalOptions.ReferencesCache.GetSDRSchet(Entity.SFT_SHPZ_DC) as SDRSchet;
        myKontragentForNaklad = GlobalOptions.ReferencesCache.GetKontragent(Entity.SFT_NAKLAD_KONTR_DC) as Kontragent;
        if (Entity.TD_26_CurrencyConvert?.Count > 0)
            foreach (var d in Entity.TD_26_CurrencyConvert)
            {
                var newItem = new InvoiceProviderRowCurrencyConvertViewModel(d)
                {
                    OLdPrice = Entity.SFT_ED_CENA ?? 0,
                    OLdNakladPrice = (SummaNaklad ?? 0) != 0
                        ? Math.Round(Entity.SFT_ED_CENA ?? 0 +
                            // ReSharper disable once PossibleInvalidOperationException
                            (decimal)Entity.SFT_ED_CENA / (SummaNaklad ?? 0), 2)
                        : Math.Round(Entity.SFT_ED_CENA ?? 0, 2)
                };
                CurrencyConvertRows.Add(newItem);
            }

        try
        {
            Shipped = Entity.TD_24?.Sum(_ => _.DDT_KOL_PRIHOD) ?? 0 + (IsUsluga ? Quantity : 0);
        }
        catch { }

        RaisePropertyAllChanged();
    }

    public override string ToString()
    {
        return Entity.SD_26 != null
            ? $"С/ф постащика № {Entity.SD_26.SF_IN_NUM}/{Entity.SD_26.SF_POSTAV_NUM} " +
              $"от {Entity.SD_26.SF_POSTAV_DATE.ToShortDateString()}"
            : base.ToString();
    }

    public override object ToJson()
    {
        return new
        {
            DocCode,
            Code,
            Номенклатурный_номер = NomenklNumber,
            Номенклатура = Nomenkl.Name,
            Услуга = IsUsluga ? "Да" : "Нет",
            Количество = Quantity.ToString("n3"),
            Единица_измерения = Unit.Name,
            Цена = Price.ToString("n2"),
            Сумма = Summa.ToString("n2"),
            Сумма_накладных = SummaNaklad?.ToString("n2"),
            Процент_НДС = NDSPercent.ToString("n2"),
            НДС_в_цене = IsIncludeInPrice ? "Да" : "Нет",
            Примечание = Note
        };
    }

    public void CalcRow()
    {
        if (Parent is InvoiceProvider p)
        {
            if (p.IsNDSInPrice)
            {
                Entity.SFT_SUMMA_K_OPLATE = Entity.SFT_KOL * Entity.SFT_ED_CENA;
                Entity.SFT_SUMMA_K_OPLATE_KONTR_CRS = Entity.SFT_SUMMA_K_OPLATE;
                Entity.SFT_SUMMA_NDS = Entity.SFT_SUMMA_K_OPLATE * Entity.SFT_NDS_PERCENT /
                                       (Entity.SFT_NDS_PERCENT + 100);
            }
            else
            {
                Entity.SFT_SUMMA_NDS = Entity.SFT_KOL * Entity.SFT_ED_CENA * Entity.SFT_NDS_PERCENT / 100;
                Entity.SFT_SUMMA_K_OPLATE = Entity.SFT_KOL * Entity.SFT_ED_CENA + Entity.SFT_SUMMA_NDS;
                Entity.SFT_SUMMA_K_OPLATE_KONTR_CRS = Entity.SFT_SUMMA_K_OPLATE;
            }

            RaisePropertyChanged(nameof(NDSSumma));
            RaisePropertyChanged(nameof(Summa));
            RaisePropertyChanged(nameof(PriceWithNDS));
            p.RaisePropertyChanged("Summa");
        }
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
        builder.Property(_ => _.Quantity).AutoGenerated().DisplayName("Кол-во").DisplayFormatString("n1");
        builder.Property(_ => _.Price).AutoGenerated().DisplayName("Цена").DisplayFormatString("n2");
        builder.Property(_ => _.Summa).AutoGenerated().DisplayName("Сумма").DisplayFormatString("n2");
        builder.Property(_ => _.Note).AutoGenerated().DisplayName("Примечание");
        builder.Property(_ => _.NDSPercent).AutoGenerated().DisplayName("НДС %").DisplayFormatString("n2");
        builder.Property(_ => _.IsNaklad).AutoGenerated().DisplayName("Накладные").DisplayFormatString("n2");
        builder.Property(_ => _.IsUsluga).AutoGenerated().DisplayName("Услуга");
        builder.Property(_ => _.NomenklNumber).AutoGenerated().DisplayName("Ном.№").ReadOnly();
    }
}

public class td_26LayoutData_FluentAPI : DataAnnotationForFluentApiBase,
    IMetadataProvider<IInvoiceProviderRow>
{
    void IMetadataProvider<IInvoiceProviderRow>.BuildMetadata(
        MetadataBuilder<IInvoiceProviderRow> builder)
    {
        SetNotAutoGenerated(builder);
        builder.Property(_ => _.UchUnit).NotAutoGenerated();
        builder.Property(_ => _.PostUnit).NotAutoGenerated();
        builder.Property(_ => _.Unit).AutoGenerated().DisplayName("Ед.изм.");
        builder.Property(_ => _.Nomenkl).AutoGenerated().DisplayName("Наименование").ReadOnly();
        builder.Property(_ => _.Quantity).AutoGenerated().DisplayName("Кол-во").DisplayFormatString("n1");
        builder.Property(_ => _.Price).AutoGenerated().DisplayName("Цена").DisplayFormatString("n2");
        builder.Property(_ => _.Summa).AutoGenerated().DisplayName("Сумма").DisplayFormatString("n2").ReadOnly();
        builder.Property(_ => _.SummaNaklad).AutoGenerated().DisplayName("Сумма накл")
            .DisplayFormatString("n2").ReadOnly();
        builder.Property(_ => _.Note).AutoGenerated().DisplayName("Примечание");
        builder.Property(_ => _.NDSPercent).AutoGenerated().DisplayName("НДС %").DisplayFormatString("n2");
        builder.Property(_ => _.NDSSumma).AutoGenerated().DisplayName("НДС сумма").ReadOnly()
            .DisplayFormatString("n2");
        builder.Property(_ => _.IsIncludeInPrice).NotAutoGenerated()
            .DisplayName("НДС включен в цену");
        builder.Property(_ => _.SDRSchet).AutoGenerated().DisplayName("Счет дох./расх.");
        builder.Property(_ => _.IsNaklad).AutoGenerated().DisplayName("Накладные").DisplayFormatString("n2");
        builder.Property(_ => _.IsUsluga).AutoGenerated().DisplayName("Услуга");
        //builder.Property(_ => _.KontragentForNaklad).NotAutoGenerated().DisplayName("Контрагент(накладные)");
        builder.Property(_ => _.NomenklNumber).AutoGenerated().DisplayName("Ном.№").ReadOnly();
        //builder.Property(_ => _.GruzoDeclaration).AutoGenerated().DisplayName("Декларация(груз)");

        //builder.Property(_ => _.RUBRate).NotAutoGenerated().DisplayName("RUB курс").DisplayFormatString("n4");
        //builder.Property(_ => _.RUBSumma).NotAutoGenerated().DisplayName("RUB сумма").DisplayFormatString("n2");
        //builder.Property(_ => _.USDRate).NotAutoGenerated().DisplayName("USD курс").DisplayFormatString("n4");
        //builder.Property(_ => _.USDSumma).NotAutoGenerated().DisplayName("USD сумма").DisplayFormatString("n2");
        //builder.Property(_ => _.EURRate).NotAutoGenerated().DisplayName("EUR курс").DisplayFormatString("n4");
        //builder.Property(_ => _.EURSumma).NotAutoGenerated().DisplayName("EUR сумма").DisplayFormatString("n2");
        //builder.Property(_ => _.GBPRate).NotAutoGenerated().DisplayName("EUR курс").DisplayFormatString("n4");
        //builder.Property(_ => _.GBPSumma).NotAutoGenerated().DisplayName("EUR сумма").DisplayFormatString("n2");
    }
}
