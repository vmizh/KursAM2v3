using System;
using System.Collections.Generic;
using System.Linq;
using Core.Helper;
using Core.ViewModel.Base;
using Data;
using DevExpress.Mvvm.DataAnnotations;
using KursDomain.IDocuments.Finance;
using KursDomain.References;

// ReSharper disable InconsistentNaming
namespace KursDomain.Documents.Invoices;

//[MetadataType(typeof(DataAnnotationsSFClientRowViewModel))]
public class InvoiceClientRowViewModel : RSViewModelBase, IEntity<TD_84>, IInvoiceClientRow
{
    private decimal myCurrentRemains;
    private TD_84 myEntity;

    // ReSharper disable once RedundantDefaultMemberInitializer
    public bool myIsNDSInPrice = true;
    private decimal myRest;
    private decimal myShipped;

    public InvoiceClientRowViewModel(bool isNDSInPrice = true)
    {
        Entity = DefaultValue();
        IsNDSInPrice = isNDSInPrice;
    }

    public InvoiceClientRowViewModel(TD_84 entity, bool isNDSInPrice = true)
    {
        if (entity == null)
        {
            Entity = DefaultValue();
        }
        else
        {
            Entity = entity;
            LoadReference();
            CalcRow();
        }

        IsNDSInPrice = isNDSInPrice;
    }

