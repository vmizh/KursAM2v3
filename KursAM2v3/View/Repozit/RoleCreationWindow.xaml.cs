﻿using System.Windows;


namespace KursAM2.View.Repozit
{
    /// <summary>
    /// Interaction logic for RoleCreationWindow.xaml
    /// </summary>
    public partial class RoleCreationWindow 
    {
        public RoleCreationWindow()
        {
            InitializeComponent();
            LayoutManager = new LayoutManager.LayoutManager(GetType().Name, this, LayoutControl);
            Loaded += RoleCreationWindow_Loaded;
            Closing += RoleCreationWindow_Closing;
        }

        private void RoleCreationWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            LayoutManager.Save();
        }

        private void RoleCreationWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LayoutManager.Load();
        }

        public LayoutManager.LayoutManager LayoutManager { get; set; }

        public string LayoutManagerName { get; set; }
    }
    
}