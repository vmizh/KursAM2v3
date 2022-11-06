using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Core;
using Core.EntityViewModel.CommonReferences;
using Core.ViewModel.Base;
using KursAM2.Managers;
using KursAM2.View.Helper;
using KursDomain;
using KursDomain.Documents.CommonReferences;
using KursDomain.Documents.Systems;
using KursDomain.Menu;

namespace KursAM2.ViewModel.StartLogin
{
    public sealed class LastDocumentWindowViewModel : RSWindowViewModelBase
    {
        #region Fields

        private LastDocumentViewModel myCurrentcLastDocument;

        #endregion

        #region Constructors

        public LastDocumentWindowViewModel()
        {
            RightMenuBar = MenuGenerator.DialogRightBar(this);
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RefreshData(null);
            LayoutName = "LastDocumentWindowView";
        }

        #endregion

        #region Commands

        public override void RefreshData(object obj)
        {
            using (var ctx = GlobalOptions.KursSystem())
            {
                LastDocuments.Clear();
                var d = DateTime.Today.AddDays(-15);
                foreach (var h in ctx.LastDocument.Where(_ => _.UserId == GlobalOptions.UserInfo.KursId
                                                              && _.DbId == GlobalOptions.DataBaseId
                                                              && _.LastOpen > d)
                    .OrderByDescending(_ => _.LastOpen))
                    LastDocuments.Add(new LastDocumentViewModel(h));
            }
        }

        public override void DocumentOpen(object obj)
        {
            DocumentsOpenManager.Open((DocumentType) CurrentLastDocument.Entity.DocType,
                // ReSharper disable once PossibleInvalidOperationException
                CurrentLastDocument.Entity.DocDC ?? 0,CurrentLastDocument.Entity.DocId);
            //Form.Close();
        }

        public override bool IsDocumentOpenAllow => CurrentLastDocument != null;

        #endregion

        #region Properties

        public ObservableCollection<LastDocumentViewModel> LastDocuments { set; get; }
            = new ObservableCollection<LastDocumentViewModel>();

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
