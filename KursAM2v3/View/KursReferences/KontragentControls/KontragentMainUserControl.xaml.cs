﻿using System;
using DevExpress.Xpf.Core;
using KursDomain;

namespace KursAM2.View.KursReferences.KontragentControls
{
    /// <summary>
    ///     Interaction logic for KontragentMainUserControl.xaml
    /// </summary>
    public partial class KontragentMainUserControl
    {
        private readonly string myLayoutFileName =
            $"{Environment.CurrentDirectory}\\Layout\\{nameof(KontragentMainUserControl)}.xml";

        public KontragentMainUserControl()
        {
            InitializeComponent();
            balansCheckEdit.IsEnabled = GlobalOptions.UserInfo.IsAdmin;

        }
    }
}
