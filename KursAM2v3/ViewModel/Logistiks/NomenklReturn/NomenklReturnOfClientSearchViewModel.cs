using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Core.Helper;
using Core.ViewModel.Base;
using DevExpress.Mvvm.DataAnnotations;
using Helper;
using KursAM2.Managers;
using KursAM2.Repositories.NomenklReturn;
using KursAM2.View.Base;
using KursAM2.View.Logistiks.NomenklReturn;
using KursDomain;
using KursDomain.Documents.CommonReferences;
using KursDomain.Documents.NomenklReturn;
using KursDomain.Menu;
using KursDomain.Repository.DocHistoryRepository;
using StackExchange.Redis;

namespace KursAM2.ViewModel.Logistiks.NomenklReturn
{
    public sealed class NomenklReturnOfClientSearchViewModel : RSWindowSearchViewModelBase
    {
        public NomenklReturnOfClientSearchViewModel()
        {
            myRepository = new NomenklReturnOfClientRepository();
            redis = ConnectionMultiplexer.Connect(ConfigurationManager.AppSettings["redis.connection"]);
            mySubscriber = redis.GetSubscriber();

            WindowName = "Возврат товаров от клиентов";
            LeftMenuBar = MenuGenerator.BaseLeftBar(this, new Dictionary<MenuGeneratorItemVisibleEnum, bool>
            {
                [MenuGeneratorItemVisibleEnum.AddSearchlist] = true
            });
            RightMenuBar = MenuGenerator.StandartSearchRightBar(this);
            EndDate = DateTime.Today;
            StartDate = DateHelper.GetFirstDate();
        }

        #region Commands

        public override void DocNewEmpty(object form)
        {
            var frm = new NomenklReturnOfClientView()
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

        public override void RefreshData()
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

                frm?.Dispatcher.Invoke(() =>
                {
                    frm.loadingIndicator.Visibility = Visibility.Hidden;
                    foreach (var d in result.Cast<NomenklReturnOfClientSearch>()) Documents.Add(d);
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
