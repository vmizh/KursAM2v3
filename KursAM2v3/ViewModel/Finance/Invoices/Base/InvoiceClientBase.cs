using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Core.ViewModel.Base;
using Data;
using KursDomain;
using KursDomain.IDocuments.Finance;
using KursDomain.References;

namespace KursAM2.ViewModel.Finance.Invoices.Base
{
    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public class InvoiceClientBase :  RSViewModelBase, IInvoiceClient
    {
        private Kontragent myClient;

        public InvoiceClientBase(SD_84 entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
        }

        public InvoiceClientBase(IEnumerable<InvoiceClientQuery> invList, bool isLoadDetails = false)
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                if (invList == null || !invList.Any())
                    throw new ArgumentNullException();
                var doc = invList.First();
                DocCode = doc.DocCode;
                Id = doc.Id;
                DocDate = doc.DocDate;
                Receiver = GlobalOptions.ReferencesCache.GetKontragent(doc.ReceiverDC) as Kontragent;
                CO = GlobalOptions.ReferencesCache.GetCentrResponsibility(doc.CentOtvetstDC) as CentrResponsibility;
                VzaimoraschetType =
                    GlobalOptions.ReferencesCache.GetNomenklProductType(doc.VzaimoraschetTypeDC) as NomenklProductType;
                PayCondition = GlobalOptions.ReferencesCache.GetPayCondition(doc.PayConditionDC) as PayCondition;
                InnerNumber = doc.InnerNumber;
                OuterNumber = doc.OuterNumber;
                Client = GlobalOptions.ReferencesCache.GetKontragent(doc.ClientDC) as Kontragent;
                Currency = GlobalOptions.ReferencesCache.GetCurrency(doc.CurrencyDC) as Currency;
                SummaOtgruz = Math.Round(invList.Sum(_ => _.SummaOtgruz), 2);
                DilerSumma = Math.Round(doc.DilerSumma, 2);
                Note = doc.Note;
                Diler = GlobalOptions.ReferencesCache.GetKontragent(doc.DilerDC) as Kontragent;
                IsAccepted = doc.IsAccepted ?? false;
                Summa = Math.Round(doc.Summa ?? 0, 2);
                CREATOR = doc.CREATOR;
                FormRaschet = GlobalOptions.ReferencesCache.GetPayForm(doc.FormRaschetDC) as PayForm;
                IsNDSIncludeInPrice = doc.IsNDSIncludeInPrice ?? false;
                PaySumma = Math.Round(doc.PaySumma ?? 0, 2);
                var r = ctx.SD_84.FirstOrDefault(_ => _.DOC_CODE == doc.DocCode);
                if (r != null)
                {
                    PersonaResponsible = GlobalOptions.ReferencesCache.GetEmployee(r.PersonalResponsibleDC) as Employee;
                }
                else
                {
                    var k = ctx.SD_43.FirstOrDefault(_ => _.DOC_CODE == doc.ClientDC);
                    if (k.OTVETSTV_LICO != null)
                        PersonaResponsible =
                            GlobalOptions.ReferencesCache.GetEmployee(k.OTVETSTV_LICO.Value) as Employee;
                }
            }

