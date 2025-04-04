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
    
    public partial class SD_303
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SD_303()
        {
            this.AccuredAmountForClientRow = new HashSet<AccuredAmountForClientRow>();
            this.AccuredAmountOfSupplierRow = new HashSet<AccuredAmountOfSupplierRow>();
            this.ND_303 = new HashSet<ND_303>();
            this.OffBalanceSheetInRow = new HashSet<OffBalanceSheetInRow>();
            this.OffBalanceSheetOutRow = new HashSet<OffBalanceSheetOutRow>();
            this.SD_117 = new HashSet<SD_117>();
            this.SD_120 = new HashSet<SD_120>();
            this.SD_122 = new HashSet<SD_122>();
            this.SD_140 = new HashSet<SD_140>();
            this.SD_161 = new HashSet<SD_161>();
            this.SD_17 = new HashSet<SD_17>();
            this.SD_187 = new HashSet<SD_187>();
            this.SD_1871 = new HashSet<SD_187>();
            this.SD_251 = new HashSet<SD_251>();
            this.SD_302 = new HashSet<SD_302>();
            this.SD_32 = new HashSet<SD_32>();
            this.SD_327 = new HashSet<SD_327>();
            this.SD_33 = new HashSet<SD_33>();
            this.SD_34 = new HashSet<SD_34>();
            this.SD_73 = new HashSet<SD_73>();
            this.SD_77 = new HashSet<SD_77>();
            this.SD_83 = new HashSet<SD_83>();
            this.TD_101 = new HashSet<TD_101>();
            this.TD_110 = new HashSet<TD_110>();
            this.TD_153 = new HashSet<TD_153>();
            this.TD_165 = new HashSet<TD_165>();
            this.TD_17 = new HashSet<TD_17>();
            this.TD_203 = new HashSet<TD_203>();
            this.TD_219 = new HashSet<TD_219>();
            this.TD_222 = new HashSet<TD_222>();
            this.TD_24_2 = new HashSet<TD_24_2>();
            this.TD_24 = new HashSet<TD_24>();
            this.TD_26 = new HashSet<TD_26>();
            this.TD_281 = new HashSet<TD_281>();
            this.TD_299 = new HashSet<TD_299>();
            this.TD_46 = new HashSet<TD_46>();
            this.TD_71 = new HashSet<TD_71>();
            this.TD_84 = new HashSet<TD_84>();
            this.UD_203 = new HashSet<UD_203>();
            this.UD_306 = new HashSet<UD_306>();
            this.UD_46 = new HashSet<UD_46>();
            this.UD_83 = new HashSet<UD_83>();
            this.WD_48 = new HashSet<WD_48>();
            this.XD_203 = new HashSet<XD_203>();
            this.YD_203 = new HashSet<YD_203>();
            this.ZD_203 = new HashSet<ZD_203>();
            this.EXT_USERS = new HashSet<EXT_USERS>();
            this.EXT_ANALS = new HashSet<EXT_ANALS>();
            this.TYPES = new HashSet<TYPES>();
            this.SD_201 = new HashSet<SD_201>();
        }
    
        public decimal DOC_CODE { get; set; }
        public string SHPZ_NAME { get; set; }
        public short SHPZ_DELETED { get; set; }
        public Nullable<int> OP_CODE { get; set; }
        public string SHPZ_SHIRF { get; set; }
        public Nullable<short> SHPZ_1OSN_0NO { get; set; }
        public Nullable<decimal> SHPZ_STATIA_DC { get; set; }
        public Nullable<decimal> SHPZ_ELEMENT_DC { get; set; }
        public Nullable<short> SHPZ_PODOTCHET { get; set; }
        public Nullable<short> SHPZ_1DOHOD_0_RASHOD { get; set; }
        public Nullable<short> SHPZ_1TARIFIC_0NO { get; set; }
        public Nullable<short> SHPZ_1ZARPLATA_0NO { get; set; }
        public Nullable<short> SHPZ_NOT_USE_IN_OTCH_DDS { get; set; }
        public Nullable<System.DateTime> UpdateDate { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<AccuredAmountForClientRow> AccuredAmountForClientRow { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<AccuredAmountOfSupplierRow> AccuredAmountOfSupplierRow { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ND_303> ND_303 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<OffBalanceSheetInRow> OffBalanceSheetInRow { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<OffBalanceSheetOutRow> OffBalanceSheetOutRow { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SD_117> SD_117 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SD_120> SD_120 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SD_122> SD_122 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SD_140> SD_140 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SD_161> SD_161 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SD_17> SD_17 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SD_187> SD_187 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SD_187> SD_1871 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SD_251> SD_251 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SD_302> SD_302 { get; set; }
        public virtual SD_99 SD_99 { get; set; }
        public virtual SD_97 SD_97 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SD_32> SD_32 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SD_327> SD_327 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SD_33> SD_33 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SD_34> SD_34 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SD_73> SD_73 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SD_77> SD_77 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SD_83> SD_83 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TD_101> TD_101 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TD_110> TD_110 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TD_153> TD_153 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TD_165> TD_165 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TD_17> TD_17 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TD_203> TD_203 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TD_219> TD_219 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TD_222> TD_222 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TD_24_2> TD_24_2 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TD_24> TD_24 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TD_26> TD_26 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TD_281> TD_281 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TD_299> TD_299 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TD_46> TD_46 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TD_71> TD_71 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TD_84> TD_84 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UD_203> UD_203 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UD_306> UD_306 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UD_46> UD_46 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UD_83> UD_83 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<WD_48> WD_48 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<XD_203> XD_203 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<YD_203> YD_203 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ZD_203> ZD_203 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<EXT_USERS> EXT_USERS { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<EXT_ANALS> EXT_ANALS { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TYPES> TYPES { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SD_201> SD_201 { get; set; }
    }
}
