﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Core.ViewModel.Base;
using Data;
using KursAM2.Managers;
using KursAM2.Repositories;
using KursAM2.View.Base;
using KursAM2.View.Logistiks.AktSpisaniya;
using KursDomain;
using KursDomain.Documents.AktSpisaniya;
using KursDomain.Documents.CommonReferences;
using KursDomain.ICommon;
using KursDomain.Managers;
using KursDomain.Menu;
using KursDomain.Repository;

namespace KursAM2.ViewModel.Logistiks.AktSpisaniya
{
    public sealed class AktSpisaniyaNomenklSearchViewModel : RSWindowViewModelBase
    {
        public readonly GenericKursDBRepository<AktSpisaniyaNomenkl_Title> BaseRepository;
        private readonly NomenklManager2 nomenklManager = new NomenklManager2(GlobalOptions.GetEntities());

        // ReSharper disable once InconsistentNaming
        private readonly UnitOfWork<ALFAMEDIAEntities> unitOfWork =
            new UnitOfWork<ALFAMEDIAEntities>(new ALFAMEDIAEntities(GlobalOptions.SqlConnectionString));

        public IAktSpisaniyaNomenkl_TitleRepository AktSpisaniyaNomenklRepository;

        // ReSharper disable once InconsistentNaming
        private AktSpisaniyaNomenklTitleViewModel myCurrentDocument;

        // ReSharper disable once InconsistentNaming
        private DateTime myDateEnd;

        // ReSharper disable once InconsistentNaming
        private DateTime myDateStart;
        private readonly ISignatureRepository mySignatureRepository;

        #region Constructors

        public AktSpisaniyaNomenklSearchViewModel()
        {
            mySignatureRepository = new SignatureRepository(unitOfWork);
            BaseRepository = new GenericKursDBRepository<AktSpisaniyaNomenkl_Title>(unitOfWork);
            AktSpisaniyaNomenklRepository = new AktSpisaniyaNomenkl_TitleRepository(unitOfWork);
            LeftMenuBar = MenuGenerator.BaseLeftBar(this, new Dictionary<MenuGeneratorItemVisibleEnum, bool>
            {
                [MenuGeneratorItemVisibleEnum.AddSearchlist] = true
            });
            RightMenuBar = MenuGenerator.StandartSearchRightBar(this);
            EndDate = DateTime.Today;
            StartDate = new DateTime(DateTime.Today.Year, 1, 1);
            IsCanDocNew = true;
            LayoutName = "AktSpisaniyaSearchView";
            WindowName = "Акты списания материалов";
        }

        #endregion

        #region Methods

        #endregion

        #region Properties

        // ReSharper disable once CollectionNeverQueried.Global
        public ObservableCollection<AktSpisaniyaNomenklTitleViewModel> Documents { set; get; } =
            new ObservableCollection<AktSpisaniyaNomenklTitleViewModel>();

        public override void AddSearchList(object obj)
        {
            var actCtx = new AktSpisaniyaNomenklSearchViewModel();
            var  form = new StandartSearchView
            {
                Owner = Application.Current.MainWindow
            };
            form.DataContext = actCtx;
            actCtx.Form = form;
            form.Show();

        }


        public DateTime EndDate
        {
            get => myDateEnd;
            set
            {
                if (myDateEnd == value) return;
                myDateEnd = value;
                RaisePropertyChanged();
                if (myDateStart < myDateEnd) return;
                myDateStart = myDateEnd;
                RaisePropertyChanged(nameof(StartDate));
            }
        }

        public DateTime StartDate
        {
            get => myDateStart;
            set
            {
                if (myDateStart == value) return;
                myDateStart = value;
                RaisePropertyChanged();
                if (myDateStart <= myDateEnd) return;
                myDateEnd = myDateStart;
                RaisePropertyChanged(nameof(EndDate));
            }
        }

        public AktSpisaniyaNomenklTitleViewModel CurrentDocument
        {
            get => myCurrentDocument;
            set
            {
                if (myCurrentDocument == value) return;
                myCurrentDocument = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region Commands

        public override bool IsDocumentOpenAllow => CurrentDocument != null;
        public override bool IsDocNewCopyAllow => false;
        public override bool IsDocNewCopyRequisiteAllow => CurrentDocument != null;

        protected override void DocumentOpen(object form)
        {
            DocumentsOpenManager.Open(DocumentType.AktSpisaniya, 0, CurrentDocument.Id);
        }

        public override void DocNewEmpty(object form)
        {
            var frm = new AktSpisaniyaView
            {
                Owner = Application.Current.MainWindow
            };
            var ctx = new AktSpisaniyaNomenklTitleWIndowViewModel
            {
                Form = frm,
                State = RowStatus.NewRow
            };
            frm.DataContext = ctx;
            frm.Show();
        }

        public override void DocNewCopy(object obj)
        {
            if (CurrentDocument == null)
                return;
            var ctx = new AktSpisaniyaNomenklTitleWIndowViewModel(CurrentDocument.Id);
            ctx.SetAsNewCopy(true);
            var frm = new AktSpisaniyaView
            {
                Owner = Application.Current.MainWindow,
                DataContext = ctx
            };

            ctx.Form = frm;
            frm.Show();
        }

        public override void DocNewCopyRequisite(object obj)
        {
            if (CurrentDocument == null)
                return;
            var ctx = new AktSpisaniyaNomenklTitleWIndowViewModel(CurrentDocument.Id);
            ctx.SetAsNewCopy(false);
            var frm = new AktSpisaniyaView
            {
                Owner = Application.Current.MainWindow,
                DataContext = ctx
            };

            ctx.Form = frm;
            frm.Show();
        }

        public override void RefreshData(object obj)
        {
            base.RefreshData(obj);
            Documents.Clear();
            foreach (var newItem in AktSpisaniyaNomenklRepository.GetAllByDates(StartDate, EndDate).ToList()
                         .Select(d => new AktSpisaniyaNomenklTitleViewModel(d)))
            {
                newItem.IsSign = false;
                var signs = mySignatureRepository.CreateSignes(72, newItem.Id, out var issign, out var isSignNew);
                newItem.IsSign = issign;
                foreach (var r in newItem.Rows)
                    r.Prices = nomenklManager.GetNomenklPrice(r.Nomenkl.DocCode, newItem.DocDate);

                Documents.Add(newItem);
            }

            foreach (var item in Documents) item.RaisePropertyAllChanged();
            RaisePropertyChanged(nameof(Documents));
        }

        #endregion
    }
}
