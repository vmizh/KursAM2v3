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
    
    public partial class SD_235
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SD_235()
        {
            this.TD_235 = new HashSet<TD_235>();
            this.UD_235 = new HashSet<UD_235>();
        }
    
        public decimal DOC_CODE { get; set; }
        public int TABELNUMBER { get; set; }
        public decimal L_FACT_LGOTA { get; set; }
        public decimal L_PODOHOD_OSN { get; set; }
        public decimal L_PODOHOD_RKSK { get; set; }
        public decimal L_OBLAG_OSN { get; set; }
        public decimal L_OBLAG_RKSK { get; set; }
        public decimal L_YEAR { get; set; }
        public Nullable<int> L_MONTH_EDIT { get; set; }
        public Nullable<decimal> L_PENS_OSN { get; set; }
        public Nullable<decimal> L_PENS_RKSK { get; set; }
        public Nullable<decimal> L_MAT_VIGODA { get; set; }
    
        public virtual SD_2 SD_2 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TD_235> TD_235 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UD_235> UD_235 { get; set; }
    }
}
