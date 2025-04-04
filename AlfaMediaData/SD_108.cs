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
    
    public partial class SD_108
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SD_108()
        {
            this.ND_114 = new HashSet<ND_114>();
            this.ND_22 = new HashSet<ND_22>();
            this.ND_27 = new HashSet<ND_27>();
            this.ND_303 = new HashSet<ND_303>();
            this.ND_43 = new HashSet<ND_43>();
            this.SD_32 = new HashSet<SD_32>();
            this.SD_321 = new HashSet<SD_32>();
            this.TD_138 = new HashSet<TD_138>();
            this.TD_32 = new HashSet<TD_32>();
            this.VD_108 = new HashSet<VD_108>();
            this.WD_138 = new HashSet<WD_138>();
            this.XD_48 = new HashSet<XD_48>();
            this.UD_108 = new HashSet<UD_108>();
            this.UD_1081 = new HashSet<UD_108>();
            this.SD_183 = new HashSet<SD_183>();
        }
    
        public short ACC { get; set; }
        public short SUBACC { get; set; }
        public string ACC_NAME { get; set; }
        public Nullable<short> ACC_TYPE { get; set; }
        public Nullable<short> DELETED { get; set; }
        public decimal DOC_CODE { get; set; }
        public string SUBACC_NAME { get; set; }
        public string ANAL_NAME { get; set; }
        public short IS_ANAL { get; set; }
        public Nullable<decimal> PARENT_DOC_CODE { get; set; }
        public string ANAL_CODE { get; set; }
        public Nullable<short> IS_SALDO_EXP { get; set; }
        public decimal balans_dc { get; set; }
        public Nullable<short> IS_CHANGABLE_ACC { get; set; }
        public Nullable<short> IS_PODOTCHETN { get; set; }
        public Nullable<decimal> START_PERIOD_DC { get; set; }
        public Nullable<decimal> START_DEBET { get; set; }
        public Nullable<decimal> START_CREDIT { get; set; }
        public Nullable<short> DEBITOR_ZADOL { get; set; }
        public Nullable<short> CREDITOE_ZADOL { get; set; }
        public string JOURNAL_DEB_HEADER { get; set; }
        public string JOURNAL_CRED_HEADER { get; set; }
        public string JOURNAL_DEB_CRED_HEADER { get; set; }
        public Nullable<int> MAIN_CHAIN_ID { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ND_114> ND_114 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ND_22> ND_22 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ND_27> ND_27 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ND_303> ND_303 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ND_43> ND_43 { get; set; }
        public virtual SD_138 SD_138 { get; set; }
        public virtual SD_298 SD_298 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SD_32> SD_32 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SD_32> SD_321 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TD_138> TD_138 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TD_32> TD_32 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<VD_108> VD_108 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<WD_138> WD_138 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<XD_48> XD_48 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UD_108> UD_108 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UD_108> UD_1081 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SD_183> SD_183 { get; set; }
    }
}
