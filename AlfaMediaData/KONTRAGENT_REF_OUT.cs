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
    
    public partial class KONTRAGENT_REF_OUT
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public KONTRAGENT_REF_OUT()
        {
            this.KONTRAGENT_REF_OUT_REQUISITE = new HashSet<KONTRAGENT_REF_OUT_REQUISITE>();
            this.SCHET_FACT_KONTR_OUT = new HashSet<SCHET_FACT_KONTR_OUT>();
            this.SCHET_FACT_KONTR_OUT1 = new HashSet<SCHET_FACT_KONTR_OUT>();
        }
    
        public System.Guid Id { get; set; }
        public string NAME { get; set; }
        public string Director { get; set; }
        public string GlavBuh { get; set; }
        public string Note { get; set; }
        public string INN { get; set; }
        public string KPP { get; set; }
        public string OKPO { get; set; }
        public string Address { get; set; }
        public string OKONH { get; set; }
        public Nullable<decimal> OLD_DC { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<KONTRAGENT_REF_OUT_REQUISITE> KONTRAGENT_REF_OUT_REQUISITE { get; set; }
        public virtual SD_43 SD_43 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SCHET_FACT_KONTR_OUT> SCHET_FACT_KONTR_OUT { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SCHET_FACT_KONTR_OUT> SCHET_FACT_KONTR_OUT1 { get; set; }
    }
}
