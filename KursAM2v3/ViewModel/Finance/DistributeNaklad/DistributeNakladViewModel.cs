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
using Core.EntityViewModel.CommonReferences;
using Core.EntityViewModel.Invoices;
using Core.EntityViewModel.NomenklManagement;
using Core.Helper;
using Core.Invoices.EntityViewModel;
using Core.Menu;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using Data.Repository;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using Helper;
using KursAM2.Dialogs;
using KursAM2.Managers;
using KursAM2.Repositories;
using KursAM2.View.Finance.DistributeNaklad;

namespace KursAM2.ViewModel.Finance.DistributeNaklad
{
    [MetadataType(typeof(DataAnnotationDistribNaklad))]
    public sealed class DistributeNakladViewModel : RSWindowViewModelBase, IViewModelToEntity<Data.DistributeNaklad>,
        IKursLayoutManager
    {
        #region Constructors

        public DistributeNakladViewModel(Window parentForm, IDocumentOpenType docOpenType) : this()
        {
            ParentFormViewModel = parentForm.DataContext as RSWindowViewModelBase;
            if (docOpenType.Id == null)
            {
                Entity = DistributeNakladRepository.CreateNew();
                State = RowStatus.NewRow;
                return;
            }

            switch (docOpenType.OpenType)
            {
                case DocumentCreateTypeEnum.Open:
                    //Entity = DistributeNakladRepository.GetById((Guid)docOpenType.Id);
                    // ReSharper disable once VirtualMemberCallInConstructor
                    Load(docOpenType.Id);
                    //State = RowStatus.NotEdited;
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

        private DistributeNakladViewModel()
        {
            BaseRepository = new GenericKursDBRepository<Data.DistributeNaklad>(unitOfWork);
            DistributeNakladRepository = new DistributeNakladRepository(unitOfWork);
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.StandartDocWithDeleteRightBar(this);
            WindowName = "Распределение накладных расходов";
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
            = new UnitOfWork<ALFAMEDIAEntities>(new ALFAMEDIAEntities(GlobalOptions.SqlConnectionString));

        // ReSharper disable once NotAccessedField.Local
        public readonly GenericKursDBRepository<Data.DistributeNaklad> BaseRepository;

        // ReSharper disable once NotAccessedField.Local
        public readonly IDistributeNakladRepository DistributeNakladRepository;

        //public readonly IInvoiceProviderRepository InvoiceProviderRepository;

        #endregion

        #region Properties

        [Display(AutoGenerateField = false)]
        public override bool IsDocNewCopyAllow => State != RowStatus.NewRow;
        [Display(AutoGenerateField = false)]
        public override bool IsDocNewCopyRequisiteAllow => State != RowStatus.NewRow;
        [Display(AutoGenerateField = false)]
        public override bool IsDocDeleteAllow => State != RowStatus.NewRow;
        [Display(AutoGenerateField = false)]
        public override bool IsCanRefresh => State != RowStatus.NewRow;
       
        [Display(AutoGenerateField = false)]
        public override string LayoutName => "DistributeNakladViewModel";

        [Display(AutoGenerateField = false)]
        public override string Name => State == RowStatus.NewRow
            ? "Новый документ"
            : $"№ {DocNum} от {DocDate.ToShortDateString()} {Currency}";

        public override string WindowName => Name;

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
            get => GetValue<DistributeNakladRowViewModel>();
            set => SetValue(value, () =>
            {
                DistributeNaklads.Clear();
                if (CurrentTovar != null)
                    foreach (var n in DistributeAllNaklads
                                 .Where(_ => _.RowId == CurrentTovar.Id))
                        DistributeNaklads.Add(n);
            });
        }

        [Display(AutoGenerateField = false)]
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
            get => GetValue<DistributeNakladInfoViewModel>();
            set => SetValue(value);
        }

        [Display(AutoGenerateField = false)]
        public ObservableCollection<DistributeNakladInvoiceViewModel> NakladInvoices { set; get; }
            = new ObservableCollection<DistributeNakladInvoiceViewModel>();

        [Display(AutoGenerateField = false)]
        public ObservableCollection<DistributeNakladInvoiceViewModel> SelectedNakladInvoices { set; get; }
            = new ObservableCollection<DistributeNakladInvoiceViewModel>();

        [Display(AutoGenerateField = false)] public DistributeNakladInvoiceViewModel CurrentNakladInvoice { get; set; }

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
            set => SetValue(value, () => CurrencyDC = value?.DocCode);
        }

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
            {
                if (i.Invoice != null)
                    i.SummaDistribute = DistributeAllNaklads.Where(_ => _.InvoiceNakladId == i.Invoice.Id)
                        .Sum(_ => _.NakladSumma);
                if (i.AccruedAmountRow != null)
                    i.SummaDistribute = DistributeAllNaklads.Where(_ => _.AccrualAmountId == i.AccruedAmountRow.Id)
                        .Sum(_ => _.NakladSumma);
            }
        }

