using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Core;
using Core.EntityViewModel;
using Core.Helper;
using Core.Menu;
using Core.Repository.Base;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using Data.Repository;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using Helper;
using KursAM2.Dialogs;
using KursAM2.Repositories;
using KursAM2.Repositories.InvoicesRepositories;

namespace KursAM2.ViewModel.Finance.DistributeNaklad
{
    [MetadataType(typeof(DataAnnotationDistribNaklad))]
    [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
    public class DistributeNakladViewModel : KursBaseControlViewModel, IViewModelToEntity<Data.DistributeNaklad>,
        IKursLayoutManager
    {
        #region Constructors

        public DistributeNakladViewModel(Window parentForm, IDocumentOpenType docOpenType) : this()
        {
            Form = parentForm;
            if (docOpenType.Id == null)
            {
                Entity = DistributeNakladRepository.CreateNew();
                State = RowStatus.NewRow;
                return;
            }

            switch (docOpenType.OpenType)
            {
                case DocumentCreateTypeEnum.Open:
                    Entity = DistributeNakladRepository.GetById((Guid) docOpenType.Id);
                    // ReSharper disable once VirtualMemberCallInConstructor
                    Load(docOpenType.Id);
                    State = RowStatus.NotEdited;
                    break;
                case DocumentCreateTypeEnum.Copy:
                    Entity = DistributeNakladRepository.CreateCopy((Guid) docOpenType.Id);
                    State = RowStatus.NewRow;
                    break;
                case DocumentCreateTypeEnum.RequisiteCopy:
                    Entity = DistributeNakladRepository.CreateRequisiteCopy((Guid) docOpenType.Id);
                    State = RowStatus.NewRow;
                    break;
            }
        }

        private DistributeNakladViewModel()
        {
            BaseRepository = new GenericKursDBRepository<Data.DistributeNaklad>(unitOfWork);
            DistributeNakladRepository = new DistributeNakladRepository(unitOfWork);
            InvoiceProviderRepository = new InvoiceProviderRepository(unitOfWork);
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

        public readonly IInvoiceProviderRepository InvoiceProviderRepository;

        #endregion

        #region Properties

        [Display(AutoGenerateField = false)] public Helper.LayoutManager LayoutManager { get; set; }

        private ILayoutSerializationService LayoutSerializationService
            => GetService<ILayoutSerializationService>();

        public override Guid Id
        {
            get => Entity.Id;
            set
            {
                if (Entity.Id == value) return;
                Entity.Id = value;
                SetChangeStatus();
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

        [Display(AutoGenerateField = false)]
        public DistributeNakladInvoiceViewModel CurrentNakladInvoice
        {
            get => GetValue<DistributeNakladInvoiceViewModel>();
            set => SetValue(value);
        }

        [DisplayName("Entity")]
        [Display(AutoGenerateField = false)]
        public Data.DistributeNaklad Entity
        {
            get => GetValue<Data.DistributeNaklad>();
            set => SetValue(value, () => { SetChangeStatus(); });
        }

        [DisplayName("Дата")]
        [Display(AutoGenerateField = true)]
        public DateTime DocDate
        {
            get => Entity.DocDate;
            set
            {
                if (Entity.DocDate == value) return;
                Entity.DocDate = value;
                SetChangeStatus();
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
                SetChangeStatus();
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
                SetChangeStatus();
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
                SetChangeStatus();
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
        public string Note
        {
            get => Entity.Note;
            set
            {
                if (Entity.Note == value) return;
                Entity.Note = value;
                SetChangeStatus();
            }
        }

        #endregion

        #region Methods

        private void recalcAllResult()
        {
            foreach (var t in Tovars)
            {
                t.DistributeSumma = DistributeAllNaklads.Where(_ => _.RowId == t.Id)
                    .Sum(_ => _.DistributeSumma);
                t.DistributePrice = t.DistributeSumma / t.Quantity;
            }

            foreach (var i in NakladInvoices)
            {
                i.SummaDistribute = DistributeAllNaklads.Where(_ => _.InvoiceNakladId == i.Invoice.Id)
                    .Sum(_ => _.NakladSumma);
            }
        }

        private void updateNakladsAll(DistributeNakladInvoiceViewModel inv)
        {
            foreach (var n in Tovars)
            {
                var old = DistributeAllNaklads.FirstOrDefault(_ => _.RowId == n.Id);
                if (old != null)
                {
                    old.NakladSumma = 0;
                    old.DistributeSumma = 0;
                }
                else
                {

                    var newInfo = new DistributeNakladInfo
                    {
                        Id = Guid.NewGuid(),
                        InvoiceNakladId = inv.Invoice.Id,
                        DistributeNakladRow = n.Entity,
                        NakladSumma = 0,
                        DistributeSumma = 0,
                        // ReSharper disable once PossibleInvalidOperationException
                        InvoiceCrsDC = (decimal) inv.Invoice.SF_CRS_DC,
                        Rate = 1,
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

            foreach (var inf in DistributeAllNaklads)
            {
                if (allSumma == 0)
                {
                    inf.NakladSumma = allSumma / Tovars.Count;
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
                inf.InvoiceInfo = $"С/ф №{CurrentNakladInvoice.DocNum} от {CurrentNakladInvoice.DocDate} пост.:{CurrentNakladInvoice.ProviderName}";

            }
            recalcAllResult();
            SetChangeStatus();
            RaisePropertiesChanged(nameof(Tovars));
            RaisePropertiesChanged(nameof(NakladInvoices));
        }

        #endregion

        #region Commands

        [Command]
        public void OnWindowClosing()
        {
            LayoutManager.Save();
        }

        [Command]
        public void OnWindowLoaded()
        {
            LayoutManager = new Helper.LayoutManager(Form, LayoutSerializationService,
                GetType().Name, null);
            LayoutManager.Load();
        }

        public override void Save()
        {
            try
            {
                unitOfWork.CreateTransaction();
                DistributeNakladRepository.UpdateSFNaklad(Entity);
                unitOfWork.Save();
                unitOfWork.Commit();
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

        public override bool IsCanRefresh { get; } = true;

        public override bool CanSave()
        {
            return State != RowStatus.NotEdited;
        }

        public override void Load(object o)
        {
            if (o is Guid id)
            {
                Entity = DistributeNakladRepository.GetById(id);
                Tovars.Clear();
                NakladInvoices.Clear();
                DistributeAllNaklads.Clear();
                foreach (var inv in Entity.DistributeNakladInvoices)
                {
                    var invoice = InvoiceProviderRepository.GetById(inv.InvoiceId);
                    NakladInvoices.Add(new DistributeNakladInvoiceViewModel(inv)
                    {
                        Invoice = invoice.Entity,
                        Summa = invoice.SF_KONTR_CRS_SUMMA ?? 0,

                    });
                }
                foreach (var r in Entity.DistributeNakladRow)
                {
                    InvoiceProvider inv = null;
                    var invrow = InvoiceProviderRepository.GetInvoiceRow(r.TovarInvoiceRowId);
                    if (invrow != null)
                        // ReSharper disable once PossibleInvalidOperationException
                        inv = InvoiceProviderRepository.GetInvoiceHead((Guid) invrow.DocId);
                    Tovars.Add(new DistributeNakladRowViewModel(r)
                    {
                        InvoiceRow = invrow,
                        Invoice = inv,
                        State = RowStatus.NotEdited
                    });
                    foreach (var inf in r.DistributeNakladInfo)
                    {
                        var newItem = new DistributeNakladInfoViewModel(inf);
                        var iinv = NakladInvoices.FirstOrDefault(_ => _.Entity.InvoiceId == inf.InvoiceNakladId);
                        if (iinv != null)
                        {
                            newItem.InvoiceInfo = $"С/ф №{iinv.DocNum} от {iinv.DocDate} пост.:{iinv.ProviderName}";
                        }
                        DistributeAllNaklads.Add(newItem);
                    }
                    foreach (var i in NakladInvoices)
                    {
                        i.SummaDistribute = DistributeAllNaklads.Where(_ => _.InvoiceNakladId == i.Invoice.Id)
                            .Sum(_ => _.NakladSumma);
                    }
                }

                recalcAllResult();
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
            InvoiceProviderDialogs dialogs = new InvoiceProviderDialogs {IsNakladInvoices = false};
            if (dialogs.ShowDialog(Currency, DateTime.Today.AddDays(-30),
                    DateTime.Today) == true)
                foreach (var d in dialogs.SelectedItems)
                foreach (var r in d.Rows)
                {
                    if (r.IsUsluga) continue;
                    if (Tovars.Any(_ => _.DocId == Id && _.TovarInvoiceRowId == r.Entity.Id)) continue;
                    var newRow = DistributeNakladRepository.CreateRowNew(Entity);
                    newRow.TovarInvoiceRowId = r.Entity.Id;
                    var newTovar = new DistributeNakladRowViewModel(newRow)
                    {
                        InvoiceRow = r,
                        State = RowStatus.NewRow
                    };
                    ((ISupportParentViewModel) newTovar).ParentViewModel = this;
                    Tovars.Add(newTovar);
                    SetChangeStatus();
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
                        .Select(_ => _.Id);
                    foreach (var id1 in rids)
                    {
                        var old = DistributeAllNaklads.First(_ => _.Id == id1);
                        DistributeAllNaklads.Remove(old);
                    }
                    Tovars.Remove(item);
                }
            }

            SelectedTovars.Clear();
            recalcAllResult();
            SetChangeStatus();
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
                var ids = DistributeAllNaklads.Where(_ => _.InvoiceNakladId == CurrentNakladInvoice.Invoice.Id)
                    .Select(_ => _.Id).ToList();
                foreach (var id in ids)
                {
                    var o = DistributeAllNaklads.First(_ => _.Id == id);
                    var r = Tovars.First(_ => _.Id == o.RowId);
                    r.DistributePrice -= o.DistributeSumma;
                    r.Entity.DistributeNakladInfo.Remove(o.Entity);
                    DistributeAllNaklads.Remove(o);
                }
                //Entity.DistributeNakladInvoices.Remove(CurrentNakladInvoice.Entity);
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
            InvoiceProviderDialogs dialogs = new InvoiceProviderDialogs {IsNakladInvoices = true};
            if (dialogs.ShowDialog(null, DateTime.Today.AddDays(-30),
                    DateTime.Today) == true)
                foreach (var d in dialogs.SelectedItems)
                    if (NakladInvoices.All(_ => _.Invoice.DOC_CODE != d.DOC_CODE))
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
                            Summa = d.SF_KONTR_CRS_SUMMA ?? 0,
                            Currency = MainReferences.GetCurrency(d.SF_CRS_DC)
                        });
                    }
            SetChangeStatus();
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