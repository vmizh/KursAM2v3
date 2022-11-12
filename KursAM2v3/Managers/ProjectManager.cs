using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using KursAM2.ViewModel.Management.Projects;
using KursDomain;
using KursDomain.Documents;
using KursDomain.Documents.CommonReferences;
using KursDomain.Documents.Management;
using KursDomain.References;

namespace KursAM2.Managers
{
    public class ProjectManager
    {
        public List<ProjectResultInfo> LoadProjects()
        {
            var ret = new List<ProjectResultInfo>();
            foreach (var p in GlobalOptions.ReferencesCache.GetProjectsAll().Cast<Project>())
                ret.Add(new ProjectResultInfo(p));
            return ret;
        }

        /// <summary>
        ///     возвращает список всех id проектов, принадлежащих текущему
        /// </summary>
        private List<Guid> getProjectTreeIds(Guid id)
        {
            var ret = new List<Guid> { id };
            var d = GlobalOptions.ReferencesCache.GetProjectsAll().Cast<Project>().Where(_ => _.ParentId == id)
                .Select(v => v.Id).ToList();
            if (d.Count == 0) return ret;
            foreach (var pid in d) ret.AddRange(getProjectTreeIds(pid));
            return ret;
        }

        public ObservableCollection<ProjectsInfoViewModel> LoadProjectsInfo()
        {
            var ret = new ObservableCollection<ProjectsInfoViewModel>();
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var documents = ctx.ProjectsInfo.AsNoTracking().ToList();
                    foreach (var p in documents) ret.Add(new ProjectsInfoViewModel(p));
                }
            }
            catch (Exception e)
            {
                WindowManager.ShowError(e);
            }

