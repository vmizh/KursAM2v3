using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;
using Core;
using Core.WindowsManager;
using DevExpress.DataProcessing.InMemoryDataProcessor;
using DevExpress.Xpf.Printing;
using DevExpress.XtraReports.UI;

namespace KursAM2.ReportManagers
{
    public static class ReportManager
    {
        private static string pathToReports = Environment.CurrentDirectory + "\\Reports\\";

        // ReSharper disable once MemberCanBePrivate.Global
        public static void OpenReport<T>(List<T> data, string reportFileName)
        {
            var report = new XtraReport();
            report.LoadLayout(reportFileName);
            report.DataSource = data;
            report.CreateDocument();
            PrintHelper.ShowPrintPreview(Application.Current.MainWindow, report);
        }

        public static void WarehouseOrderInReport(decimal dc)
        {
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var data = (from s24 in ctx.SD_24
                        join t24 in ctx.TD_24 on s24.DOC_CODE equals t24.DOC_CODE
                        join s27 in ctx.SD_27 on s24.DD_SKLAD_POL_DC equals s27.DOC_CODE
                        join s83 in ctx.SD_83 on t24.DDT_NOMENKL_DC equals s83.DOC_CODE
                        join s175 in ctx.SD_175 on t24.DDT_ED_IZM_DC equals s175.DOC_CODE
                        join s43 in ctx.SD_43 on s24.DD_KONTR_OTPR_DC equals s43.DOC_CODE
                        join s2 in ctx.SD_2 on s27.TABELNUMBER equals s2.TABELNUMBER
                        where s24.DOC_CODE == dc
                        select new
                        {
                            DD_IN_NUM = s24.DD_IN_NUM,
                            DD_EXT_NUM = s24.DD_EXT_NUM,
                            DD_DATE = s24.DD_DATE,
                            SkladName = s27.SKL_NAME,
                            KontrName = s43.NAME,
                            Corporate = GlobalOptions.SystemProfile.OwnerKontragent.Name,
                            NomenklNumber = s83.NOM_NOMENKL,
                            NomName = s83.NOM_NAME,
                            EdIzmName = s175.ED_IZM_NAME,
                            EdIzmCode = s175.ED_IZM_OKEI_CODE,
                            Quantity = t24.DDT_KOL_PRIHOD,
                            KladovschikName = s2.NAME,
                            OtKogoPolucheno = s24.DD_OT_KOGO_POLUCHENO
                        }).ToList();
                    var fileName = pathToReports + "WarehouseOrderIn.repx";
                    OpenReport(data, fileName);
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }
        }

        private static int LineNum = 0;
        private static int LineNumber()
        {
            return LineNum++;
        }

        public static void WarehouseOrderOutReport(decimal dc)
        {
            LineNum = 0;
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var data = ctx.TD_24.Include(_ => _.SD_24)
                        .Include(_ => _.SD_83)
                        .Include(_ => _.SD_175)
                        .Include(_ => _.SD_24.SD_43)
                        .Include(_ => _.SD_24.SD_27)
                        .Include(_ => _.SD_24.SD_271)
                        .Include(_ => _.SD_24.SD_272)
                        .Include(_ => _.SD_24.SD_273)
                        .Where(_ => _.DOC_CODE == dc).ToList();
                    var repdata = (from d in data
                        select new
                        {
                            LineNumber = d.CODE,
                            NumberOrder = d.SD_24.DD_IN_NUM + d.SD_24.DD_EXT_NUM != null
                                ? " / " + d.SD_24.DD_EXT_NUM
                                : string.Empty,
                            DocumentDate = d.SD_24.DD_DATE,
                            Poluchatel = d.SD_24.SD_271.SKL_NAME,
                            Postavshik = GlobalOptions.SystemProfile.OwnerKontragent.Name,
                            Tovar = d.SD_83.NOM_NAME,
                            EdIzmName = d.SD_175.ED_IZM_NAME,
                            NumberItems = d.DDT_KOL_RASHOD,
                            KladovschikName = MainReferences.Warehouses[d.SD_24.DD_SKLAD_OTPR_DC.Value].Employee.Name,
                            SkladName = d.SD_24.SD_273.SKL_NAME
                        }).OrderBy(_ => _.LineNumber).ToList();
                    var fileName = pathToReports + "WarehouseOrderOut.repx";
                    OpenReport(repdata, fileName);
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }

        }
    }
}