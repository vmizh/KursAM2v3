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
    
    public partial class MAIN_DOCUMENT_ITEM
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public MAIN_DOCUMENT_ITEM()
        {
            this.USER_FORMS_RIGHT = new HashSet<USER_FORMS_RIGHT>();
        }
    
        public int ID { get; set; }
        public int GROUP_ID { get; set; }
        public string NAME { get; set; }
        public byte[] PICTURE { get; set; }
        public string NOTES { get; set; }
        public Nullable<int> ORDERBY { get; set; }
        public string CODE { get; set; }
    
        public virtual MAIN_DOCUMENT_GROUP MAIN_DOCUMENT_GROUP { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<USER_FORMS_RIGHT> USER_FORMS_RIGHT { get; set; }
    }
}
