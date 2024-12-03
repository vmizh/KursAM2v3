using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Core.Helper;
using Core.ViewModel.Base;
using Data;
using DevExpress.Mvvm.DataAnnotations;
using KursDomain.Documents.CommonReferences.Kontragent;
using KursDomain.ICommon;
using KursDomain.IReferences;
using KursDomain.IReferences.Kontragent;
using KursDomain.References.RedisCache;
using Newtonsoft.Json;

namespace KursDomain.References;

[DebuggerDisplay("{DocCode,nq} {Name,nq} {Currency,nq}")]
public class Kontragent : IKontragent, IDocCode, IDocGuid, IName, IEquatable<Kontragent>, IComparable, ICache
{
    public Kontragent()
    {
        DocCode = -1;
        
    }

    
    public int CompareTo(object obj)
    {
        var c = obj as Unit;
        return c == null ? 0 : string.Compare(Name, c.Name, StringComparison.Ordinal);
    }

    [Display(AutoGenerateField = false, Name = "DocCode")]
    public decimal DocCode { get; set; }

    [Display(AutoGenerateField = false, Name = "Id")]
    public Guid Id { get; set; }

    public bool Equals(Kontragent other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return DocCode == other.DocCode;
    }

    [Display(AutoGenerateField = true, Name = "Коротокие имя")]
    public string ShortName { get; set; }

    [Display(AutoGenerateField = true, Name = "Полное имя")]
    public string FullName { get; set; }

    public decimal? GroupDC { get; set; }

    [Display(AutoGenerateField = false, Name = "Группа")]
    [JsonIgnore]
    public IKontragentGroup Group { get; set; }

    [Display(AutoGenerateField = true, Name = "ИНН")]
    public string INN { get; set; }

    [Display(AutoGenerateField = true, Name = "КПП")]
    public string KPP { get; set; }

    [Display(AutoGenerateField = true, Name = "Руководитель")]
    public string Director { get; set; }

    [Display(AutoGenerateField = true, Name = "Гл.бух.")]
    public string GlavBuh { get; set; }

    [Display(AutoGenerateField = true, Name = "Удален")]
    public bool IsDeleted { get; set; }

    [Display(AutoGenerateField = true, Name = "Адрес")]
    public string Address { get; set; }

    [Display(AutoGenerateField = true, Name = "Телефон")]
    public string Phone { get; set; }

    [Display(AutoGenerateField = true, Name = "ОКПО")]
    public string OKPO { get; set; }

    [Display(AutoGenerateField = true, Name = "ОКОНХ")]
    public string OKONH { get; set; }

    [Display(AutoGenerateField = true, Name = "Физ.лицо")]
    public bool IsPersona { get; set; }

    [Display(AutoGenerateField = true, Name = "Паспорт")]
    public string Passport { get; set; }

    [Display(AutoGenerateField = true, Name = "Работник")]
    [JsonIgnore]
    public IEmployee Employee { get; set; }

    public decimal? ClientCategoryDC { get; set; }
    [Display(AutoGenerateField = true, Name = "Категогрия")]
    [JsonIgnore]
    public IClientCategory ClientCategory { get; set; }

    [Display(AutoGenerateField = true, Name = "Вкл.в баланс")]
    public bool IsBalans { get; set; }

    public decimal? CurrencyDC { set; get; }

    [Display(AutoGenerateField = true, Name = "Валюта", Order = 1)]
    [JsonIgnore]
    public ICurrency Currency { get; set; }

    [Display(AutoGenerateField = true, Name = "Нач.Сумма")]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal StartSumma { get; set; }

    [Display(AutoGenerateField = true, Name = "Нач.учета")]
    public DateTime StartBalans { get; set; }

    [Display(AutoGenerateField = true, Name = "EMail")]
    public string EMail { get; set; }

    public decimal? ResponsibleEmployeeDC { get; set; }

    [Display(AutoGenerateField = true, Name = "Ответственный")]
    [JsonIgnore]
    public IEmployee ResponsibleEmployee { get; set; }

    public decimal? RegionDC { get; set; }

    [Display(AutoGenerateField = true, Name = "Регион")]
    [JsonIgnore]
    public IRegion Region { get; set; }

    [Display(AutoGenerateField = false, Name = "")]
    public int OrderCount { get; set; }

    [Display(AutoGenerateField = true, Name = "Наименование", Order = 0)]
    public string Name { get; set; }

    [Display(AutoGenerateField = true, Name = "Примечание")]
    public string Notes { get; set; }

    [JsonIgnore]
    [Display(AutoGenerateField = false, Name = "Описание")]
    public string Description => $"Контрагент: {Name}({Currency})";

