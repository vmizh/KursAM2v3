using System;
using System.Collections.Generic;

namespace Core.ViewModel.Base
{
    public class EntityLoadCodition
    {
        public bool IsShort { set; get; } = true;
        public string SearchText { set; get; }
        public DateTime? Start { set; get; }
        public DateTime? End { set; get; }
        public List<decimal> DocCodeList { set; get; }
        public List<Guid> IdList { set; get; }
        public bool IsAll { set; get; } = false;
    }
}