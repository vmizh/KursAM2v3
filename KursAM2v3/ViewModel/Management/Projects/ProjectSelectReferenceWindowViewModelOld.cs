using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Core.ViewModel.Base;
using Core.WindowsManager;
using KursAM2.View.Management;
using KursAM2.View.Management.Controls;
using KursDomain;
using KursDomain.ICommon;
using KursDomain.References;

namespace KursAM2.ViewModel.Management.Projects
{
    public class ProjectSelectReferenceWindowViewModelOld : RSWindowViewModelBase
    {
        private Project myCurrentProject;
        private ProjectReferenceSelectDialogUI myDataUserControl;

        public ProjectSelectReferenceWindowViewModelOld()
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

        public ObservableCollection<ProjectViewModel> ProjectCollection { set; get; } =
            new ObservableCollection<ProjectViewModel>();

        public Project CurrentProject
        {
            get => myCurrentProject;
            set
            {
                if (Equals(myCurrentProject, value)) return;
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
                    ProjectCollection = new ObservableCollection<ProjectViewModel>();
                    foreach (var p in ctx.Projects.ToList())
                    {
                        ProjectCollection.Add(new ProjectViewModel(p) {State = RowStatus.NotEdited});
                    }
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
            //var form = new ProjectsView
            //{
            //    Owner = Application.Current.MainWindow
            //};
            //form.DataContext = new ProjectWindowViewModelOld
            //{
            //    Form = form
            //};
            //form.Show();
        }

        #endregion
    }
}
