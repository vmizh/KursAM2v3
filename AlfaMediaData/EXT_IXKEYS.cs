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
    
    public partial class EXT_IXKEYS
    {
        public int IX_ID { get; set; }
        public int EXCOL_ID { get; set; }
        public string IXC_FUNCTION { get; set; }
        public Nullable<short> IXC_NUM_PP { get; set; }
        public int EXTT_ID { get; set; }
    
        public virtual EXT_COLS EXT_COLS { get; set; }
    }
}
