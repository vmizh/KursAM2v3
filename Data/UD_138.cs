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
    
    public partial class UD_138
    {
        public decimal DOC_CODE { get; set; }
        public decimal BALANS_DC { get; set; }
        public Nullable<short> EXECUTED { get; set; }
    
        public virtual SD_138 SD_138 { get; set; }
        public virtual SD_298 SD_298 { get; set; }
    }
}
