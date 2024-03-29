﻿using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using Core.ViewModel.Base;
using Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using KursAM2.View.KursReferences.UC;
using KursDomain.Documents.Systems;
using KursDomain.Menu;

namespace KursAM2.ViewModel.Reference.Dialogs
{
    public sealed class SelectKursMainMenuItemViewModel : RSWindowViewModelBase
    {
        private readonly KursSystemEntities context;
        private KursMenuItemViewModel myCurrentMenu;
        private ICurrentWindowService winCurrentService;

        public SelectKursMainMenuItemViewModel(KursSystemEntities ctx)
        {
            context = ctx;
            RightMenuBar = MenuGenerator.DialogStandartBar(this);
            RefreshData(null);
        }

        public UserControl CustomDataUserControl { set; get; } = new SelectKursMainMenuItem();

        public override string LayoutName => "SelectKursMainMenuItemViewModel";

        public List<KursMenuItemViewModel> MenuItems { set; get; } = new List<KursMenuItemViewModel>();

        public KursMenuItemViewModel CurrentMenu
        {
            get => myCurrentMenu;
            set
            {
                if (myCurrentMenu == value) return;
                myCurrentMenu = value;
                RaisePropertyChanged();
            }
        }

        #region Command

        public override bool IsOkAllow()
        {
            return CurrentMenu != null;
        }

        public override void Ok(object obj)
        {
            winCurrentService = this.GetService<ICurrentWindowService>();
            if (winCurrentService != null) winCurrentService.Close();
        }

        public override void Cancel(object obj)
        {
            winCurrentService = this.GetService<ICurrentWindowService>();
            if (winCurrentService != null)
            {
                CurrentMenu = null;
                winCurrentService.Close();
            }
        }

        public override void RefreshData(object obj)
        {
            if (context == null) return;
            MenuItems.Clear();
            foreach (var d in context.KursMenuItem.Where(_ => _.IsSign))
                MenuItems.Add(new KursMenuItemViewModel(d));
        }

        #endregion
    }
}
