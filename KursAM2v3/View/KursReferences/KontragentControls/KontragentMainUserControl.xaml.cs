using System;

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
            //Loaded += (o, args) => LayoutManagerOld.Load(myLayoutFileName, KontragentMainLayoutControl);
            //Unloaded += (o, args) => LayoutManagerOld.Save(myLayoutFileName, KontragentMainLayoutControl);
        }
    }
}