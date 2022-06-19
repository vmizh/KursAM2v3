using DevExpress.Xpf.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;


namespace KursAM2.View.Repozit
{
    /// <summary>
    /// Interaction logic for UserProfileWindow.xaml
    /// </summary>
    public partial class UserProfileWindow : ThemedWindow
    {
        public UserProfileWindow()
        {
            InitializeComponent(); ApplicationThemeHelper.ApplicationThemeName = Theme.MetropolisLightName;
        }
    }
}
