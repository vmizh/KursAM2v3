﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Core.EntityViewModel.CommonReferences;
using Core.EntityViewModel.CommonReferences.Kontragent;
using Core.EntityViewModel.NomenklManagement;
using Core.EntityViewModel.Vzaimozachet;
using Core.Helper;
using Core.Invoices.EntityViewModel;
using Core.ViewModel.Base;
using Data;
using Data.Repository;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;

namespace Core.EntityViewModel.Invoices
{
    [MetadataType(typeof(DataAnnotationsSFClientViewModel))]
    [SuppressMessage("ReSharper", "MemberInitializerValueIgnored")]
    [SuppressMessage("ReSharper", "MethodOverloadWithOptionalParameter")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public sealed class InvoiceClient : RSViewModelBase, IEntity<SD_84>, IDataErrorInfo
    {
        private readonly UnitOfWork<ALFAMEDIAEntities> context;
        private SD_84 myEntity;
        private bool isLoadPayment = false;

        public InvoiceClient()
        {
            Entity = DefaultValue();
            DeletedRows = new List<InvoiceClientRow>();
            Rows = new ObservableCollection<InvoiceClientRow>();
            ShipmentRows = new ObservableCollection<ShipmentRowViewModel>();
            Rows.CollectionChanged += (o, args) => State = RowStatus.Edited;
        }

        public InvoiceClient(SD_84 entity)
        {
            Entity = entity ?? DefaultValue();
            Rows = new ObservableCollection<InvoiceClientRow>();
            ShipmentRows = new ObservableCollection<ShipmentRowViewModel>();
            LoadReferences();

            // ReSharper disable once PossibleNullReferenceException
            if (Entity.TD_84 == null || Entity.TD_84.Count <= 0) return;
            foreach (var row in Entity.TD_84)
            {
                var newRow = new InvoiceClientRow(row)
                {
                    Parent = this,
                    IsNDSInPrice = IsNDSIncludeInPrice
                };
                Rows.Add(newRow);
            }
        }

        public InvoiceClient(SD_84 entity, UnitOfWork<ALFAMEDIAEntities> ctx)
        {
            context = ctx;
            Entity = entity ?? DefaultValue();
            LoadReferences();
        }
        
        public InvoiceClient(SD_84 entity, UnitOfWork<ALFAMEDIAEntities> ctx, bool isLoadPaymentDocs = false)
        {
            isLoadPayment = isLoadPaymentDocs;
            context = ctx;
            Entity = entity ?? DefaultValue();
            LoadReferences();
        }

        public bool IsAccepted
        {
            get => Entity.SF_ACCEPTED == 1;
            set
            {
                if (Entity.SF_ACCEPTED == 1 == value) return;
                Entity.SF_ACCEPTED = (short) (value ? 1 : 0);
                RaisePropertyChanged();
            }
        }

        public override Guid Id
        {
            set
            {
                if (Entity.Id == value) return;
                Entity.Id = value;
                RaisePropertyChanged();
            }
            get => Entity.Id;
        }

        public Kontragent Diler
        {
            get
            {
                if (Entity.SF_DILER_DC == null) return null;
                return MainReferences.AllKontragents.ContainsKey(Entity.SF_DILER_DC.Value)
                    ? MainReferences.AllKontragents[Entity.SF_DILER_DC.Value]
                    : null;
            }
            set
            {
                if (value == null)
                {
                    Entity.SF_DILER_DC = null;
                }
                else
                {
                    if (Entity.SF_DILER_DC == value.DOC_CODE) return;
                    Entity.SF_DILER_DC = value.DOC_CODE;
                }

                RaisePropertyChanged();
            }
        }

        public List<InvoiceClientRow> DeletedRows { set; get; } = new();

        public ObservableCollection<InvoiceClientRow> Rows { set; get; } =
            new();

        public ObservableCollection<ShipmentRowViewModel> ShipmentRows { set; get; } =
            new();

        public ObservableCollection<InvoicePaymentDocument> PaymentDocs { set; get; } =
            new();

        public decimal DilerSumma => Rows.Sum(_ => _.SFT_NACENKA_DILERA ?? 0);

        public Kontragent Receiver
        {
            //SF_RECEIVER_KONTR_DC
            get => MainReferences.GetKontragent(Entity.SF_RECEIVER_KONTR_DC);
            set
            {
                if (MainReferences.GetKontragent(Entity.SF_RECEIVER_KONTR_DC) == value) return;
                Entity.SF_RECEIVER_KONTR_DC = value?.DOC_CODE;
                RaisePropertyChanged();
            }
        }

        public Kontragent Client
        {
            get => MainReferences.GetKontragent(Entity.SF_CLIENT_DC);
            set
            {
                if (MainReferences.GetKontragent(Entity.SF_CLIENT_DC) == value) return;
                Entity.SF_CLIENT_DC = value?.DocCode;
                Entity.SF_CLIENT_NAME = value?.Name;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        ///     Ответственный
        /// </summary>
        public Employee.Employee PersonaResponsible
        {
            get => MainReferences.GetEmployee(Entity.PersonalResponsibleDC);
            set
            {
                if (MainReferences.GetEmployee(Entity.PersonalResponsibleDC) == value) return;
                Entity.PersonalResponsibleDC = value?.DocCode;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(EmployeeTabelNumber));
            }
        }

        public int? EmployeeTabelNumber => PersonaResponsible?.TabelNumber;

        public CentrOfResponsibility CO
        {
            get => MainReferences.GetCO(Entity.SF_CENTR_OTV_DC);
            set
            {
                if (MainReferences.GetCO(Entity.SF_CENTR_OTV_DC) == value) return;
                Entity.SF_CENTR_OTV_DC = value?.DocCode;
                RaisePropertyChanged();
            }
        }

        public VzaimoraschetType VzaimoraschetType
        {
            get => MainReferences.GetVzaimoraschetType(Entity.SF_VZAIMOR_TYPE_DC);
            set
            {
                if (MainReferences.GetVzaimoraschetType(Entity.SF_VZAIMOR_TYPE_DC) == value) return;
                Entity.SF_VZAIMOR_TYPE_DC = value?.DocCode;
                RaisePropertyChanged();
            }
        }

        public FormPay FormRaschet
        {
            get => MainReferences.GetFormPay(Entity.SF_FORM_RASCH_DC);
            set
            {
                if (MainReferences.GetFormPay(Entity.SF_FORM_RASCH_DC) == value) return;
                Entity.SF_FORM_RASCH_DC = value?.DocCode;
                RaisePropertyChanged();
            }
        }

        public PayCondition PayCondition
        {
            get => MainReferences.GetPayCondition(Entity.SF_PAY_COND_DC);
            set
            {
                if (MainReferences.GetPayCondition(Entity.SF_PAY_COND_DC) == value) return;
                if (value != null)
                    Entity.SF_PAY_COND_DC = value.DocCode;
                RaisePropertyChanged();
            }
        }

        public decimal SummaOtgruz
        {
            get
            {
                decimal sum = 0;
                if (Entity.TD_84 is {Count: > 0})
                    sum += (from d in Entity.TD_84
                        from o in d.TD_24
                        select o.DDT_KOL_RASHOD * ((d.SFT_SUMMA_K_OPLATE ?? 0) / (decimal) d.SFT_KOL)).Sum();
                return sum;
            }
        }

        public Currency Currency
        {
            get => MainReferences.GetCurrency(Entity.SF_CRS_DC);
            set
            {
                if (MainReferences.GetCurrency(Entity.SF_CRS_DC) == value) return;
                if (value != null)
                {
                    Entity.SF_CRS_DC = value.DocCode;
                    Entity.SF_KONTR_CRS_DC = value.DocCode;
                }

                RaisePropertyChanged();
            }
        }

        public decimal PaySumma => PaymentDocs?.Sum(_ => _.Summa) ?? 0;

        public override decimal DocCode
        {
            get => Entity.DOC_CODE;
            set
            {
                if (Entity.DOC_CODE == value) return;
                Entity.DOC_CODE = value;
                RaisePropertyChanged();
            }
        }

        public override string Description => Entity != null
            ? $"С/ф №{Entity.SF_IN_NUM}/{Entity.SF_OUT_NUM} " +
              $"от {Entity.SF_DATE.ToShortDateString()} {Client} {Summa} {Currency} {Note} "
            : null;

        public override string Name
            => Entity.DOC_CODE > 0
                ? $"С/ф №{Entity.SF_IN_NUM}/{Entity.SF_OUT_NUM} " +
                  $"от {Entity.SF_DATE.ToShortDateString()} {Summa} {Currency} {Note}"
                : null;

        public int InnerNumber
        {
            get => Entity.SF_IN_NUM;
            set
            {
                if (Entity.SF_IN_NUM == value) return;
                Entity.SF_IN_NUM = value;
                RaisePropertyChanged();
            }
        }

        public string OuterNumber

        {
            get => Entity.SF_OUT_NUM;
            set
            {
                if (Entity.SF_OUT_NUM == value) return;
                Entity.SF_OUT_NUM = value;
                RaisePropertyChanged();
            }
        }

        public DateTime DocDate
        {
            get => Entity.SF_DATE;
            set
            {
                if (Entity.SF_DATE == value) return;
                Entity.SF_DATE = value;
                RaisePropertyChanged();
            }
        }

        public string SF_CLIENT_NAME
        {
            get => Entity.SF_CLIENT_NAME;
            set
            {
                if (Entity.SF_CLIENT_NAME == value) return;
                Entity.SF_CLIENT_NAME = value;
                RaisePropertyChanged();
            }
        }

        public decimal? Summa =>
            Rows == null || Rows.Count == 0 ? 0 : Rows.Sum(_ => _.Summa);

        public double SF_CRS_RATE
        {
            get => Entity.SF_CRS_RATE;
            set
            {
                if (Math.Abs(Entity.SF_CRS_RATE - value) < 0) return;
                Entity.SF_CRS_RATE = value;
                RaisePropertyChanged();
            }
        }

        public decimal? SF_RUB_SUMMA_K_OPLATE
        {
            get => Entity.SF_RUB_SUMMA_K_OPLATE;
            set
            {
                if (Entity.SF_RUB_SUMMA_K_OPLATE == value) return;
                Entity.SF_RUB_SUMMA_K_OPLATE = value;
                RaisePropertyChanged();
            }
        }

        public decimal SF_PAY_COND_DC
        {
            get => Entity.SF_PAY_COND_DC;
            set
            {
                if (Entity.SF_PAY_COND_DC == value) return;
                Entity.SF_PAY_COND_DC = value;
                RaisePropertyChanged();
            }
        }

        public string CREATOR
        {
            get => Entity.CREATOR;
            set
            {
                if (Entity.CREATOR == value) return;
                Entity.CREATOR = value;
                RaisePropertyChanged();
            }
        }

        public short SF_ACCEPTED
        {
            get => Entity.SF_ACCEPTED;
            set
            {
                if (Entity.SF_ACCEPTED == value) return;
                Entity.SF_ACCEPTED = value;
                RaisePropertyChanged();
            }
        }

        public string SF_GROZOOTPRAVITEL
        {
            get => Entity.SF_GROZOOTPRAVITEL;
            set
            {
                if (Entity.SF_GROZOOTPRAVITEL == value) return;
                Entity.SF_GROZOOTPRAVITEL = value;
                RaisePropertyChanged();
            }
        }

        public string SF_GRUZOPOLUCHATEL
        {
            get => Entity.SF_GRUZOPOLUCHATEL;
            set
            {
                if (Entity.SF_GRUZOPOLUCHATEL == value) return;
                Entity.SF_GRUZOPOLUCHATEL = value;
                RaisePropertyChanged();
            }
        }

        public string SF_PAYDOC_TEXT
        {
            get => Entity.SF_PAYDOC_TEXT;
            set
            {
                if (Entity.SF_PAYDOC_TEXT == value) return;
                Entity.SF_PAYDOC_TEXT = value;
                RaisePropertyChanged();
            }
        }

        public decimal? SF_BASE_DC
        {
            get => Entity.SF_BASE_DC;
            set
            {
                if (Entity.SF_BASE_DC == value) return;
                Entity.SF_BASE_DC = value;
                RaisePropertyChanged();
            }
        }

        public string SF_DOPOLN
        {
            get => Entity.SF_DOPOLN;
            set
            {
                if (Entity.SF_DOPOLN == value) return;
                Entity.SF_DOPOLN = value;
                RaisePropertyChanged();
            }
        }

        public short? SF_SCHET_FLAG
        {
            get => Entity.SF_SCHET_FLAG;
            set
            {
                if (Entity.SF_SCHET_FLAG == value) return;
                Entity.SF_SCHET_FLAG = value;
                RaisePropertyChanged();
            }
        }

        public short? SF_SCHET_FACT_FLAG
        {
            get => Entity.SF_SCHET_FACT_FLAG;
            set
            {
                if (Entity.SF_SCHET_FACT_FLAG == value) return;
                Entity.SF_SCHET_FACT_FLAG = value;
                RaisePropertyChanged();
            }
        }

        public decimal? SF_VZAIMOR_TYPE_DC
        {
            get => Entity.SF_VZAIMOR_TYPE_DC;
            set
            {
                if (Entity.SF_VZAIMOR_TYPE_DC == value) return;
                Entity.SF_VZAIMOR_TYPE_DC = value;
                RaisePropertyChanged();
            }
        }

        public decimal? SF_FACT_SUMMA
        {
            get => Entity.SF_FACT_SUMMA;
            set
            {
                if (Entity.SF_FACT_SUMMA == value) return;
                Entity.SF_FACT_SUMMA = value;
                RaisePropertyChanged();
            }
        }

        public short? SF_PAY_FLAG
        {
            get => Entity.SF_PAY_FLAG;
            set
            {
                if (Entity.SF_PAY_FLAG == value) return;
                Entity.SF_PAY_FLAG = value;
                RaisePropertyChanged();
            }
        }

        public decimal? SF_CENTR_OTV_DC
        {
            get => Entity.SF_CENTR_OTV_DC;
            set
            {
                if (Entity.SF_CENTR_OTV_DC == value) return;
                Entity.SF_CENTR_OTV_DC = value;
                RaisePropertyChanged();
            }
        }

        public decimal? SF_RECEIVER_KONTR_DC
        {
            get => Entity.SF_RECEIVER_KONTR_DC;
            set
            {
                if (Entity.SF_RECEIVER_KONTR_DC == value) return;
                Entity.SF_RECEIVER_KONTR_DC = value;
                RaisePropertyChanged();
            }
        }

        public int? SF_RECEIVER_RS_CODE
        {
            get => Entity.SF_RECEIVER_RS_CODE;
            set
            {
                if (Entity.SF_RECEIVER_RS_CODE == value) return;
                Entity.SF_RECEIVER_RS_CODE = value;
                RaisePropertyChanged();
            }
        }

        public short? SF_TRANZIT
        {
            get => Entity.SF_TRANZIT;
            set
            {
                if (Entity.SF_TRANZIT == value) return;
                Entity.SF_TRANZIT = value;
                RaisePropertyChanged();
            }
        }

        public decimal? SF_TRANZIT_SPOST_DC
        {
            get => Entity.SF_TRANZIT_SPOST_DC;
            set
            {
                if (Entity.SF_TRANZIT_SPOST_DC == value) return;
                Entity.SF_TRANZIT_SPOST_DC = value;
                RaisePropertyChanged();
            }
        }

        public double? SF_TRANZIT_NACEN_PERC
        {
            get => Entity.SF_TRANZIT_NACEN_PERC;
            set
            {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (Entity.SF_TRANZIT_NACEN_PERC == value) return;
                Entity.SF_TRANZIT_NACEN_PERC = value;
                RaisePropertyChanged();
            }
        }

        public decimal? SF_KONTR_CRS_DC
        {
            get => Entity.SF_KONTR_CRS_DC;
            set
            {
                if (Entity.SF_KONTR_CRS_DC == value) return;
                Entity.SF_KONTR_CRS_DC = value;
                RaisePropertyChanged();
            }
        }

        public decimal? SF_KONTR_CRS_SUMMA
        {
            get => Entity.SF_KONTR_CRS_SUMMA;
            set
            {
                if (Entity.SF_KONTR_CRS_SUMMA == value) return;
                Entity.SF_KONTR_CRS_SUMMA = value;
                RaisePropertyChanged();
            }
        }

        public double? SF_KONTR_CRS_RATE
        {
            get => Entity.SF_KONTR_CRS_RATE;
            set
            {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (Entity.SF_KONTR_CRS_RATE == value) return;
                Entity.SF_KONTR_CRS_RATE = value;
                RaisePropertyChanged();
            }
        }

        public decimal? SF_UCHET_VALUTA_DC
        {
            get => Entity.SF_UCHET_VALUTA_DC;
            set
            {
                if (Entity.SF_UCHET_VALUTA_DC == value) return;
                Entity.SF_UCHET_VALUTA_DC = value;
                RaisePropertyChanged();
            }
        }

        public double? SF_UCHET_VALUTA_RATE
        {
            get => Entity.SF_UCHET_VALUTA_RATE;
            set
            {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (Entity.SF_UCHET_VALUTA_RATE == value) return;
                Entity.SF_UCHET_VALUTA_RATE = value;
                RaisePropertyChanged();
            }
        }

        public decimal? SF_SUMMA_V_UCHET_VALUTE
        {
            get => Entity.SF_SUMMA_V_UCHET_VALUTE;
            set
            {
                if (Entity.SF_SUMMA_V_UCHET_VALUTE == value) return;
                Entity.SF_SUMMA_V_UCHET_VALUTE = value;
                RaisePropertyChanged();
            }
        }

        // ReSharper disable once InconsistentNaming
        public short? SF_NDS_1INCLUD_0NO
        {
            get => Entity.SF_NDS_1INCLUD_0NO;
            set
            {
                if (Entity.SF_NDS_1INCLUD_0NO == value) return;
                Entity.SF_NDS_1INCLUD_0NO = value;
                RaisePropertyChanged();
            }
        }

        public bool IsNDSIncludeInPrice
        {
            get => (SF_NDS_1INCLUD_0NO ?? 0) == 1;
            set
            {
                if ((SF_NDS_1INCLUD_0NO ?? 0) == 1 == value) return;
                SF_NDS_1INCLUD_0NO = (short?) ((SF_NDS_1INCLUD_0NO ?? 0) == 1 ? 0 : 1);
                foreach (var r in Rows) r.IsNDSInPrice = (SF_NDS_1INCLUD_0NO ?? 0) == 1;
                RaisePropertyChanged();
            }
        }

        public decimal? SF_FORM_RASCH_DC
        {
            get => Entity.SF_FORM_RASCH_DC;
            set
            {
                if (Entity.SF_FORM_RASCH_DC == value) return;
                Entity.SF_FORM_RASCH_DC = value;
                RaisePropertyChanged();
            }
        }

        public double? SF_NALOG_NA_PROD_PROC
        {
            get => Entity.SF_NALOG_NA_PROD_PROC;
            set
            {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (Entity.SF_NALOG_NA_PROD_PROC == value) return;
                Entity.SF_NALOG_NA_PROD_PROC = value;
                RaisePropertyChanged();
            }
        }

        public decimal? SF_DILER_DC
        {
            get => Entity.SF_DILER_DC;
            set
            {
                if (Entity.SF_DILER_DC == value) return;
                Entity.SF_DILER_DC = value;
                RaisePropertyChanged();
            }
        }

        public decimal? SF_DILER_SUMMA
        {
            get => Entity.SF_DILER_SUMMA;
            set
            {
                if (Entity.SF_DILER_SUMMA == value) return;
                Entity.SF_DILER_SUMMA = value;
                RaisePropertyChanged();
            }
        }

        public decimal? SF_DILER_CRS_DC
        {
            get => Entity.SF_DILER_CRS_DC;
            set
            {
                if (Entity.SF_DILER_CRS_DC == value) return;
                Entity.SF_DILER_CRS_DC = value;
                RaisePropertyChanged();
            }
        }

        public double? SF_DILER_RATE
        {
            get => Entity.SF_DILER_RATE;
            set
            {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (Entity.SF_DILER_RATE == value) return;
                Entity.SF_DILER_RATE = value;
                RaisePropertyChanged();
            }
        }

        // ReSharper disable once InconsistentNaming
        public short? SF_1INCLUD_NAL_S_PROD_0NO
        {
            get => Entity.SF_1INCLUD_NAL_S_PROD_0NO;
            set
            {
                if (Entity.SF_1INCLUD_NAL_S_PROD_0NO == value) return;
                Entity.SF_1INCLUD_NAL_S_PROD_0NO = value;
                RaisePropertyChanged();
            }
        }

        public decimal? SF_DOP_USL_DC
        {
            get => Entity.SF_DOP_USL_DC;
            set
            {
                if (Entity.SF_DOP_USL_DC == value) return;
                Entity.SF_DOP_USL_DC = value;
                RaisePropertyChanged();
            }
        }

        public short? SF_AUTOMAT_CREATE
        {
            get => Entity.SF_AUTOMAT_CREATE;
            set
            {
                if (Entity.SF_AUTOMAT_CREATE == value) return;
                Entity.SF_AUTOMAT_CREATE = value;
                RaisePropertyChanged();
            }
        }

        public override string Note
        {
            get => Entity.SF_NOTE;
            set
            {
                if (Entity.SF_NOTE == value) return;
                Entity.SF_NOTE = value;
                RaisePropertyChanged();
            }
        }

        public int? SF_SOTRUDNIK_TN
        {
            get => Entity.SF_SOTRUDNIK_TN;
            set
            {
                if (Entity.SF_SOTRUDNIK_TN == value) return;
                Entity.SF_SOTRUDNIK_TN = value;
                RaisePropertyChanged();
            }
        }

        public byte[] TSTAMP
        {
            get => Entity.TSTAMP;
            set
            {
                if (Entity.TSTAMP == value) return;
                Entity.TSTAMP = value;
                RaisePropertyChanged();
            }
        }

        public string SF_CORRECT_NUM
        {
            get => Entity.SF_CORRECT_NUM;
            set
            {
                if (Entity.SF_CORRECT_NUM == value) return;
                Entity.SF_CORRECT_NUM = value;
                RaisePropertyChanged();
            }
        }

        public DateTime? SF_CORRECT_DATE
        {
            get => Entity.SF_CORRECT_DATE;
            set
            {
                if (Entity.SF_CORRECT_DATE == value) return;
                Entity.SF_CORRECT_DATE = value;
                RaisePropertyChanged();
            }
        }

        public string SF_CRS_CODE
        {
            get => Entity.SF_CRS_CODE;
            set
            {
                if (Entity.SF_CRS_CODE == value) return;
                Entity.SF_CRS_CODE = value;
                RaisePropertyChanged();
            }
        }

        public string CORRECTION_NUM
        {
            get => Entity.CORRECTION_NUM;
            set
            {
                if (Entity.CORRECTION_NUM == value) return;
                Entity.CORRECTION_NUM = value;
                RaisePropertyChanged();
            }
        }

        public string CORRECTION_DATE
        {
            get => Entity.CORRECTION_DATE;
            set
            {
                if (Entity.CORRECTION_DATE == value) return;
                Entity.CORRECTION_DATE = value;
                RaisePropertyChanged();
            }
        }

        public string COUNTRY_NAME
        {
            get => Entity.COUNTRY_NAME;
            set
            {
                if (Entity.COUNTRY_NAME == value) return;
                Entity.COUNTRY_NAME = value;
                RaisePropertyChanged();
            }
        }

        public string COUNTRY_CODE
        {
            get => Entity.COUNTRY_CODE;
            set
            {
                if (Entity.COUNTRY_CODE == value) return;
                Entity.COUNTRY_CODE = value;
                RaisePropertyChanged();
            }
        }

        public string KPP
        {
            get => Entity.KPP;
            set
            {
                if (Entity.KPP == value) return;
                Entity.KPP = value;
                RaisePropertyChanged();
            }
        }

        public Guid? GUID_ID
        {
            get => Entity.GUID_ID;
            set
            {
                if (Entity.GUID_ID == value) return;
                Entity.GUID_ID = value;
                RaisePropertyChanged();
            }
        }

        public Guid? GRUZO_INFO_ID
        {
            get => Entity.GRUZO_INFO_ID;
            set
            {
                if (Entity.GRUZO_INFO_ID == value) return;
                Entity.GRUZO_INFO_ID = value;
                RaisePropertyChanged();
            }
        }

        public DateTime? REGISTER_DATE
        {
            get => Entity.REGISTER_DATE;
            set
            {
                if (Entity.REGISTER_DATE == value) return;
                Entity.REGISTER_DATE = value;
                RaisePropertyChanged();
            }
        }

        public decimal? BUH_CLIENT_DC
        {
            get => Entity.BUH_CLIENT_DC;
            set
            {
                if (Entity.BUH_CLIENT_DC == value) return;
                Entity.BUH_CLIENT_DC = value;
                RaisePropertyChanged();
            }
        }

        public decimal? BUH_RECEIVER_DC
        {
            get => Entity.BUH_RECEIVER_DC;
            set
            {
                if (Entity.BUH_RECEIVER_DC == value) return;
                Entity.BUH_RECEIVER_DC = value;
                RaisePropertyChanged();
            }
        }

        public string NAKL_GRUZOOTPRAV
        {
            get => Entity.NAKL_GRUZOOTPRAV;
            set
            {
                if (Entity.NAKL_GRUZOOTPRAV == value) return;
                Entity.NAKL_GRUZOOTPRAV = value;
                RaisePropertyChanged();
            }
        }

        public string NAKL_GRUZOPOLUCH
        {
            get => Entity.NAKL_GRUZOPOLUCH;
            set
            {
                if (Entity.NAKL_GRUZOPOLUCH == value) return;
                Entity.NAKL_GRUZOPOLUCH = value;
                RaisePropertyChanged();
            }
        }

        public GROZO_REQUISITE GROZO_REQUISITE
        {
            get => Entity.GROZO_REQUISITE;
            set
            {
                if (Entity.GROZO_REQUISITE == value) return;
                Entity.GROZO_REQUISITE = value;
                RaisePropertyChanged();
            }
        }

        public SD_179 SD_179
        {
            get => Entity.SD_179;
            set
            {
                if (Entity.SD_179 == value) return;
                Entity.SD_179 = value;
                RaisePropertyChanged();
            }
        }

        public SD_189 SD_189
        {
            get => Entity.SD_189;
            set
            {
                if (Entity.SD_189 == value) return;
                Entity.SD_189 = value;
                RaisePropertyChanged();
            }
        }

        public SD_301 SD_301
        {
            get => Entity.SD_301;
            set
            {
                if (Entity.SD_301 == value) return;
                Entity.SD_301 = value;
                RaisePropertyChanged();
            }
        }

        public SD_301 SD_3011
        {
            get => Entity.SD_3011;
            set
            {
                if (Entity.SD_3011 == value) return;
                Entity.SD_3011 = value;
                RaisePropertyChanged();
            }
        }

        public SD_40 SD_40
        {
            get => Entity.SD_40;
            set
            {
                if (Entity.SD_40 == value) return;
                Entity.SD_40 = value;
                RaisePropertyChanged();
            }
        }

        public SD_43 SD_43
        {
            get => Entity.SD_43;
            set
            {
                if (Entity.SD_43 == value) return;
                Entity.SD_43 = value;
                RaisePropertyChanged();
            }
        }

        public SD_43 SD_431
        {
            get => Entity.SD_431;
            set
            {
                if (Entity.SD_431 == value) return;
                Entity.SD_431 = value;
                RaisePropertyChanged();
            }
        }

        public SD_43 SD_432
        {
            get => Entity.SD_432;
            set
            {
                if (Entity.SD_432 == value) return;
                Entity.SD_432 = value;
                RaisePropertyChanged();
            }
        }

        public SD_43 SD_433
        {
            get => Entity.SD_433;
            set
            {
                if (Entity.SD_433 == value) return;
                Entity.SD_433 = value;
                RaisePropertyChanged();
            }
        }

        public SD_43 SD_434
        {
            get => Entity.SD_434;
            set
            {
                if (Entity.SD_434 == value) return;
                Entity.SD_434 = value;
                RaisePropertyChanged();
            }
        }

        public SD_77 SD_77
        {
            get => Entity.SD_77;
            set
            {
                if (Entity.SD_77 == value) return;
                Entity.SD_77 = value;
                RaisePropertyChanged();
            }
        }

        public EntityLoadCodition LoadCondition { get; set; }

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(PersonaResponsible):
                        return PersonaResponsible == null ? ValidationError.FieldNotNull : null;
                    case nameof(Client):
                        return Client == null ? ValidationError.FieldNotNull : null;
                    case nameof(Currency):
                        return Currency == null ? ValidationError.FieldNotNull : null;
                    case nameof(CO):
                        return CO == null ? ValidationError.FieldNotNull : null;
                    case nameof(PayCondition):
                        return PayCondition == null ? ValidationError.FieldNotNull : null;
                    case nameof(VzaimoraschetType):
                        return VzaimoraschetType == null ? ValidationError.FieldNotNull : null;
                    case nameof(FormRaschet):
                        return FormRaschet == null ? ValidationError.FieldNotNull : null;
                }

                return null;
            }
        }

