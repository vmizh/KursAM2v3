using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Core;
using Core.ViewModel.Common;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Grid;
using KursAM2.ViewModel.Management.Projects;
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
            LayoutManager = new LayoutManager.LayoutManager(GetType().Name, this, mainLayoutControl);
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
                ctx.ProjectList = new List<Project>(MainReferences.Projects.Values.ToList());
        }

        private void ButtonEditSettings_OnDefaultButtonClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is ProjectProviderPrihodWindowViewModel ctx)
                ctx.ProjectList = new List<Project>(MainReferences.Projects.Values.ToList());
        }
    }
}