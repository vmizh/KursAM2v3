﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Core;
using Core.EntityViewModel.CommonReferences;
using Core.ViewModel.Base;
using Data;
using DevExpress.Mvvm;
using KursAM2.Managers;
using KursAM2.Repositories.DogovorsRepositories;
using KursAM2.View.Dogovors;
using KursDomain;
using KursDomain.Documents.CommonReferences;
using KursDomain.Documents.Dogovora;
using KursDomain.Menu;
using KursDomain.Repository;

namespace KursAM2.ViewModel.Dogovora
{
    public sealed class DogovorClientSearchViewModel : RSWindowSearchViewModelBase
    {
        // ReSharper disable once NotAccessedField.Local
        public readonly GenericKursDBRepository<DogovorClient> BaseRepository;

        // ReSharper disable once NotAccessedField.Local
        public readonly IDogovorClientRepository DogovorClientRepository;

        private readonly UnitOfWork<ALFAMEDIAEntities> unitOfWork
            = new UnitOfWork<ALFAMEDIAEntities>(new ALFAMEDIAEntities(GlobalOptions.SqlConnectionString));

        private DogovorClientViewModel myCurrentDocument;
        private DateTime myDateEnd;
        private DateTime myDateStart;

        #region Constructors

        public DogovorClientSearchViewModel()
        {
            BaseRepository = new GenericKursDBRepository<DogovorClient>(unitOfWork);
            DogovorClientRepository = new DogovorClientRepository(unitOfWork);
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.StandartSearchRightBar(this);
            DateEnd = DateTime.Today;
            DateStart = DateEnd.AddDays(-30);
            IsCanDocNew = true;
            IsDocNewCopyAllow = true;
            IsDocNewCopyRequisiteAllow = true;
            LayoutName = "DogovorClientSearchView";
        }

        #endregion

        #region Methods

        #endregion

        #region Properties

        public ObservableCollection<DogovorClientViewModel> Documents { set; get; } =
            new ObservableCollection<DogovorClientViewModel>();

        public DateTime DateEnd
        {
            get => myDateEnd;
            set
            {
                if (myDateEnd == value) return;
                myDateEnd = value;
                RaisePropertyChanged();
                if (myDateStart < myDateEnd) return;
                myDateStart = myDateEnd;
                RaisePropertyChanged(nameof(DateStart));
            }
        }

        public DateTime DateStart
        {
            get => myDateStart;
            set
            {
                if (myDateStart == value) return;
                myDateStart = value;
                RaisePropertyChanged();
                if (myDateStart <= myDateEnd) return;
                myDateEnd = myDateStart;
                RaisePropertyChanged(nameof(DateEnd));
            }
        }

        public DogovorClientViewModel CurrentDocument
        {
            get => myCurrentDocument;
            set
            {
                // ReSharper disable once PossibleUnintendedReferenceComparison
                if (myCurrentDocument == value) return;
                myCurrentDocument = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region Commands

        protected override void DocumentOpen(object obj)
        {
            DocumentsOpenManager.Open(DocumentType.DogovorClient, 0, CurrentDocument.Id);
        }

        public override void DocNewCopy(object form)
        {
            if (CurrentDocument == null) return;
            var ctx = new DogovorClientWindowViewModel(CurrentDocument.Id);
            ctx.SetAsNewCopy(true);
            var frm = new DogovorClientView
            {
                Owner = Application.Current.MainWindow,
                DataContext = ctx
            };
            ctx.Form = frm;
            frm.Show();
        }

        public override void DocNewEmpty(object form)
        {
            var view = new DogovorClientView
            {
                Owner = Application.Current.MainWindow
            };
            var ctx = new DogovorClientWindowViewModel(null)
            {
                Form = view,
                ParentFormViewModel = this
            };
            view.DataContext = ctx;
            view.Show();
        }

        public override void DocNewCopyRequisite(object form)
        {
            if (CurrentDocument == null) return;
            var ctx = new DogovorClientWindowViewModel(CurrentDocument.Id);
            ctx.SetAsNewCopy(false);
            var frm = new DogovorClientView
            {
                Owner = Application.Current.MainWindow,
                DataContext = ctx
            };
            ctx.Form = frm;
            frm.Show();
        }


        public override bool IsDocumentOpenAllow => CurrentDocument != null;

        private Task Load()
        {
            var ctx = GlobalOptions.GetEntities();
            var res = Task.Factory.StartNew(() =>
                new List<DogovorClient>(ctx.DogovorClient.Where(_ => _.DogDate >= DateStart && _.DogDate <= DateEnd)));
            Documents.Clear();
            foreach (var d in res.Result) Documents.Add(new DogovorClientViewModel(d));
            RaisePropertyChanged(nameof(Documents));
            DispatcherService.BeginInvoke(SplashScreenService.HideSplashScreen);
            return res;
        }

        public override async void RefreshData(object data)
        {
            SplashScreenService.ShowSplashScreen();
            base.RefreshData(null);
            await Load();
        }

        #endregion
    }
}