    public bool IsNDSInPrice
    {
        get => ((InvoiceClientViewModel)Parent)?.IsNDSIncludeInPrice ?? myIsNDSInPrice;
        set
        {
            if (GetValue(value)) return;
            myIsNDSInPrice = value;
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(Entity.SFT_ED_CENA));
        }
    }


    public decimal? SFT_ACCIZ
    {
        get => Entity.SFT_ACCIZ;
        set
        {
            if (Entity.SFT_ACCIZ == value) return;
            Entity.SFT_ACCIZ = value;
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

    public decimal? SFT_UCHET_ED_IZM_DC
    {
        get => Entity.SFT_UCHET_ED_IZM_DC;
        set
        {
            if (Entity.SFT_UCHET_ED_IZM_DC == value) return;
            Entity.SFT_UCHET_ED_IZM_DC = value;
            RaisePropertyChanged();
        }
    }

    public short? SFT_KOMPLEKT
    {
        get => Entity.SFT_KOMPLEKT;
        set
        {
            if (Entity.SFT_KOMPLEKT == value) return;
            Entity.SFT_KOMPLEKT = value;
            RaisePropertyChanged();
        }
    }

    public decimal? SFT_NALOG_NA_PROD
    {
        get => Entity.SFT_NALOG_NA_PROD;
        set
        {
            if (Entity.SFT_NALOG_NA_PROD == value) return;
            Entity.SFT_NALOG_NA_PROD = value;
            RaisePropertyChanged();
        }
    }

    public decimal? SFT_DOG_OTGR_DC
    {
        get => Entity.SFT_DOG_OTGR_DC;
        set
        {
            if (Entity.SFT_DOG_OTGR_DC == value) return;
            Entity.SFT_DOG_OTGR_DC = value;
            RaisePropertyChanged();
        }
    }

    public int? SFT_DOG_OTGR_PLAN_CODE
    {
        get => Entity.SFT_DOG_OTGR_PLAN_CODE;
        set
        {
            if (Entity.SFT_DOG_OTGR_PLAN_CODE == value) return;
            Entity.SFT_DOG_OTGR_PLAN_CODE = value;
            RaisePropertyChanged();
        }
    }

    public double? SFT_PROCENT_ZS_RASHODOV
    {
        get => Entity.SFT_PROCENT_ZS_RASHODOV;
        set
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (Entity.SFT_PROCENT_ZS_RASHODOV == value) return;
            Entity.SFT_PROCENT_ZS_RASHODOV = value;
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

    public double? SFT_TARA_MEST
    {
        get => Entity.SFT_TARA_MEST;
        set
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (Entity.SFT_TARA_MEST == value) return;
            Entity.SFT_TARA_MEST = value;
            RaisePropertyChanged();
        }
    }

    public decimal? SFT_TARA_DC
    {
        get => Entity.SFT_TARA_DC;
        set
        {
            if (Entity.SFT_TARA_DC == value) return;
            Entity.SFT_TARA_DC = value;
            RaisePropertyChanged();
        }
    }

    public short? SFT_TARA_FLAG
    {
        get => Entity.SFT_TARA_FLAG;
        set
        {
            if (Entity.SFT_TARA_FLAG == value) return;
            Entity.SFT_TARA_FLAG = value;
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

    public string SFT_COUNTRY_CODE
    {
        get => Entity.SFT_COUNTRY_CODE;
        set
        {
            if (Entity.SFT_COUNTRY_CODE == value) return;
            Entity.SFT_COUNTRY_CODE = value;
            RaisePropertyChanged();
        }
    }

    public string OLD_NOM_NOMENKL
    {
        get => Entity.OLD_NOM_NOMENKL;
        set
        {
            if (Entity.OLD_NOM_NOMENKL == value) return;
            Entity.OLD_NOM_NOMENKL = value;
            RaisePropertyChanged();
        }
    }

    public string OLD_NOM_NAME
    {
        get => Entity.OLD_NOM_NAME;
        set
        {
            if (Entity.OLD_NOM_NAME == value) return;
            Entity.OLD_NOM_NAME = value;
            RaisePropertyChanged();
        }
    }

    public string OLD_OVERHEAD_NAME
    {
        get => Entity.OLD_OVERHEAD_NAME;
        set
        {
            if (Entity.OLD_OVERHEAD_NAME == value) return;
            Entity.OLD_OVERHEAD_NAME = value;
            RaisePropertyChanged();
        }
    }

    public string OLD_OVERHEAD_CRS_NAME
    {
        get => Entity.OLD_OVERHEAD_CRS_NAME;
        set
        {
            if (Entity.OLD_OVERHEAD_CRS_NAME == value) return;
            Entity.OLD_OVERHEAD_CRS_NAME = value;
            RaisePropertyChanged();
        }
    }

    public string OLD_UNIT_NAME
    {
        get => Entity.OLD_UNIT_NAME;
        set
        {
            if (Entity.OLD_UNIT_NAME == value) return;
            Entity.OLD_UNIT_NAME = value;
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

    public SD_83 SD_831
    {
        get => Entity.SD_831;
        set
        {
            if (Entity.SD_831 == value) return;
            Entity.SD_831 = value;
            RaisePropertyChanged();
        }
    }

    public SD_84 SD_84
    {
        get => Entity.SD_84;
        set
        {
            if (Entity.SD_84 == value) return;
            Entity.SD_84 = value;
            RaisePropertyChanged();
        }
    }

    public VD_9 VD_9
    {
        get => Entity.VD_9;
        set
        {
            if (Entity.VD_9 == value) return;
            Entity.VD_9 = value;
            RaisePropertyChanged();
        }
    }

    public COUNTRY COUNTRY
    {
        get => Entity.COUNTRY;
        set
        {
            if (Entity.COUNTRY == value) return;
            Entity.COUNTRY = value;
            RaisePropertyChanged();
        }
    }

    public string NomenklNumber => Nomenkl?.NomenklNumber;

    public EntityLoadCodition LoadCondition { get; set; }

    public bool IsAccessRight { get; set; }

    public TD_84 Entity
    {
        get => myEntity;
        set
        {
            if (myEntity == value) return;
            myEntity = value;
            RaisePropertyChanged();
        }
    }

    public TD_84 DefaultValue()
    {
        return new TD_84
        {
            Id = Guid.NewGuid(),
            DOC_CODE = -1,
            CODE = -1
        };
    }

    public decimal PriceWithNDS
    {
        get
        {
            if (!IsNDSInPrice)
            {
                var n = Price * NDSPercent / 100;
                Entity.SFT_SUMMA_K_OPLATE = Quantity * (Price + n);
                Entity.SFT_SUMMA_K_OPLATE_KONTR_CRS = Entity.SFT_SUMMA_K_OPLATE;
                return Price + n;
            }

            if (Quantity <= 0)
            {
                Entity.SFT_SUMMA_K_OPLATE = 0;
                Entity.SFT_SUMMA_K_OPLATE_KONTR_CRS = Entity.SFT_SUMMA_K_OPLATE;
                return 0;
            }

            Entity.SFT_SUMMA_K_OPLATE = Quantity * Price;
            Entity.SFT_SUMMA_K_OPLATE_KONTR_CRS = Entity.SFT_SUMMA_K_OPLATE;
            return Price;
        }
    }

    public string NomNomenkl
    {
        set { }
        get => Nomenkl?.NomenklNumber;
    }

    public Unit Unit
    {
        get => Nomenkl?.Unit as Unit;
        set { }
    }

    public Guid DocId
    {
        get => Entity.DocId;
        set
        {
            if (Entity.DocId == value) return;
            Entity.DocId = value;
            RaisePropertyChanged();
        }
    }

    public override Guid Id
    {
        set
        {
            if (Entity.Id == value) return;
            Entity.Id = value;
            RaisePropertyChanged();
        }
        get => Entity.Id;
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

    public Nomenkl Nomenkl
    {
        get => GlobalOptions.ReferencesCache.GetNomenkl(Entity.SFT_NEMENKL_DC) as Nomenkl;
        set
        {
            if (Equals(GlobalOptions.ReferencesCache.GetNomenkl(Entity.SFT_NEMENKL_DC), value)) return;
            Entity.SFT_NEMENKL_DC = value.DocCode;
            RaisePropertyChanged();
        }
    }

    public decimal Quantity
    {
        get => (decimal)Entity.SFT_KOL;
        set
        {
            if (value <= (decimal)0.01 || value < Shipped)
            {
                //WindowManager.ShowMessage("Кол-во должно быть больше нуля", "Ошибка",
                //    MessageBoxImage.Error);
                Entity.SFT_KOL = 1;
                CalcRow();
                RaisePropertyChanged();
                return;
            }

            //if (Math.Abs(Entity.SFT_KOL - (double)value) < 0.00001) return;
            //if ()
            //    //WindowManager.ShowMessage($"Отгружено {Shipped}. Уменьшить кол-во в счете нельзя", "Ошибка",
            //    //    MessageBoxImage.Error);
            //    return;

            Entity.SFT_KOL = (double)value;
            CalcRow();
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

            if (Entity.SFT_ED_CENA == value) return;
            Entity.SFT_ED_CENA = value;
            CalcRow();
            RaisePropertyChanged();
        }
    }

    public decimal NDSPercent
    {
        get => (decimal)Entity.SFT_NDS_PERCENT;
        set
        {
            if (value < 0)
                //WindowManager.ShowMessage("НДС должен быть больше нуля", "Ошибка",
                //    MessageBoxImage.Error);
                return;

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (Entity.SFT_NDS_PERCENT == (double)value) return;
            Entity.SFT_NDS_PERCENT = (double)value;
            CalcRow();
            RaisePropertyChanged();
        }
    }

    public decimal? SFT_SUMMA_NDS => Entity.SFT_SUMMA_NDS;

    public decimal Summa
    {
        set { }
        get => Entity.SFT_SUMMA_K_OPLATE ?? 0;
    }

    public decimal? SFT_NACENKA_DILERA
    {
        get => Entity.SFT_NACENKA_DILERA;
        set
        {
            if (Entity.SFT_NACENKA_DILERA == value) return;
            Entity.SFT_NACENKA_DILERA = value;
            if (Parent is InvoiceClientViewModel doc)
                // ReSharper disable once PossibleInvalidOperationException
                doc.SF_DILER_SUMMA = (decimal?)doc.Rows.Cast<InvoiceClientRowViewModel>()
                    .Sum(_ => _.Entity.SFT_KOL * (double)(_.SFT_NACENKA_DILERA ?? 0));
            RaisePropertyChanged();
        }
    }

    public SDRSchet SDRSchet
    {
        get => GlobalOptions.ReferencesCache.GetSDRSchet(Entity.SFT_SHPZ_DC) as SDRSchet;
        set
        {
            if (Equals(GlobalOptions.ReferencesCache.GetSDRSchet(Entity.SFT_SHPZ_DC), value)) return;
            Entity.SFT_SHPZ_DC = value?.DocCode;
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

    public bool IsUsluga
    {
        set { }
        get => Nomenkl?.IsUsluga ?? false;
    }

    /// <summary>
    ///     Отгружено
    /// </summary>
    public decimal Shipped
    {
        get => myShipped;
        set
        {
            if (myShipped == value) return;
            myShipped = value;
            myRest = IsUsluga ? 0 : (decimal)(Entity.SFT_KOL - (double)myShipped);
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(Rest));
        }
    }

    /// <summary>
    ///     остаток для отгрузки по счету тест !!! 123
    /// </summary>
    public decimal Rest
    {
        get => myRest;
        set
        {
            if (myRest == value) return;
            myRest = value;
            RaisePropertyChanged();
        }
    }

    public decimal CurrentRemains
    {
        get => myCurrentRemains;
        set
        {
            if (myCurrentRemains == value) return;
            myCurrentRemains = value;
            RaisePropertyChanged();
        }
    }

    private bool GetValue(bool value)
    {
        if (myIsNDSInPrice == value) return true;
        return false;
    }

    public List<TD_84> LoadList()
    {
        throw new NotImplementedException();
    }

    public void CalcRow()
    {
        if (IsNDSInPrice)
        {   var s = (decimal)Entity.SFT_KOL * Entity.SFT_ED_CENA ?? 0;
            Entity.SFT_SUMMA_NDS = Math.Round(s - s * 100 / (100 + (decimal)Entity.SFT_NDS_PERCENT), 2);
        }
        else
        {
            Entity.SFT_SUMMA_NDS =
                (decimal)Math.Round(
                    Entity.SFT_KOL * (double)(Entity.SFT_ED_CENA ?? 0) * Entity.SFT_NDS_PERCENT / 100, 2);
            Entity.SFT_SUMMA_K_OPLATE =
                Math.Round((decimal)(Entity.SFT_KOL * (double)(Entity.SFT_ED_CENA ?? 0)), 2)
                + Entity.SFT_SUMMA_NDS;
            Entity.SFT_SUMMA_K_OPLATE_KONTR_CRS = Entity.SFT_SUMMA_K_OPLATE;
        }

        if (Parent is InvoiceClientViewModel p)
        {
            foreach (var f in p.ShipmentRows)
            {
                var r = p.Rows.First(_ => _.Nomenkl.DocCode == f.Nomenkl.DocCode);
                p.SummaOtgruz += Math.Round(r.Summa * r.Quantity / f.DDT_KOL_RASHOD, 2);
            }
        }

        Shipped = IsUsluga ? Quantity : Entity.TD_24.Sum(_ => _.DDT_KOL_RASHOD);
        RaisePropertyChanged(nameof(SFT_SUMMA_NDS));
        RaisePropertyChanged(nameof(Summa));
    }

    private void LoadReference()
    {
        Nomenkl = GlobalOptions.ReferencesCache.GetNomenkl(Entity.SFT_NEMENKL_DC) as Nomenkl;
        if (Entity.SFT_SHPZ_DC != null)
            SDRSchet = GlobalOptions.ReferencesCache.GetSDRSchet(Entity.SFT_SHPZ_DC.Value) as SDRSchet;
    }

    public virtual void Save(TD_84 doc)
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

    public void UpdateFrom(TD_84 ent)
    {
    }

    public void UpdateTo(TD_84 ent)
    {
    }

    public virtual TD_84 Load(decimal dc)
    {
        throw new NotImplementedException();
    }

    public virtual TD_84 Load(Guid id)
    {
        throw new NotImplementedException();
    }

    public override object ToJson()
    {
        return new
        {
            DocCode,
            Code,
            Номенклатурный_Номер = NomNomenkl,
            Номенклатура = Nomenkl.Name,
            Услуга = IsUsluga ? "Да" : "Нет",
            Количество = Quantity.ToString("n3"),
            Цена = Price.ToString("n2"),
            Сумма = Summa.ToString("n2"),
            Наценка_Дилера = SFT_NACENKA_DILERA?.ToString("n2"),
            Отгружено = Shipped.ToString("n3"),
            Процент_НДС = NDSPercent.ToString("n2"),
            Сумма_НДС = SFT_SUMMA_NDS?.ToString("n2"),
            Примечание = Note
        };
    }
}

public class DataAnnotationsSFClientRowViewModel : DataAnnotationForFluentApiBase,
    IMetadataProvider<InvoiceClientRowViewModel>
{
    void IMetadataProvider<InvoiceClientRowViewModel>.BuildMetadata(MetadataBuilder<InvoiceClientRowViewModel> builder)
    {
        SetNotAutoGenerated(builder);
        builder.Property(_ => _.Nomenkl).AutoGenerated().DisplayName("Наименование");
        builder.Property(_ => _.NomNomenkl).AutoGenerated().ReadOnly().DisplayName("Ном.№");
        builder.Property(_ => _.IsUsluga).AutoGenerated().DisplayName("Услуга").ReadOnly();
        builder.Property(_ => _.Quantity).AutoGenerated().DisplayName("Кол-во").DisplayFormatString("n2");
        builder.Property(_ => _.Price).AutoGenerated().DisplayName("Цена").DisplayFormatString("n2");
        builder.Property(_ => _.Summa).AutoGenerated().DisplayName("Сумма").DisplayFormatString("n2").ReadOnly();
        builder.Property(_ => _.SFT_NACENKA_DILERA).AutoGenerated().DisplayName("Наценка дилера на единицу")
            .DisplayFormatString("n2");
        builder.Property(_ => _.Shipped).AutoGenerated().DisplayName("Отгружено").DisplayFormatString("n2").ReadOnly();
        builder.Property(_ => _.Rest).AutoGenerated().DisplayName("Остаток").DisplayFormatString("n2").ReadOnly();
        builder.Property(_ => _.CurrentRemains).AutoGenerated().DisplayName("Текущие остатки")
            .DisplayFormatString("n4").ReadOnly();
        builder.Property(_ => _.Note).AutoGenerated().DisplayName("Примечание");
        builder.Property(_ => _.NDSPercent).AutoGenerated().DisplayName("НДС %").DisplayFormatString("n2");
        builder.Property(_ => _.SFT_SUMMA_NDS).AutoGenerated().DisplayName("Сумма НДС").ReadOnly()
            .DisplayFormatString("n2");
        builder.Property(_ => _.SDRSchet).AutoGenerated().DisplayName("Счет дох./расх.");
        builder.Property(_ => _.GruzoDeclaration).AutoGenerated().DisplayName("Декларация(груз)");
    }
}
