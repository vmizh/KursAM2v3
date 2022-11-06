using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using Core;
using Core.WindowsManager;
using Data;
using KursDomain.ICommon;
using KursDomain.References;

namespace KursAM2.Managers
{
    public class KontragentManager
    {
        public static List<Kontragent> GetAllKontragentSortedByUsed(decimal KontrDC)
        {
            try
            {
                using (var dtx = GlobalOptions.KursSystem())
                {
                    var dd = dtx.KontragentCashes.Where(_ => _.UserId == GlobalOptions.UserInfo.KursId
                                                             && _.DBId == GlobalOptions.DataBaseId).ToList();
                    if (dd.Count > 0)
                        foreach (var d in dd)
                        {
                            var k = GlobalOptions.ReferencesCache.GetKontragent(d.KontragentDC);
                            if (k != null)
                                k.OrderCount = d.Count;
                        }
                }

                var ret = GlobalOptions.ReferencesCache.GetKontragentsAll().OrderByDescending(x => x.OrderCount)
                    .ThenBy(x => ((IName) x).Name).Cast<Kontragent>()
                    .ToList();
                return ret;
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
                var f = File.Create((string) Application.Current.Properties["DataPath"] + "\\dblite3.err");
                using (var fs = File.OpenWrite(f.Name))
                {
                    var content = new UTF8Encoding(true).GetBytes((string) Application.Current.Properties["DataPath"]);
                    fs.Write(content, 0, content.Length);
                }
            }

            return null;
        }

        public static List<Kontragent> GetAllKontragentSortedByUsed()
        {
            try
            {
                MainReferences.LoadKontragents();
                using (var dtx = GlobalOptions.KursSystem())
                {
                    var dd = dtx.KontragentCashes.Where(_ => _.UserId == GlobalOptions.UserInfo.KursId
                                                             && _.DBId == GlobalOptions.DataBaseId).ToList();
                    if (dd.Count > 0)
                        foreach (var d in dd)
                        {
                            var k = GlobalOptions.ReferencesCache.GetKontragent(d.KontragentDC);
                            if (k != null)
                                k.OrderCount = d.Count;
                        }
                }

                var ret = GlobalOptions.ReferencesCache.GetKontragentsAll().OrderByDescending(x => x.OrderCount)
                    .ThenBy(x => ((IName) x).Name).Cast<Kontragent>()
                    .ToList();
                return ret;
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
                var f = File.Create((string) Application.Current.Properties["DataPath"] + "\\dblite3.err");
                using (var fs = File.OpenWrite(f.Name))
                {
                    var content = new UTF8Encoding(true).GetBytes((string) Application.Current.Properties["DataPath"]);
                    fs.Write(content, 0, content.Length);
                }
            }

            return null;
        }

        public static void UpdateSelectCount(decimal dc)
        {
            try
            {
                using (var ctx = GlobalOptions.KursSystem())
                {
                    var k = ctx.KontragentCashes.FirstOrDefault(_ =>
                        _.KontragentDC == dc && _.DBId == GlobalOptions.DataBaseId
                                             && _.UserId == GlobalOptions.UserInfo.KursId);
                    if (k != null)
                    {
                        k.Count += 1;
                        k.LastUpdate = DateTime.Now;
                    }
                    else
                    {
                        ctx.KontragentCashes.Add(new KontragentCashes
                        {
                            Id = Guid.NewGuid(),
                            Count = 1,
                            DBId = GlobalOptions.DataBaseId,
                            KontragentDC = dc,
                            UserId = GlobalOptions.UserInfo.KursId,
                            LastUpdate = DateTime.Now
                        });
                    }

                    ctx.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }
        }
    }
}
