using System;

namespace Calculates.Materials
{
    public class LoaderOperation
    {
        public Guid Id { set; get; }
        public decimal DOC_CODE { set; get; }
        public int CODE { set; get; }
        public DateTime Date { set; get; }
        public decimal OperType { set; get; }
        public decimal NomenklDC { set; get; }
        public decimal Receipt { set; get; }
        public decimal Expense { set; get; }
        public decimal? SkladPolDC { set; get; }
        public decimal? SkladOtprDC { set; get; }
        public decimal? KontrPolDC { set; get; }
        public decimal? KontrOtpravDC { set; get; }
        public decimal? SummaIn { set; get; }
    }
}