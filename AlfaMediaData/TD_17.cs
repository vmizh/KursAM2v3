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
    
    public partial class TD_17
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TD_17()
        {
            this.SD_10 = new HashSet<SD_10>();
            this.SD_101 = new HashSet<SD_10>();
            this.SD_102 = new HashSet<SD_10>();
            this.SD_13 = new HashSet<SD_13>();
            this.SD_258 = new HashSet<SD_258>();
            this.SD_265 = new HashSet<SD_265>();
            this.SD_275 = new HashSet<SD_275>();
            this.SD_291 = new HashSet<SD_291>();
            this.SD_292 = new HashSet<SD_292>();
            this.SD_293 = new HashSet<SD_293>();
            this.SD_294 = new HashSet<SD_294>();
            this.SD_390 = new HashSet<SD_390>();
            this.SD_5 = new HashSet<SD_5>();
            this.SD_51 = new HashSet<SD_5>();
            this.SD_6 = new HashSet<SD_6>();
            this.SD_61 = new HashSet<SD_6>();
            this.TD_11 = new HashSet<TD_11>();
            this.TD_156 = new HashSet<TD_156>();
            this.TD_207 = new HashSet<TD_207>();
            this.TD_212 = new HashSet<TD_212>();
            this.TD_392 = new HashSet<TD_392>();
            this.TD_399 = new HashSet<TD_399>();
            this.UD_11 = new HashSet<UD_11>();
            this.UD_17 = new HashSet<UD_17>();
            this.VD_17 = new HashSet<VD_17>();
        }
    
        public int CODE { get; set; }
        public Nullable<int> IERARHY_CODE { get; set; }
        public Nullable<int> TABELNUMBER { get; set; }
        public string WORK_NAME { get; set; }
        public Nullable<decimal> OKLAD { get; set; }
        public Nullable<short> RAZRIAD { get; set; }
        public Nullable<decimal> PLAN_OKLAD { get; set; }
        public string PLAN_WORK_NAME { get; set; }
        public Nullable<short> PLAN_RAZRIAD { get; set; }
        public Nullable<decimal> PLAN_DLC_DC { get; set; }
        public Nullable<short> HAS_WORKER { get; set; }
        public string NOTES { get; set; }
        public Nullable<short> DELETED { get; set; }
        public Nullable<System.DateTime> DELETE_DATE { get; set; }
        public Nullable<short> IS_REMOVED { get; set; }
        public Nullable<decimal> CATEG_DOL { get; set; }
        public Nullable<decimal> CAL_TYPE { get; set; }
        public Nullable<short> OKLAD_TYPE { get; set; }
        public short SMENA { get; set; }
        public Nullable<int> GRT_ID { get; set; }
        public Nullable<double> KOEF_RAI { get; set; }
        public Nullable<double> KOEF_VREDN { get; set; }
        public Nullable<double> PREMIA_PERCENT { get; set; }
        public Nullable<decimal> PREMIA_FIXED { get; set; }
        public Nullable<double> NADBAVKA { get; set; }
        public Nullable<decimal> PERSGROUP_DC { get; set; }
        public Nullable<decimal> SHPZ_DC { get; set; }
        public Nullable<decimal> DLC_DC { get; set; }
        public Nullable<double> SHTAT_ED_NUM { get; set; }
        public Nullable<int> SUBCATEG_DOL { get; set; }
        public string STARY_SHIFR { get; set; }
        public Nullable<double> KOEF_VREDN_WINTER { get; set; }
        public Nullable<decimal> ZPL_CRS_DC { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SD_10> SD_10 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SD_10> SD_101 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SD_10> SD_102 { get; set; }
        public virtual SD_12 SD_12 { get; set; }
        public virtual SD_12 SD_121 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SD_13> SD_13 { get; set; }
        public virtual SD_152 SD_152 { get; set; }
        public virtual SD_17 SD_17 { get; set; }
        public virtual SD_191 SD_191 { get; set; }
        public virtual SD_2 SD_2 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SD_258> SD_258 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SD_265> SD_265 { get; set; }
        public virtual SD_271 SD_271 { get; set; }
        public virtual SD_271 SD_2711 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SD_275> SD_275 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SD_291> SD_291 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SD_292> SD_292 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SD_293> SD_293 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SD_294> SD_294 { get; set; }
        public virtual SD_297 SD_297 { get; set; }
        public virtual SD_301 SD_301 { get; set; }
        public virtual SD_303 SD_303 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SD_390> SD_390 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SD_5> SD_5 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SD_5> SD_51 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SD_6> SD_6 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SD_6> SD_61 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TD_11> TD_11 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TD_156> TD_156 { get; set; }
        public virtual TD_191 TD_191 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TD_207> TD_207 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TD_212> TD_212 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TD_392> TD_392 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TD_399> TD_399 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UD_11> UD_11 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UD_17> UD_17 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<VD_17> VD_17 { get; set; }
    }
}
