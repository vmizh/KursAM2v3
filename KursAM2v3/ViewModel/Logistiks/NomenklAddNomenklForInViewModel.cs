using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows.Input;
using Core;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using KursAM2.View.Base;
using KursAM2.View.Logistiks;
using KursDomain;
using KursDomain.Documents.NomenklManagement;
using KursDomain.ICommon;
using KursDomain.Menu;
using KursDomain.References;

namespace KursAM2.ViewModel.Logistiks
{
    public class NomenklAddNomenklForInViewModel : RSWindowViewModelBase
    {
        private NomenklViewModel myCurrentNomenkl;
        private NomAddNomenklToMoveUC myDataUserControl;

        public NomenklAddNomenklForInViewModel(Guid nomMainId)
        {
            myDataUserControl = new NomAddNomenklToMoveUC();
            RightMenuBar = MenuGenerator.ReferenceRightBar(this);
            LoadReferences();
            GetNomenklMain(nomMainId);
        }

        public NomenklMainViewModel MainNomenkl { set; get; }
        public bool IsNomenklMainNull { set; get; }
        public ObservableCollection<Currency> CurrencyCollection { set; get; }

        public NomenklViewModel CurrentNomenkl
        {
            get => myCurrentNomenkl;
            set
            {
                if (myCurrentNomenkl != null && myCurrentNomenkl.Equals(value)) return;
                myCurrentNomenkl = value;
                if (myCurrentNomenkl != null)
                    try
                    {
                        IsCanChangeCurrency = true;
                        using (var ctx = GlobalOptions.GetEntities())
                        {
                            if (ctx.TD_24.Any(_ => _.DDT_NOMENKL_DC == myCurrentNomenkl.DocCode))
                            {
                                IsCanChangeCurrency = false;
                            }
                            else
                            {
                                if (ctx.TD_26.Any(_ => _.SFT_NEMENKL_DC == myCurrentNomenkl.DocCode))
                                {
                                    IsCanChangeCurrency = false;
                                }
                                else
                                {
                                    if (ctx.TD_84.Any(_ => _.SFT_NEMENKL_DC == myCurrentNomenkl.DocCode))
                                        IsCanChangeCurrency = false;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        WindowManager.ShowError(ex);
                    }

                RaisePropertyChanged();
            }
        }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public bool IsCanChangeCurrency { get; set; }

        public NomAddNomenklToMoveUC DataUserControl
        {
            get => myDataUserControl;
            set
            {
                if (Equals(myDataUserControl, value)) return;
                myDataUserControl = value;
                RaisePropertyChanged();
            }
        }

        public override bool IsCanSaveData => NomenklCollection.Any(_ => _.State != RowStatus.NotEdited);
        public ObservableCollection<NomenklViewModel> NomenklCollection { set; get; } = new ObservableCollection<NomenklViewModel>();

        private void LoadReferences()
        {
            if (CurrencyCollection == null)
                CurrencyCollection = new ObservableCollection<Currency>();
            using (var ctx = GlobalOptions.GetEntities())
            {
                CurrencyCollection.Clear();
                foreach (var c in ctx.SD_301.ToList())
                {
                    var newCrs = new Currency();
                    newCrs.LoadFromEntity(c);
                    CurrencyCollection.Add(newCrs);
                }
            }
        }

        private void GetNomenklMain(Guid id)
        {
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var d = ctx.NomenklMain.Include(_ => _.SD_50).FirstOrDefault(_ => _.Id == id);
                    if (d == null)
                    {
                        IsNomenklMainNull = true;
                        return;
                    }

                    MainNomenkl = new NomenklMainViewModel(d);
                    var noms = (from n in ctx.SD_83
                        join sd301 in ctx.SD_301 on n.NOM_SALE_CRS_DC equals sd301.DOC_CODE
                        join sd50 in ctx.SD_50 on n.NOM_PRODUCT_DC equals sd50.DOC_CODE
                        where n.MainId == id
                        select new NomenklViewModel
                        {
                            DocCode = n.DOC_CODE,
                            Name = n.NOM_NAME,
                            NomenklNumber = n.NOM_NOMENKL,
                            NameFull = n.NOM_FULL_NAME,
                            Currency = new Currency { DocCode = sd301.DOC_CODE, Name = sd301.CRS_SHORTNAME },
                            Note = n.NOM_NOTES
                        }).ToList();
                    NomenklCollection.Clear();
                    foreach (var nom in noms)
                    {
                        nom.Currency = CurrencyCollection.SingleOrDefault(_ => _.DocCode == ((IDocCode)nom.Currency).DocCode);
                        nom.Parent = MainNomenkl;
                        nom.State = RowStatus.NotEdited;
                        NomenklCollection.Add(nom);
                    }
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(null, ex);
            }
        }

        //private void SaveNomenkl(Nomenkl nom)
        //{
        //    using (var ctx = GlobalOptions.GetEntities())
        //    {
        //        using (var tnx = ctx.Database.BeginTransaction())
        //        {
        //            try
        //            {
        //                switch (nom.State)
        //                {
        //                    case RowStatus.NewRow:
        //                        var newDC = ctx.SD_83.Max(_ => _.DOC_CODE) + 1;
        //                        var newItem = new SD_83
        //                        {
        //                            DOC_CODE = newDC,
        //                            NOM_NAME = nom.Name,
        //                            NOM_NOMENKL = nom.NomenklNumber + (nom.NomenklNumber.EndsWith(nom.Currency.Name) == false ? " " + nom.Currency.Name : null),
        //                            NOM_NOTES = nom.Note,
        //                            NOM_SALE_CRS_DC = nom.Currency.DocCode,
        //                            NOM_FULL_NAME = nom.NameFull,
        //                            NOM_ED_IZM_DC = MainNomenkl.Unit.DocCode,
        //                            NOM_CATEG_DC = MainNomenkl.NomenklCategory.DocCode,
        //                            NOM_0MATER_1USLUGA = MainNomenkl.IsUsluga ? 1 : 0,
        //                            NOM_1PROD_0MATER = 0,
        //                            NOM_1NAKLRASH_0NO = MainNomenkl.IsNakladExpense ? 1 : 0,
        //                            Id = nom.Id,
        //                            MainId = nom.MainId
        //                        };
        //                        ctx.SD_83.Add(newItem);
        //                        break;
        //                    case RowStatus.Edited:
        //                        var d = ctx.SD_83.FirstOrDefault(_ => _.DOC_CODE == nom.DocCode);
        //                        if (d == null) return;
        //                        d.NOM_NAME = nom.Name;
        //                        d.NOM_NOMENKL = nom.NomenklNumber;
        //                        d.NOM_NOTES = nom.Note;
        //                        d.NOM_SALE_CRS_DC = nom.Currency.DocCode;
        //                        d.NOM_FULL_NAME = nom.NameFull ?? nom.Name;
        //                        break;
        //                }

        //                ctx.SaveChanges();
        //                tnx.Commit();
        //            }
        //            catch (Exception ex)
        //            {
        //                WinManager.ShowError(ex);
        //                tnx.Rollback();
        //            }
        //        }
        //    }
        //}

        public override void SaveData(object data)
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                using (var tnx = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        foreach (var nom in NomenklCollection.Where(_ => _.State != RowStatus.NotEdited))
                            switch (nom.State)
                            {
                                case RowStatus.NewRow:
                                    var newDC = ctx.SD_83.Max(_ => _.DOC_CODE) + 1;
                                    var newItem = new SD_83
                                    {
                                        DOC_CODE = newDC,
                                        NOM_NAME = nom.Name,
                                        NOM_NOMENKL =
                                            nom.NomenklNumber +
                                            (nom.NomenklNumber.EndsWith(((IName)nom.Currency).Name) == false
                                                ? " " + ((IName)nom.Currency).Name
                                                : null),
                                        NOM_NOTES = nom.Note,
                                        NOM_SALE_CRS_DC = ((IDocCode)nom.Currency).DocCode,
                                        NOM_FULL_NAME = nom.NameFull ?? nom.Name,
                                        NOM_ED_IZM_DC = ((IDocCode)MainNomenkl.Unit).DocCode,
                                        NOM_CATEG_DC = MainNomenkl.NomenklCategory.DocCode,
                                        NOM_0MATER_1USLUGA = MainNomenkl.IsUsluga ? 1 : 0,
                                        NOM_1PROD_0MATER = 0,
                                        NOM_1NAKLRASH_0NO = MainNomenkl.IsNakladExpense ? 1 : 0,
                                        NOM_PRODUCT_DC = MainNomenkl.ProductType.DOC_CODE,
                                        UpdateDate = DateTime.Now,
                                        Id = nom.Id,
                                        MainId = nom.MainId
                                    };
                                    ctx.SD_83.Add(newItem);
                                    nom.DocCode = newDC;
                                    break;
                                case RowStatus.Edited:
                                    var d = ctx.SD_83.FirstOrDefault(_ => _.DOC_CODE == nom.DocCode);
                                    if (d == null) continue;
                                    d.NOM_NAME = nom.Name;
                                    d.NOM_NOMENKL = nom.NomenklNumber;
                                    d.NOM_NOTES = nom.Note;
                                    d.NOM_SALE_CRS_DC = ((IDocCode)nom.Currency).DocCode;
                                    d.NOM_FULL_NAME = nom.NameFull ?? nom.Name;
                                    d.UpdateDate = DateTime.Now;
                                    d.MainId = nom.MainId;
                                    break;
                            }

                        ctx.SaveChanges();
                        tnx.Commit();
                        foreach (var n in NomenklCollection.Where(_ => _.State != RowStatus.NotEdited))
                        {
                            n.UpdateDate = DateTime.Now;
                            n.State = RowStatus.NotEdited;
                        }
                    }
                    catch (Exception ex)
                    {
                        tnx.Rollback();
                        WindowManager.ShowError(ex);
                    }
                }
            }
        }

