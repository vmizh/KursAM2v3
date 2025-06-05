using System;
using System.Collections.ObjectModel;
using Core.ViewModel.Base;
using KursDomain.WindowsManager.WindowsManager;
using KursAM2.View.KursReferences.UC;
using KursDomain;
using KursDomain.ICommon;

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
                    var sql = "SELECT DISTINCT storeDC FROM NomenklMoveForCalc nmfc " +
                              "INNER JOIN HD_27 h ON h.DOC_CODE = nmfc.StoreDC " +
                              "INNER JOIN  EXT_USERS U ON U.USR_ID = H.USR_ID " +
                              $"AND UPPER(U.USR_NICKNAME) = UPPER('{GlobalOptions.UserInfo.NickName}')";
                    var skls = ctx.Database.SqlQuery<decimal>(sql);
                    foreach (var s in skls)
                    {
                        var o = new ReferenceName();
                        o.LoadFromIName((IName)GlobalOptions.ReferencesCache.GetWarehouse(s));
                        ObjectCollection.Add(o);
                    }
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(null, ex);
            }
        }
    }

    public class ReferenceName : ISimpleObject
    {
        public string Name { get; set; }
        public string Note { get; set; }

        public void LoadFromIName(IName obj)
        {
            Name = obj.Name;
            Note = obj.Notes;
        }
    }
}
