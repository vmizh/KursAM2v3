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
    
    public partial class SD_148
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SD_148()
        {
            this.SD_161 = new HashSet<SD_161>();
            this.SD_43 = new HashSet<SD_43>();
            this.SD_9 = new HashSet<SD_9>();
            this.TD_148 = new HashSet<TD_148>();
            this.UD_193 = new HashSet<UD_193>();
            this.YD_193 = new HashSet<YD_193>();
        }
    
        public decimal DOC_CODE { get; set; }
        public decimal CK_MIN_OBOROT { get; set; }
        public double CK_MAX_PROSROCH_DNEY { get; set; }
        public decimal CK_CREDIT_SUM { get; set; }
        public string CK_NAME { get; set; }
        public Nullable<double> CK_NACEN_DEFAULT_ROZN { get; set; }
        public Nullable<double> CK_NACEN_DEFAULT_KOMPL { get; set; }
        public Nullable<short> CK_IMMEDIATE_PRICE_CHANGE { get; set; }
        public string CK_GROUP { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SD_161> SD_161 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SD_43> SD_43 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SD_9> SD_9 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TD_148> TD_148 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UD_193> UD_193 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<YD_193> YD_193 { get; set; }
    }
}
