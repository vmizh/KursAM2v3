using System;
using System.Collections.ObjectModel;
using Core;
using Core.EntityViewModel;
using Core.ViewModel.Base;
using Core.WindowsManager;
using KursAM2.View.KursReferences.UC;

namespace KursAM2.ViewModel.Reference
{
    public class SimpleObjectsSelectWindowViewModel : RSWindowViewModelBase
    {
        private ISimpleObject myCurrentObject;
        private SelectSimpleObjects myDataUserControl;

        public SimpleObjectsSelectWindowViewModel()
        {
            myDataUserControl = new SelectSimpleObjects();
            LoadReference();
        }

        public SimpleObjectsSelectWindowViewModel(string windowName) : this()
        {
            WindowName = windowName;
        }

        public ObservableCollection<ISimpleObject> ObjectCollection { set; get; } =
            new ObservableCollection<ISimpleObject>();

        public SelectSimpleObjects DataUserControl
        {
            get => myDataUserControl;
            set
            {
                if (Equals(myDataUserControl, value)) return;
                myDataUserControl = value;
                RaisePropertyChanged();
            }
        }

        public ISimpleObject CurrentObject
        {
            get => myCurrentObject;
            set
            {
                if (Equals(myCurrentObject, value)) return;
                myCurrentObject = value;
                RaisePropertyChanged();
            }
        }

        private void LoadReference()
        {
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    foreach (var c in ctx.SD_27)
                        ObjectCollection.Add(new Warehouse(c));
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(null, ex);
            }
        }
    }
}