        public string Error { get; } = null;

        public SD_84 Entity
        {
            get => myEntity;
            set
            {
                if (myEntity == value) return;
                myEntity = value;
                RaisePropertyChanged();
            }
        }

        public List<SD_84> LoadList()
        {
            throw new NotImplementedException();
        }

        public bool IsAccessRight { get; set; }

        public void UpdateActualValues()
        {
            RaisePropertiesChanged(nameof(Rows));
            RaisePropertiesChanged(nameof(Summa));
        }

        private void LoadReferences()
        {
            if (MainReferences.Currencies.ContainsKey(Entity.SF_CRS_DC))
                Currency = MainReferences.Currencies[Entity.SF_CRS_DC];
            if (Entity.SF_CLIENT_DC != null) Client = MainReferences.GetKontragent(Entity.SF_CLIENT_DC);
            if (Entity.SF_CENTR_OTV_DC != null) CO = MainReferences.COList[Entity.SF_CENTR_OTV_DC.Value];
            if (Entity.SF_RECEIVER_KONTR_DC != null)
                Receiver = MainReferences.GetKontragent(Entity.SF_RECEIVER_KONTR_DC);
            if (Entity.SF_FORM_RASCH_DC != null)
                FormRaschet = MainReferences.FormRaschets[Entity.SF_FORM_RASCH_DC.Value];
            if (Entity.SD_179 != null)
                PayCondition = MainReferences.PayConditions[Entity.SD_179.DOC_CODE];
            if (Entity.SD_77 != null)
                VzaimoraschetType = MainReferences.VzaimoraschetTypes[Entity.SD_77.DOC_CODE];
            if (Entity.SF_CENTR_OTV_DC != null)
                CO = MainReferences.COList[Entity.SF_CENTR_OTV_DC.Value];
            if (Entity.PersonalResponsibleDC != null)
                PersonaResponsible = MainReferences.Employees[Entity.PersonalResponsibleDC.Value];
            Rows = new ObservableCollection<InvoiceClientRow>();
            if (Entity.TD_84 != null && Entity.TD_84.Count > 0)
                foreach (var t in Entity.TD_84)
                {
                    var newRow = new InvoiceClientRow(t)
                    {
                        Parent = this
                    };
                    Rows.Add(newRow);
                }

            if (isLoadPayment)
            {
                PaymentDocs = new ObservableCollection<InvoicePaymentDocument>();
                foreach (var c in context.Context.SD_33.Where(_ => _.SFACT_DC == DocCode).ToList())
                    PaymentDocs.Add(new InvoicePaymentDocument
                    {
                        DocCode = c.DOC_CODE,
                        Code = 0,
                        DocumentType = DocumentType.CashIn,
                        // ReSharper disable once PossibleInvalidOperationException
                        DocumentName =
                            $"{c.NUM_ORD} от {c.DATE_ORD.Value.ToShortDateString()} на {c.SUMM_ORD} " +
                            // ReSharper disable once PossibleInvalidOperationException
                            $"{MainReferences.Currencies[(decimal) c.CRS_DC]} ({c.CREATOR})",
                        // ReSharper disable once PossibleInvalidOperationException
                        Summa = (decimal) c.SUMM_ORD,
                        Currency = MainReferences.Currencies[(decimal) c.CRS_DC],
                        Note = c.NOTES_ORD
                    });
                foreach (var c in context.Context.TD_101.Include(_ => _.SD_101)
                    .Where(_ => _.VVT_SFACT_CLIENT_DC == DocCode)
                    .ToList())
                    PaymentDocs.Add(new InvoicePaymentDocument
                    {
                        DocCode = c.DOC_CODE,
                        Code = c.CODE,
                        DocumentType = DocumentType.Bank,
                        DocumentName =
                            // ReSharper disable once PossibleInvalidOperationException
                            $"{c.SD_101.VV_START_DATE.ToShortDateString()} на {(decimal) c.VVT_VAL_PRIHOD} {MainReferences.BankAccounts[c.SD_101.VV_ACC_DC]}",
                        Summa = (decimal) c.VVT_VAL_PRIHOD,
                        Currency = MainReferences.Currencies[c.VVT_CRS_DC],
                        Note = c.VVT_DOC_NUM
                    });
                foreach (var c in context.Context.TD_110.Include(_ => _.SD_110).Where(_ => _.VZT_SFACT_DC == DocCode)
                    .ToList())
                    PaymentDocs.Add(new InvoicePaymentDocument
                    {
                        DocCode = c.DOC_CODE,
                        Code = c.CODE,
                        DocumentType = DocumentType.MutualAccounting,
                        DocumentName =
                            // ReSharper disable once PossibleInvalidOperationException
                            $"Взаимозачет №{c.SD_110.VZ_NUM} от {c.SD_110.VZ_DATE.ToShortDateString()} на {c.VZT_CRS_SUMMA}",
                        // ReSharper disable once PossibleInvalidOperationException
                        Summa = (decimal) c.VZT_CRS_SUMMA,
                        Currency = MainReferences.Currencies[c.SD_110.CurrencyFromDC],
                        Note = c.VZT_DOC_NOTES
                    });
            }

            ShipmentRows = new ObservableCollection<ShipmentRowViewModel>();
            if (Entity.TD_84 is {Count: > 0})
                foreach (var r in Entity.TD_84)
                    if (r.TD_24 is {Count: > 0})
                        foreach (var r2 in r.TD_24)
                        {
                            var newItem = new ShipmentRowViewModel(r2);
                            if (r2.SD_24 != null) newItem.Parent = new WarehouseOrderOut(r2.SD_24);

                            ShipmentRows.Add(newItem);
                        }
        }

