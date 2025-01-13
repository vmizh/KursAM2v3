using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Windows.Media;
using Core.ViewModel.Base;
using Core.WindowsManager;
using DevExpress.Data;
using DevExpress.Internal.WinApi.Windows.UI.Notifications;
using DevExpress.Xpf.Core.ConditionalFormatting;
using DevExpress.Xpf.Grid;
using KursAM2.Managers;
using KursAM2.Managers.Base;
using KursAM2.Repositories.RedisRepository;
using KursAM2.View.Base;
using KursAM2.View.Finance;
using KursDomain;
using KursDomain.Documents.CommonReferences;
using KursDomain.Documents.Vzaimozachet;
using KursDomain.ICommon;
using KursDomain.Menu;
using KursDomain.Repository.DocHistoryRepository;
using Newtonsoft.Json;
using StackExchange.Redis;
using Application = System.Windows.Application;

namespace KursAM2.ViewModel.Finance
{
    public sealed class MutualAccountingWindowSearchViewModel : RSWindowSearchViewModelBase
    {
        private readonly MutualAccountingManager manager = new MutualAccountingManager();
        private readonly ISubscriber mySubscriber;

        private readonly ConnectionMultiplexer redis;
        private SD_110ViewModel myCurrentDocument;
        private bool myIsConvert;


