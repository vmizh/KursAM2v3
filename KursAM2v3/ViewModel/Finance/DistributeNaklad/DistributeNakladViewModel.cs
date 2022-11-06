using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Core;
using Core.Helper;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using Data.Repository;
using DevExpress.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Editors.Settings;
using DevExpress.Xpf.Grid;
using Helper;
using KursAM2.Dialogs;
using KursAM2.Managers;
using KursAM2.Repositories;
using KursAM2.View.DialogUserControl.Invoices.ViewModels;
using KursAM2.View.DialogUserControl.Standart;
using KursAM2.View.Finance.DistributeNaklad;
using KursDomain;
using KursDomain.Documents.CommonReferences;
using KursDomain.Documents.Currency;
using KursDomain.Documents.Invoices;
using KursDomain.ICommon;
using KursDomain.Menu;
using KursDomain.References;
using InvoiceProviderRow = KursDomain.Documents.Invoices.InvoiceProviderRow;

namespace KursAM2.ViewModel.Finance.DistributeNaklad
{
    [MetadataType(typeof(DataAnnotationDistribNaklad))]
    public sealed class DistributeNakladViewModel : RSWindowViewModelBase, IViewModelToEntity<Data.DistributeNaklad>,
        IKursLayoutManager
    {
        #region Constructors

        public DistributeNakladViewModel(Window parentForm, IDocumentOpenType docOpenType) : this()
        {
            ParentFormViewModel = parentForm?.DataContext as RSWindowViewModelBase;
            if (docOpenType.Id == null)
            {
                Entity = DistributeNakladRepository.CreateNew();
                State = RowStatus.NewRow;
                return;
            }

            switch (docOpenType.OpenType)
            {
                case DocumentCreateTypeEnum.Open:
                    // ReSharper disable once VirtualMemberCallInConstructor
                    Load(docOpenType.Id);
                    myState = RowStatus.NotEdited;
                    break;
                case DocumentCreateTypeEnum.Copy:
                    Entity = DistributeNakladRepository.CreateCopy((Guid)docOpenType.Id);
                    State = RowStatus.NewRow;
                    break;
                case DocumentCreateTypeEnum.RequisiteCopy:
                    Entity = DistributeNakladRepository.CreateRequisiteCopy((Guid)docOpenType.Id);
                    State = RowStatus.NewRow;
                    break;
            }
        }

        public DistributeNakladViewModel()
        {
            BaseRepository = new GenericKursDBRepository<Data.DistributeNaklad>(unitOfWork);
            DistributeNakladRepository = new DistributeNakladRepository(unitOfWork);
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.StandartDocWithDeleteRightBar(this);
        }

        public DistributeNakladViewModel(Data.DistributeNaklad entity) : this()
        {
            Entity = entity;
            State = RowStatus.NotEdited;
        }

        #endregion

        #region Fields

        private readonly WindowManager winManager = new WindowManager();

        private readonly UnitOfWork<ALFAMEDIAEntities> unitOfWork
            // ReSharper disable once ArrangeObjectCreationWhenTypeEvident
            = new UnitOfWork<ALFAMEDIAEntities>(new ALFAMEDIAEntities(GlobalOptions.SqlConnectionString));

        // ReSharper disable once NotAccessedField.Local
        public readonly GenericKursDBRepository<Data.DistributeNaklad> BaseRepository;

        // ReSharper disable once NotAccessedField.Local
        public readonly IDistributeNakladRepository DistributeNakladRepository;
        private DistributeNakladInvoiceViewModel myCurrentNakladInvoice;
        private DistributeNakladRowViewModel myCurrentTovar;
        private DistributeNakladInfoViewModel myCurrentDistributeNaklad;

        #endregion

        #region Properties

        [Display(AutoGenerateField = false)] public override bool IsCanDocNew => true;

        [Display(AutoGenerateField = false)] public override bool IsDocNewCopyAllow => false;

        [Display(AutoGenerateField = false)] public override bool IsDocNewCopyRequisiteAllow => false;

        [Display(AutoGenerateField = false)] public override bool IsDocDeleteAllow => State != RowStatus.NewRow;

        [Display(AutoGenerateField = false)] public override bool IsCanRefresh => State != RowStatus.NewRow;

        [Display(AutoGenerateField = false)] public override string LayoutName => "DistributeNakladViewModel";

        [Display(AutoGenerateField = false)]
        public override string Name => State == RowStatus.NewRow
            ? "Новый документ"
            : $"№ {DocNum} от {DocDate.ToShortDateString()} {Currency}";

        public override string WindowName => $"Распределение накладных расходов №{DocNum} от {DocDate}";

        public override string Description =>
            $"№ {DocNum} от {DocDate.ToShortDateString()} {Currency} {Note} Создатель: {Creator}";

        public override Guid Id
        {
            get => Entity.Id;
            set
            {
                if (Entity.Id == value) return;
                Entity.Id = value;
                RaisePropertyChanged();
            }
        }

        [Display(AutoGenerateField = false)] public DistributeNakladViewModel Current => this;

        [Display(AutoGenerateField = false)]
        // ReSharper disable once CollectionNeverUpdated.Global
        public ObservableCollection<DistributeNakladRowViewModel> Tovars { set; get; }
            = new ObservableCollection<DistributeNakladRowViewModel>();

        [Display(AutoGenerateField = false)]
        // ReSharper disable once CollectionNeverUpdated.Global
        public ObservableCollection<DistributeNakladRowViewModel> SelectedTovars { set; get; }
            = new ObservableCollection<DistributeNakladRowViewModel>();

        [Display(AutoGenerateField = false)]
        public DistributeNakladRowViewModel CurrentTovar
        {
            get => myCurrentTovar;
            set
            {
                if (myCurrentTovar == value) return;
                myCurrentTovar = value;
                DistributeNaklads.Clear();
                if (myCurrentTovar != null)
                    foreach (var n in DistributeAllNaklads
                                 .Where(_ => _.RowId == CurrentTovar.Id))
                        DistributeNaklads.Add(n);
                RaisePropertyChanged();
            }
        }

        [Display(AutoGenerateField = false)]
        // ReSharper disable once CollectionNeverQueried.Global
        public ObservableCollection<DistributeNakladInfoViewModel> DistributeNaklads { set; get; }
            = new ObservableCollection<DistributeNakladInfoViewModel>();

        [Display(AutoGenerateField = false)]
        public ObservableCollection<DistributeNakladInfoViewModel> DistributeAllNaklads { set; get; }
            = new ObservableCollection<DistributeNakladInfoViewModel>();

        [Display(AutoGenerateField = false)]
        public ObservableCollection<DistributeNakladInfoViewModel> SelectedDistributeNaklads { set; get; }
            = new ObservableCollection<DistributeNakladInfoViewModel>();

        [Display(AutoGenerateField = false)]
        public DistributeNakladInfoViewModel CurrentDistributeNaklad
        {
            get => myCurrentDistributeNaklad;
            set
            {
                if (myCurrentDistributeNaklad == value) return;
                myCurrentDistributeNaklad = value;
                RaisePropertyChanged();
            }
        }

        [Display(AutoGenerateField = false)]
        public ObservableCollection<DistributeNakladInvoiceViewModel> NakladInvoices { set; get; }
            = new ObservableCollection<DistributeNakladInvoiceViewModel>();

        [Display(AutoGenerateField = false)]
        public ObservableCollection<DistributeNakladInvoiceViewModel> SelectedNakladInvoices { set; get; }
            = new ObservableCollection<DistributeNakladInvoiceViewModel>();

        [Display(AutoGenerateField = false)]
        public DistributeNakladInvoiceViewModel CurrentNakladInvoice
        {
            get => myCurrentNakladInvoice;
            set
            {
                if (myCurrentNakladInvoice == value) return;
                myCurrentNakladInvoice = value;
                if (myCurrentNakladInvoice != null)
                    if (Form is DistributedNakladView frm)
                    {
                        var col = frm.gridNaklad.Columns.GetColumnByName("Rate");
                        col.ReadOnly = myCurrentNakladInvoice.DistributeType != Repositories.DistributeNakladRepository
                            .DistributeNakladTypeEnum.NotDistribute;
                    }

                RaisePropertyChanged();
            }
        }

        [DisplayName("Entity")]
        [Display(AutoGenerateField = false)]
        public Data.DistributeNaklad Entity { set; get; }

        [DisplayName("Дата")]
        [Display(AutoGenerateField = true)]
        public DateTime DocDate
        {
            get => Entity.DocDate;
            set
            {
                if (Entity.DocDate == value) return;
                Entity.DocDate = value;
                //RaisePropertyChanged();
            }
        }

        [DisplayName("№ ")]
        [Display(AutoGenerateField = true)]
        public string DocNum
        {
            get => Entity.DocNum;
            set
            {
                if (Entity.DocNum == value) return;
                Entity.DocNum = value;
                RaisePropertyChanged();
            }
        }

        [DisplayName("Создатель")]
        [Display(AutoGenerateField = true)]
        public string Creator
        {
            get => Entity.Creator;
            set
            {
                if (Entity.Creator == value) return;
                Entity.Creator = value;
                RaisePropertyChanged();
            }
        }

        [DisplayName("Код валюты")]
        [Display(AutoGenerateField = false)]
        public decimal? CurrencyDC
        {
            get => Entity.CurrencyDC;
            set
            {
                if (Entity.CurrencyDC == value) return;
                Entity.CurrencyDC = value;
                RaisePropertyChanged();
            }
        }

        [DisplayName("Валюта")]
        [Display(AutoGenerateField = true)]
        // ReSharper disable once PossibleInvalidOperationException
        public Currency Currency
        {
            get => MainReferences.GetCurrency(Entity.CurrencyDC);

            set
            {
                if (MainReferences.GetCurrency(Entity.CurrencyDC) == value) return;
                Entity.CurrencyDC = value?.DocCode;
                RaisePropertyChanged();
            }
        }

        [DisplayName("Распределено")]
        [Display(AutoGenerateField = true)]
        public decimal DistributedSumma => Entity.DistributeNakladRow.Sum(_ => _.DistributeSumma);

        [DisplayName("Примечания")]
        [Display(AutoGenerateField = true)]
        public override string Note
        {
            get => Entity.Note;
            set
            {
                if (Entity.Note == value) return;
                Entity.Note = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region Methods

        public void RecalcAllResult()
        {
            foreach (var t in Tovars)
            {
                t.DistributeSumma = DistributeAllNaklads.Where(_ => _.RowId == t.Id)
                    .Sum(_ => _.DistributeSumma);
                t.DistributePrice = Math.Round((t.Summa + t.DistributeSumma) / t.Quantity, 4);
            }

            foreach (var i in NakladInvoices)
                i.SummaDistribute = DistributeAllNaklads.Where(_ => _.FinanceDocId == i.Id)
                    .Sum(_ => _.NakladSumma);
        }

        private void updateNakladsAll(DistributeNakladInvoiceViewModel inv)
        {
            foreach (var n in Tovars)
            {
                DistributeNakladInfoViewModel old;
                old = DistributeAllNaklads.FirstOrDefault(_ => _.FinanceDocId == inv.Id
                                                               && _.RowId == n.Id);
                if (old != null)
                {
                    old.NakladSumma = 0;
                    old.DistributeSumma = 0;
                    old.Rate = inv.Rate;
                }
                else
                {
                    // ReSharper disable once PossibleInvalidOperationException
                    var crsDC = (decimal)(inv.Invoice != null
                        ? inv.Invoice.SF_CRS_DC
                        // ReSharper disable once PossibleNullReferenceException
                        : MainReferences.GetKontragent(inv.AccruedAmountRow.AccruedAmountOfSupplier.KontrDC)
                            .BalansCurrency.DocCode);
                    var newInfo = new DistributeNakladInfo
                    {
                        Id = Guid.NewGuid(),
                        InvoiceNakladId = inv.Invoice?.Id,
                        AccrualAmountId = inv.AccruedAmountRow?.Id,
                        DistributeNakladRow = n.Entity,
                        NakladSumma = 0,
                        DistributeSumma = 0,
                        // ReSharper disable once PossibleInvalidOperationException
                        InvoiceCrsDC = crsDC,
                        FinanceDocId = inv.Id,
                        Rate = inv.Rate,
                        RowId = n.Id
                    };
                    n.Entity.DistributeNakladInfo.Add(newInfo);
                    DistributeAllNaklads.Add(new DistributeNakladInfoViewModel(newInfo)
                    {
                        State = RowStatus.NewRow
                    });
                }
            }
        }

        public static DistributeNakladViewModel Create()
        {
            var factory = ViewModelSource.Factory(()
                => new DistributeNakladViewModel());
            return factory();
        }

        private void recalcDistributeSummForInvoice()
        {
            var sumRemain = CurrentNakladInvoice.Summa - (CurrentNakladInvoice.SummaDistribute - DistributeAllNaklads
                .Where(_ => _.FinanceDocId == CurrentNakladInvoice.Id).Sum(_ => _.DistributeSumma)) ?? 0;
            if (Tovars.Count == 0) return;
            updateNakladsAll(CurrentNakladInvoice);
            CurrentNakladInvoice.SummaDistribute = 0;
            decimal allSumma = 0;
            switch (CurrentNakladInvoice.DistributeType)
            {
                case Repositories.DistributeNakladRepository.DistributeNakladTypeEnum.ManualValue:
                    break;
                case Repositories.DistributeNakladRepository.DistributeNakladTypeEnum.NotDistribute:
                    break;
                case Repositories.DistributeNakladRepository.DistributeNakladTypeEnum.PriceValue:
                    allSumma = Tovars.Sum(_ => _.Price);
                    break;
                case Repositories.DistributeNakladRepository.DistributeNakladTypeEnum.QuantityValue:
                    allSumma = Tovars.Sum(_ => _.Quantity);
                    break;
                case Repositories.DistributeNakladRepository.DistributeNakladTypeEnum.SummaValue:
                    allSumma = Tovars.Sum(_ => _.Price * _.Quantity);
                    break;
                case Repositories.DistributeNakladRepository.DistributeNakladTypeEnum.VolumeValue:
                    allSumma = 0;
                    break;
                case Repositories.DistributeNakladRepository.DistributeNakladTypeEnum.WeightValue:
                    allSumma = 0;
                    break;
            }

            var data = CurrentNakladInvoice.Invoice != null
                ? DistributeAllNaklads
                    .Where(_ => _.InvoiceNakladId == CurrentNakladInvoice.Invoice.Id)
                : DistributeAllNaklads
                    .Where(_ => _.AccrualAmountId == CurrentNakladInvoice.AccruedAmountRow.Id);
            foreach (var inf in data)
            {
                if (allSumma == 0)
                {
                    inf.NakladSumma = 0;
                }
                else
                {
                    decimal prc = 0;
                    switch (CurrentNakladInvoice.DistributeType)
                    {
                        case Repositories.DistributeNakladRepository.DistributeNakladTypeEnum.ManualValue:
                            break;
                        case Repositories.DistributeNakladRepository.DistributeNakladTypeEnum.NotDistribute:
                            break;
                        case Repositories.DistributeNakladRepository.DistributeNakladTypeEnum.PriceValue:
                            prc = Tovars.First(_ => _.Id == inf.RowId).Price;
                            break;
                        case Repositories.DistributeNakladRepository.DistributeNakladTypeEnum.QuantityValue:
                            prc = Tovars.First(_ => _.Id == inf.RowId).Quantity;
                            break;
                        case Repositories.DistributeNakladRepository.DistributeNakladTypeEnum.SummaValue:
                            var p = Tovars.First(_ => _.Id == inf.RowId);
                            prc = p.Quantity * p.Price;
                            break;
                        case Repositories.DistributeNakladRepository.DistributeNakladTypeEnum.VolumeValue:
                            prc = 0;
                            break;
                        case Repositories.DistributeNakladRepository.DistributeNakladTypeEnum.WeightValue:
                            prc = 0;
                            break;
                    }

                    inf.NakladSumma = prc * sumRemain / allSumma;
                }

                inf.DistributeSumma = inf.NakladSumma * inf.Rate;
                inf.InvoiceInfo = CurrentNakladInvoice.Invoice != null
                    ? $"С/ф №{CurrentNakladInvoice.DocNum} от {CurrentNakladInvoice.DocDate} пост.:{CurrentNakladInvoice.ProviderName}"
                    : $"Прямые затраты №{CurrentNakladInvoice.AccruedAmountRow.AccruedAmountOfSupplier.DocInNum} / {CurrentNakladInvoice.AccruedAmountRow.AccruedAmountOfSupplier.DocExtNum} " +
                      $"от {CurrentNakladInvoice.AccruedAmountRow.AccruedAmountOfSupplier.DocDate.ToShortDateString()}";
            }

            RecalcAllResult();
            RaisePropertyAllChanged();
            //RaisePropertiesChanged(nameof(Tovars));
            //RaisePropertiesChanged(nameof(NakladInvoices));
        }

        #endregion

        #region Commands

        public override void RefreshData(object obj)
        {
            if (IsCanSaveData)
            {
                var res = MessageBox.Show("В документ внесены изменения. Сохранить?", "Запрос",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                switch (res)
                {
                    case MessageBoxResult.Yes:
                        SaveData(null);
                        break;
                    case MessageBoxResult.No:
                        NakladInvoices.Clear();
                        Tovars.Clear();
                        DistributeAllNaklads.Clear();
                        DistributeNaklads.Clear();
                        foreach (var entity in unitOfWork.Context.ChangeTracker.Entries()) entity.Reload();
                        loadLists();
                        Load(Id);
                        foreach (var t in Tovars) t.State = RowStatus.NotEdited;
                        State = RowStatus.NotEdited;
                        break;
                }
            }
        }

        protected override void OnWindowLoaded(object obj)
        {
            base.OnWindowLoaded(obj);
            if (Form is DistributedNakladView frm)
            {
                frm.gridNaklad.TotalSummary.Clear();
                foreach (var col in frm.gridNaklad.Columns)
                {
                    if (col.FieldName == "Rate")
                    {
                        if (myCurrentNakladInvoice?.DistributeType ==
                            Repositories.DistributeNakladRepository.DistributeNakladTypeEnum.NotDistribute)
                            col.ReadOnly = false;
                        else
                            col.ReadOnly = true;

                        continue;
                    }

                    if (KursGridControlHelper.ColumnFieldTypeCheckDecimal(col.FieldType))
                    {
                        col.EditSettings = new CalcEditSettings
                        {
                            DisplayFormat = "n2",
                            Name = col.FieldName + "Calc",
                            AllowDefaultButton = !col.ReadOnly
                        };
                        var summary = new GridSummaryItem
                        {
                            SummaryType = SummaryItemType.Sum,
                            ShowInColumn = col.FieldName,
                            DisplayFormat = "{0:n2}",
                            FieldName = col.FieldName
                        };
                        frm.gridNaklad.TotalSummary.Add(summary);
                    }
                }

                frm.gridDistributeSumma.TotalSummary.Clear();
                foreach (var col in frm.gridDistributeSumma.Columns)
                {
                    if (col.FieldName == "Rate") continue;
                    if (KursGridControlHelper.ColumnFieldTypeCheckDecimal(col.FieldType))
                    {
                        col.EditSettings = new CalcEditSettings
                        {
                            DisplayFormat = "n2",
                            Name = col.FieldName + "Calc",
                            AllowDefaultButton = !col.ReadOnly
                        };
                        var summary = new GridSummaryItem
                        {
                            SummaryType = SummaryItemType.Sum,
                            ShowInColumn = col.FieldName,
                            DisplayFormat = "{0:n2}",
                            FieldName = col.FieldName
                        };
                        frm.gridDistributeSumma.TotalSummary.Add(summary);
                    }
                }
            }
        }

        public override void DocNewEmpty(object form)
        {
            var dsForm = new DistributedNakladView
            {
                Owner = Application.Current.MainWindow
            };
            var dtx = new DistributeNakladViewModel(Form, new DocumentOpenType
            {
                OpenType = DocumentCreateTypeEnum.New
            })
            {
                Form = dsForm
            };
            dsForm.DataContext = dtx;
            dsForm.Show();
        }

        [Display(AutoGenerateField = false)]
        public ICommand DocumentNakladOpenCommand
        {
            get { return new Command(DocumentNakladOpen, _ => CurrentNakladInvoice != null); }
        }

        private void DocumentNakladOpen(object obj)
        {
            if (CurrentNakladInvoice.Invoice != null)
                DocumentsOpenManager.Open(DocumentType.InvoiceProvider, CurrentNakladInvoice.Invoice.DOC_CODE);

            if (CurrentNakladInvoice.AccruedAmountRow != null)
                DocumentsOpenManager.Open(DocumentType.AccruedAmountOfSupplier, 0,
                    CurrentNakladInvoice.AccruedAmountRow.DocId);
        }

        [Display(AutoGenerateField = false)]
        public ICommand AddAccrualAmountCommand
        {
            get { return new Command(AddAccrualAmount, _ => true); }
        }

        private void AddAccrualAmount(object obj)
        {
            var dialogs = new AccrualAmountDialogs();
            if (dialogs.ShowDialog() == true)
                foreach (var d in dialogs.ItemsCollection.Where(_ => _.IsSelected))
                    if (NakladInvoices.Where(_ => _.AccruedAmountRow != null).All(_ => _.AccruedAmountRow.Id != d.Id))
                    {
                        var ent = new DistributeNakladInvoices
                        {
                            Id = Guid.NewGuid(),
                            DocId = Entity.Id,
                            AccrualAmountId = d.Id,
                            Rate = d.Currency.DocCode == GlobalOptions.SystemProfile.NationalCurrency.DocCode
                                ? 1
                                : new CurrencyRates(d.DocDate.AddDays(-10), d.DocDate).GetRate(d.Currency.DocCode,
                                    GlobalOptions.SystemProfile.NationalCurrency.DocCode, d.DocDate)
                        };
                        Entity.DistributeNakladInvoices.Add(ent);
                        NakladInvoices.Add(new DistributeNakladInvoiceViewModel(ent)
                        {
                            AccruedAmountRow = d.Entity,
                            Currency = d.Kongtragent.Currency as Currency,
                            SummaDistribute = 0
                        });
                    }

            RaisePropertyChanged();
        }

        [Display(AutoGenerateField = false)]
        public ICommand OpenDocumentCommand
        {
            get
            {
                return new Command(OpenDocument, _ => CurrentTovar != null && CurrentTovar.Invoice?.DocCode != null);
            }
        }

        private void OpenDocument(object obj)
        {
            DocumentsOpenManager.Open(
                DocumentType.InvoiceProvider, CurrentTovar.Invoice.DocCode);
        }

        [Display(AutoGenerateField = false)]
        public ICommand RecalcCommand
        {
            get { return new Command(Recalc, _ => true); }
        }

        private void Recalc(object obj)
        {
            RecalcAllResult();
        }

        [Display(AutoGenerateField = false)]
        public ICommand NakladRowChangedCommand
        {
            get { return new Command(NakladRowChanged, _ => true); }
        }

        private void NakladRowChanged(object obj)
        {
            var ctrl = Form as DistributedNakladView;
            if (CurrentNakladInvoice == null)
            {
                if (ctrl != null) ctrl.NakladSummaColumn.ReadOnly = true;
            }
            else
            {
                if (ctrl?.NakladSummaColumn == null || CurrentDistributeNaklad == null)
                {
                    // ReSharper disable PossibleNullReferenceException
                    ctrl.NakladSummaColumn.ReadOnly = true;
                    // ReSharper restore PossibleNullReferenceException
                    return;
                }

                if (CurrentDistributeNaklad.InvoiceNakladId != null)
                {
                    var naklinv = NakladInvoices
                        .Single(_ => _.Invoice != null && _.Invoice.Id == CurrentDistributeNaklad.InvoiceNakladId);
                    ctrl.NakladSummaColumn.ReadOnly = naklinv.DistributeType !=
                                                      Repositories.DistributeNakladRepository.DistributeNakladTypeEnum
                                                          .ManualValue;
                }

                if (CurrentDistributeNaklad.AccrualAmountId != null)
                {
                    var naklinv = NakladInvoices
                        .SingleOrDefault(_ =>
                            _.AccruedAmountRow != null &&
                            _.AccruedAmountRow.Id == CurrentDistributeNaklad.AccrualAmountId);
                    ctrl.NakladSummaColumn.ReadOnly = naklinv?.DistributeType !=
                                                      Repositories.DistributeNakladRepository.DistributeNakladTypeEnum
                                                          .ManualValue;
                }
            }
        }

        [Display(AutoGenerateField = false)]
        public ICommand UpdateNakladSummaCommand
        {
            get { return new Command(UpdateNakladSumma, _ => true); }
        }

        private void UpdateNakladSumma(object obj)
        {
            var sum = DistributeAllNaklads
                .Where(_ => _.InvoiceNakladId == CurrentDistributeNaklad.InvoiceNakladId).Sum(_ => _.NakladSumma);
            var invnakl = NakladInvoices.SingleOrDefault(_ => _.Invoice?.Id == CurrentDistributeNaklad.InvoiceNakladId);
            if (invnakl != null)
                if (sum > invnakl.Summa)
                {
                    winManager.ShowWinUIMessageBox($"Сумма распредления {sum} больше, чем сумма счета {invnakl.Summa}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    CurrentDistributeNaklad.NakladSumma -= (decimal)(sum - invnakl.Summa);
                }

            var directZatrat =
                NakladInvoices.SingleOrDefault(_ => _.AccruedAmountRow?.Id == CurrentDistributeNaklad.AccrualAmountId);
            if (directZatrat != null)
                if (sum > directZatrat.Summa)
                {
                    winManager.ShowWinUIMessageBox($"Сумма распредления {sum} больше, чем сумма счета {invnakl?.Summa}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    CurrentDistributeNaklad.NakladSumma -= (decimal)(sum - directZatrat.Summa);
                }

            CurrentDistributeNaklad.DistributeSumma =
                CurrentDistributeNaklad.NakladSumma / CurrentDistributeNaklad.Rate;
            RecalcAllResult();
            if (State != RowStatus.NewRow)
                State = RowStatus.Edited;
            RaisePropertyChanged(nameof(Tovars));
        }

        public override void DocDelete(object form)
        {
            var res = MessageBox.Show("Удалить текущий документ?", "Запрос",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            switch (res)
            {
                case MessageBoxResult.Yes:
                    unitOfWork.CreateTransaction();
                    var noms = Tovars.Select(_ => _.Nomenkl).ToList();
                    DistributeNakladRepository.Delete(Entity);
                    foreach (var n in noms)
                    {
                        var sql = $"exec dbo.NomenklCalculateCostsForOne {CustomFormat.DecimalToSqlDecimal(n.DocCode)}";
                        unitOfWork.Context.Database.ExecuteSqlCommand(sql);
                    }

                    unitOfWork.Commit();
                    Form.Close();
                    break;
                case MessageBoxResult.No:
                    return;
            }
        }

        private void CalcNomenkls(List<Nomenkl> noms)
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                if (noms == null)
                {
                    foreach (var n in Tovars)
                    {
                        var sql =
                            $"exec dbo.NomenklCalculateCostsForOne {CustomFormat.DecimalToSqlDecimal(n.Nomenkl.DocCode)}";
                        ctx.Database.ExecuteSqlCommand(sql);
                    }

                    return;
                }

                foreach (var n in noms)
                {
                    var sql = $"exec dbo.NomenklCalculateCostsForOne {CustomFormat.DecimalToSqlDecimal(n.DocCode)}";
                    ctx.Database.ExecuteSqlCommand(sql);
                }
            }
        }

        public override void SaveData(object data)
        {
            try
            {
                unitOfWork.CreateTransaction();
                DistributeNakladRepository.UpdateSFNaklad(Entity);
                unitOfWork.Save();
                unitOfWork.Commit();
                CalcNomenkls(null);
                myState = RowStatus.NotEdited;
                RaisePropertyChanged(nameof(State));
                RaisePropertyChanged(nameof(Tovars));
                RaisePropertyChanged(nameof(SelectedTovars));
                Load(Id);
                DocumentsOpenManager.SaveLastOpenInfo(DocumentType.Naklad, Id, null, Creator,
                    GlobalOptions.UserInfo.Name, Description);
            }

            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
                unitOfWork.Rollback();
            }
        }

        public override bool IsCanSaveData => CanSave();

        public bool CanSave()
        {
            return State != RowStatus.NotEdited || Tovars.Any(_ => _.State != RowStatus.NotEdited);
        }

        public void Load(object o)
        {
            if (o is Guid id)
            {
                Entity = DistributeNakladRepository.GetById(id);
                Tovars.Clear();
                NakladInvoices.Clear();
                DistributeAllNaklads.Clear();
                loadLists();
                RecalcAllResult();
                State = RowStatus.NotEdited;
                //RaisePropertyAllChanged();
            }
        }

        private void loadLists()
        {
            foreach (var inv in Entity.DistributeNakladInvoices)
            {
                if (inv.InvoiceId != null)
                {
                    var invoice = DistributeNakladRepository.GetInvoiceProviderById(inv.InvoiceId.Value);
                    NakladInvoices.Add(new DistributeNakladInvoiceViewModel(inv)
                    {
                        Invoice = invoice.Entity
                        //Summa = invoice.SF_KONTR_CRS_SUMMA ?? 0
                    });
                }

                if (inv.AccrualAmountId != null)
                {
                    var acrrual = unitOfWork.Context.AccuredAmountOfSupplierRow
                        .Include(_ => _.AccruedAmountOfSupplier)
                        .FirstOrDefault(_ => _.Id == inv.AccrualAmountId);
                    NakladInvoices.Add(new DistributeNakladInvoiceViewModel(inv)
                    {
                        AccruedAmountRow = acrrual
                    });
                }
            }

            var list = new List<DistributeNakladRow>(Entity.DistributeNakladRow);
            foreach (var r in list)
            {
                InvoiceProvider inv = null;
                // ReSharper disable once PossibleInvalidOperationException
                InvoiceProviderRowShort invrow = null;
                InvoiceProviderRowCurrencyConvertViewModel conv = null;
                if (r.TovarInvoiceRowId != null)
                    invrow = DistributeNakladRepository.GetInvoiceRow(r.TovarInvoiceRowId.Value);
                if (r.TransferRowId != null)
                {
                    conv = DistributeNakladRepository.GetTransferRow(r.TransferRowId.Value);
                    if (conv.Entity.TD_26 != null)
                        // ReSharper disable once PossibleInvalidOperationException
                        invrow = DistributeNakladRepository.GetInvoiceRow(conv.Entity.TD_26.Id);
                }

                if (invrow != null)
                    // ReSharper disable once PossibleInvalidOperationException
                    inv = DistributeNakladRepository.GetInvoiceHead(invrow.DocId);
                Tovars.Add(new DistributeNakladRowViewModel(r)
                {
                    InvoiceRow = invrow,
                    Convert = conv,
                    Invoice = inv,
                    State = RowStatus.NotEdited
                });
                foreach (var inf in r.DistributeNakladInfo)
                {
                    var newItem = new DistributeNakladInfoViewModel(inf);
                    var iinv = NakladInvoices.FirstOrDefault(_ =>
                        inf.InvoiceNakladId != null && _.Entity.InvoiceId == inf.InvoiceNakladId);
                    var arram = NakladInvoices.FirstOrDefault(_ =>
                        inf.AccrualAmountId != null && _.Entity.AccrualAmountId == inf.AccrualAmountId);
                    if (iinv != null)
                        newItem.InvoiceInfo = $"С/ф №{iinv.DocNum} от {iinv.DocDate} пост.:{iinv.ProviderName}";
                    if (arram != null)
                        newItem.InvoiceInfo =
                            $"Прямые затраты №{arram.DocNum} от {arram.DocDate} пост.:{arram.ProviderName}";
                    DistributeAllNaklads.Add(newItem);
                }

                foreach (var i in NakladInvoices)
                {
                    if (i.Invoice != null)
                        i.SummaDistribute = DistributeAllNaklads.Where(_ => _.InvoiceNakladId == i.Invoice.Id)
                            .Sum(_ => _.NakladSumma);
                    if (i.AccruedAmountRow != null)
                        i.SummaDistribute = DistributeAllNaklads
                            .Where(_ => _.AccrualAmountId == i.AccruedAmountRow.Id)
                            .Sum(_ => _.NakladSumma);
                }
            }
        }

        [Display(AutoGenerateField = false)]
        public ICommand DistributeTypeChangedCommand
        {
            get { return new Command(DistributeTypeChanged, _ => CurrentNakladInvoice != null); }
        }

        public void DistributeTypeChanged(object o)
        {
            if (o is GridControl grid)
                if (grid.CurrentColumn != null && grid.CurrentColumn.FieldName == "Rate")
                    if (grid.View.ActiveEditor != null)
                        return;
            recalcDistributeSummForInvoice();
            if (Form is DistributedNakladView frm)
            {
                var col = frm.gridNaklad.Columns.GetColumnByName("Rate");
                if (myCurrentNakladInvoice.DistributeType ==
                    Repositories.DistributeNakladRepository.DistributeNakladTypeEnum.NotDistribute)
                    col.ReadOnly = false;
                else
                    col.ReadOnly = true;
            }
        }

        public ICommand AddNomenklCommand
        {
            get { return new Command(AddNomenkl, _ => true); }
        }

        public void AddNomenkl(object o)
        {
            var loadType = InvoiceProviderSearchType.RemoveNakladRashod | InvoiceProviderSearchType.IsCurrencyUsed;
            var dtx = new InvoiceProviderSearchDialogViewModel(true, true, loadType, unitOfWork.Context)
            {
                WindowName = "Выбор номенклатуры",
                LayoutName = "InvoiceProviderSearchMulti",
                myCurrency = Currency,
                IsLoadNotDistributeCurrencyConvert = true
            };
            dtx.RefreshData(null);
            var dialog = new SelectInvoiceMultipleDialogView
            {
                DataContext = dtx,
                Owner = Form
            };
            dialog.ShowDialog();
            if (dtx.DialogResult == MessageResult.OK)
                foreach (var r in dtx.SelectedItems)
                {
                    if (Tovars.Any(_ => _.InvoiceRow.DocCode == r.DocCode && _.InvoiceRow.Code == r.CODE)) continue;
                    var newRow = DistributeNakladRepository.CreateRowNew(Entity);
                    var inv = unitOfWork.Context.TD_26.Include(_ => _.TD_26_CurrencyConvert).FirstOrDefault(_ =>
                        _.DOC_CODE == r.DocCode && _.CODE == r.CODE);
                    if (inv != null)
                    {
                        if (inv.TD_26_CurrencyConvert != null && inv.TD_26_CurrencyConvert.Count > 0)
                        {
                            foreach (var c in inv.TD_26_CurrencyConvert)
                            {
                                newRow.TransferRowId = c.Id;
                                var newTovar = new DistributeNakladRowViewModel(newRow)
                                {
                                    Convert = new InvoiceProviderRowCurrencyConvertViewModel(c),
                                    InvoiceRow = new InvoiceProviderRow(inv),
                                    State = RowStatus.NewRow
                                };
                                ((ISupportParentViewModel)newTovar).ParentViewModel = this;
                                Tovars.Add(newTovar);
                            }
                        }
                        else
                        {
                            newRow.TovarInvoiceRowId = inv.Id;
                            var newTovar = new DistributeNakladRowViewModel(newRow)
                            {
                                InvoiceRow = new InvoiceProviderRow(inv),
                                State = RowStatus.NewRow
                            };
                            ((ISupportParentViewModel)newTovar).ParentViewModel = this;
                            Tovars.Add(newTovar);
                        }
                    }
                }
        }

        public bool CanAddNomenkl()
        {
            return Currency != null;
        }


        [Display(AutoGenerateField = false)]
        public ICommand DeleteNomenklCommand
        {
            get { return new Command(DeleteNomenkl, _ => true); }
        }

        public void DeleteNomenkl(object o)
        {
            var ids = SelectedTovars.Select(_ => _.Id).ToList();
            foreach (var d in SelectedTovars)
                unitOfWork.Context.Entry(d.Entity).State = unitOfWork.Context.Entry(d.Entity).State == EntityState.Added
                    ? EntityState.Detached
                    : EntityState.Deleted;

            foreach (var id in ids)
            {
                var item = Tovars.SingleOrDefault(_ => _.Id == id);
                if (item != null)
                {
                    var rids = DistributeAllNaklads.Where(_ => _.RowId == item.Id)
                        .Select(_ => _.Id).ToList();
                    foreach (var id1 in rids)
                    {
                        var old = DistributeAllNaklads.First(_ => _.Id == id1);
                        foreach (var inf in unitOfWork.Context.DistributeNakladInfo
                                     .Where(_ => _.RowId == item.Id).ToList())
                        {
                            var d = unitOfWork.Context.DistributeNakladInfo.FirstOrDefault(_ => _.Id == inf.Id);
                            if (d != null)
                                unitOfWork.Context.DistributeNakladInfo.Remove(d);
                        }

                        DistributeAllNaklads.Remove(old);
                    }

                    Tovars.Remove(item);
                }
            }

            foreach (var n in Tovars)
            {
                var sql = $"exec dbo.NomenklCalculateCostsForOne {CustomFormat.DecimalToSqlDecimal(n.Nomenkl.DocCode)}";
                unitOfWork.Context.Database.ExecuteSqlCommand(sql);
            }

            SelectedTovars.Clear();
            RecalcAllResult();
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(Tovars));
            RaisePropertyChanged(nameof(SelectedTovars));
        }

        public bool CanDeleteNomenkl()
        {
            return CurrentTovar != null;
        }


        [Display(AutoGenerateField = false)]
        public ICommand DeleteNakladInvoiceCommand
        {
            get { return new Command(DeleteNakladInvoice, _ => true); }
        }

        // ReSharper disable once UnusedMember.Global
        public void DeleteNakladInvoice(object o)
        {
            if (winManager.ShowWinUIMessageBox("Вы уверены, что хотите удалисть счет накладных?",
                    "Запрос", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                CurrentNakladInvoice.SummaDistribute = 0;
                var ids = new List<Guid>();
                if (CurrentNakladInvoice.Invoice != null)
                    ids = DistributeAllNaklads.Where(_ => _.InvoiceNakladId == CurrentNakladInvoice.Invoice.Id)
                        .Select(_ => _.Id).ToList();
                if (CurrentNakladInvoice.AccruedAmountRow != null)
                    ids = DistributeAllNaklads.Where(_ => _.AccrualAmountId == CurrentNakladInvoice.AccruedAmountRow.Id)
                        .Select(_ => _.Id).ToList();
                foreach (var id in ids)
                {
                    var d = DistributeAllNaklads.First(_ => _.Id == id);
                    var r = Tovars.First(_ => _.Id == d.RowId);
                    r.DistributePrice -= d.DistributeSumma;
                    unitOfWork.Context.DistributeNakladInfo.Remove(d.Entity);
                    DistributeAllNaklads.Remove(d);
                }

                unitOfWork.Context.DistributeNakladInvoices.Remove(CurrentNakladInvoice.Entity);
                NakladInvoices.Remove(CurrentNakladInvoice);
                State = RowStatus.Edited;
            }
        }

        // ReSharper disable once UnusedMember.Global
        public bool CanDeleteNakladInvoice()
        {
            return CurrentNakladInvoice != null;
        }

        [Display(AutoGenerateField = false)]
        public ICommand AddNakladInvoiceCommand
        {
            get { return new Command(AddNakladInvoice, _ => true); }
        }

        // ReSharper disable once UnusedMember.Global
        public void AddNakladInvoice(object o)
        {
            var loadType = InvoiceProviderSearchType.OnlyNakladDistrubuted;
            var dtx = new InvoiceProviderSearchDialogViewModel(false, true, loadType, unitOfWork.Context)
            {
                WindowName = "Выбор счетов фактур",
                LayoutName = "InvoiceProviderSearchMulti"
            };
            dtx.RefreshData(null);
            var dialog = new SelectInvoiceMultipleDialogView
            {
                DataContext = dtx,
                Owner = Application.Current.MainWindow
            };
            dialog.ShowDialog();
            if (dtx.DialogResult == MessageResult.OK)
                foreach (var item in dtx.ItemsCollection.Where(_ => _.IsSelected == true).ToList())
                {
                    if (NakladInvoices.Where(_ => _.Invoice != null)
                        .Any(_ => _.Invoice.DOC_CODE == item.DocCode)) continue;
                    if (NakladInvoices.Where(_ => _.AccruedAmountRow != null)
                        .Any(_ => _.AccruedAmountRow.Id == item.Id)) continue;
                    new CurrencyRates(item.Date.AddDays(-10), item.Date).GetRate(
                        GlobalOptions.SystemProfile.NationalCurrency.DocCode,
                        item.CurrencyDC, item.Date);
                    var ent = new DistributeNakladInvoices
                    {
                        Id = Guid.NewGuid(),
                        DocId = Entity.Id,
                        InvoiceId = item.Id,
                        Rate = item.CurrencyDC == GlobalOptions.SystemProfile.NationalCurrency.DocCode
                            ? 1
                            : new CurrencyRates(item.Date.AddDays(-10), item.Date).GetRate(item.CurrencyDC,
                                GlobalOptions.SystemProfile.NationalCurrency.DocCode, item.Date)
                    };
                    Entity.DistributeNakladInvoices.Add(ent);
                    var s = unitOfWork.Context.SD_26.FirstOrDefault(_ => _.DOC_CODE == item.DocCode);
                    NakladInvoices.Add(new DistributeNakladInvoiceViewModel(ent)
                    {
                        Invoice = s,
                        Currency = MainReferences.GetCurrency(item.CurrencyDC)
                    });
                }
        }

        public bool CanAddNakladInvoice()
        {
            return true;
        }

        #endregion
    }

    public class DataAnnotationDistribNaklad : DataAnnotationForFluentApiBase,
        IMetadataProvider<DistributeNakladViewModel>
    {
        void IMetadataProvider<DistributeNakladViewModel>.BuildMetadata(
            MetadataBuilder<DistributeNakladViewModel> builder)
        {
            builder.Property(_ => _.State).AutoGenerated().DisplayName("Статус").ReadOnly();
            builder.Property(_ => _.AddNomenklCommand).NotAutoGenerated();
            builder.Property(_ => _.CanSerialize).NotAutoGenerated();
            builder.Property(_ => _.DocumentManagerService).NotAutoGenerated();

            #region Form Layout

            // @formatter:off
            builder.DataFormLayout()
                .Group("g0", Orientation.Vertical)
                    .Group("a0",Orientation.Horizontal)
                        .ContainsProperty(_ => _.DocNum)
                        .ContainsProperty(_ => _.DocDate)
                        .ContainsProperty(_ => _.Currency)
                        .ContainsProperty(_ => _.Creator)
                        .ContainsProperty(_ => _.State)
                    .EndGroup()
                    .Group("a2", Orientation.Vertical)
                        
                        .ContainsProperty(_ => _.Note)
                    .EndGroup()
                .EndGroup();
            // @formatter:on

            #endregion
        }
    }
}