    public bool Equals(IDocCode x, IDocCode y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (ReferenceEquals(x, null)) return false;
        if (ReferenceEquals(y, null)) return false;
        if (x.GetType() != y.GetType()) return false;
        return x.DocCode == y.DocCode;
    }

    public int GetHashCode(IDocCode obj)
    {
        return obj.DocCode.GetHashCode();
    }

    public override string ToString()
    {
        return Name;
    }

    //TODO Подключить все свойства
    public void LoadFromEntity(SD_43 entity, IReferencesCache refCache)
    {
        if (entity == null) return;
        DocCode = entity.DOC_CODE;
        Id = entity.Id;
        FullName = entity.NAME_FULL;
        StartBalans = entity.START_BALANS ?? new DateTime(2000, 1, 1);
        Name = entity.NAME;
        Notes = entity.NOTES;
        IsBalans = entity.FLAG_BALANS == 1;
        IsDeleted = entity.DELETED == 1;
        Id = entity.Id;
        ResponsibleEmployee = refCache.GetEmployee(entity.OTVETSTV_LICO);
        Currency = refCache.GetCurrency(entity.VALUTA_DC);
        Group = refCache.GetKontragentGroup(entity.EG_ID);
        UpdateDate = entity.UpdateDate ?? DateTime.Now;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Kontragent)obj);
    }

    public override int GetHashCode()
    {
        return DocCode.GetHashCode();
    }

    public void LoadFromCache()
    {
        if (GlobalOptions.ReferencesCache is not RedisCacheReferences cache) return;
        if (ClientCategoryDC is not null)
            ClientCategory = cache.GetClientCategory(ClientCategoryDC.Value);
        if (ResponsibleEmployeeDC is not null)
            ResponsibleEmployee = cache.GetEmployee(ResponsibleEmployeeDC.Value);
        if (CurrencyDC is not null)
            Currency = cache.GetCurrency(CurrencyDC.Value);
        if (RegionDC is not null)
            Region = cache.GetRegion(RegionDC.Value);
    }
    [Display(AutoGenerateField = false, Name = "Посл.обновление")]
    public DateTime UpdateDate { get; set; }
}

public class DataAnnotationsKontragent : DataAnnotationForFluentApiBase, IMetadataProvider<KontragentViewModel>
{
    void IMetadataProvider<KontragentViewModel>.BuildMetadata(MetadataBuilder<KontragentViewModel> builder)
    {
        SetNotAutoGenerated(builder);
        builder.Property(_ => _.Group).NotAutoGenerated();
        builder.Property(_ => _.Employee).NotAutoGenerated();
        builder.Property(_ => _.ClientCategory).NotAutoGenerated();
        builder.Property(_ => _.Name).AutoGenerated().DisplayName("Наименование");
        builder.Property(_ => _.BalansCurrency).AutoGenerated().DisplayName("Валюта");
        builder.Property(_ => _.OtvetstLico).AutoGenerated().DisplayName("Ответственный");
        //builder.Property(_ => _.FullName).AutoGenerated().DisplayName("Полное наименование");
        builder.Property(_ => _.IsBalans).AutoGenerated().DisplayName("Включен в баланс");
        builder.Property(_ => _.INN).AutoGenerated().DisplayName("ИНН");
        builder.Property(_ => _.Note).AutoGenerated().DisplayName("Примечание");
    }
}

