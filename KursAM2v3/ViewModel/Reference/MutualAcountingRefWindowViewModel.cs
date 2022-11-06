using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Core;
using Core.ViewModel.Base;
using Core.WindowsManager;
using KursDomain;
using KursDomain.Documents.Vzaimozachet;
using KursDomain.ICommon;
using KursDomain.Menu;

namespace KursAM2.ViewModel.Reference
{
    public class MutualAcountingRefWindowViewModel : RSWindowViewModelBase
    {
        public MutualAcountingRefWindowViewModel()
        {
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.ReferenceRightBar(this);
            // ReSharper disable once VirtualMemberCallInConstructor
            RefreshData(null);
        }

        public ObservableCollection<SD_111ViewModel> ReferenceCollection { set; get; } =
            new ObservableCollection<SD_111ViewModel>();

        public override bool IsCanSaveData => ReferenceCollection.Any(_ => _.State != RowStatus.NotEdited);

        public override void RefreshData(object obj)
        {
            ReferenceCollection.Clear();
            using (var ctx = GlobalOptions.GetEntities())
            {
                foreach (var item in ctx.SD_111.ToList())
                    ReferenceCollection.Add(new SD_111ViewModel(item) { State = RowStatus.NotEdited });
            }
        }

        public override void SaveData(object data)
        {
            foreach (var r in ReferenceCollection.Where(_ => _.State != RowStatus.NotEdited))
                r.Save();
            RefreshData(null);
        }

        public override void CloseWindow(object form)
        {
            var WinManager = new WindowManager();
            var vin = form as Window;
            if (IsCanSaveData)
            {
                var res = MessageBox.Show("Были внесены изменения, сохранить?", "Запрос",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);
                if (res == MessageBoxResult.Cancel) return;
                if (res == MessageBoxResult.No)
                    vin?.Close();
                if (res != MessageBoxResult.Yes) return;
                try
                {
                    SaveData(null);
                    vin?.Close();
                }
                catch (Exception ex)
                {
                    WinManager.ShowWinUIMessageBox(ex.Message, "Ошибка");
                }
            }
            else
            {
                vin?.Close();
            }
        }
    }
}