        public MutualAccountingWindowSearchViewModel()
        {
            LeftMenuBar = MenuGenerator.BaseLeftBar(this, new Dictionary<MenuGeneratorItemVisibleEnum, bool>
            {
                [MenuGeneratorItemVisibleEnum.AddSearchlist] = true
            });
            RightMenuBar = MenuGenerator.StandartSearchRightBar(this);
            StartDate = new DateTime(DateTime.Today.Month == 1 ? DateTime.Today.Year - 1 : DateTime.Today.Year,
                DateTime.Today.Month == 1 ? 12 : DateTime.Today.Month - 1, 1);
            EndDate = DateTime.Today;
            try
            {
                redis = ConnectionMultiplexer.Connect(ConfigurationManager.AppSettings["redis.connection"]);
                mySubscriber = redis.GetSubscriber();

                if (mySubscriber.IsConnected())
                    mySubscriber.Subscribe(
                        new RedisChannel(RedisMessageChannels.MutualAccounting, RedisChannel.PatternMode.Auto),
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
        }

        public MutualAccountingWindowSearchViewModel(bool isConvert)
        {
            LeftMenuBar = MenuGenerator.BaseLeftBar(this, new Dictionary<MenuGeneratorItemVisibleEnum, bool>
            {
                [MenuGeneratorItemVisibleEnum.AddSearchlist] = true
            });
            RightMenuBar = MenuGenerator.StandartSearchRightBar(this);
            //StartDate = DateTime.Today.AddDays(-100);
            StartDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            EndDate = DateTime.Today;
            IsConvert = isConvert;
            try
            {
                redis = ConnectionMultiplexer.Connect(ConfigurationManager.AppSettings["redis.connection"]);
                mySubscriber = redis.GetSubscriber();

                if (mySubscriber.IsConnected())
                    mySubscriber.Subscribe(
                        new RedisChannel(RedisMessageChannels.MutualAccounting, RedisChannel.PatternMode.Auto),
                        (_, message) =>
                        {
                            if (KursNotyficationService != null)
                            {
                                Console.WriteLine($@"Redis - {message}");
                                Form.Dispatcher.Invoke(() => UpdateList(message));
                            }
                        });
            }
            catch
            {
                Console.WriteLine($@"Redis {ConfigurationManager.AppSettings["redis.connection"]} не обнаружен");
            }
        }

        
        public override string WindowName => IsConvert ? "Поиск валютной конвертации" : "Поиск актов взаимозачета";
        public override string LayoutName => "MutualAccountingWindowSearchViewModel";

        public bool IsConvert
        {
            get => myIsConvert;
            set
            {
                if (myIsConvert == value) return;
                myIsConvert = value;
                RaisePropertyChanged();
            }
        }

        public override bool IsDocumentOpenAllow => CurrentDocument != null;

        public override DateTime StartDate
        {
            get => base.StartDate;
            set
            {
                if (base.StartDate == value) return;
                base.StartDate = value;
                if (base.StartDate > base.EndDate)
                    base.EndDate = base.StartDate;
                RaisePropertyChanged();
            }
        }

        public override DateTime EndDate
        {
            get => base.EndDate;
            set
            {
                if (base.EndDate == value) return;
                base.EndDate = value;
                if (base.EndDate < base.StartDate)
                    base.StartDate = base.EndDate;
                RaisePropertyChanged();
            }
        }

        public SD_110ViewModel CurrentDocument
        {
            get => myCurrentDocument;
            set
            {
                if (Equals(myCurrentDocument, value)) return;
                myCurrentDocument = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<SD_110ViewModel> Documents { set; get; } =
            new ObservableCollection<SD_110ViewModel>();

        public override string SearchText
        {
            get => mySearchText;
            set
            {
                if (mySearchText == value) return;
                mySearchText = value;
                RaisePropertyChanged();
            }
        }

        private void UpdateList(RedisValue message)
        {
            if (string.IsNullOrWhiteSpace(message)) return;
            var msg = JsonConvert.DeserializeObject<RedisMessage>(message);
            if (msg is null || msg.DocCode is null) return;
            if (msg.DbId != GlobalOptions.DataBaseId) return;
            if (msg.OperationType == RedisMessageDocumentOperationTypeEnum.Open
                || (msg.DocDate ?? DateTime.Today) < StartDate || (msg.DocDate ?? DateTime.Today) > EndDate) return;
            if (msg.OperationType == RedisMessageDocumentOperationTypeEnum.Delete)
            {
                var del = Documents.FirstOrDefault(_ => _.DocCode == msg.DocCode);
                if (del != null) Documents.Remove(del);
                return;
            }

            if (msg.OperationType != RedisMessageDocumentOperationTypeEnum.Create
                && Documents.All(_ => _.DocCode != msg.DocCode)) return;

            var lastDocumentRopository = new DocHistoryRepository(GlobalOptions.GetEntities());


            var dc = new List<decimal>(new[] { msg.DocCode.Value });
            using (var ctx = GlobalOptions.GetEntities())
            {
                var d = ctx.SD_110
                    .Include(_ => _.TD_110)
                    .Include(_ => _.SD_111)
                    .Include("TD_110.SD_26")
                    .Include("TD_110.SD_301")
                    .Include("TD_110.SD_3011")
                    .Include("TD_110.SD_303")
                    .Include("TD_110.SD_43")
                    .Include("TD_110.SD_77")
                    .Include("TD_110.SD_84")
                    .FirstOrDefault(_ => _.DOC_CODE == msg.DocCode);

                if (d is null) return;
                var data = new SD_110ViewModel(d);
                var last = lastDocumentRopository.GetLastChanges(dc);
                if (last.Count > 0)
                {
                    data.LastChanger = last.First().Value.Item1;
                    data.LastChangerDate = last.First().Value.Item2;
                }
                else
                {
                    data.LastChanger = data.CREATOR;
                    data.LastChangerDate = data.VZ_DATE;
                }

                var old = Documents.FirstOrDefault(_ => _.DocCode == msg.DocCode);
                if (old != null)
                    switch (msg.OperationType)
                    {
                        case RedisMessageDocumentOperationTypeEnum.Update:
                            var idx = Documents.IndexOf(old);
                            Documents[idx] = data;
                            break;
                        case RedisMessageDocumentOperationTypeEnum.Create:
                            Documents.Add(data);
                            break;
                    }
            }
        }

        public override void AddSearchList(object obj)
        {
            if (IsConvert)
            {
                var form = new StandartSearchView
                {
                    Owner = Application.Current.MainWindow
                };
                form.DataContext = new MutualAccountingWindowSearchViewModel(true)
                {
                    WindowName = "Поиск актов конвертации",
                    Form = form
                };
                form.Show();
            }
            else
            {
                var form = new StandartSearchView
                {
                    Owner = Application.Current.MainWindow
                };
                var mutCtx2 = new MutualAccountingWindowSearchViewModel
                {
                    WindowName = "Поиск актов взаимозачета",
                    Form = form
                };
            }
        }

        public override void DocumentOpen(object form)
        {
            DocumentsOpenManager.Open(
                IsConvert ? DocumentType.CurrencyConvertAccounting : DocumentType.MutualAccounting,
                CurrentDocument.DocCode);
        }

        public override void RefreshData(object obj)
        {
            GlobalOptions.ReferencesCache.IsChangeTrackingOn = false;
            base.RefreshData(obj);
            SearchText = null;
            Documents.Clear();
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    if (IsConvert)
                    {
                        var data = ctx.SD_110
                            .Include(_ => _.TD_110)
                            .Include(_ => _.SD_111)
                            .Include("TD_110.SD_26")
                            .Include("TD_110.SD_301")
                            .Include("TD_110.SD_3011")
                            .Include("TD_110.SD_303")
                            .Include("TD_110.SD_43")
                            .Include("TD_110.SD_77")
                            .Include("TD_110.SD_84")
                            .Where(_ => _.VZ_DATE >= StartDate && _.VZ_DATE <= EndDate && _.SD_111.IsCurrencyConvert)
                            .ToList();
                        foreach (var d in data)
                        {
                            var newDoc = new SD_110ViewModel(d) { IsOld = false };
                            Documents.Add(newDoc);
                        }
                    }
                    else
                    {
                        var data = ctx.SD_110
                            .Include(_ => _.TD_110)
                            .Include(_ => _.SD_111)
                            .Include("TD_110.SD_26")
                            .Include("TD_110.SD_301")
                            .Include("TD_110.SD_3011")
                            .Include("TD_110.SD_303")
                            .Include("TD_110.SD_43")
                            .Include("TD_110.SD_77")
                            .Include("TD_110.SD_84")
                            .Where(_ => _.VZ_DATE >= StartDate && _.VZ_DATE <= EndDate && !_.SD_111.IsCurrencyConvert)
                            .ToList();
                        foreach (var d in data)
                        {
                            var newDoc = new SD_110ViewModel(d);
                            newDoc.IsOld = manager.CheckDocumentForOld(newDoc.DocCode);
                            Documents.Add(newDoc);
                        }
                    }
                }
                var lastDocumentRopository = new DocHistoryRepository(GlobalOptions.GetEntities());
                if (Documents.Count > 0)
                {
                    var lasts = lastDocumentRopository.GetLastChanges(Documents.Select(_ => _.DocCode).Distinct());
                    foreach (var r in Documents)
                        if (lasts.ContainsKey(r.DocCode))
                        {
                            var last = lasts[r.DocCode];
                            r.LastChanger = last.Item1;
                            r.LastChangerDate = last.Item2;
                        }
                        else
                        {
                            r.LastChanger = r.CREATOR;
                            r.LastChangerDate = r.VZ_DATE;
                        }
                }

                foreach (var d in Documents)
                {

                    d.myState = RowStatus.NotEdited;
                }
                RaisePropertyChanged(nameof(Documents));
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }
            finally
            {
                GlobalOptions.ReferencesCache.IsChangeTrackingOn = true;
            }
        }
        
        #region Command

        public override void Search(object obj)
        {
            Documents.Clear();
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var data = ctx.SD_110
                        .Include(_ => _.TD_110)
                        .Include(_ => _.SD_111)
                        .Include("TD_110.SD_26")
                        .Include("TD_110.SD_301")
                        .Include("TD_110.SD_3011")
                        .Include("TD_110.SD_303")
                        .Include("TD_110.SD_43")
                        .Include("TD_110.SD_77")
                        .Include("TD_110.SD_84")
                        .Where(_ => _.VZ_DATE >= StartDate && _.VZ_DATE <= EndDate)
                        .ToList();
                    foreach (var d in data.Where(_ => _.VZ_NOTES?.ToUpper().Contains(SearchText.ToUpper()) ??
                                                      _.VZ_NUM.ToString().Contains(SearchText)))
                    {
                        var newDoc = new SD_110ViewModel(d);
                        Documents.Add(newDoc);
                    }

                    foreach (var d in data)
                    foreach (var t in d.TD_110)
                    {
                        if (!t.SD_43.NAME.ToUpper().Contains(SearchText.ToUpper()) ||
                            Documents.Any(_ => _.DocCode == d.DOC_CODE)) continue;
                        var newDoc = new SD_110ViewModel(d);
                        Documents.Add(newDoc);
                    }
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }
        }

        public override void SearchClear(object obj)
        {
            SearchText = null;
            RefreshData(null);
        }

        public override void DocNewEmpty(object form)
        {
            var frm = new MutualAccountingView { Owner = Application.Current.MainWindow };
            var ctx = new MutualAcountingWindowViewModel
            {
                IsCurrencyConvert = IsConvert,
                Form = frm
            };
            ctx.CreateMenu();
            ctx.Document = ctx.Manager.New();
            frm.Show();
            frm.DataContext = ctx;
        }


        private bool summaryExists(string fieldName)
        {
            if (Form is not StandartSearchView frm) return false;
            foreach (var s in frm.gridDocuments.TotalSummary)
                if (s.FieldName == fieldName)
                    return true;
            return false;
        }

        protected override void OnWindowLoaded(object obj)
        {
            base.OnWindowLoaded(obj);
            if (Form is StandartSearchView frm)
            {
                foreach (var col in frm.gridDocuments.Columns)
                    switch (col.FieldName)
                    {
                        case "VZ_PRIBIL_UCH_CRS_SUM":
                            if (!summaryExists("VZ_PRIBIL_UCH_CRS_SUM"))
                                frm.gridDocuments.TotalSummary.Add(new GridSummaryItem
                                {
                                    FieldName = "VZ_PRIBIL_UCH_CRS_SUM",
                                    DisplayFormat = "n2",
                                    SummaryType = SummaryItemType.Sum
                                });
                            break;
                    }

                frm.gridDocumentsTableView.FormatConditions.Clear();
                var resultMinus = new FormatCondition
                {
                    FieldName = "VZ_PRIBIL_UCH_CRS_SUM",
                    ApplyToRow = false,
                    Format = new Format
                    {
                        Foreground = Brushes.Red
                    },
                    ValueRule = ConditionRule.Less,
                    Value1 = 0m
                };
                var resultPlus = new FormatCondition
                {
                    FieldName = "VZ_PRIBIL_UCH_CRS_SUM",
                    ApplyToRow = false,
                    Format = new Format
                    {
                        Foreground = Brushes.Green
                    },
                    ValueRule = ConditionRule.Greater,
                    Value1 = 0m
                };
                frm.gridDocumentsTableView.FormatConditions.Add(resultMinus);
                frm.gridDocumentsTableView.FormatConditions.Add(resultPlus);
            }
        }

        public override void OnWindowClosing(object obj)
        {
            mySubscriber?.UnsubscribeAll();
            base.OnWindowClosing(obj);
        }

        #endregion
    }
}