[MetadataType(typeof(DataAnnotationsKontragent))]
[SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
public class KontragentViewModel : RSViewModelBase, IEntity<SD_43>, IDataErrorInfo
{
    private Currency myCurrency;
    private Employee myEmployee;
    private SD_43 myEntity;
    private KontragentGroupViewModel myKontragentGroup;
    private Employee myOtvetstLico;

    public KontragentViewModel()
    {
        Entity = DefaultValue();
        UpdateFrom(Entity);
    }

    public KontragentViewModel(SD_43 entity)
    {
        Entity = entity ?? DefaultValue();
        LoadReference();
    }

    public ObservableCollection<KontragentGruzoRequisite> GruzoRequisities { set; get; } =
        new ObservableCollection<KontragentGruzoRequisite>();

    public ObservableCollection<KontragentBank> KontragentBanks { set; get; } =
        new ObservableCollection<KontragentBank>();

    public Employee Employee
    {
        get => myEmployee;
        set
        {
            if (myEmployee != null && myEmployee.Equals(value)) return;
            myEmployee = value;
            TABELNUMBER = myEmployee?.TabelNumber;
            RaisePropertyChanged();
        }
    }

    public string FullName
    {
        set
        {
            if (NAME_FULL == value) return;
            NAME_FULL = value;
            RaisePropertyChanged();
        }
        get => !string.IsNullOrEmpty(NAME_FULL) ? NAME_FULL : Name;
    }

    public Employee OtvetstLico
    {
        get => myOtvetstLico;
        set
        {
            if (myOtvetstLico != null && myOtvetstLico.Equals(value)) return;
            myOtvetstLico = value;
            OTVETSTV_LICO = myOtvetstLico?.TabelNumber;
            RaisePropertyChanged();
        }
    }

    public string GruzoRequisiteForSchet
    {
        get
        {
            var d = GlobalOptions.GetEntities().SD_43_GRUZO.Where(_ => _.doc_code == Entity.DOC_CODE).ToList();
            if (d.Count == 0)
            {
                if (string.IsNullOrEmpty(ADDRESS))
                    return Name;
                return Name + "," + ADDRESS;
            }

            var ddd = d.FirstOrDefault(_ => _.IsDefault == true);
            return ddd != null ? ddd.GRUZO_TEXT_SF : d.First().GRUZO_TEXT_SF;
        }
    }

    public string GruzoRequisiteForWaybill
    {
        get
        {
            string bank = null;
            var d = GlobalOptions.GetEntities().SD_43_GRUZO.Where(_ => _.doc_code == Entity.DOC_CODE).ToList();
            if (d.Count == 0)
            {
                var s = string.IsNullOrEmpty(ADDRESS) ? null : "," + ADDRESS;
                var s1 = string.IsNullOrEmpty(ADDRESS) ? null : "," + TEL;
                var s2 = string.IsNullOrEmpty(ADDRESS) ? null : "," + FAX;
                return
                    // ReSharper disable once ExpressionIsAlwaysNull
                    Name + s + s1 + s2 + bank;
            }

            var ddd = d.FirstOrDefault(_ => _.IsDefault == true);
            return ddd != null ? ddd.GRUZO_TEXT_NAKLAD : d.First().GRUZO_TEXT_NAKLAD;
        }
    }

    public KontragentGroupViewModel Group
    {
        get => myKontragentGroup;
        set
        {
            if (myKontragentGroup != null && myKontragentGroup.Equals(value)) return;
            myKontragentGroup = value;
            if (myKontragentGroup != null)
                Entity.EG_ID = myKontragentGroup.EG_ID;
            RaisePropertyChanged();
        }
    }

    public int OrderCount { set; get; }

    public override decimal DocCode
    {
        get => DOC_CODE;
        set
        {
            if (DOC_CODE == value) return;
            DOC_CODE = value;
            RaisePropertyChanged();
        }
    }

    public Currency BalansCurrency
    {
        get => myCurrency;
        set
        {
            if (Equals(myCurrency, value)) return;
            myCurrency = value;
            VALUTA_DC = myCurrency?.DocCode;
            RaisePropertyChanged();
        }
    }

    public bool IsBalans
    {
        get => FLAG_BALANS == 1;
        set
        {
            FLAG_BALANS = (short?)(value ? 1 : 0);
            RaisePropertyChanged();
        }
    }

    public bool IsPhysEmployes
    {
        get => FLAG_0UR_1PHYS == 1;
        set
        {
            FLAG_0UR_1PHYS = (short?)(value ? 1 : 0);
            RaisePropertyChanged();
        }
    }

    public string GlavBuh
    {
        get => Entity.GLAVBUH;
        set
        {
            Entity.GLAVBUH = value;
            RaisePropertyChanged();
        }
    }

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

    public string INN
    {
        get => Entity.INN;
        set
        {
            if (Entity.INN == value) return;
            Entity.INN = value;
            RaisePropertyChanged();
        }
    }

    public override string Name
    {
        get => Entity.NAME;
        set
        {
            if (Entity.NAME == value) return;
            Entity.NAME = value;
            RaisePropertyChanged();
        }
    }

    public string Header
    {
        get => Entity.HEADER;
        set
        {
            if (Entity.HEADER == value) return;
            Entity.HEADER = value;
            RaisePropertyChanged();
        }
    }

    public override string Note
    {
        get => Entity.NOTES;
        set
        {
            if (Entity.NOTES == value) return;
            Entity.NOTES = value;
            RaisePropertyChanged();
        }
    }

    public string TYPE_PROP
    {
        get => Entity.TYPE_PROP;
        set
        {
            if (Entity.TYPE_PROP == value) return;
            Entity.TYPE_PROP = value;
            RaisePropertyChanged();
        }
    }

    public short? DELETED
    {
        get => Entity.DELETED;
        set
        {
            if (Entity.DELETED == value) return;
            Entity.DELETED = value;
            RaisePropertyChanged();
        }
    }

    public bool IsDeleted
    {
        get => Entity.DELETED == 1;
        set
        {
            if (Entity.DELETED == 1 == value) return;
            Entity.DELETED = (short?)(value ? 1 : 0);
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(DELETED));
        }
    }

    public string ADDRESS
    {
        get => Entity.ADDRESS;
        set
        {
            if (Entity.ADDRESS == value) return;
            Entity.ADDRESS = value;
            RaisePropertyChanged();
        }
    }

    public string TEL
    {
        get => Entity.TEL;
        set
        {
            if (Entity.TEL == value) return;
            Entity.TEL = value;
            RaisePropertyChanged();
        }
    }

    public string FAX
    {
        get => Entity.FAX;
        set
        {
            if (Entity.FAX == value) return;
            Entity.FAX = value;
            RaisePropertyChanged();
        }
    }

    public string OKPO
    {
        get => Entity.OKPO;
        set
        {
            if (Entity.OKPO == value) return;
            Entity.OKPO = value;
            RaisePropertyChanged();
        }
    }

    public string OKONH
    {
        get => Entity.OKONH;
        set
        {
            if (Entity.OKONH == value) return;
            Entity.OKONH = value;
            RaisePropertyChanged();
        }
    }

    public short? FLAG_0UR_1PHYS
    {
        get => Entity.FLAG_0UR_1PHYS;
        set
        {
            if (Entity.FLAG_0UR_1PHYS == value) return;
            Entity.FLAG_0UR_1PHYS = value;
            RaisePropertyChanged();
        }
    }

    public string PASSPORT
    {
        get => Entity.PASSPORT;
        set
        {
            if (Entity.PASSPORT == value) return;
            Entity.PASSPORT = value;
            RaisePropertyChanged();
        }
    }

    public short? SHIPPING_TRAIN_DAYS
    {
        get => Entity.SHIPPING_TRAIN_DAYS;
        set
        {
            if (Entity.SHIPPING_TRAIN_DAYS == value) return;
            Entity.SHIPPING_TRAIN_DAYS = value;
            RaisePropertyChanged();
        }
    }

    public short? SHIPPING_AUTO_DAYS
    {
        get => Entity.SHIPPING_AUTO_DAYS;
        set
        {
            if (Entity.SHIPPING_AUTO_DAYS == value) return;
            Entity.SHIPPING_AUTO_DAYS = value;
            RaisePropertyChanged();
        }
    }

    public short? PAYMENT_DAYS
    {
        get => Entity.PAYMENT_DAYS;
        set
        {
            if (Entity.PAYMENT_DAYS == value) return;
            Entity.PAYMENT_DAYS = value;
            RaisePropertyChanged();
        }
    }

    public ClientCategory ClientCategory
    {
        get => GlobalOptions.ReferencesCache.GetClientCategory(Entity.CLIENT_CATEG_DC) as ClientCategory;
        set
        {
            if (Equals(GlobalOptions.ReferencesCache.GetClientCategory(Entity.CLIENT_CATEG_DC), value)) return;
            Entity.CLIENT_CATEG_DC = value.DocCode;
            RaisePropertyChanged();
        }
    }

    public int? EG_ID
    {
        get => Entity.EG_ID;
        set
        {
            if (Entity.EG_ID == value) return;
            Entity.EG_ID = value;
            RaisePropertyChanged();
        }
    }

    public int? TABELNUMBER
    {
        get => Entity.TABELNUMBER;
        set
        {
            if (Entity.TABELNUMBER == value) return;
            Entity.TABELNUMBER = value;
            RaisePropertyChanged();
        }
    }

    public decimal? NAL_PAYER_DC
    {
        get => Entity.NAL_PAYER_DC;
        set
        {
            if (Entity.NAL_PAYER_DC == value) return;
            Entity.NAL_PAYER_DC = value;
            RaisePropertyChanged();
        }
    }

    public decimal? REGION_DC
    {
        get => Entity.REGION_DC;
        set
        {
            if (Entity.REGION_DC == value) return;
            Entity.REGION_DC = value;
            RaisePropertyChanged();
        }
    }

    public decimal? CLIENT_CATEG_DC
    {
        get => Entity.CLIENT_CATEG_DC;
        set
        {
            if (Entity.CLIENT_CATEG_DC == value) return;
            Entity.CLIENT_CATEG_DC = value;
            RaisePropertyChanged();
        }
    }

    public short? AUTO_CLIENT_CATEGORY
    {
        get => Entity.AUTO_CLIENT_CATEGORY;
        set
        {
            if (Entity.AUTO_CLIENT_CATEGORY == value) return;
            Entity.AUTO_CLIENT_CATEGORY = value;
            RaisePropertyChanged();
        }
    }

    public decimal? AB_OTRASL_DC
    {
        get => Entity.AB_OTRASL_DC;
        set
        {
            if (Entity.AB_OTRASL_DC == value) return;
            Entity.AB_OTRASL_DC = value;
            RaisePropertyChanged();
        }
    }

    public decimal? AB_BUDGET_DC
    {
        get => Entity.AB_BUDGET_DC;
        set
        {
            if (Entity.AB_BUDGET_DC == value) return;
            Entity.AB_BUDGET_DC = value;
            RaisePropertyChanged();
        }
    }

    public decimal? AB_MINISTRY_DC
    {
        get => Entity.AB_MINISTRY_DC;
        set
        {
            if (Entity.AB_MINISTRY_DC == value) return;
            Entity.AB_MINISTRY_DC = value;
            RaisePropertyChanged();
        }
    }

    public short? PODRAZD_CORP_OBOSOBL
    {
        get => Entity.PODRAZD_CORP_OBOSOBL;
        set
        {
            if (Entity.PODRAZD_CORP_OBOSOBL == value) return;
            Entity.PODRAZD_CORP_OBOSOBL = value;
            RaisePropertyChanged();
        }
    }

    public short? PODRAZD_CORP_GOLOVNOE
    {
        get => Entity.PODRAZD_CORP_GOLOVNOE;
        set
        {
            if (Entity.PODRAZD_CORP_GOLOVNOE == value) return;
            Entity.PODRAZD_CORP_GOLOVNOE = value;
            RaisePropertyChanged();
        }
    }

    public short? FLAG_BALANS
    {
        get => Entity.FLAG_BALANS;
        set
        {
            if (Entity.FLAG_BALANS == value) return;
            Entity.FLAG_BALANS = value;
            RaisePropertyChanged();
        }
    }

    public decimal? VALUTA_DC
    {
        get => Entity.VALUTA_DC;
        set
        {
            if (Entity.VALUTA_DC == value) return;
            Entity.VALUTA_DC = value;
            RaisePropertyChanged();
        }
    }

    public DateTime? START_BALANS
    {
        get => Entity.START_BALANS;
        set
        {
            if (Entity.START_BALANS == value) return;
            Entity.START_BALANS = value;
            RaisePropertyChanged();
        }
    }

    public decimal? START_SUMMA
    {
        get => Entity.START_SUMMA;
        set
        {
            if (Entity.START_SUMMA == value) return;
            Entity.START_SUMMA = value;
            RaisePropertyChanged();
        }
    }

    public int? INNER_CODE
    {
        get => Entity.INNER_CODE;
        set
        {
            if (Entity.INNER_CODE == value) return;
            Entity.INNER_CODE = value;
            RaisePropertyChanged();
        }
    }

    public string NAME_FULL
    {
        get => Entity.NAME_FULL;
        set
        {
            if (Entity.NAME_FULL == value) return;
            Entity.NAME_FULL = value;
            RaisePropertyChanged();
        }
    }

    public short? NO_NDS
    {
        get => Entity.NO_NDS;
        set
        {
            if (Entity.NO_NDS == value) return;
            Entity.NO_NDS = value;
            RaisePropertyChanged();
        }
    }

    public string PREFIX_IN_NUMBER
    {
        get => Entity.PREFIX_IN_NUMBER;
        set
        {
            if (Entity.PREFIX_IN_NUMBER == value) return;
            Entity.PREFIX_IN_NUMBER = value;
            RaisePropertyChanged();
        }
    }

    public string CONTAKT_LICO
    {
        get => Entity.CONTAKT_LICO;
        set
        {
            if (Entity.CONTAKT_LICO == value) return;
            Entity.CONTAKT_LICO = value;
            RaisePropertyChanged();
        }
    }

    public string KASSIR
    {
        get => Entity.KASSIR;
        set
        {
            if (Entity.KASSIR == value) return;
            Entity.KASSIR = value;
            RaisePropertyChanged();
        }
    }

    public decimal? SPOSOB_OTPRAV_DC
    {
        get => Entity.SPOSOB_OTPRAV_DC;
        set
        {
            if (Entity.SPOSOB_OTPRAV_DC == value) return;
            Entity.SPOSOB_OTPRAV_DC = value;
            RaisePropertyChanged();
        }
    }

    public string KPP
    {
        get => Entity.KPP;
        set
        {
            if (Entity.KPP == value) return;
            Entity.KPP = value;
            RaisePropertyChanged();
        }
    }

    public short? KONTR_DISABLE
    {
        get => Entity.KONTR_DISABLE;
        set
        {
            if (Entity.KONTR_DISABLE == value) return;
            Entity.KONTR_DISABLE = value;
            RaisePropertyChanged();
        }
    }

    public decimal? MAX_KREDIT_SUM
    {
        get => Entity.MAX_KREDIT_SUM;
        set
        {
            if (Entity.MAX_KREDIT_SUM == value) return;
            Entity.MAX_KREDIT_SUM = value;
            RaisePropertyChanged();
        }
    }

    public double? TRANSP_KOEF
    {
        get => Entity.TRANSP_KOEF;
        set
        {
            if (Entity.TRANSP_KOEF == value) return;
            Entity.TRANSP_KOEF = value;
            RaisePropertyChanged();
        }
    }

    public string TELEKS
    {
        get => Entity.TELEKS;
        set
        {
            if (Entity.TELEKS == value) return;
            Entity.TELEKS = value;
            RaisePropertyChanged();
        }
    }

    public string E_MAIL
    {
        get => Entity.E_MAIL;
        set
        {
            if (Entity.E_MAIL == value) return;
            Entity.E_MAIL = value;
            RaisePropertyChanged();
        }
    }

    public string WWW
    {
        get => Entity.WWW;
        set
        {
            if (Entity.WWW == value) return;
            Entity.WWW = value;
            RaisePropertyChanged();
        }
    }

    public int? OTVETSTV_LICO
    {
        get => Entity.OTVETSTV_LICO;
        set
        {
            if (Entity.OTVETSTV_LICO == value) return;
            Entity.OTVETSTV_LICO = value;
            RaisePropertyChanged();
        }
    }

    public DateTime? LAST_CALC_BALANS
    {
        get => Entity.LAST_CALC_BALANS;
        set
        {
            if (Entity.LAST_CALC_BALANS == value) return;
            Entity.LAST_CALC_BALANS = value;
            RaisePropertyChanged();
        }
    }

    public byte[] LAST_MAX_VERSION
    {
        get => Entity.LAST_MAX_VERSION;
        set
        {
            if (Entity.LAST_MAX_VERSION == value) return;
            Entity.LAST_MAX_VERSION = value;
            RaisePropertyChanged();
        }
    }

    public DateTime? UpdateDate
    {
        get => Entity.UpdateDate;
        set
        {
            if (Entity.UpdateDate == value) return;
            Entity.UpdateDate = value;
            RaisePropertyChanged();
        }
    }

    public SD_148 SD_148
    {
        get => Entity.SD_148;
        set
        {
            if (Entity.SD_148 == value) return;
            Entity.SD_148 = value;
            RaisePropertyChanged();
        }
    }

    public SD_2 SD_2
    {
        get => Entity.SD_2;
        set
        {
            if (Entity.SD_2 == value) return;
            Entity.SD_2 = value;
            RaisePropertyChanged();
        }
    }

    public SD_23 SD_23
    {
        get => Entity.SD_23;
        set
        {
            if (Entity.SD_23 == value) return;
            Entity.SD_23 = value;
            RaisePropertyChanged();
        }
    }

    public SD_252 SD_252
    {
        get => Entity.SD_252;
        set
        {
            if (Entity.SD_252 == value) return;
            Entity.SD_252 = value;
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

    public UD_43 UD_43
    {
        get => Entity.UD_43;
        set
        {
            if (Entity.UD_43 == value) return;
            Entity.UD_43 = value;
            RaisePropertyChanged();
        }
    }

    public SD_57 SD_57
    {
        get => Entity.SD_57;
        set
        {
            if (Entity.SD_57 == value) return;
            Entity.SD_57 = value;
            RaisePropertyChanged();
        }
    }

    public SD_53 SD_53
    {
        get => Entity.SD_53;
        set
        {
            if (Entity.SD_53 == value) return;
            Entity.SD_53 = value;
            RaisePropertyChanged();
        }
    }

    public SD_92 SD_92
    {
        get => Entity.SD_92;
        set
        {
            if (Entity.SD_92 == value) return;
            Entity.SD_92 = value;
            RaisePropertyChanged();
        }
    }

    public SD_56 SD_56
    {
        get => Entity.SD_56;
        set
        {
            if (Entity.SD_56 == value) return;
            Entity.SD_56 = value;
            RaisePropertyChanged();
        }
    }

    public EntityLoadCodition LoadCondition { get; set; }

    public bool IsAccessRight { get; set; }

    public string this[string columnName] =>
        string.IsNullOrWhiteSpace(Name) ? ValidationError.GetErrorMan("Контрагент") : null;

    public string Error => null;

    public SD_43 Entity
    {
        get => myEntity;
        set
        {
            if (myEntity == value) return;
            myEntity = value;
            RaisePropertyChanged();
        }
    }

    public SD_43 DefaultValue()
    {
        return new SD_43
        {
            DOC_CODE = -1
        };
    }

    public List<SD_43> LoadList()
    {
        throw new NotImplementedException();
    }

    private void LoadReference()
    {
        BalansCurrency = GlobalOptions.ReferencesCache.GetCurrency(Entity.VALUTA_DC) as Currency;
        Group = new KontragentGroupViewModel(Entity.UD_43);
        ClientCategory = GlobalOptions.ReferencesCache.GetClientCategory(Entity.SD_148?.DOC_CODE) as ClientCategory;
        var emp = GlobalOptions.ReferencesCache.GetEmployee(Entity.TABELNUMBER);
        if (emp != null)
            Employee = emp as Employee;
        var otv = GlobalOptions.ReferencesCache.GetEmployee(Entity.OTVETSTV_LICO) as Employee;
        if (otv != null)
            OtvetstLico = otv;
        if (Entity.SD_43_GRUZO != null && Entity.SD_43_GRUZO.Count > 0)
            foreach (var item in Entity.SD_43_GRUZO.ToList())
                GruzoRequisities.Add(new KontragentGruzoRequisite(item) { myState = RowStatus.NotEdited });
        if (Entity.TD_43 != null && Entity.TD_43.Count > 0)
            foreach (var item in Entity.TD_43.ToList())
            {
                var newItem = new KontragentBank(item) { myState = RowStatus.NotEdited };
                KontragentBanks.Add(newItem);
            }

        // ReSharper disable once VirtualMemberCallInConstructor
        State = RowStatus.NotEdited;
    }

    public virtual void Save(SD_43 doc)
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

    public void UpdateFrom(SD_43 ent)
    {
        INN = ent.INN;
        Name = ent.NAME;
        Header = ent.HEADER;
        GlavBuh = ent.GLAVBUH;
        Note = ent.NOTES;
        TYPE_PROP = ent.TYPE_PROP;
        DELETED = ent.DELETED;
        ADDRESS = ent.ADDRESS;
        TEL = ent.TEL;
        FAX = ent.FAX;
        OKPO = ent.OKPO;
        OKONH = ent.OKONH;
        FLAG_0UR_1PHYS = ent.FLAG_0UR_1PHYS;
        PASSPORT = ent.PASSPORT;
        SHIPPING_TRAIN_DAYS = ent.SHIPPING_TRAIN_DAYS;
        SHIPPING_AUTO_DAYS = ent.SHIPPING_AUTO_DAYS;
        PAYMENT_DAYS = ent.PAYMENT_DAYS;
        EG_ID = ent.EG_ID;
        TABELNUMBER = ent.TABELNUMBER;
        NAL_PAYER_DC = ent.NAL_PAYER_DC;
        REGION_DC = ent.REGION_DC;
        CLIENT_CATEG_DC = ent.CLIENT_CATEG_DC;
        AUTO_CLIENT_CATEGORY = ent.AUTO_CLIENT_CATEGORY;
        AB_OTRASL_DC = ent.AB_OTRASL_DC;
        AB_BUDGET_DC = ent.AB_BUDGET_DC;
        AB_MINISTRY_DC = ent.AB_MINISTRY_DC;
        PODRAZD_CORP_OBOSOBL = ent.PODRAZD_CORP_OBOSOBL;
        PODRAZD_CORP_GOLOVNOE = ent.PODRAZD_CORP_GOLOVNOE;
        FLAG_BALANS = ent.FLAG_BALANS;
        VALUTA_DC = ent.VALUTA_DC;
        START_BALANS = ent.START_BALANS;
        START_SUMMA = ent.START_SUMMA;
        INNER_CODE = ent.INNER_CODE;
        NAME_FULL = ent.NAME_FULL;
        NO_NDS = ent.NO_NDS;
        PREFIX_IN_NUMBER = ent.PREFIX_IN_NUMBER;
        CONTAKT_LICO = ent.CONTAKT_LICO;
        KASSIR = ent.KASSIR;
        SPOSOB_OTPRAV_DC = ent.SPOSOB_OTPRAV_DC;
        KPP = ent.KPP;
        KONTR_DISABLE = ent.KONTR_DISABLE;
        MAX_KREDIT_SUM = ent.MAX_KREDIT_SUM;
        TRANSP_KOEF = ent.TRANSP_KOEF;
        TELEKS = ent.TELEKS;
        E_MAIL = ent.E_MAIL;
        WWW = ent.WWW;
        OTVETSTV_LICO = ent.OTVETSTV_LICO;
        LAST_CALC_BALANS = ent.LAST_CALC_BALANS;
        LAST_MAX_VERSION = ent.LAST_MAX_VERSION;
        UpdateDate = ent.UpdateDate;
        SD_148 = ent.SD_148;
        SD_2 = ent.SD_2;
        SD_23 = ent.SD_23;
        SD_252 = ent.SD_252;
        SD_301 = ent.SD_301;
        UD_43 = ent.UD_43;
        SD_57 = ent.SD_57;
        SD_53 = ent.SD_53;
        SD_92 = ent.SD_92;
        SD_56 = ent.SD_56;
    }

    public void UpdateTo(SD_43 ent)
    {
        ent.INN = INN;
        ent.NAME = Name;
        ent.HEADER = Header;
        ent.GLAVBUH = GlavBuh;
        ent.NOTES = Note;
        ent.TYPE_PROP = TYPE_PROP;
        ent.DELETED = DELETED;
        ent.ADDRESS = ADDRESS;
        ent.TEL = TEL;
        ent.FAX = FAX;
        ent.OKPO = OKPO;
        ent.OKONH = OKONH;
        ent.FLAG_0UR_1PHYS = FLAG_0UR_1PHYS;
        ent.PASSPORT = PASSPORT;
        ent.SHIPPING_TRAIN_DAYS = SHIPPING_TRAIN_DAYS;
        ent.SHIPPING_AUTO_DAYS = SHIPPING_AUTO_DAYS;
        ent.PAYMENT_DAYS = PAYMENT_DAYS;
        ent.EG_ID = EG_ID;
        ent.TABELNUMBER = TABELNUMBER;
        ent.NAL_PAYER_DC = NAL_PAYER_DC;
        ent.REGION_DC = REGION_DC;
        ent.CLIENT_CATEG_DC = CLIENT_CATEG_DC;
        ent.AUTO_CLIENT_CATEGORY = AUTO_CLIENT_CATEGORY;
        ent.AB_OTRASL_DC = AB_OTRASL_DC;
        ent.AB_BUDGET_DC = AB_BUDGET_DC;
        ent.AB_MINISTRY_DC = AB_MINISTRY_DC;
        ent.PODRAZD_CORP_OBOSOBL = PODRAZD_CORP_OBOSOBL;
        ent.PODRAZD_CORP_GOLOVNOE = PODRAZD_CORP_GOLOVNOE;
        ent.FLAG_BALANS = FLAG_BALANS;
        ent.VALUTA_DC = VALUTA_DC;
        ent.START_BALANS = START_BALANS;
        ent.START_SUMMA = START_SUMMA;
        ent.INNER_CODE = INNER_CODE;
        ent.NAME_FULL = NAME_FULL;
        ent.NO_NDS = NO_NDS;
        ent.PREFIX_IN_NUMBER = PREFIX_IN_NUMBER;
        ent.CONTAKT_LICO = CONTAKT_LICO;
        ent.KASSIR = KASSIR;
        ent.SPOSOB_OTPRAV_DC = SPOSOB_OTPRAV_DC;
        ent.KPP = KPP;
        ent.KONTR_DISABLE = KONTR_DISABLE;
        ent.MAX_KREDIT_SUM = MAX_KREDIT_SUM;
        ent.TRANSP_KOEF = TRANSP_KOEF;
        ent.TELEKS = TELEKS;
        ent.E_MAIL = E_MAIL;
        ent.WWW = WWW;
        ent.OTVETSTV_LICO = OTVETSTV_LICO;
        ent.LAST_CALC_BALANS = LAST_CALC_BALANS;
        ent.LAST_MAX_VERSION = LAST_MAX_VERSION;
        ent.UpdateDate = UpdateDate;
        ent.SD_148 = SD_148;
        ent.SD_2 = SD_2;
        ent.SD_23 = SD_23;
        ent.SD_252 = SD_252;
        ent.SD_301 = SD_301;
        ent.UD_43 = UD_43;
        ent.SD_57 = SD_57;
        ent.SD_53 = SD_53;
        ent.SD_92 = SD_92;
        ent.SD_56 = SD_56;
    }

    public SD_43 Load(decimal dc, bool isShort = true)
    {
        throw new NotImplementedException();
    }

    public SD_43 Load(Guid id, bool isShort = true)
    {
        throw new NotImplementedException();
    }

    public virtual SD_43 Load(decimal dc)
    {
        throw new NotImplementedException();
    }

    public virtual SD_43 Load(Guid id)
    {
        throw new NotImplementedException();
    }
}
