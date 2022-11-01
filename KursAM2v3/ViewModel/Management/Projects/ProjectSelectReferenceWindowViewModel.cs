using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Core;
using Core.EntityViewModel.CommonReferences;
using Core.ViewModel.Base;
using Core.WindowsManager;
using KursAM2.View.Management;
using KursAM2.View.Management.Controls;
using KursDomain.Documents.CommonReferences;
using KursDomain.ICommon;

namespace KursAM2.ViewModel.Management.Projects
{
    public class ProjectSelectReferenceWindowViewModel : RSWindowViewModelBase
    {
        private Project myCurrentProject;
        private ProjectReferenceSelectDialogUI myDataUserControl;

        public ProjectSelectReferenceWindowViewModel()
        {
            myDataUserControl = new ProjectReferenceSelectDialogUI();
            // ReSharper disable once VirtualMemberCallInConstructor
            RefreshData(null);
        }

        public ProjectReferenceSelectDialogUI DataUserControl
        {
            get => myDataUserControl;
            set
            {
                if (Equals(myDataUserControl, value)) return;
                myDataUserControl = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<Project> ProjectCollection { set; get; } =
            new ObservableCollection<Project>();

        public Project CurrentProject
        {
            get => myCurrentProject;
            set
            {
                if (myCurrentProject != null && myCurrentProject.Equals(value)) return;
                myCurrentProject = value;
                RaisePropertyChanged();
            }
        }

        public override void RefreshData(object obj)
        {
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    ProjectCollection = new ObservableCollection<Project>();
                    foreach (var p in ctx.Projects.ToList())
                        ProjectCollection.Add(new Project(p) {State = RowStatus.NotEdited});
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }
        }

        #region Commands

        public ICommand OpenProjectReferenceCommand
        {
            get { return new Command(OpenProjectReference, _ => IsCanOpenProjectReference()); }
        }

        private bool IsCanOpenProjectReference()
        {
            return true;
        }

        private void OpenProjectReference(object obj)
        {
            var form = new ProjectsView
            {
                Owner = Application.Current.MainWindow
            };
            form.DataContext = new ProjectWindowViewModel
            {
                Form = form
            };
            form.Show();
        }

        #endregion
    }
}
