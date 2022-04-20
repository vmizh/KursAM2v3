using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Core;
using Core.EntityViewModel.Invoices;
using Core.Menu;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using KursAM2.Managers.Invoices;
using KursAM2.View.Finance.Invoices;
using KursAM2.ViewModel.Finance.Invoices;

namespace KursAM2.ViewModel.Shop
{
    public class ShopParserExtFilesWindowViewModel : RSWindowViewModelBase, IDataErrorInfo
    {
        public ShopParserExtFilesWindowViewModel()
        {
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.ExitOnlyRightBar(this);
        }

        #region Properties

        public override string LayoutName => "ShopParserExtFilesWindowViewModel";

        public ObservableCollection<ShopExtFileOrderItem> GeneratedItems { set; get; } =
            new ObservableCollection<ShopExtFileOrderItem>();
        public ObservableCollection<NomenklInfo> NomenklMainList { set; get; } = new ObservableCollection<NomenklInfo>();

       
        private string myTextForParse;
        public string TextForParse
        {
            get => myTextForParse;
            set
            {
                if (myTextForParse == value) return;
                myTextForParse = value;
                RaisePropertyChanged();
            }
        } 
        
        #endregion

        #region Command

        public ICommand ParserChangedCommand
        {
            get { return new Command(ParserChanged, _ => true); }
        }

        private void ParserChanged(object obj)
        {
            GeneratedItems.Clear();
            NomenklMainList.Clear();
        }

        public ICommand GenerateSFCommand
        {
            get
            {
                return new Command(GenerateSF, _ => GeneratedItems != null && GeneratedItems.Count > 0
                                                                           && NomenklMainList.All(x => x.IsInDataBase));
            }
        }

        private void GenerateSF(object obj)
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                var invoice = new ProviderWindowViewModel(null)
                {
                    Document =
                    {
                        Kontragent = MainReferences.GetKontragent(10430001717),
                        KontrReceiver = MainReferences.GetKontragent(10430000001),
                        CO = MainReferences.GetCO(10400000012),
                        PayCondition = MainReferences.GetPayCondition(11790000001),
                        VzaimoraschetType = MainReferences.GetVzaimoraschetType(10770000005),
                        FormRaschet = MainReferences.GetFormPay(11890000001),
                        IsNDSInPrice = true,
                        CREATOR = GlobalOptions.UserInfo.NickName,
                        PersonaResponsible = MainReferences.GetEmployee((int?)104),
                        Currency = GlobalOptions.SystemProfile.NationalCurrency,
                        State = RowStatus.NewRow,
                        DocCode = -1,
                        DocDate = DateTime.Today

                    }
                };

                //var inv = InvoicesManager.NewProvider();
                int Code = 1;
                foreach (var n in NomenklMainList)
                {
                    var nom = ctx.SD_83.FirstOrDefault(_ =>
                        _.MainId == n.Id && _.NOM_SALE_CRS_DC == GlobalOptions.SystemProfile.NationalCurrency.DocCode);
                    if (nom != null)
                    {
                        var nomenkl = MainReferences.GetNomenkl(nom.DOC_CODE);
                        var r = new InvoiceProviderRow
                        {
                            DocCode = invoice.Document.DocCode,
                            DocId = invoice.Document.Id,
                            Id = Guid.NewGuid(),
                            Code = Code,
                            SFT_NDS_PERCENT = (decimal) (nom.NOM_NDS_PERCENT ?? Convert.ToDouble(GlobalOptions
                                .SystemProfile.Profile
                                .FirstOrDefault(_ => _.SECTION == @"НОМЕНКЛАТУРА" && _.ITEM == @"НДС")
                                ?.ITEM_VALUE)),
                            Quantity = n.Count,
                            IsIncludeInPrice = true,
                            Price = n.Price,
                            SFT_SUMMA_K_OPLATE = n.Summa,
                            State = RowStatus.NewRow,
                            Nomenkl = nomenkl,
                            Parent = invoice.Document
                        };
                        r.Entity.SFT_NEMENKL_DC = nom.DOC_CODE;
                        invoice.Document.Entity.TD_26.Add(r.Entity);
                        invoice.Document.Rows.Add(r);
                        Code++;
                    }
                }

