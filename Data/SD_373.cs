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
    
    public partial class SD_373
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SD_373()
        {
            this.SD_3731 = new HashSet<SD_373>();
            this.TD_373 = new HashSet<TD_373>();
            this.TD_397 = new HashSet<TD_397>();
            this.TD_437 = new HashSet<TD_437>();
        }
    
        public decimal DOC_CODE { get; set; }
        public int SF_IN_NUM { get; set; }
        public string SF_OUT_NUM { get; set; }
        public System.DateTime SF_DATE { get; set; }
        public decimal SF_POSTAV_KONTR_DC { get; set; }
        public decimal SF_POLUCH_KONTR_DC { get; set; }
        public Nullable<decimal> SF_SUMMA_K_OPLATE { get; set; }
        public Nullable<decimal> SF_SUMMA_OPLACHENO { get; set; }
        public double SF_NP_PROC { get; set; }
        public Nullable<decimal> SF_SFACT_DC { get; set; }
        public Nullable<System.DateTime> SF_OPLATA_DATE { get; set; }
        public string SF_NOTES { get; set; }
        public Nullable<short> SF_NDS_VKL_V_CENU { get; set; }
        public Nullable<short> SF_NP_VKL_V_CENU { get; set; }
        public Nullable<short> SF_LISTOV_SERTIFICATOV { get; set; }
        public string SF_DOVERENNOST { get; set; }
        public string SF_NOMER_DOGOVORA { get; set; }
        public Nullable<System.DateTime> SF_DATA_DOGOVORA { get; set; }
        public Nullable<decimal> SF_DOP_USL_DC { get; set; }
        public Nullable<decimal> SF_SF_NA_PREDOPLATU_DC { get; set; }
        public Nullable<short> SF_AUTO_CREATE { get; set; }
        public string SF_PLAT_DOCUM { get; set; }
        public Nullable<System.DateTime> SF_SCHET_FACTURA_DATE { get; set; }
        public Nullable<decimal> SF_PROD_ZA_NAL_DC { get; set; }
        public Nullable<decimal> SF_ZAKAZ_DC { get; set; }
        public Nullable<decimal> SF_UCHET_VALUTA_DC { get; set; }
        public Nullable<double> SF_UCHET_VALUTA_RATE { get; set; }
        public Nullable<decimal> SF_SUMMA_V_UCHET_VALUTE { get; set; }
    
        public virtual SD_259 SD_259 { get; set; }
        public virtual SD_301 SD_301 { get; set; }
        public virtual SD_375 SD_375 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SD_373> SD_3731 { get; set; }
        public virtual SD_373 SD_3732 { get; set; }
        public virtual SD_43 SD_43 { get; set; }
        public virtual SD_43 SD_431 { get; set; }
        public virtual SD_84 SD_84 { get; set; }
        public virtual SD_9 SD_9 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TD_373> TD_373 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TD_397> TD_397 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TD_437> TD_437 { get; set; }
    }
}
