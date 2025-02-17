using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Core.ViewModel.Base;
using Data;
using Helper;
using KursAM2.Managers;
using KursAM2.Repositories.NomenklReturn;
using KursAM2.Repositories.RedisRepository;
using KursAM2.View.Base;
using KursAM2.View.Logistiks.NomenklReturn;
using KursDomain;
using KursDomain.Documents.CommonReferences;
using KursDomain.Documents.NomenklManagement;
using KursDomain.Documents.NomenklReturn;
using KursDomain.ICommon;
using KursDomain.Menu;
using KursDomain.Repository.DocHistoryRepository;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace KursAM2.ViewModel.Logistiks.NomenklReturn
{
    public sealed class NomenklReturnOfClientSearchViewModel : RSWindowSearchViewModelBase
    {
        private readonly ALFAMEDIAEntities myContext = GlobalOptions.GetEntities();
        public NomenklReturnOfClientSearchViewModel()
        {
            myRepository = new NomenklReturnOfClientRepository(myContext);
            redis = ConnectionMultiplexer.Connect(ConfigurationManager.AppSettings["redis.connection"]);
            mySubscriber = redis.GetSubscriber();
            try
            {
                redis = ConnectionMultiplexer.Connect(ConfigurationManager.AppSettings["redis.connection"]);
                mySubscriber = redis.GetSubscriber();
                if (mySubscriber.IsConnected())
                    mySubscriber.Subscribe(new RedisChannel(RedisMessageChannels.NomenklReturnOfClient, RedisChannel.PatternMode.Auto),
                        (_, message) =>
                        {
                            Console.WriteLine($@"Redis - {message}");
                            Form.Dispatcher.Invoke(() => UpdateList(message));
                        });
            }
            catch
            {
                Console.WriteLine($@"Redis {ConfigurationManager.AppSettings["redis.connection"]} не обнаружен");
            }

            WindowName = "Возврат товаров от клиентов";
            LeftMenuBar = MenuGenerator.BaseLeftBar(this, new Dictionary<MenuGeneratorItemVisibleEnum, bool>
            {
                [MenuGeneratorItemVisibleEnum.AddSearchlist] = true
            });
            RightMenuBar = MenuGenerator.StandartSearchRightBar(this);
            EndDate = DateTime.Today;
            StartDate = DateHelper.GetFirstDate();
        }


        #region Methods

         private void UpdateList(RedisValue message)
        {
            if (string.IsNullOrWhiteSpace(message)) return;
            var msg = JsonConvert.DeserializeObject<RedisMessage>(message);
            if (msg == null || msg.Id == null) return;
            if (msg.DbId != GlobalOptions.DataBaseId) return;
            if (msg.OperationType == RedisMessageDocumentOperationTypeEnum.Open
                || (msg.DocDate ?? DateTime.Today) < StartDate || (msg.DocDate ?? DateTime.Today) > EndDate) return;
            if (msg.OperationType == RedisMessageDocumentOperationTypeEnum.Delete)
            {
                var del = Documents.FirstOrDefault(_ => _.Id == msg.Id);
                if (del != null) Documents.Remove(del);
                return;
            }

            if (msg.OperationType != RedisMessageDocumentOperationTypeEnum.Create
                && Documents.All(_ => _.Id != msg.Id)) return;

            var lastDocumentRopository = new DocHistoryRepository(GlobalOptions.GetEntities());

            using (var ctx = GlobalOptions.GetEntities())
            {
                //var dc = new List<decimal>(new[] { msg.DocCode.Value });
                //var d = ctx.SD_24.FirstOrDefault(_ => _.DOC_CODE == msg.DocCode.Value); /*приходный складской ордер*/
                if ( msg.Id is null) return;
                var doc = myRepository.GetSeacrh(msg.Id.Value);
                var last = lastDocumentRopository.GetLastChanges(new[] { msg.Id.Value });
                if (last.Count > 0)
                {
                    doc.LastChanger = last.First().Value.Item1;
                    doc.LastChangerDate = last.First().Value.Item2;
                }
                else
                {
                    doc.LastChanger = doc.Creator;
                    doc.LastChangerDate = doc.DocDate;
                }

                var old = Documents.FirstOrDefault(_ => _.Id == msg.Id);
                if (old != null)
                {
                    switch (msg.OperationType)
                    {
                        case RedisMessageDocumentOperationTypeEnum.Update:
                        {
                            var idx = Documents.IndexOf(old);
                            Documents[idx] = doc;
                            break;
                        }
                        case RedisMessageDocumentOperationTypeEnum.Delete:
                            Documents.Remove(old);
                            break;
                    }
                }
            }
        }

        #endregion

        #region Commands

        public override bool IsDocumentOpenAllow => CurrentDocument != null;

        public override void DocNewEmpty(object form)
        {
            var frm = new NomenklReturnOfClientView
            {
                Owner = Application.Current.MainWindow
            };
            var ctx = new NomenklReturnOfClientWindowViewModel(null) { Form = frm };
            frm.DataContext = ctx;
            frm.Show();
        }

        public override void DocumentOpen(object obj)
        {
            if (CurrentDocument == null) return;
            DocumentsOpenManager.Open(
                DocumentType.NomenklReturnOfClient, CurrentDocument.Id);
        }

        public override void RefreshData(object data)
        {
            var frm = Form as StandartSearchView;
            var lastDocumentRopository = new DocHistoryRepository(GlobalOptions.GetEntities());
            Documents.Clear();
            Task.Run(() =>
            {
                frm?.Dispatcher.Invoke(() =>
                {
                    if (frm.DataContext is NomenklReturnOfClientSearchViewModel dtx) dtx.IsCanRefresh = false;
                    frm.loadingIndicator.Visibility = Visibility.Visible;
                });
                var result = myRepository.GetForPeriod(StartDate, EndDate).ToList();
                if (result.Count > 0)
                {
                    var lasts = lastDocumentRopository.GetLastChanges(result.Select(_ => _.Id).Distinct());
                    foreach (var r in result.Cast<NomenklReturnOfClientSearch>())
                    {
                        if (lasts.ContainsKey(r.Id))
                        {
                            var last = lasts[r.Id];
                            r.LastChanger = last.Item1;
                            r.LastChangerDate = last.Item2;
                        }
                        else
                        {
                            r.LastChanger = r.Creator;
                            r.LastChangerDate = r.DocDate;
                        }
                    }
                }

                frm?.Dispatcher.Invoke(() =>
                {
                    frm.loadingIndicator.Visibility = Visibility.Hidden;
                    foreach (var d in result.Cast<NomenklReturnOfClientSearch>())
                        Documents.Add(d);
                    if (frm.DataContext is NomenklReturnOfClientSearchViewModel dtx) dtx.IsCanRefresh = true;
                });
            });
            frm?.gridDocuments.RefreshData();
        }

        #endregion

        #region Fields

        private readonly INomenklReturnOfClientRepository myRepository;
        private readonly ISubscriber mySubscriber;
        private readonly ConnectionMultiplexer redis;
        private NomenklReturnOfClientSearch myCurrentDocument;

        #endregion

        #region Properties

        public override string LayoutName => "NomenklReturnOfClientSearch";

        public ObservableCollection<NomenklReturnOfClientSearch> Documents { set; get; } =
            new ObservableCollection<NomenklReturnOfClientSearch>();

        public ObservableCollection<NomenklReturnOfClientSearch> SelectedDocs { set; get; } =
            new ObservableCollection<NomenklReturnOfClientSearch>();

        public NomenklReturnOfClientSearch CurrentDocument
        {
            get => myCurrentDocument;
            set
            {
                if (Equals(value, myCurrentDocument)) return;
                myCurrentDocument = value;
                RaisePropertyChanged();
            }
        }

        #endregion
    }
}
