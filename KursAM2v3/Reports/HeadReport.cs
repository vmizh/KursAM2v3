using DevExpress.XtraReports.UI;
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using DevExpress.Xpf.Core;

namespace KursAM2.Reports
{
    public partial class HeadReport : DevExpress.XtraReports.UI.XtraReport
    {
        public HeadReport()
        {
            InitializeComponent(); 
            ApplicationThemeHelper.ApplicationThemeName = Theme.MetropolisLightName;
        }

    }
}
