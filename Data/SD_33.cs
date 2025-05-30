//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Data
{
    using System;
    using System.Collections.Generic;
    
    public partial class SD_33
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SD_33()
        {
            this.AccuredAmountForClientRow = new HashSet<AccuredAmountForClientRow>();
            this.SD_327 = new HashSet<SD_327>();
            this.TD_101 = new HashSet<TD_101>();
            this.TD_397 = new HashSet<TD_397>();
            this.TD_60 = new HashSet<TD_60>();
            this.UD_259 = new HashSet<UD_259>();
            this.UD_281 = new HashSet<UD_281>();
            this.ProjectDocuments = new HashSet<ProjectDocuments>();
        }
    
        public decimal DOC_CODE { get; set; }
        public Nullable<int> NUM_ORD { get; set; }
        public Nullable<decimal> SUMM_ORD { get; set; }
        public string NAME_ORD { get; set; }
        public string OSN_ORD { get; set; }
        public string NOTES_ORD { get; set; }
        public Nullable<System.DateTime> DATE_ORD { get; set; }
        public Nullable<decimal> CODE_CASS { get; set; }
        public Nullable<int> TABELNUMBER { get; set; }
        public Nullable<short> OP_CODE { get; set; }
        public Nullable<decimal> CA_DC { get; set; }
        public Nullable<decimal> KONTRAGENT_DC { get; set; }
        public string SHPZ_ORD { get; set; }
        public Nullable<decimal> SHPZ_DC { get; set; }
        public Nullable<decimal> RASH_ORDER_FROM_DC { get; set; }
        public Nullable<decimal> SFACT_DC { get; set; }
        public Nullable<short> NCODE { get; set; }
        public Nullable<decimal> POS_DC { get; set; }
        public Nullable<decimal> POS_PREV_OST { get; set; }
        public Nullable<decimal> POS_PRIHOD { get; set; }
        public Nullable<decimal> POS_NOW_OST { get; set; }
        public Nullable<decimal> KONTR_CRS_DC { get; set; }
        public Nullable<decimal> CRS_KOEF { get; set; }
        public Nullable<decimal> CRS_SUMMA { get; set; }
        public Nullable<byte> CHANGE_ORD { get; set; }
        public Nullable<decimal> CRS_DC { get; set; }
        public Nullable<decimal> UCH_VALUTA_DC { get; set; }
        public Nullable<decimal> SUMMA_V_UCH_VALUTE { get; set; }
        public Nullable<double> UCH_VALUTA_RATE { get; set; }
        public Nullable<decimal> SUM_NDS { get; set; }
        public Nullable<decimal> SFACT_OPLACHENO { get; set; }
        public Nullable<double> SFACT_CRS_RATE { get; set; }
        public Nullable<decimal> SFACT_CRS_DC { get; set; }
        public Nullable<decimal> BANK_RASCH_SCHET_DC { get; set; }
        public string CREATOR { get; set; }
        public Nullable<decimal> RUB_SUMMA { get; set; }
        public Nullable<double> RUB_RATE { get; set; }
        public Nullable<short> OBRATNY_RASCHET { get; set; }
        public Nullable<double> KONTR_CRS_SUM_CORRECT_PERCENT { get; set; }
        public string V_TOM_CHISLE { get; set; }
        public Nullable<decimal> KONTR_FROM_DC { get; set; }
        public Nullable<int> SFACT_FLAG { get; set; }
        public byte[] TSTAMP { get; set; }
        public Nullable<System.Guid> StockHolderId { get; set; }
        public Nullable<System.Guid> ProjectId { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<AccuredAmountForClientRow> AccuredAmountForClientRow { get; set; }
        public virtual SD_114 SD_114 { get; set; }
        public virtual SD_2 SD_2 { get; set; }
        public virtual SD_22 SD_22 { get; set; }
        public virtual SD_301 SD_301 { get; set; }
        public virtual SD_301 SD_3011 { get; set; }
        public virtual SD_301 SD_3012 { get; set; }
        public virtual SD_301 SD_3013 { get; set; }
        public virtual SD_303 SD_303 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SD_327> SD_327 { get; set; }
        public virtual SD_34 SD_34 { get; set; }
        public virtual VD_46 VD_46 { get; set; }
        public virtual SD_90 SD_90 { get; set; }
        public virtual SD_84 SD_84 { get; set; }
        public virtual SD_43 SD_43 { get; set; }
        public virtual StockHolders StockHolders { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TD_101> TD_101 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TD_397> TD_397 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TD_60> TD_60 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UD_259> UD_259 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UD_281> UD_281 { get; set; }
        public virtual Projects Projects { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProjectDocuments> ProjectDocuments { get; set; }
    }
}
