using System;
using System.Runtime.Serialization;
using Core;
using Core.ViewModel.Base;
using Core.ViewModel.Common;
using Data;

namespace KursAM2.ViewModel.Management
{
    [DataContract]
    public class KonragentBalansRowViewModel : RSViewModelBase
    {
        private Project myProject;

        [DataMember] public string DocName { get; set; }

        [DataMember] public string DocNum { get; set; }

        [DataMember] public string DocExtNum { get; set; }

        [DataMember] public DateTime DocDate { get; set; }

        [DataMember] public decimal CrsKontrIn { get; set; }

        [DataMember] public decimal CrsKontrOut { get; set; }

        [DataMember] public decimal DocDC { get; set; }

        [DataMember] public int DocRowCode { get; set; }

        [DataMember] public DocumentType DocTypeCode { get; set; }

        [DataMember] public decimal CrsOperIn { get; set; }

        [DataMember] public decimal CrsOperOut { get; set; }

        [DataMember] public decimal CrsOperDC { get; set; }

        [DataMember] public decimal CrsOperRate { get; set; }

        [DataMember] public decimal CrsUchRate { get; set; }

        [DataMember] public decimal Nakopit { set; get; }

        [DataMember]
        public Project Project
        {
            get => myProject;
            set
            {
                if (myProject != null && myProject.Equals(value)) return;
                myProject = value;
                RaisePropertyChanged();
            }
        }

        public override decimal DocCode => DocDC;

        public static KonragentBalansRowViewModel DbToViewModel(KONTR_BALANS_OPER_ARC db)
        {
            return new KonragentBalansRowViewModel
            {
                DocNum = db.DOC_NUM,
                DocDC = db.DOC_DC,
                DocDate = db.DOC_DATE,
                DocName = db.DOC_NAME,
                CrsKontrIn = (decimal) db.CRS_KONTR_IN,
                CrsKontrOut = (decimal) db.CRS_KONTR_OUT,
                CrsOperDC = db.OPER_CRS_DC,
                CrsOperIn = (decimal) db.CRS_OPER_IN,
                CrsOperOut = (decimal) db.CRS_OPER_OUT,
                DocTypeCode = (DocumentType) db.DOC_TYPE_CODE,
                DocRowCode = db.DOC_ROW_CODE,
                CrsOperRate = (decimal) db.OPER_CRS_RATE,
                Nakopit = 0,
                CrsUchRate = (decimal) db.UCH_CRS_RATE
            };
        }

        public static KonragentBalansRowViewModel DbToViewModel(KONTR_BALANS_OPER_ARC db, decimal nakopit)
        {
            var ret = DbToViewModel(db);
            ret.Nakopit = nakopit;
            return ret;
        }
    }
}