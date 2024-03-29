﻿using System.ComponentModel;
using System.Linq;
using System.Windows;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Grid;
using KursAM2.ViewModel.Management.Projects;
using KursDomain;
using KursDomain.References;
using LayoutManager;

namespace KursAM2.View.Management
{
    /// <summary>
    ///     Interaction logic for ProjectProviderPrihodView.xaml
    /// </summary>
    public partial class ProjectProviderPrihodView : ILayout
    {
        public ProjectProviderPrihodView()
        {
            InitializeComponent();
            LayoutManager = new LayoutManager.LayoutManager(GlobalOptions.KursSystem(), GetType().Name, this, mainLayoutControl);
            Loaded += ProjectProviderPrihodView_Loaded;
            Closing += ProjectProviderPrihodView_Closing;
        }

        public LayoutManager.LayoutManager LayoutManager { get; set; }
        public string LayoutManagerName { get; set; }

        public void SaveLayout()
        {
            LayoutManager.Save();
        }

        private void ProjectProviderPrihodView_Closing(object sender, CancelEventArgs e)
        {
            gridDistDocuments.SelectionMode = MultiSelectMode.None;
            LayoutManager.Save();
        }

        private void ProjectProviderPrihodView_Loaded(object sender, RoutedEventArgs e)
        {
            LayoutManager.Load();
            gridDistDocuments.SelectionMode = MultiSelectMode.None;
        }

        private void PopupBaseEdit_OnPopupOpening(object sender, OpenPopupEventArgs e)
        {
            if (DataContext is ProjectProviderPrihodWindowViewModel ctx)
                ctx.ProjectList = GlobalOptions.ReferencesCache.GetProjectsAll().Cast<Project>().ToList();
        }

        private void ButtonEditSettings_OnDefaultButtonClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is ProjectProviderPrihodWindowViewModel ctx)
                ctx.ProjectList = GlobalOptions.ReferencesCache.GetProjectsAll().Cast<Project>().ToList();
        }
    }
}
