using System;
using System.ComponentModel;
using System.Configuration;
using Core.ViewModel.Base;
using KursAM2.Repositories.NomenklReturn;
using KursDomain;
using KursDomain.Documents.NomenklReturn;
using KursDomain.ICommon;
using KursDomain.Menu;
using StackExchange.Redis;

namespace KursAM2.ViewModel.Logistiks.NomenklReturn
{
    public class NomenklReturnOfClientWindowViewModel : RSWindowViewModelBase, IDataErrorInfo
    {
        #region Fields

        private readonly INomenklReturnOfClientRepository myRepository = new NomenklReturnOfClientRepository();
        private NomenklReturnOfClientViewModel myDocument;

        private readonly ConnectionMultiplexer redis;
        private readonly ISubscriber mySubscriber;

        #endregion

        #region Constructors
        public NomenklReturnOfClientWindowViewModel(Guid? id)
        {
            redis = ConnectionMultiplexer.Connect(ConfigurationManager.AppSettings["redis.connection"]);
            mySubscriber = redis.GetSubscriber();
            LeftMenuBar = GlobalOptions.UserInfo.IsAdmin
                ? MenuGenerator.DocWithCustomizeFormDocumentLeftBar(this)
                : MenuGenerator.DocWithRowsLeftBar(this);
            RightMenuBar = MenuGenerator.StandartDocWithDeleteRightBar(this);
            if (id is null)
            {
                Document = myRepository.CrateNewEmpty() as NomenklReturnOfClientViewModel;
                // ReSharper disable once PossibleNullReferenceException
                Document.State = RowStatus.NewRow;
            } 
            else
            {
                Document = myRepository.Get(id.Value) as NomenklReturnOfClientViewModel;
                // ReSharper disable once PossibleNullReferenceException
                Document.State = RowStatus.NotEdited;
            }


        }

        #endregion

        #region Properties

        public override string WindowName => "Возврат товара от клиента";
        public override string LayoutName => "NomenklReturnOfClientWindow";

        public NomenklReturnOfClientViewModel Document
        {
            get => myDocument;
            set
            {
                if (Equals(myDocument, value)) return;
                myDocument = value;
                RaisePropertyChanged();
            }
        }


        #endregion
        
        #region ErrirInfo

        public string this[string columnName] => throw new System.NotImplementedException();

        public string Error { get; }

        #endregion
       
    }
}