            if (!isLoadDetails) return;
            Rows = new ObservableCollection<IInvoiceClientRow>();
            foreach (var r in invList) Rows.Add(new InvoiceClientRowBase(r));
        }

        public PayCondition PayCondition { get; set; }

        public Kontragent Receiver { get; set; }

        public Kontragent Client
        {
            get => myClient;
            set
            {
                if (Equals(value, myClient)) return;
                myClient = value;
                RaisePropertyChanged();
            }
        }

        public Kontragent Diler { get; set; }

        public override decimal DocCode { get; set; }
        public Guid Id { get; set; }
        public CentrResponsibility CO { get; set; }
        public NomenklProductType VzaimoraschetType { get; set; }
        public PayForm FormRaschet { get; set; }
        public DateTime DocDate { get; set; }
        public int InnerNumber { get; set; }
        public string OuterNumber { get; set; }
        public Currency Currency { get; set; }
        public decimal SummaOtgruz { get; set; }
        public decimal DilerSumma { get; set; }
        public override string Note { get; set; }
        public bool IsAccepted { get; set; }
        public decimal Summa { get; set; }
        public string CREATOR { get; set; }
        public bool IsNDSIncludeInPrice { get; set; }
        public decimal PaySumma { get; set; }
        public Employee PersonaResponsible { get; set; }
        public string LastChanger { get; set; }
        public DateTime LastChangerDate { get; set; }
        public ObservableCollection<IInvoiceClientRow> Rows { get; set; }
    }

    public class InvoiceClientRowBase : IInvoiceClientRow
    {
        public InvoiceClientRowBase(InvoiceClientQuery row)
        {
            DocCode = row.DocCode;
            Code = row.RowCode ?? 0;
            Id = row.Row2d ?? Guid.Empty;
            DocId = row.DocId ?? Guid.Empty;
            Nomenkl = GlobalOptions.ReferencesCache.GetNomenkl(row.NomenklDC) as Nomenkl;
            NomNomenkl = Nomenkl?.NomenklNumber;
            IsUsluga = Nomenkl?.IsUsluga ?? false;
            Quantity = row.Quantity ?? 0;
            Price = row.Price ?? 0;
            Summa = Quantity * Price;
            SFT_NACENKA_DILERA = row.SFT_NACENKA_DILERA;
            Shipped = row.Shipped;
            Rest = Quantity - Shipped;
            CurrentRemains = 0;
            NDSPercent = row.NDSPercent ?? 0;
            SFT_SUMMA_NDS = row.SFT_SUMMA_NDS;
        }

        public InvoiceClientRowBase()
        {
        }

        public InvoiceClientRowBase(decimal docCode, int code, Guid id, Guid docId, Nomenkl nomenkl, string nomNomenkl,
            bool isUsluga,
            decimal quantity, decimal price, decimal summa, decimal? sFT_NACENKA_DILERA, decimal shipped,
            decimal rest, decimal currentRemains, string note, decimal nDSPercent, decimal? sFT_SUMMA_NDS,
            SDRSchet sDRSchet, string gruzoDeclaration)
        {
            DocCode = docCode;
            Code = code;
            Id = id;
            DocId = docId;
            Nomenkl = nomenkl;
            NomNomenkl = nomNomenkl;
            IsUsluga = isUsluga;
            Quantity = quantity;
            Price = price;
            Summa = summa;
            SFT_NACENKA_DILERA = sFT_NACENKA_DILERA;
            Shipped = shipped;
            Rest = rest;
            CurrentRemains = currentRemains;
            Note = note;
            NDSPercent = nDSPercent;
            SFT_SUMMA_NDS = sFT_SUMMA_NDS;
            SDRSchet = sDRSchet;
            GruzoDeclaration = gruzoDeclaration;
        }

        public Nomenkl Nomenkl { get; set; }

        public decimal DocCode { get; set; }
        public int Code { get; set; }
        public Guid Id { get; set; }
        public Guid DocId { get; set; }
        public string NomNomenkl { get; set; }
        public Unit Unit { get; set; }
        public bool IsUsluga { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal PriceWithNDS { get; }
        public decimal Summa { get; set; }
        public decimal? SFT_NACENKA_DILERA { get; set; }
        public decimal Shipped { get; set; }
        public decimal Rest { get; set; }
        public decimal CurrentRemains { get; set; }
        public string Note { get; set; }
        public decimal NDSPercent { get; set; }
        public decimal? SFT_SUMMA_NDS { get; set; }
        public SDRSchet SDRSchet { get; set; }
        public string GruzoDeclaration { get; set; }
    }
    
    public class InvoiceClientRemains : IInvoiceClientRemains
    {
        public InvoiceClientRemains(IInvoiceClient inv)
        {
            DocCode = inv.DocCode;
            Id = inv.Id;
            Receiver = inv.Receiver;
            CO = inv.CO;
            VzaimoraschetType = inv.VzaimoraschetType;
            FormRaschet = inv.FormRaschet;
            PayCondition = inv.PayCondition;
            DocDate = inv.DocDate;
            InnerNumber = inv.InnerNumber;
            OuterNumber = inv.OuterNumber;
            Client = inv.Client;
            Currency = inv.Currency;
            SummaOtgruz = inv.SummaOtgruz;
            DilerSumma = inv.DilerSumma;
            Note = inv.Note;
            Diler = inv.Diler;
            IsAccepted = inv.IsAccepted;
            Summa = inv.Summa;
            CREATOR = inv.CREATOR;
            IsNDSIncludeInPrice = inv.IsNDSIncludeInPrice;
            PaySumma = inv.PaySumma;
            PersonaResponsible = inv.PersonaResponsible;
        }

        [Display(AutoGenerateField = false)] 
        public decimal DocCode { get; set; }
        [Display(AutoGenerateField = false)] 
        public Guid Id { get; set; }
        [Display(AutoGenerateField = true, Name = "Поставщик", Order = 9)]
        public Kontragent Receiver { get; set; }
        [Display(AutoGenerateField = true, Name = "Центр ответственности", Order = 12)]
        public CentrResponsibility CO { get; set; }
        [Display(AutoGenerateField = true, Name = "Тип продукции", Order = 13)]
        public NomenklProductType VzaimoraschetType { get; set; }
        [Display(AutoGenerateField = true, Name = "Форма расчетов", Order = 15)]
        public PayForm FormRaschet { get; set; }
        [Display(AutoGenerateField = true, Name = "Условия оплаты", Order = 14)]
        public PayCondition PayCondition { get; set; }
        [Display(AutoGenerateField = true, Name = "Дата", Order = 1)]
        public DateTime DocDate { get; set; }
        [Display(AutoGenerateField = true, Name = "№", Order = 2)]
        public int InnerNumber { get; set; }
        [Display(AutoGenerateField = true, Name = "Внешний №", Order = 3)]
        public string OuterNumber { get; set; }
        [Display(AutoGenerateField = true, Name = "Клиент", Order = 4)]
        public Kontragent Client { get; set; }
        [Display(AutoGenerateField = true, Name = "Валюта", Order = 6)]
        public Currency Currency { get; set; }
        [Display(AutoGenerateField = true, Name = "Отгружено", Order = 8)]
        public decimal SummaOtgruz { get; set; }
        [Display(AutoGenerateField = true, Name = "Сумма дилера", Order = 17)]
        public decimal DilerSumma { get; set; }
        [Display(AutoGenerateField = true, Name = "Примечание", Order = 10)]
        public string Note { get; set; }
        [Display(AutoGenerateField = true, Name = "Дилер", Order = 16)]
        public Kontragent Diler { get; set; }
        [Display(AutoGenerateField = true, Name = "Акцептован", Order = 18)]
        public bool IsAccepted { get; set; }
        [Display(AutoGenerateField = true, Name = "Сумма", Order = 5)]
        [DisplayFormat (DataFormatString="n2") ]
        public decimal Summa { get; set; }
        [Display(AutoGenerateField = true, Name = "Создатель", Order = 11)]
        public string CREATOR { get; set; }
        [Display(AutoGenerateField = true, Name = "НДС в цене", Order = 19)]
        public bool IsNDSIncludeInPrice { get; set; }
        [Display(AutoGenerateField = true, Name = "Оплачено", Order = 7)]
        [DisplayFormat (DataFormatString="n2") ]
        public decimal PaySumma { get; set; }
        [Display(AutoGenerateField = true, Name = "Ответственный", Order = 20)]
        public Employee PersonaResponsible { get; set; }

        public string LastChanger { get; set; }
        public DateTime LastChangerDate { get; set; }
        public ObservableCollection<IInvoiceClientRow> Rows { get; set; }
        [Display(AutoGenerateField = true, Name = "Кол-во", Order = 21)]
        public decimal NomQuantity { get; set; }
    }
}
