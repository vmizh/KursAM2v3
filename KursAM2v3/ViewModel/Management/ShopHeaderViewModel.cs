﻿using Core.ViewModel.Base;

namespace KursAM2.ViewModel.Management
{
    public class ShopHeaderViewModel : RSViewModelBase
    {
        public decimal Summa { set; get; }
        public decimal Sebestoimost { set; get; }
        public decimal Result { set; get; }
    }
}