using System;
using DevExpress.Data;
using DevExpress.Xpf.Grid;

namespace KursAM2.ViewModel.Finance
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class GridControlSummaryItem
    {
        public SummaryItemType Type { get; set; }
        public GridSummaryItemAlignment Alignment { get; set; }
        public string FieldName { get; set; }
        public Type FieldType { set; get; }
        public string Format { get; set; }
        public string Tag { get; set; }
        public decimal Summa { get; set; }
    }
}