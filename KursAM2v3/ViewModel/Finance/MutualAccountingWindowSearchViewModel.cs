using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using Core;
using Core.EntityViewModel.CommonReferences;
using Core.EntityViewModel.Vzaimozachet;
using Core.Menu;
using Core.ViewModel.Base;
using Core.WindowsManager;
using KursAM2.Managers;
using KursAM2.Managers.Base;
using KursAM2.View.Finance;

namespace KursAM2.ViewModel.Finance
{
    public class MutualAccountingWindowSearchViewModel : RSWindowSearchViewModelBase
    {
        private readonly MutualAccountingManager manager = new MutualAccountingManager();
        private SD_110ViewModel myCurrentDocument;
        private DateTime myDateEnd;
        private DateTime myDateStart;
        private bool myIsConvert;

        public MutualAccountingWindowSearchViewModel()
        {
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.StandartSearchRightBar(this);
            DateStart = DateTime.Today.AddDays(-100);
            DateEnd = DateTime.Today;
        }

        public MutualAccountingWindowSearchViewModel(bool isConvert)
        {
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.StandartSearchRightBar(this);
            DateStart = DateTime.Today.AddDays(-100);
            DateEnd = DateTime.Today;
            IsConvert = isConvert;
        }

        public override string WindowName => IsConvert ? "Поиск валютной конвертации" : "Поис актов взаимозачета";
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

        public DateTime DateStart
        {
            get => myDateStart;
            set
            {
                if (myDateStart == value) return;
                myDateStart = value;
                if (DateStart > DateEnd)
                    DateEnd = DateStart;
                RaisePropertyChanged();
            }
        }

        public DateTime DateEnd
        {
            get => myDateEnd;
            set
            {
                if (myDateEnd == value) return;
                myDateEnd = value;
                if (DateEnd < DateStart)
                    DateStart = DateEnd;
                RaisePropertyChanged();
            }
        }

        public SD_110ViewModel CurrentDocument
        {
            get => myCurrentDocument;
            set
            {
                if (myCurrentDocument != null && myCurrentDocument.Equals(value)) return;
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

        public override void DocumentOpen(object form)
        {
            DocumentsOpenManager.Open(
                IsConvert ? DocumentType.CurrencyConvertAccounting : DocumentType.MutualAccounting,
                CurrentDocument.DocCode);
        }

        public override void RefreshData(object obj)
        {
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
                            .Where(_ => _.VZ_DATE >= DateStart && _.VZ_DATE <= DateEnd && _.SD_111.IsCurrencyConvert)
                            .ToList();
                        foreach (var d in data)
                        {
                            var newDoc = new SD_110ViewModel(d) {IsOld = false};
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
                            .Where(_ => _.VZ_DATE >= DateStart && _.VZ_DATE <= DateEnd && !_.SD_111.IsCurrencyConvert)
                            .ToList();
                        foreach (var d in data)
                        {
                            var newDoc = new SD_110ViewModel(d);
                            newDoc.IsOld = manager.CheckDocumentForOld(newDoc.DocCode);
                            Documents.Add(newDoc);
                        }
                    }
                }

                foreach (var d in Documents) d.myState = RowStatus.NotEdited;
                RaisePropertyChanged(nameof(Documents));
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
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
                        .Where(_ => _.VZ_DATE >= DateStart && _.VZ_DATE <= DateEnd)
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
            var frm = new MutualAccountingView {Owner = Application.Current.MainWindow};
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

        #endregion
    }
}