        public void Save(SD_84 doc)
        {
            throw new NotImplementedException();
        }

        public void Save()
        {
            throw new NotImplementedException();
        }

        public void Delete()
        {
            throw new NotImplementedException();
        }

        public void Delete(Guid id)
        {
            throw new NotImplementedException();
        }

        public void Delete(decimal dc)
        {
            throw new NotImplementedException();
        }

        public void UpdateFrom(SD_84 ent)
        {
        }

        public void UpdateTo(SD_84 ent)
        {
        }

        public SD_84 DefaultValue()
        {
            return new()
            {
                DOC_CODE = -1,
                Id = Guid.NewGuid()
            };
        }

        public SD_84 Load(decimal dc, bool isShort = true)
        {
            throw new NotImplementedException();
        }

        public SD_84 Load(Guid id, bool isShort = true)
        {
            throw new NotImplementedException();
        }

        public SD_84 Load(decimal dc)
        {
            throw new NotImplementedException();
        }

        public SD_84 Load(Guid id)
        {
            throw new NotImplementedException();
        }
    }

    public class DataAnnotationsSFClientViewModel : DataAnnotationForFluentApiBase, IMetadataProvider<InvoiceClient>
    {
        void IMetadataProvider<InvoiceClient>.BuildMetadata(MetadataBuilder<InvoiceClient> builder)
        {
            SetNotAutoGenerated(builder);
            builder.Property(_ => _.Receiver).AutoGenerated().DisplayName("Поставщик");
            builder.Property(_ => _.CO).AutoGenerated().DisplayName("ЦО");
            builder.Property(_ => _.VzaimoraschetType).AutoGenerated().DisplayName("Тип продукции");
            builder.Property(_ => _.FormRaschet).AutoGenerated().DisplayName("Форма расчетов");
            builder.Property(_ => _.PayCondition).AutoGenerated().DisplayName("Условия оплаты");
            builder.Property(_ => _.DocDate).AutoGenerated().DisplayName("Дата");
            builder.Property(_ => _.InnerNumber).AutoGenerated().DisplayName("№");
            builder.Property(_ => _.OuterNumber).AutoGenerated().DisplayName("Внешний №");
            builder.Property(_ => _.Client).AutoGenerated().DisplayName("Клиент");
            builder.Property(_ => _.Currency).AutoGenerated().DisplayName("Валюта");
            builder.Property(_ => _.SummaOtgruz).AutoGenerated().DisplayName("Отгружено").ReadOnly()
                .DisplayFormatString("n2");
            builder.Property(_ => _.SF_DILER_SUMMA).AutoGenerated().DisplayName("Сумма дилера").ReadOnly()
                .DisplayFormatString("n2");
            builder.Property(_ => _.Note).AutoGenerated().DisplayName("Примечание");
            builder.Property(_ => _.Diler).AutoGenerated().DisplayName("Дилер");
            builder.Property(_ => _.IsAccepted).AutoGenerated().DisplayName("Акцептован");
            builder.Property(_ => _.Summa).AutoGenerated().DisplayName("Сумма").ReadOnly()
                .DisplayFormatString("n2");
            builder.Property(_ => _.CREATOR).AutoGenerated().DisplayName("Создатель");
            builder.Property(_ => _.State).AutoGenerated().DisplayName("Статус");
            builder.Property(_ => _.IsNDSIncludeInPrice).AutoGenerated().DisplayName("НДС вкл. в цену");
            builder.Property(_ => _.PaySumma).AutoGenerated().DisplayName("Оплачено");
            builder.Property(_ => _.PersonaResponsible).AutoGenerated().DisplayName("Ответственный");

        }
    }
}