        #region Command

        private void NomenklAdd(object obj)
        {
            var newItem = new NomenklViewModel
            {
                DOC_CODE = -1,
                Name = MainNomenkl.Name,
                NomenklNumber = MainNomenkl.NomenklNumber,
                MainId = MainNomenkl.Id,
                Id = Guid.NewGuid(),
                IsDeleted = false,
                Group = MainNomenkl.NomenklCategory,
                IsUsluga = MainNomenkl.IsUsluga,
                IsNaklRashod = MainNomenkl.IsNakladExpense,
                State = RowStatus.NewRow
            };
            NomenklCollection.Add(newItem);
            RaisePropertyChanged(nameof(NomenklCollection));
            RaisePropertyChanged(nameof(DataUserControl));
        }

        public ICommand NomenklAddCommand
        {
            get { return new Command(NomenklAdd, _ => true); }
        }

        public ICommand NomenklLinkExistCommand
        {
            get { return new Command(NomenklLinkExist, _ => true); }
        }

        private void NomenklLinkExist(object obj)
        {
            var ctx = new NomenklSelectedSimpleDialogViewModel();
            var dlg = new SelectDialogView { DataContext = ctx };
            dlg.ShowDialog();
            if (!ctx.DialogResult) return;
            ctx.CurrentNomenkl.MainId = MainNomenkl.Id;
            ctx.CurrentNomenkl.State = RowStatus.Edited;
            NomenklCollection.Add(ctx.CurrentNomenkl);
            RaisePropertyChanged(nameof(NomenklCollection));
            RaisePropertyChanged(nameof(DataUserControl));
        }

        #endregion
    }
}