            return ret;
        }

        public void SaveDocument(ProjectDocumentViewModel doc, Guid projectId)
        {
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    switch (doc.DocType)
                    {
                        case DocumentType.Bank:
                            ctx.ProjectsDocs.Add(new ProjectsDocs
                            {
                                Id = Guid.NewGuid(),
                                DocRowId = doc.DocRowId,
                                DocDC = doc.DocCode,
                                DocInfo = "_",
                                DocTypeName = doc.DocName,
                                Note = doc.Note,
                                ProjectId = projectId,
                                FactCrsDC = doc.Currency.DocCode,
                                FactCRSSumma = doc.ConfirmedSum
                            });
                            break;
                        default:
                            ctx.ProjectsDocs.Add(new ProjectsDocs
                            {
                                Id = Guid.NewGuid(),
                                DocRowId = doc.DocRowId,
                                DocDC = doc.DocCode,
                                DocInfo = "_",
                                DocTypeName = doc.DocName,
                                Note = doc.Note,
                                ProjectId = projectId,
                                FactCrsDC = doc.Currency.DocCode,
                                FactCRSSumma = doc.Sum
                            });
                            break;
                    }

                    ctx.SaveChanges();
                }
            }
            catch (Exception e)
            {
                WindowManager.ShowError(e);
            }
        }

        public List<ProjectDocumentViewModel> GetDocument(Guid prID, DateTime dateStart, DateTime dateEnd)
        {
            var result = new List<ProjectDocumentViewModel>();
            foreach (var prId in getProjectTreeIds(prID))
            {
                result.AddRange(LoadAktVzaimozachet(prId, dateStart, dateEnd));
                result.AddRange(LoadAktVzaimozachetConvert(prId, dateStart, dateEnd));
                result.AddRange(LoadBank(prId, dateStart, dateEnd));
                result.AddRange(LoadClientUslugi(prId, dateStart, dateEnd));
                result.AddRange(LoadClientStore(prId, dateStart, dateEnd));
                result.AddRange(LoadCash(prId, dateStart, dateEnd));
                result.AddRange(LoadProviderUslugi(prId, dateStart, dateEnd));
                result.AddRange(LoadProviderStore(prId, dateStart, dateEnd));
            }

            return result;
        }

        public void DeleteDocument(Guid ProjectId, decimal dc, int? rowDc, string docName)
        {
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    switch (docName)
                    {
                        default:
                        {
                            ProjectsDocs del;
                            if (rowDc != null)
                                del = ctx.ProjectsDocs.FirstOrDefault(_ =>
                                    _.ProjectId == ProjectId && _.DocDC == dc && _.DocRowId == rowDc);
                            else
                                del = ctx.ProjectsDocs.FirstOrDefault(_ =>
                                    _.ProjectId == ProjectId && _.DocDC == dc);
                            if (del != null)
                                ctx.ProjectsDocs.Remove(del);
                            break;
                        }
                    }

                    ctx.SaveChanges();
                }
            }
            catch (Exception e)
            {
                WindowManager.ShowError(e);
            }
        }

        public void DeleteDocument(ProjectDocumentViewModel doc)
        {
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    switch (doc.DocName)
                    {
                        case "Приход товара":
                            ProjectProviderPrihod delPrihod;
                            delPrihod = ctx.ProjectProviderPrihod.FirstOrDefault(_ => _.RowId == doc.Id);
                            if (delPrihod != null) ctx.ProjectProviderPrihod.Remove(delPrihod);
                            break;
                        case "Услуги поставщиков":
                            ProjectProviderUslugi delUslugi;
                            delUslugi = ctx.ProjectProviderUslugi.FirstOrDefault(_ => _.RowId == doc.Id);
                            if (delUslugi != null) ctx.ProjectProviderUslugi.Remove(delUslugi);
                            break;
                        default:
                        {
                            ProjectsDocs del;
                            if (doc.DocRowId != null)
                                del = ctx.ProjectsDocs.FirstOrDefault(_ =>
                                    _.ProjectId == doc.ProjectId && _.DocDC == doc.DocCode &&
                                    _.DocRowId == doc.DocRowId);
                            else
                                del = ctx.ProjectsDocs.FirstOrDefault(_ =>
                                    _.ProjectId == doc.ProjectId && _.DocDC == doc.DocCode);
                            if (del != null)
                                ctx.ProjectsDocs.Remove(del);
                            break;
                        }
                    }

                    ctx.SaveChanges();
                }
            }
            catch (Exception e)
            {
                WindowManager.ShowError(e);
            }
        }

        public void DeleteDocument(Guid ProjectId, decimal dc, int? rowDc)
        {
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    ProjectsDocs del;
                    if (rowDc != null)
                        del = ctx.ProjectsDocs.FirstOrDefault(_ =>
                            _.ProjectId == ProjectId && _.DocDC == dc && _.DocRowId == rowDc);
                    else
                        del = ctx.ProjectsDocs.FirstOrDefault(_ =>
                            _.ProjectId == ProjectId && _.DocDC == dc);
                    if (del != null)
                        ctx.ProjectsDocs.Remove(del);
                    ctx.SaveChanges();
                }
            }
            catch (Exception e)
            {
                WindowManager.ShowError(e);
            }
        }

        #region logicsForGetDocument

        private void setCurrencyValue(decimal? summa, Currency crs, ProjectDocumentViewModel row,
            TypeProfitAndLossCalc calcType, decimal dilerSumma = 0)
        {
            setCurrencyValue(summa ?? 0, crs, row, calcType, dilerSumma);
        }

        // ReSharper disable once FunctionRecursiveOnAllPaths
        private void setCurrencyValue(decimal? summa, Currency crs, ProjectPrihodRowViewModel row,
            TypeProfitAndLossCalc calcType, decimal dilerSumma = 0)
        {
            setCurrencyValue(summa ?? 0, crs, row, calcType, dilerSumma);
        }

        private void setCurrencyValue(decimal summa, Currency crs, ProjectPrihodRowViewModel row,
            TypeProfitAndLossCalc calcType, decimal dilerSumma = 0)
        {
            switch (crs.Name)
            {
                case CurrencyCode.RUBName:
                case CurrencyCode.RURName:
                    switch (calcType)
                    {
                        case TypeProfitAndLossCalc.IsProfit:
                            row.ProfitRUB = summa;
                            break;
                        case TypeProfitAndLossCalc.IsLoss:
                            row.LossRUB = summa;
                            row.DilerRUB = dilerSumma;
                            break;
                        case TypeProfitAndLossCalc.IsAll:
                            row.ProfitRUB = summa;
                            row.LossRUB = summa;
                            row.DilerRUB = dilerSumma;
                            break;
                    }

                    break;
                case CurrencyCode.USDName:
                    switch (calcType)
                    {
                        case TypeProfitAndLossCalc.IsProfit:
                            row.ProfitUSD = summa;
                            break;
                        case TypeProfitAndLossCalc.IsLoss:
                            row.LossUSD = summa;
                            row.DilerUSD = dilerSumma;
                            break;
                        case TypeProfitAndLossCalc.IsAll:
                            row.ProfitUSD = summa;
                            row.LossUSD = summa;
                            row.DilerUSD = dilerSumma;
                            break;
                    }

                    break;
                case CurrencyCode.EURName:
                    switch (calcType)
                    {
                        case TypeProfitAndLossCalc.IsProfit:
                            row.ProfitEUR = summa;
                            break;
                        case TypeProfitAndLossCalc.IsLoss:
                            row.LossEUR = summa;
                            row.DilerEUR = dilerSumma;
                            break;
                        case TypeProfitAndLossCalc.IsAll:
                            row.ProfitEUR = summa;
                            row.LossEUR = summa;
                            row.DilerEUR = dilerSumma;
                            break;
                    }

                    break;
                case CurrencyCode.GBPName:
                    switch (calcType)
                    {
                        case TypeProfitAndLossCalc.IsProfit:
                            row.ProfitGBP = summa;
                            break;
                        case TypeProfitAndLossCalc.IsLoss:
                            row.LossGBP = summa;
                            row.DilerGBP = dilerSumma;
                            break;
                        case TypeProfitAndLossCalc.IsAll:
                            row.ProfitGBP = summa;
                            row.LossGBP = summa;
                            row.DilerGBP = dilerSumma;
                            break;
                    }

                    break;
                case CurrencyCode.CHFName:
                    switch (calcType)
                    {
                        case TypeProfitAndLossCalc.IsProfit:
                            row.ProfitCHF = summa;
                            break;
                        case TypeProfitAndLossCalc.IsLoss:
                            row.LossCHF = summa;
                            row.DilerCHF = dilerSumma;
                            break;
                        case TypeProfitAndLossCalc.IsAll:
                            row.ProfitCHF = summa;
                            row.LossCHF = summa;
                            row.DilerCHF = dilerSumma;
                            break;
                    }

                    break;
                case CurrencyCode.SEKName:
                    switch (calcType)
                    {
                        case TypeProfitAndLossCalc.IsProfit:
                            row.ProfitSEK = summa;
                            break;
                        case TypeProfitAndLossCalc.IsLoss:
                            row.LossSEK = summa;
                            row.DilerSEK = dilerSumma;
                            break;
                        case TypeProfitAndLossCalc.IsAll:
                            row.ProfitSEK = summa;
                            row.LossSEK = summa;
                            row.DilerSEK = dilerSumma;
                            break;
                    }

                    break;
            }
        }

        private void setCurrencyValue(decimal summa, Currency crs, ProjectDocumentViewModel row,
            TypeProfitAndLossCalc calcType, decimal dilerSumma = 0)
        {
            switch (crs.Name)
            {
                case CurrencyCode.RUBName:
                case CurrencyCode.RURName:
                    switch (calcType)
                    {
                        case TypeProfitAndLossCalc.IsProfit:
                            row.ProfitRUB = summa;
                            row.DilerRUB = dilerSumma;
                            break;
                        case TypeProfitAndLossCalc.IsLoss:
                            row.LossRUB = summa;

                            break;
                        case TypeProfitAndLossCalc.IsAll:
                            row.ProfitRUB = summa;
                            row.LossRUB = summa;
                            row.DilerRUB = dilerSumma;
                            break;
                    }

                    break;
                case CurrencyCode.USDName:
                    switch (calcType)
                    {
                        case TypeProfitAndLossCalc.IsProfit:
                            row.ProfitUSD = summa;
                            row.DilerUSD = dilerSumma;
                            break;
                        case TypeProfitAndLossCalc.IsLoss:
                            row.LossUSD = summa;
                            break;
                        case TypeProfitAndLossCalc.IsAll:
                            row.ProfitUSD = summa;
                            row.LossUSD = summa;
                            row.DilerUSD = dilerSumma;
                            break;
                    }

                    break;
                case CurrencyCode.EURName:
                    switch (calcType)
                    {
                        case TypeProfitAndLossCalc.IsProfit:
                            row.ProfitEUR = summa;
                            row.DilerEUR = dilerSumma;
                            break;
                        case TypeProfitAndLossCalc.IsLoss:
                            row.LossEUR = summa;
                            break;
                        case TypeProfitAndLossCalc.IsAll:
                            row.ProfitEUR = summa;
                            row.LossEUR = summa;
                            row.DilerEUR = dilerSumma;
                            break;
                    }

                    break;
                case CurrencyCode.GBPName:
                    switch (calcType)
                    {
                        case TypeProfitAndLossCalc.IsProfit:
                            row.ProfitGBP = summa;
                            row.DilerGBP = dilerSumma;
                            break;
                        case TypeProfitAndLossCalc.IsLoss:
                            row.LossGBP = summa;
                            break;
                        case TypeProfitAndLossCalc.IsAll:
                            row.ProfitGBP = summa;
                            row.LossGBP = summa;
                            row.DilerGBP = dilerSumma;
                            break;
                    }

                    break;
                case CurrencyCode.CHFName:
                    switch (calcType)
                    {
                        case TypeProfitAndLossCalc.IsProfit:
                            row.ProfitCHF = summa;
                            row.DilerCHF = dilerSumma;
                            break;
                        case TypeProfitAndLossCalc.IsLoss:
                            row.LossCHF = summa;

                            break;
                        case TypeProfitAndLossCalc.IsAll:
                            row.ProfitCHF = summa;
                            row.LossCHF = summa;
                            row.DilerCHF = dilerSumma;
                            break;
                    }

                    break;
                case CurrencyCode.SEKName:
                    switch (calcType)
                    {
                        case TypeProfitAndLossCalc.IsProfit:
                            row.ProfitSEK = summa;
                            row.DilerSEK = dilerSumma;
                            break;
                        case TypeProfitAndLossCalc.IsLoss:
                            row.LossSEK = summa;
                            break;
                        case TypeProfitAndLossCalc.IsAll:
                            row.ProfitSEK = summa;
                            row.LossSEK = summa;
                            row.DilerSEK = dilerSumma;
                            break;
                    }

                    break;
            }
        }

        private List<ProjectDocumentViewModel> LoadAktVzaimozachet(Guid prID, DateTime dateStart, DateTime dateEnd)
        {
            var result =
                new List<ProjectDocumentViewModel>();
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var data = (from d in ctx.TD_110
                            .Include(_ => _.SD_110)
                            .Include(_ => _.SD_110.SD_111)
                            .AsNoTracking()
                            .Where(_ => _.VZT_DOC_DATE >= dateStart && _.VZT_DOC_DATE <= dateEnd &&
                                        _.SD_110.SD_111.IsCurrencyConvert == false)
                        from p in ctx.ProjectsDocs
                        where p.DocDC == d.DOC_CODE && p.DocRowId == d.CODE && p.ProjectId == prID
                        select d).ToList();
                    foreach (var d in data)
                    {
                        var newdoc = new ProjectDocumentViewModel
                        {
                            ProjectId = prID,
                            Currency = GlobalOptions.ReferencesCache.GetCurrency(d.VZT_CRS_DC) as Currency,
                            Name = "Акт взаимозачета",
                            DocCode = d.DOC_CODE,
                            Kontragent = GlobalOptions.ReferencesCache.GetKontragent(d.VZT_KONTR_DC) as Kontragent,
                            DocDate = d.SD_110.VZ_DATE,
                            DocName = "Акт взаимозачета",
                            DocNum = d.VZT_DOC_NUM,
                            DocType = DocumentType.MutualAccounting,
                            Employee = null,
                            DocRowId = d.CODE,
                            Note = null,
                            Sum = d.VZT_CRS_SUMMA
                        };
                        if (d.VZT_1MYDOLZH_0NAMDOLZH == 1)
                        {
                            setCurrencyValue(newdoc.Sum,
                                GlobalOptions.ReferencesCache.GetCurrency(d.VZT_CRS_DC) as Currency, newdoc,
                                TypeProfitAndLossCalc.IsLoss);
                            result.Add(newdoc);
                        }
                        else
                        {
                            setCurrencyValue(newdoc.Sum,
                                GlobalOptions.ReferencesCache.GetCurrency(d.VZT_CRS_DC) as Currency, newdoc,
                                TypeProfitAndLossCalc.IsProfit);
                            result.Add(newdoc);
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex, "Ошибка в методе LoadAktVzaimozachet");
            }

            return null;
        }

        private List<ProjectDocumentViewModel> LoadAktVzaimozachetConvert(Guid prID, DateTime dateStart,
            DateTime dateEnd)
        {
            var result =
                new List<ProjectDocumentViewModel>();
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var data = (from d in ctx.TD_110
                            .Include(_ => _.SD_110)
                            .Include(_ => _.SD_110.SD_111)
                            .AsNoTracking()
                            .Where(_ => _.VZT_DOC_DATE >= dateStart && _.VZT_DOC_DATE <= dateEnd &&
                                        _.SD_110.SD_111.IsCurrencyConvert)
                        from p in ctx.ProjectsDocs
                        where p.DocDC == d.DOC_CODE && p.DocRowId == d.CODE && p.ProjectId == prID
                        select d).ToList();
                    foreach (var d in data)
                    {
                        var newdoc = new ProjectDocumentViewModel
                        {
                            ProjectId = prID,
                            Currency = GlobalOptions.ReferencesCache.GetCurrency(d.VZT_CRS_DC) as Currency,
                            Name = "Акт конвертации",
                            DocCode = d.DOC_CODE,
                            DocRowId = d.CODE,
                            Kontragent = GlobalOptions.ReferencesCache.GetKontragent(d.VZT_KONTR_DC) as Kontragent,
                            DocDate = d.SD_110.VZ_DATE,
                            DocName = "Акт конвертации",
                            DocNum = d.VZT_DOC_NUM,
                            DocType = DocumentType.MutualAccounting,
                            Employee = null,
                            Note = null,
                            Sum = d.VZT_CRS_SUMMA
                        };
                        if (d.VZT_1MYDOLZH_0NAMDOLZH == 1)
                        {
                            setCurrencyValue(newdoc.Sum,
                                GlobalOptions.ReferencesCache.GetCurrency(d.VZT_CRS_DC) as Currency, newdoc,
                                TypeProfitAndLossCalc.IsLoss);
                            result.Add(newdoc);
                        }
                        else
                        {
                            setCurrencyValue(newdoc.Sum,
                                GlobalOptions.ReferencesCache.GetCurrency(d.VZT_CRS_DC) as Currency, newdoc,
                                TypeProfitAndLossCalc.IsProfit);
                            result.Add(newdoc);
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex, "Ошибка в методе LoadAktVzaimozachet");
            }

            return null;
        }

        private List<ProjectDocumentViewModel> LoadBank(Guid prID, DateTime dateStart, DateTime dateEnd)
        {
            var result =
                new List<ProjectDocumentViewModel>();
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var data = (from d in ctx.TD_101
                            .Include(_ => _.SD_101)
                            .AsNoTracking()
                            .Where(_ => _.SD_101.VV_START_DATE >= dateStart && _.SD_101.VV_START_DATE <= dateEnd)
                        from p in ctx.ProjectsDocs
                        where p.DocDC == d.DOC_CODE && p.DocRowId == d.CODE && p.ProjectId == prID
                        select d).ToList();
                    foreach (var d in data)
                    {
                        var newdoc = new ProjectDocumentViewModel
                        {
                            ProjectId = prID,
                            Currency = GlobalOptions.ReferencesCache.GetCurrency(d.VVT_CRS_DC) as Currency,
                            Name = "Банковская выписка",
                            DocCode = d.DOC_CODE,
                            Kontragent = GlobalOptions.ReferencesCache.GetKontragent(d.VVT_KONTRAGENT) as Kontragent,
                            DocDate = d.SD_101.VV_STOP_DATE,
                            DocName = "Банковская выписка",
                            DocRowId = d.CODE,
                            DocNum = d.VVT_DOC_NUM,
                            DocType = DocumentType.Bank,
                            Employee = null,
                            Note = null
                        };
                        if (d.VVT_VAL_RASHOD == 0)
                            setCurrencyValue(d.VVT_VAL_PRIHOD,
                                GlobalOptions.ReferencesCache.GetCurrency(d.VVT_CRS_DC) as Currency, newdoc,
                                TypeProfitAndLossCalc.IsProfit);
                        else
                            setCurrencyValue(d.VVT_VAL_RASHOD,
                                GlobalOptions.ReferencesCache.GetCurrency(d.VVT_CRS_DC) as Currency, newdoc,
                                TypeProfitAndLossCalc.IsLoss);
                        result.Add(newdoc);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex, "Ошибка в методе LoadProviderUslugi");
            }

            return null;
        }

        private List<ProjectDocumentViewModel> LoadClientUslugi(Guid prID, DateTime dateStart, DateTime dateEnd)
        {
            var result =
                new List<ProjectDocumentViewModel>();
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var data = (from d in ctx.TD_84
                            .Include(_ => _.SD_84)
                            .Include(_ => _.SD_83)
                            .AsNoTracking()
                            .Where(_ => _.SD_83.NOM_0MATER_1USLUGA == 1 && _.SD_84.SF_ACCEPTED == 1
                                                                        && _.SD_84.SF_DATE >= dateStart &&
                                                                        _.SD_84.SF_DATE <= dateEnd)
                        from p in ctx.ProjectsDocs
                        where p.DocDC == d.DOC_CODE && p.ProjectId == prID
                        select d).ToList();
                    foreach (var d in data)
                    {
                        var newdoc = new ProjectDocumentViewModel
                        {
                            ProjectId = prID,
                            Currency = GlobalOptions.ReferencesCache.GetCurrency(d.SD_84.SF_CRS_DC) as Currency,
                            Name = "Услуги клиентам",
                            DocCode = d.DOC_CODE,
                            Kontragent =
                                GlobalOptions.ReferencesCache.GetKontragent(d.SD_84.SF_CLIENT_DC) as Kontragent,
                            DocDate = d.SD_84.SF_DATE,
                            DocName = "Услуги клиентам",
                            DocNum = Convert.ToString(d.SD_84.SF_IN_NUM) + "/" + d.SD_84.SF_OUT_NUM,
                            DocType = DocumentType.InvoiceClient,
                            Employee = null,
                            Note = d.SD_84.SF_NOTE,
                            Sum = d.SFT_ED_CENA * (decimal)d.SFT_KOL
                        };
                        if (result.Any(_ => _.DocCode == d.DOC_CODE && _.DocName == "Услуги клиентам"))
                        {
                            var doc = result.FirstOrDefault(_ => _.DocCode == d.DOC_CODE);
                            // ReSharper disable once PossibleNullReferenceException
                            doc.Sum += newdoc.Sum;
                            setCurrencyValue(doc.Sum,
                                GlobalOptions.ReferencesCache.GetCurrency(d.SD_84.SF_CRS_DC) as Currency, doc,
                                TypeProfitAndLossCalc.IsProfit,
                                d.SFT_NACENKA_DILERA ?? 0);
                        }
                        else
                        {
                            setCurrencyValue(newdoc.Sum,
                                GlobalOptions.ReferencesCache.GetCurrency(d.SD_84.SF_CRS_DC) as Currency, newdoc,
                                TypeProfitAndLossCalc.IsProfit,
                                d.SFT_NACENKA_DILERA ?? 0);
                            result.Add(newdoc);
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex, "Ошибка в методе LoadClientUslugi");
            }

            return null;
        }

        private List<ProjectDocumentViewModel> LoadProviderUslugi(Guid prID, DateTime dateStart, DateTime dateEnd)
        {
            var result =
                new List<ProjectDocumentViewModel>();
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var data = (from d in ctx.TD_26
                            .Include(_ => _.SD_26)
                            .Include(_ => _.SD_83)
                            .AsNoTracking()
                            .Where(_ => _.SD_83.NOM_0MATER_1USLUGA == 1 && _.SD_26.SF_ACCEPTED == 1
                                                                        && _.SD_26.SF_POSTAV_DATE >= dateStart &&
                                                                        _.SD_26.SF_POSTAV_DATE <= dateEnd)
                        from p in ctx.ProjectProviderUslugi
                        where p.RowId == d.Id && p.ProjectId == prID
                        select d).ToList();
                    foreach (var d in data)
                    {
                        var newdoc = new ProjectDocumentViewModel
                        {
                            Id = d.Id,
                            ProjectId = prID,
                            Currency = GlobalOptions.ReferencesCache.GetCurrency(d.SD_26.SF_CRS_DC) as Currency,
                            Name = "Услуги поставщиков",
                            DocCode = d.DOC_CODE,
                            Kontragent = GlobalOptions.ReferencesCache.GetKontragent(d.SD_26.SF_POST_DC) as Kontragent,
                            DocDate = d.SD_26.SF_POSTAV_DATE,
                            DocName = "Услуги поставщиков",
                            DocNum = Convert.ToString(d.SD_26.SF_IN_NUM) + "/" + d.SD_26.SF_POSTAV_NUM,
                            DocType = DocumentType.InvoiceProvider,
                            Sum = d.SFT_ED_CENA * d.SFT_KOL,
                            Employee = null,
                            Note = d.SD_26.SF_NOTES
                        };
                        if (result.Any(_ => _.DocCode == d.DOC_CODE && _.DocName == "Услуги поставщиков"))
                        {
                            var doc = result.FirstOrDefault(_ => _.DocCode == d.DOC_CODE);
                            // ReSharper disable once PossibleNullReferenceException
                            doc.Sum += newdoc.Sum;
                            setCurrencyValue(doc.Sum,
                                GlobalOptions.ReferencesCache.GetCurrency(d.SD_26.SF_CRS_DC) as Currency, doc,
                                TypeProfitAndLossCalc.IsLoss);
                        }
                        else
                        {
                            setCurrencyValue(newdoc.Sum,
                                GlobalOptions.ReferencesCache.GetCurrency(d.SD_26.SF_CRS_DC) as Currency, newdoc,
                                TypeProfitAndLossCalc.IsLoss);
                            result.Add(newdoc);
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex, "Ошибка в методе LoadProviderUslugi");
            }

            return null;
        }

        private List<ProjectDocumentViewModel> LoadClientStore(Guid prID, DateTime dateStart, DateTime dateEnd)
        {
            var result =
                new List<ProjectDocumentViewModel>();
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var data = (from d in ctx.TD_24
                            .Include(_ => _.SD_24)
                            .Include(_ => _.TD_84)
                            .Include(_ => _.TD_84.SD_84)
                            .AsNoTracking()
                            .Where(_ => _.DDT_SFACT_DC != null && _.SD_24.DD_DATE >= dateStart &&
                                        _.SD_24.DD_DATE <= dateEnd)
                        from p in ctx.ProjectsDocs
                        where p.DocDC == d.TD_84.SD_84.DOC_CODE && p.ProjectId == prID
                        select d).ToList();
                    foreach (var item in data)
                    {
                        var newdoc = new ProjectDocumentViewModel
                        {
                            ProjectId = prID,
                            Currency =
                                GlobalOptions.ReferencesCache.GetCurrency(item.TD_84.SD_84.SF_CRS_DC) as Currency,
                            Name = "С/Ф Клиенту",
                            DocCode = item.DDT_SFACT_DC.Value,
                            Kontragent =
                                GlobalOptions.ReferencesCache.GetKontragent(item.SD_24.DD_KONTR_OTPR_DC) as Kontragent,
                            DocInNum = Convert.ToString(item.TD_84.SD_84.SF_IN_NUM),
                            DocDate = item.SD_24.DD_DATE,
                            DocName = "С/Ф Клиенту",
                            DocNum = "С/ф №" + item.TD_84.SD_84.SF_IN_NUM + "/" + item.TD_84.SD_84.SF_OUT_NUM,
                            DocType = DocumentType.InvoiceClient,
                            Employee = null,
                            Note = item.TD_84.SD_84.SF_NOTE,
                            Sum = item.TD_84.SFT_ED_CENA * item.DDT_KOL_RASHOD
                        };
                        if (result.Any(_ => _.DocCode == item.DDT_SFACT_DC && _.DocName == "С/Ф Клиенту"))
                        {
                            var doc = result.FirstOrDefault(_ => _.DocCode == item.DDT_SFACT_DC);
                            // ReSharper disable once PossibleNullReferenceException
                            doc.Sum += newdoc.Sum;
                            setCurrencyValue(doc.Sum,
                                GlobalOptions.ReferencesCache.GetCurrency(item.TD_84.SD_84.SF_CRS_DC) as Currency, doc,
                                TypeProfitAndLossCalc.IsProfit);
                        }
                        else
                        {
                            setCurrencyValue(newdoc.Sum,
                                GlobalOptions.ReferencesCache.GetCurrency(item.TD_84.SD_84.SF_CRS_DC) as Currency,
                                newdoc,
                                TypeProfitAndLossCalc.IsProfit);
                            result.Add(newdoc);
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex, "Ошибка в методе LoadClientStore");
            }

            return null;
        }

        private List<ProjectDocumentViewModel> LoadCash(Guid prID, DateTime dateStart, DateTime dateEnd)
        {
            var result =
                new List<ProjectDocumentViewModel>();
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var cashIn = (from d in ctx.SD_33.Where(_ => _.DATE_ORD >= dateStart && _.DATE_ORD <= dateEnd)
                        from p in ctx.ProjectsDocs
                        where p.DocDC == d.DOC_CODE && p.ProjectId == prID
                        select d).ToList();
                    var cashOut = (from d in ctx.SD_34.Where(_ => _.DATE_ORD >= dateStart && _.DATE_ORD <= dateEnd)
                        from p in ctx.ProjectsDocs
                        where p.DocDC == d.DOC_CODE && p.ProjectId == prID
                        select d).ToList();
                    foreach (var d in cashIn)
                    {
                        var newdoc = new ProjectDocumentViewModel
                        {
                            ProjectId = prID,
                            Currency = GlobalOptions.ReferencesCache.GetCurrency(d.CRS_DC) as Currency,
                            Name = "Приходный кассовый ордер",
                            DocCode = d.DOC_CODE,
                            Kontragent = GlobalOptions.ReferencesCache.GetKontragent(d.KONTRAGENT_DC) as Kontragent,
                            DocDate = d.DATE_ORD ?? DateTime.MinValue,
                            DocName = "Приходный кассовый ордер",
                            DocNum = Convert.ToString(d.NUM_ORD),
                            DocType = DocumentType.CashIn,
                            Employee = GlobalOptions.ReferencesCache.GetEmployee(d.TABELNUMBER) as Employee,
                            Note = d.NOTES_ORD
                        };
                        setCurrencyValue(d.SUMM_ORD, GlobalOptions.ReferencesCache.GetCurrency(d.CRS_DC) as Currency,
                            newdoc,
                            TypeProfitAndLossCalc.IsProfit);
                        result.Add(newdoc);
                    }

                    foreach (var d in cashOut)
                    {
                        var newdoc = new ProjectDocumentViewModel
                        {
                            Currency = GlobalOptions.ReferencesCache.GetCurrency(d.CRS_DC) as Currency,
                            Name = "Расходный кассовый ордер",
                            DocCode = d.DOC_CODE,
                            Kontragent = GlobalOptions.ReferencesCache.GetKontragent(d.KONTRAGENT_DC) as Kontragent,
                            DocDate = d.DATE_ORD.Value,
                            DocName = "Расходный кассовый ордер",
                            DocNum = Convert.ToString(d.NUM_ORD),
                            DocType = DocumentType.CashOut,
                            Employee = GlobalOptions.ReferencesCache.GetEmployee(d.TABELNUMBER) as Employee,
                            Note = d.NOTES_ORD,
                            Sum = d.SUMM_ORD
                        };
                        setCurrencyValue(d.SUMM_ORD, GlobalOptions.ReferencesCache.GetCurrency(d.CRS_DC) as Currency,
                            newdoc,
                            TypeProfitAndLossCalc.IsLoss);
                        result.Add(newdoc);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex, "Ошибка в методе LoadCash");
            }

            return null;
        }

        private List<ProjectDocumentViewModel> LoadProviderStore(Guid prID, DateTime dateStart, DateTime dateEnd)
        {
            var result =
                new List<ProjectDocumentViewModel>();
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var data = (from d in ctx.TD_24
                            .Include(_ => _.SD_24)
                            .Include(_ => _.TD_26)
                            .Include(_ => _.TD_26.SD_26)
                            .AsNoTracking()
                            .Where(_ => _.DDT_SPOST_DC != null && _.SD_24.DD_DATE >= dateStart &&
                                        _.SD_24.DD_DATE <= dateEnd)
                        from p in ctx.ProjectProviderPrihod
                        where p.RowId == d.Id && p.ProjectId == prID
                        select new
                        {
                            d.Id,
                            CurrencyDC = d.TD_26.SD_26.SF_CRS_DC.Value,
                            DocDC = d.TD_26.SD_26.DOC_CODE,
                            KontragentDC = d.SD_24.DD_KONTR_OTPR_DC,
                            KontragentOtpravDC = d.SD_24.DD_KONTR_OTPR_DC,
                            Date = d.SD_24.DD_DATE,
                            InNum = d.SD_24.DD_IN_NUM,
                            OutNum = d.SD_24.DD_EXT_NUM,
                            SFInNum = d.TD_26.SD_26.SF_IN_NUM,
                            SFOutNum = d.TD_26.SD_26.SF_POSTAV_NUM,
                            Note = d.TD_26.SD_26.SF_NOTES,
                            Sum = d.DDT_FACT_CRS_CENA * p.Quantity,
                            // ReSharper disable once RedundantAnonymousTypePropertyName
                            Quantity = p.Quantity,
                            NomenklDC = d.DDT_NOMENKL_DC
                        }).ToList();
                    foreach (var d in data)
                        if (result.Any(_ => _.DocCode == d.DocDC && _.DocName == "Приход товара"))
                        {
                            var doc = result.FirstOrDefault(_ => _.DocCode == d.DocDC);
                            // ReSharper disable once PossibleNullReferenceException
                            doc.Sum += doc.Sum;
                            setCurrencyValue(doc.Sum,
                                doc.Currency, doc,
                                TypeProfitAndLossCalc.IsLoss);
                            var r = new ProjectPrihodRowViewModel
                            {
                                Id = d.Id,
                                Quantity = d.Quantity,
                                Summa = (decimal)d.Sum,
                                NomenklNumber = doc.Nomenkl.NomenklNumber,
                                NomenklName = doc.Nomenkl.Name
                            };
                            setCurrencyValue(doc.Sum,
                                doc.Currency, r,
                                TypeProfitAndLossCalc.IsLoss);
                            doc.DocList.Add(r);
                        }
                        else
                        {
                            var newdoc = new ProjectDocumentViewModel
                            {
                                Id = d.Id,
                                ProjectId = prID,
                                Currency = GlobalOptions.ReferencesCache.GetCurrency(d.CurrencyDC) as Currency,
                                Name = "Приход товара",
                                DocCode = d.DocDC,
                                Kontragent =
                                    GlobalOptions.ReferencesCache.GetKontragent(d.KontragentOtpravDC) as Kontragent,
                                DocDate = d.Date,
                                DocName = "Приход товара",
                                DocNum = Convert.ToString(d.InNum) + "/" + d.OutNum + "С/ф №" +
                                         d.SFInNum + "/" + d.SFOutNum,
                                DocType = DocumentType.InvoiceProvider,
                                Employee = null,
                                Note = d.Note,
                                Sum = d.Sum,
                                Nomenkl =
                                    GlobalOptions.ReferencesCache.GetNomenkl(d.NomenklDC) as
                                        KursDomain.References.Nomenkl
                            };
                            setCurrencyValue(newdoc.Sum,
                                newdoc.Currency, newdoc,
                                TypeProfitAndLossCalc.IsLoss);
                            result.Add(newdoc);
                            var r = new ProjectPrihodRowViewModel
                            {
                                Id = d.Id,
                                Quantity = d.Quantity,
                                Summa = (decimal)d.Sum,
                                NomenklNumber = newdoc.Nomenkl.NomenklNumber,
                                NomenklName = newdoc.Nomenkl.Name
                            };
                            setCurrencyValue(newdoc.Sum,
                                newdoc.Currency, r,
                                TypeProfitAndLossCalc.IsLoss);
                            newdoc.DocList.Add(r);
                        }
                }

                return result;
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex, "Ошибка в методе LoadProviderStore");
            }

            return null;
        }

        #endregion
    }
}