        private void updateNakladsAll(DistributeNakladInvoiceViewModel inv)
        {
            foreach (var n in Tovars)
            {
                DistributeNakladInfoViewModel old;
                if (inv.Invoice != null)
                    old = DistributeAllNaklads.FirstOrDefault(_ => _.InvoiceNakladId == inv.Invoice.Id
                                                                   && _.RowId == n.Id);
                else
                    old = DistributeAllNaklads.FirstOrDefault(_ => _.AccrualAmountId == inv.AccruedAmountRow.Id
                                                                   && _.RowId == n.Id);
                if (old != null)
                {
                    old.NakladSumma = 0;
                    old.DistributeSumma = 0;
                    old.Rate = inv.Rate;
                }
                else
                {
                    var crsDC = (decimal)(inv.Invoice != null
                        ? inv.Invoice.SF_CRS_DC
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

                    inf.NakladSumma = prc * CurrentNakladInvoice.SummaRemain / allSumma;
                }

                inf.DistributeSumma = inf.NakladSumma * inf.Rate;
                inf.InvoiceInfo = CurrentNakladInvoice.Invoice != null
                    ? $"С/ф №{CurrentNakladInvoice.DocNum} от {CurrentNakladInvoice.DocDate} пост.:{CurrentNakladInvoice.ProviderName}"
                    : $"Прямые затраты №{CurrentNakladInvoice.AccruedAmountRow.AccruedAmountOfSupplier.DocInNum} / {CurrentNakladInvoice.AccruedAmountRow.AccruedAmountOfSupplier.DocExtNum} " +
                      $"от {CurrentNakladInvoice.AccruedAmountRow.AccruedAmountOfSupplier.DocDate.ToShortDateString()}";
            }

            RecalcAllResult();
            RaisePropertyChanged();
            RaisePropertiesChanged(nameof(Tovars));
            RaisePropertiesChanged(nameof(NakladInvoices));
        }

        #endregion

        #region Commands

        //DocumentOpenCommand
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
                            AccrualAmountId = d.Id
                        };
                        Entity.DistributeNakladInvoices.Add(ent);
                        NakladInvoices.Add(new DistributeNakladInvoiceViewModel(ent)
                        {
                            AccruedAmountRow = d.Entity,
                            //Summa = d.Summa - d.DistributeSumm,
                            Currency = d.Kongtragent.BalansCurrency
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
                    winManager.ShowWinUIMessageBox($"Сумма распредления {sum} больше, чем сумма счета {invnakl.Summa}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    CurrentDistributeNaklad.NakladSumma -= (decimal)(sum - directZatrat.Summa);
                }

            CurrentDistributeNaklad.DistributeSumma =
                CurrentDistributeNaklad.NakladSumma * CurrentDistributeNaklad.Rate;
            RecalcAllResult();
            if (State != RowStatus.NewRow)
                State = RowStatus.Edited;
            RaisePropertyChanged(nameof(Tovars));
        }


        //[Command]
        //public void OnWindowClosing()
        //{
        //    LayoutManager.Save();
        //}

        //[Command]
        //public void OnWindowLoaded()
        //{
        //    LayoutManager = new Helper.LayoutManager(Form, LayoutSerializationService,
        //        GetType().Name, null);
        //    LayoutManager.Load();
        //}

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
                State = RowStatus.NotEdited;
                RaisePropertiesChanged(nameof(State));
                RaisePropertiesChanged(nameof(Tovars));
                RaisePropertiesChanged(nameof(SelectedTovars));
                Load(Id);
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
                        inv = DistributeNakladRepository.GetInvoiceHead((Guid)invrow.DocId);
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

                RecalcAllResult();
                State = RowStatus.NotEdited;
            }
        }

        [Command]
        public void DistributeTypeChanged()
        {
            recalcDistributeSummForInvoice();
        }

        [Command]
        public void AddNomenkl()
        {
            var dialogs = new InvoiceProviderDialogs { IsNakladInvoices = false, IsLoadForDistributeNaklad = true };
            if (dialogs.ShowDialog(Currency, DateTime.Today.AddDays(-30),
                    DateTime.Today) == true)
                foreach (var d in dialogs.SelectedItems)
                foreach (var r in d.Rows)
                {
                    if (r.IsUsluga) continue;
                    if (Tovars.Any(_ => _.DocId == Id && _.TovarInvoiceRowId == r.Entity.Id)) continue;
                    var newRow = DistributeNakladRepository.CreateRowNew(Entity);
                    if (r.Entity.TD_26_CurrencyConvert != null && r.Entity.TD_26_CurrencyConvert.Count > 0)
                    {
                        foreach (var c in r.Entity.TD_26_CurrencyConvert)
                        {
                            newRow.TransferRowId = c.Id;
                            var newTovar = new DistributeNakladRowViewModel(newRow)
                            {
                                Convert = new InvoiceProviderRowCurrencyConvertViewModel(c),
                                InvoiceRow = r,
                                State = RowStatus.NewRow
                            };
                            ((ISupportParentViewModel)newTovar).ParentViewModel = this;
                            Tovars.Add(newTovar);
                        }
                    }
                    else
                    {
                        newRow.TovarInvoiceRowId = r.Entity.Id;
                        var newTovar = new DistributeNakladRowViewModel(newRow)
                        {
                            InvoiceRow = r,
                            State = RowStatus.NewRow
                        };
                        ((ISupportParentViewModel)newTovar).ParentViewModel = this;
                        Tovars.Add(newTovar);
                    }

                    RaisePropertyChanged();
                }

            SelectedTovars.Clear();
            RaisePropertiesChanged(nameof(Tovars));
            RaisePropertiesChanged(nameof(SelectedTovars));
        }

        public bool CanAddNomenkl()
        {
            return Currency != null;
        }

        [Command]
        public void DeleteNomenkl()
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
                            var o = unitOfWork.Context.DistributeNakladInfo.FirstOrDefault(_ => _.Id == inf.Id);
                            if (o != null)
                                unitOfWork.Context.DistributeNakladInfo.Remove(o);
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
            RaisePropertiesChanged(nameof(Tovars));
            RaisePropertiesChanged(nameof(SelectedTovars));
        }

        public bool CanDeleteNomenkl()
        {
            return CurrentTovar != null;
        }


        [Command]
        public void DeleteNakladInvoice()
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
                    var o = DistributeAllNaklads.First(_ => _.Id == id);
                    var r = Tovars.First(_ => _.Id == o.RowId);
                    r.DistributePrice -= o.DistributeSumma;
                    unitOfWork.Context.DistributeNakladInfo.Remove(o.Entity);
                    DistributeAllNaklads.Remove(o);
                }

                unitOfWork.Context.DistributeNakladInvoices.Remove(CurrentNakladInvoice.Entity);
                NakladInvoices.Remove(CurrentNakladInvoice);
                State = RowStatus.Edited;
            }
        }

        public bool CanDeleteNakladInvoice()
        {
            return CurrentNakladInvoice != null;
        }

        [Command]
        public void AddNakladInvoice()
        {
            var dialogs = new InvoiceProviderDialogs { IsNakladInvoices = true };
            if (dialogs.ShowDialog(null, DateTime.Today.AddDays(-30),
                    DateTime.Today) == true)
                foreach (var d in dialogs.SelectedItems)
                    if (NakladInvoices.Where(_ => _.Invoice != null).All(_ => _.Invoice.DOC_CODE != d.DocCode))
                    {
                        var ent = new DistributeNakladInvoices
                        {
                            Id = Guid.NewGuid(),
                            DocId = Entity.Id,
                            InvoiceId = d.Id
                        };
                        Entity.DistributeNakladInvoices.Add(ent);
                        NakladInvoices.Add(new DistributeNakladInvoiceViewModel(ent)
                        {
                            Invoice = d.Entity,
                            Currency = MainReferences.GetCurrency(d.SF_CRS_DC)
                        });
                    }

            RaisePropertyChanged();
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