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
    
    public partial class SD_236
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SD_236()
        {
            this.TD_236 = new HashSet<TD_236>();
        }
    
        public decimal DOC_CODE { get; set; }
        public int TTN_NUM { get; set; }
        public System.DateTime TTN_DATE { get; set; }
        public string TTN_GRUS { get; set; }
        public decimal TTN_POLUCH_DC { get; set; }
        public string TTN_STANCIA_NAZNACH { get; set; }
        public Nullable<decimal> TTN_PEREVOZCHIK_DC { get; set; }
        public decimal TTN_SPOSOB_OTPRAV_DC { get; set; }
        public string TTN_NOMER_SREDSTVA_PEREVOZ { get; set; }
        public Nullable<System.DateTime> TTN_PODACH_DATETIME { get; set; }
        public Nullable<System.DateTime> TTN_UBOR_DATETIME { get; set; }
        public string TTN_VOZRVAT_DOC_NUM { get; set; }
        public string CREATOR { get; set; }
        public short TTN_OTPRAVLENO { get; set; }
        public Nullable<double> TTN_VES { get; set; }
        public string TTN_SOPROVOZH { get; set; }
    
        public virtual SD_43 SD_43 { get; set; }
        public virtual SD_43 SD_431 { get; set; }
        public virtual SD_252 SD_252 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TD_236> TD_236 { get; set; }
    }
}
