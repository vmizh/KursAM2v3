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
    
    public partial class VD_17
    {
        public int SHTAT_CODE { get; set; }
        public System.DateTime FROM_MONTH { get; set; }
        public decimal OKLAD { get; set; }
        public double NADBAVKA { get; set; }
        public double PRIMIA_PERCENT { get; set; }
        public double KOEF_VREDN { get; set; }
        public double KOEF_VREDN_WINTER { get; set; }
        public short RAZRIAD { get; set; }
        public int GRT_ID { get; set; }
        public Nullable<decimal> PRIKAZ_O_NAZ_OKLADA_DC { get; set; }
        public Nullable<int> PRIKAZ_O_NAZ_OKLADA_CODE { get; set; }
    
        public virtual SD_271 SD_271 { get; set; }
        public virtual TD_17 TD_17 { get; set; }
    }
}
