﻿using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using Core;
using Core.EntityViewModel.CommonReferences;
using Core.EntityViewModel.Systems;
using Core.Invoices.EntityViewModel;
using Core.Menu;
using Core.ViewModel.Base;
using KursAM2.Managers;

namespace KursAM2.ViewModel.StartLogin
{
    public sealed class LastUsersDocumentWindowViewModel : RSWindowViewModelBase
    {
        #region Fields

        private LastDocumentViewModel myCurrentcLastDocument;

        #endregion

        #region Constructors

        public LastUsersDocumentWindowViewModel()
        {
            RightMenuBar = MenuGenerator.DialogRightBar(this);
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RefreshData(null);
            LayoutName = "LastIUsersDocumentWindowView";
        }

        #endregion

        #region Commands

        public override void RefreshData(object obj)
        {
            using (var ctx = GlobalOptions.KursSystem())
            {
                LastDocuments.Clear();
                var d = DateTime.Today.AddDays(-30);
                foreach (var h in ctx.LastDocument.Include(_ => _.Users)
                    .Where(_ => _.DbId == GlobalOptions.DataBaseId && _.LastOpen > d)
                    .OrderByDescending(_ => _.LastOpen))
                {
                    LastDocuments.Add(new LastUsersDocumentViewModel(h));
                }
            }
        }

        public override void DocumentOpen(object obj)
        {
            // ReSharper disable once PossibleInvalidOperationException
            DocumentsOpenManager.Open((DocumentType)CurrentLastDocument.Entity.DocType, (decimal)CurrentLastDocument.Entity.DocDC);
            Form.Close();
        }

        public override bool IsDocumentOpenAllow => CurrentLastDocument != null;

        #endregion

        #region Properties

        public ObservableCollection<LastUsersDocumentViewModel> LastDocuments { set; get; }
            = new ObservableCollection<LastUsersDocumentViewModel>();

        public LastDocumentViewModel CurrentLastDocument
        {
            get => myCurrentcLastDocument;
            set
            {
                if (myCurrentcLastDocument == value) return;
                myCurrentcLastDocument = value;
                RaisePropertyChanged();
            }
        }

        #endregion


    }
}