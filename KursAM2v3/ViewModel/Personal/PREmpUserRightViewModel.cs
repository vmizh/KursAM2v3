using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Core;
using Core.EntityViewModel;
using Core.ViewModel.Base;
using Core.ViewModel.Base.Column;
using Core.WindowsManager;
using Data;
using DevExpress.Xpf.Grid;

namespace KursAM2.ViewModel.Personal
{
    public class PREmpUserRightViewModel : RSViewModelBase, IViewModel<PREmpUserRight>
    {
        public PREmpUserRightViewModel()
        {
            TableViewInfo = new GridTableViewInfo();
            TableViewInfo.Generate(typeof(PREmpUserRight));
            Source = new ObservableCollection<PREmpUserRight>();
            DeletedItems = new List<PREmpUserRight>();
        }

        public void Load()
        {
            try
            {
                var pers = GlobalOptions.GetEntities().SD_2.ToList();
                foreach (var item in GlobalOptions.GetEntities().EXT_USERS.ToList())
                    Source.Add(new PREmpUserRight
                    {
                        Name = item.USR_NICKNAME,
                        UserName = item.USR_FULLNAME
                    });
                var dcList = new List<EMP_USER_RIGHTS>();
                dcList.AddRange(GlobalOptions.GetEntities().EMP_USER_RIGHTS.ToList());
                var userList = new List<string>();
                foreach (var r in dcList.Where(r => userList.All(t => t != r.USER.ToUpper())))
                    userList.Add(r.USER.ToUpper());
                foreach (var name in userList)
                {
                    var usr = Source.FirstOrDefault(t => t.Name.ToUpper() == name);
                    if (usr == null) continue;
                    var name1 = name;
                    foreach (var empDC in dcList.Where(t => t.USER.ToUpper() == name1))
                        // ReSharper disable once PossibleNullReferenceException
                        usr.Employee.Add(
                            pers.Where(t => t.DOC_CODE == empDC.EMP_DC)
                                .Select(s => new Employee(s))
                                .FirstOrDefault());
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(null, ex);
            }
        }

        #region IViewModel<PREmpUserRight> Members

        public GridTableViewInfo TableViewInfo { get; set; }
        public GridControl Grid { get; set; }
        public ObservableCollection<PREmpUserRight> Source { get; set; }
        public ObservableCollection<PREmpUserRight> SourceAll { get; set; }
        public List<PREmpUserRight> DeletedItems { get; set; }

        public void ResetSummary()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}