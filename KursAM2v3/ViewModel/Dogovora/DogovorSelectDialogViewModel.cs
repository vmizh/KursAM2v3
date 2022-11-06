using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Windows.Controls;
using Core;
using Core.ViewModel.Base;
using Data;
using KursAM2.View.Dogovors;
using KursDomain.References;
using ContractType = KursDomain.Documents.Dogovora.ContractType;

namespace KursAM2.ViewModel.Dogovora
{
    public sealed class DogovorSelectDialogViewModel : RSWindowViewModelBase
    {
        #region Constructors

        public DogovorSelectDialogViewModel(bool isSupplier, decimal? kontrDC = null)
        {
            IsSupplier = isSupplier;
            KontrDC = kontrDC;
            RefreshData(null);
        }

        #endregion

        #region Commands

        public override void RefreshData(object obj)
        {
            DogovorList.Clear();
            DogovorPositionList.Clear();
            using (var ctx = GlobalOptions.GetEntities())
            {
                if (IsSupplier)
                {
                    List<DogovorOfSupplier> data;
                    if (KontrDC != null)
                        data = ctx.DogovorOfSupplier.Include(_ => _.DogovorOfSupplierRow)
                            .AsNoTracking()
                            .Where(_ => _.KontrDC == KontrDC && _.IsClosed == false).ToList();
                    else
                        data = ctx.DogovorOfSupplier.Include(_ => _.DogovorOfSupplierRow)
                            .AsNoTracking()
                            .Where(_ => _.IsClosed == false).ToList();

                    foreach (var d in data)
                        DogovorList.Add(new Dogovor
                        {
                            Dog = d,
                            Id = d.Id,
                            DogNumber = string.IsNullOrWhiteSpace(d.OutNum)
                                ? d.InNum.ToString()
                                : $"{d.InNum} / {d.OutNum}",
                            Date = d.DocDate,
                            DogType = MainReferences.ContractTypes[d.DogType],
                            Kontragent = GlobalOptions.ReferencesCache.GetKontragent(d.KontrDC) as Kontragent,
                            Creator = d.Creator,
                            Note = d.Note,
                            Summa = Math.Round(
                                d.DogovorOfSupplierRow.Sum(_ =>
                                    _.Price * _.Quantity + _.Price * _.Quantity * _.NDSPercent / 100), 2)
                        });
                }
            }
        }

        #endregion

        #region Methods

        private void LoadPositions()
        {
            DogovorPositionList.Clear();
            using (var ctx = GlobalOptions.GetEntities())
            {
                var data = ctx.DogovorOfSupplierRow.Where(_ => _.DocId == CurrentDogovor.Id).ToList();
                foreach (var d in data)
                    DogovorPositionList.Add(new DogovorPosition
                    {
                        Id = d.Id,
                        Quantity = d.Quantity,
                        IsSelected = false,
                        Nomenkl = MainReferences.GetNomenkl(d.NomenklDC),
                        NDSPercent = d.NDSPercent,
                        Note = d.Note,
                        Price = d.Price
                    });
            }
        }

        #endregion

        #region Fields

        private readonly bool IsSupplier;
        private Dogovor myCurrentDogovor;
        private DogovorPosition myCurrentPosition;
        private readonly decimal? KontrDC;

        #endregion

        #region Properties

        public override string LayoutName => "DogovorSelectDialogViewModel";
        public UserControl CustomDataUserControl => new DialogSelectDogovorAndPosition();

        public ObservableCollection<Dogovor> DogovorList { set; get; } = new();

        // ReSharper disable once CollectionNeverUpdated.Global
        public ObservableCollection<DogovorPosition> DogovorPositionList { set; get; } = new();

        public Dogovor CurrentDogovor
        {
            get => myCurrentDogovor;
            set
            {
                if (myCurrentDogovor == value) return;
                myCurrentDogovor = value;
                LoadPositions();
                RaisePropertyChanged();
            }
        }

        public DogovorPosition CurrentPosition
        {
            get => myCurrentPosition;
            set
            {
                if (myCurrentPosition == value) return;
                myCurrentPosition = value;
                RaisePropertyChanged();
            }
        }

        #endregion


        #region Auxiliary classes

        public class Dogovor
        {
            [Display(AutoGenerateField = false)] public DogovorOfSupplier Dog { set; get; }

            [Display(AutoGenerateField = false)] public Guid Id { set; get; }

            [Display(AutoGenerateField = true, Name = "Тип договора")]
            [ReadOnly(true)]
            public ContractType DogType { set; get; }

            [Display(AutoGenerateField = true, Name = "№")]
            [ReadOnly(true)]
            public string DogNumber { set; get; }

            [Display(AutoGenerateField = true, Name = "Дата")]
            [ReadOnly(true)]
            public DateTime Date { set; get; }

            [Display(AutoGenerateField = true, Name = "Контрагент")]
            [ReadOnly(true)]
            public Kontragent Kontragent { set; get; }

            [Display(AutoGenerateField = true, Name = "Сумма")]
            [ReadOnly(true)]
            [DisplayFormat(DataFormatString = "n2")]
            public decimal Summa { set; get; }

            [Display(AutoGenerateField = true, Name = "Валюта")]
            [ReadOnly(true)]
            public Currency Currency => Kontragent?.Currency as Currency;

            [Display(AutoGenerateField = true, Name = "Создатель")]
            [ReadOnly(true)]
            public string Creator { set; get; }

            [Display(AutoGenerateField = true, Name = "Примечание")]
            [ReadOnly(true)]
            public string Note { set; get; }
        }

        public class DogovorPosition
        {
            [Display(AutoGenerateField = true, Name = "Выбрать")]
            [ReadOnly(false)]
            public bool IsSelected { set; get; }

            [Display(AutoGenerateField = false)] public Guid Id { set; get; }

            [Display(AutoGenerateField = true, Name = "Ном.№")]
            [ReadOnly(true)]
            public string NomenklNumber => Nomenkl?.NomenklNumber;

            [Display(AutoGenerateField = true, Name = "Номенклатура")]
            [ReadOnly(true)]
            public Nomenkl Nomenkl { set; get; }

            [Display(AutoGenerateField = true, Name = "Ед.изм.")]
            [ReadOnly(true)]
            public Unit Unit => (Unit) Nomenkl?.Unit;

            [Display(AutoGenerateField = true, Name = "Кол-во")]
            [ReadOnly(true)]
            [DisplayFormat(DataFormatString = "n1")]
            public decimal Quantity { set; get; }

            [Display(AutoGenerateField = true, Name = "Цена")]
            [ReadOnly(true)]
            [DisplayFormat(DataFormatString = "n2")]
            public decimal Price { set; get; }

            [Display(AutoGenerateField = true, Name = "НДС %")]
            [ReadOnly(true)]
            [DisplayFormat(DataFormatString = "n2")]
            public decimal NDSPercent { set; get; }

            [Display(AutoGenerateField = true, Name = "НДС Сумма")]
            [ReadOnly(true)]
            [DisplayFormat(DataFormatString = "n2")]
            public decimal NDSSumma => Math.Round(Price * Quantity * NDSPercent / 100, 2);

            [Display(AutoGenerateField = true, Name = "Сумма")]
            [ReadOnly(true)]
            [DisplayFormat(DataFormatString = "n2")]
            public decimal Summa => Price * Quantity + NDSSumma;

            [Display(AutoGenerateField = true, Name = "Примечание")]
            [ReadOnly(true)]
            public string Note { set; get; }
        }

        #endregion
    }
}
