using Core;
using Core.EntityViewModel.AktSpisaniya;
using Core.ViewModel.Base;
using Data;
using Data.Repository;
using System;

namespace KursAM2.ViewModel.Logistiks.AktSpisaniya
{
    class AktSpisaniyaNomenkl_TitleWIndowViewModel : RSWindowViewModelBase
    {
        #region Fields

        private AktSpisaniyaNomenkl_TitleViewModel myDocument;
        private Guid myId;
        private AktSpisaniyaRowViewModel myCurrentRow;

        private readonly UnitOfWork<ALFAMEDIAEntities> unitOfWork
            = new UnitOfWork<ALFAMEDIAEntities>(new ALFAMEDIAEntities(GlobalOptions.SqlConnectionString));

        public readonly GenericKursDBRepository<AktSpisaniyaNomenkl_Title> BaseRepository;

        #endregion

        #region Properties

        public AktSpisaniyaNomenkl_TitleViewModel Document
        {
            get => myDocument;
            set
            {
                if (myDocument == value)
                    return;
                myDocument = value;
                RaisePropertyChanged();
            }
        }

        public AktSpisaniyaRowViewModel CurrentRow
        {
            get => myCurrentRow;
            set
            {
                if (myCurrentRow == value)
                    return;
                myCurrentRow = value;
                RaisePropertyChanged();
            }
        }
        public override Guid Id
        {
            get => myId;
            set
            {
                if (myId == value) return;
                myId = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region Methods

        public override bool IsCorrect()
        {
            return Document.IsCorrect();
        }

        #endregion

        #region Command

        public override bool IsCanSaveData =>
            Document != null && Document.State != RowStatus.NotEdited;

        #endregion



    }
}
