using Core;

namespace KursAM2.ViewModel.Personal
{
    public class NachEmployeeForPeriod : DatePeriod
    {
        public decimal Start { set; get; }
        public decimal In { set; get; }
        public decimal Out { set; get; }
        public decimal End { set; get; }
        public decimal StartUSD { set; get; }
        public decimal StartEUR { set; get; }
        public decimal StartRUB { set; get; }
        public decimal EndUSD { set; get; }
        public decimal EndEUR { set; get; }
        public decimal EndRUB { set; get; }
        /// <summary>
        /// выплачено
        /// </summary>
        public decimal USD { set; get; }
        public decimal EUR { set; get; }
        public decimal RUB { set; get; }

        public decimal NachUSD { set; get; }
        public decimal NachEUR { set; get; }
        public decimal NachRUB { set; get; }
        public string CrsName { set; get; }
    }
}