                var frm = new InvoiceProviderView
                {
                    Owner = Application.Current.MainWindow,
                    DataContext = invoice
                };
                invoice.Form = frm;
                //invoice.Document.Summa = (decimal) invoice.Document.Rows.Sum(_ => _.SFT_SUMMA_K_OPLATE);
                frm.Show();
                frm.DataContext = invoice;

            }
        }

        public ICommand GenerateNomenklCommand
        {
            get { return new Command(GenerateNomenkl, _ => !string.IsNullOrWhiteSpace(TextForParse)); }
        }

        private void GenerateNomenkl(object obj)
        {
            List<ShopExtFileOrderItem> nomExistsInDB = new List<ShopExtFileOrderItem>();
            GeneratedItems = new ObservableCollection<ShopExtFileOrderItem>(ShopXMLParsingOrder(TextForParse));
            using (var ctx = GlobalOptions.GetEntities())
            {
                nomExistsInDB.AddRange(from item in GeneratedItems
                    let old = ctx.NomenklMain.FirstOrDefault(_ => _.NomenklNumber == item.OfferId)
                    where old != null
                    select item);


                NomenklMainList.Clear();
                foreach (var item in GeneratedItems)
                {
                    Guid id = Guid.NewGuid();
                    var isInBase = nomExistsInDB.Exists(_ => _.OfferId == item.OfferId);
                    if (isInBase)
                    {
                        id = ctx.NomenklMain.Single(_ => _.NomenklNumber == item.OfferId).Id;
                    }

                    NomenklMainList.Add(new NomenklInfo
                    {
                        Id = id,
                        FullName = item.Name.Replace("\"", "'"),
                        Name = item.Name.Replace("\"", "'")
                            .Substring(0, item.Name.Length > 500 ? 500 : item.Name.Length - 1),
                        NomenklNumber = item.OfferId,
                        Unit = MainReferences.GetUnit(1750000001),
                        Category = MainReferences.GetNomenklGroup(10820000053),
                        NomType = MainReferences.GetNomenklProductType(11190000034),
                        IsInDataBase = isInBase,
                        Count = item.Count,
                        Price = item.Price
                    });
                }
            }
        }

        public ICommand SaveGenerateNomenklCommand
        {
            get
            {
                return new Command(SaveGenerateNomenkl, _ => NomenklMainList != null && NomenklMainList.Count > 0
                    && NomenklMainList.Any(x => x.IsInDataBase == false));
            }
        }

        private void SaveGenerateNomenkl(object obj)
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                using (var tnx = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        var newDC = ctx.SD_83.Any() ? ctx.SD_83.Max(_ => _.DOC_CODE) : 10830000001;
                        foreach (var m in NomenklMainList.Where(_ => _.IsInDataBase == false))
                        {
                            newDC++;
                            var newItem = new NomenklMain
                            {
                                Id = m.Id,
                                Name = m.Name,
                                FullName = m.FullName,
                                CategoryDC = m.Category.DocCode,
                                TypeDC = m.NomType.DocCode,
                                CountryId = null,
                                NomenklNumber = m.NomenklNumber,
                                Note = m.Note,
                                IsComplex = m.IsComplex,
                                IsNakladExpense = m.IsNakladExpense,
                                IsUsluga = m.IsUsluga,
                                UnitDC = m.Unit.DocCode,
                                ProductDC = 10500000008 ,
                                IsRentabelnost = m.IsRentabelnost,
                                IsCurrencyTransfer = m.IsCurrencyTransfer
                            };
                            ctx.NomenklMain.Add(newItem);
                            var crsDC = GlobalOptions.SystemProfile.NationalCurrency.DocCode;
                            var newNomItem = new SD_83
                            {
                                DOC_CODE = newDC,
                                NOM_NAME = newItem.Name,
                                NOM_NOMENKL = newItem.NomenklNumber + " " +
                                              GlobalOptions.SystemProfile.NationalCurrency.Name,
                                NOM_NOTES = newItem.Note,
                                NOM_SALE_CRS_DC = crsDC,
                                NOM_FULL_NAME = newItem.Name,
                                NOM_ED_IZM_DC = newItem.UnitDC,
                                NOM_CATEG_DC = newItem.CategoryDC,
                                NOM_0MATER_1USLUGA = newItem.IsUsluga ? 1 : 0,
                                NOM_1PROD_0MATER = 0,
                                NOM_1NAKLRASH_0NO = newItem.IsNakladExpense ? 1 : 0,
                                NOM_PRODUCT_DC = newItem.ProductDC,
                                NOM_NDS_PERCENT = Convert.ToDouble(GlobalOptions.SystemProfile.Profile
                                    .FirstOrDefault(_ => _.SECTION == @"НОМЕНКЛАТУРА" && _.ITEM == @"НДС")
                                    ?.ITEM_VALUE),
                                Id = Guid.NewGuid(),
                                MainId = newItem.Id,
                                IsUslugaInRent = newItem.IsRentabelnost,
                                IsCurrencyTransfer = m.IsCurrencyTransfer
                            };
                            ctx.SD_83.Add(newNomItem);
                        }
                        foreach(var m in NomenklMainList.Where(_ => _.IsInDataBase))
                        {
                            var oldMain = ctx.NomenklMain.FirstOrDefault(_ => _.Id == m.Id);
                            var oldNom = ctx.SD_83.FirstOrDefault(_ =>
                                _.MainId == m.Id && _.NOM_SALE_CRS_DC ==
                                GlobalOptions.SystemProfile.NationalCurrency.DocCode);
                            if (oldMain != null)
                            {
                                oldMain.Name = m.Name;
                                oldMain.FullName = m.FullName;
                                oldMain.Note = m.Note;
                            }
                            // ReSharper disable once InvertIf
                            if (oldNom != null)
                            {
                                oldNom.NOM_NAME = m.Name;
                                oldNom.NOM_FULL_NAME = m.FullName;
                                oldNom.NOM_NOTES = m.Note;
                            }
                        }
                        ctx.SaveChanges();
                        tnx.Commit();
                        foreach (var item in NomenklMainList)
                        {
                            item.IsInDataBase = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        if(tnx.UnderlyingTransaction.Connection != null)
                            tnx.Rollback();
                        WindowManager.ShowError(ex);
                    }
                }

            }
        }

        #endregion

        #region methods

        public static List<ShopExtFileOrderItem> ShopXMLParsingOrder(string text)
        {
            //string[] separatingStrings = {"<item", "/>"};
            string[] separatingStrings = {"\r\n"};
            string[] separatingStrings2 = {"\""};
            var OrderItems = new List<ShopExtFileOrderItem>();

            var str = text.Split(separatingStrings, StringSplitOptions.RemoveEmptyEntries);
            var str2 = new List<string>();
            for (var i = 3; i < str.Length - 3; i++) str2.Add(str[i]);
            foreach (var s in str2)
            {
                var sp = s.Split(separatingStrings2, StringSplitOptions.None);
                var newItem = new ShopExtFileOrderItem
                {
                    OfferId = sp[1],
                    Price = Convert.ToDecimal(sp[3],new NumberFormatInfo()
                    {
                        CurrencyDecimalSeparator = ".,"
                    }),
                    Count = Convert.ToDecimal(sp[5],new NumberFormatInfo()
                    {
                        CurrencyDecimalSeparator = ".,"
                    })
                };
                var ss = "";
                for (var i = 7; i < sp.Length; i++) ss = ss + sp[i];
                newItem.Name = ss.Remove(ss.Length-2);
                var old = OrderItems.FirstOrDefault(_ => _.OfferId == newItem.OfferId);
                if (old == null)
                {
                    OrderItems.Add(newItem);
                }
                else
                {
                    old.Count = old.Count + newItem.Count;
                }
            }

            return OrderItems;
        }

        #endregion

        public string this[string columnName] => null;

        public string Error { get; } = null;
    }
}