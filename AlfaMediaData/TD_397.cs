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
    
    public partial class TD_397
    {
        public decimal DOC_CODE { get; set; }
        public int CODE { get; set; }
        public Nullable<decimal> PRDT_SF_DC { get; set; }
        public Nullable<decimal> PRDT_SN_DC { get; set; }
        public Nullable<decimal> PRDT_CA_DC { get; set; }
        public System.DateTime PRDT_OTRAZH_DATE { get; set; }
        public string PRDT_DOC_NUM { get; set; }
        public Nullable<System.DateTime> PRDT_DOC_DATE { get; set; }
        public Nullable<decimal> PRDT_KONTR_DC { get; set; }
        public Nullable<System.DateTime> PRDT_OPL_DATE { get; set; }
        public Nullable<decimal> PRDT_ALL_SUM_WITH_NDS { get; set; }
        public Nullable<decimal> PRDT_SUM_TO_20PR { get; set; }
        public Nullable<decimal> PRDT_20PR_SUM { get; set; }
        public Nullable<decimal> PRDT_SUM_TO_10PR { get; set; }
        public Nullable<decimal> PRDT_10PR_SUM { get; set; }
        public Nullable<decimal> PRDT_SUM_TO_0PR { get; set; }
        public Nullable<decimal> PRDT_FREE_NDS_SUM { get; set; }
        public string PRDT_PP_BOOK_BUY { get; set; }
        public Nullable<short> PRDT_CHASICH_OPLATA { get; set; }
        public Nullable<decimal> PRDT_CASH_PRIH_ORD_DC { get; set; }
        public Nullable<int> PRDT_BANK_CODE { get; set; }
        public Nullable<decimal> PRDT_ACT_VZ_DC { get; set; }
        public Nullable<int> PRDT_ACT_VZ_CODE { get; set; }
        public Nullable<decimal> PRDT_SUM_TO_18PR { get; set; }
        public Nullable<decimal> PRDT_18PR_SUM { get; set; }
    
        public virtual SD_22 SD_22 { get; set; }
        public virtual SD_33 SD_33 { get; set; }
        public virtual SD_373 SD_373 { get; set; }
        public virtual SD_397 SD_397 { get; set; }
        public virtual SD_43 SD_43 { get; set; }
        public virtual SD_84 SD_84 { get; set; }
        public virtual TD_101 TD_101 { get; set; }
        public virtual TD_110 TD_110 { get; set; }
    }
}
