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
    
    public partial class EXT_DOC2TAB
    {
        public int TABLE_CODE { get; set; }
        public Nullable<int> EXTT_ID { get; set; }
        public string MASTER_OR_DETAIL { get; set; }
    
        public virtual EXT_TBLS EXT_TBLS { get; set; }
        public virtual TYPES TYPES { get; set; }
    }
}
