//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AlfaMediaData
{
    using System;
    using System.Collections.Generic;
    
    public partial class TD_26
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TD_26()
        {
            this.TD_120 = new HashSet<TD_120>();
            this.TD_24 = new HashSet<TD_24>();
            this.TD_24_2 = new HashSet<TD_24_2>();
            this.TD_800 = new HashSet<TD_800>();
            this.UD_800 = new HashSet<UD_800>();
        }
    
        public decimal DOC_CODE { get; set; }
        public int CODE { get; set; }
        public string SFT_TEXT { get; set; }
        public decimal SFT_POST_ED_IZM_DC { get; set; }
        public Nullable<decimal> SFT_POST_ED_CENA { get; set; }
        public decimal SFT_POST_KOL { get; set; }
        public decimal SFT_NEMENKL_DC { get; set; }
        public decimal SFT_UCHET_ED_IZM_DC { get; set; }
        public Nullable<decimal> SFT_ED_CENA { get; set; }
        public decimal SFT_KOL { get; set; }
        public Nullable<decimal> SFT_SUMMA_CBOROV { get; set; }
        public decimal SFT_NDS_PERCENT { get; set; }
        public Nullable<decimal> SFT_SUMMA_NAKLAD { get; set; }
        public Nullable<decimal> SFT_SUMMA_NDS { get; set; }
        public Nullable<decimal> SFT_SUMMA_K_OPLATE { get; set; }
        public Nullable<decimal> SFT_ED_CENA_PRIHOD { get; set; }
        public short SFT_IS_NAKLAD { get; set; }
        public short SFT_VKLUCH_V_CENU { get; set; }
        public short SFT_AUTO_FLAG { get; set; }
        public Nullable<decimal> SFT_STDP_DC { get; set; }
        public Nullable<decimal> SFT_NOM_CRS_DC { get; set; }
        public Nullable<decimal> SFT_NOM_CRS_RATE { get; set; }
        public Nullable<decimal> SFT_NOM_CRS_CENA { get; set; }
        public Nullable<decimal> SFT_CENA_V_UCHET_VALUTE { get; set; }
        public Nullable<decimal> SFT_SUMMA_V_UCHET_VALUTE { get; set; }
        public Nullable<decimal> SFT_DOG_POKUP_DC { get; set; }
        public Nullable<int> SFT_DOG_POKUP_PLAN_ROW_CODE { get; set; }
        public Nullable<decimal> SFT_SUMMA_K_OPLATE_KONTR_CRS { get; set; }
        public Nullable<decimal> SFT_SHPZ_DC { get; set; }
        public string SFT_STRANA_PROIS { get; set; }
        public string SFT_N_GRUZ_DECLAR { get; set; }
        public Nullable<short> SFT_PEREVOZCHIK_POZITION { get; set; }
        public Nullable<decimal> SFT_NAKLAD_KONTR_DC { get; set; }
        public Nullable<decimal> SFT_SALE_PRICE_IN_UCH_VAL { get; set; }
        public Nullable<decimal> SFT_PERCENT { get; set; }
        public byte[] TSTAMP { get; set; }
        public System.Guid Id { get; set; }
        public Nullable<System.Guid> DocId { get; set; }
    
        public virtual SD_165 SD_165 { get; set; }
        public virtual SD_175 SD_175 { get; set; }
        public virtual SD_175 SD_1751 { get; set; }
        public virtual SD_26 SD_26 { get; set; }
        public virtual SD_26 SD_261 { get; set; }
        public virtual SD_301 SD_301 { get; set; }
        public virtual SD_303 SD_303 { get; set; }
        public virtual SD_43 SD_43 { get; set; }
        public virtual SD_83 SD_83 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TD_120> TD_120 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TD_24> TD_24 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TD_24_2> TD_24_2 { get; set; }
        public virtual UD_112 UD_112 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TD_800> TD_800 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UD_800> UD_800 { get; set; }
    }
}
