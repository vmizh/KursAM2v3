using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using Core;
using Core.ViewModel.Common;
using Core.WindowsManager;
using SQLite.Base;
using SQLite.Base.Entity;

namespace KursAM2.Managers
{
    public class KontragentManager
    {
        public static List<Kontragent> GetAllKontragentSortedByUsed()
        {
            try
            {
                using (var ctx = new LocalDbContext())
                {
                    var dd = ctx.KontragentCashes.Where(_ => _.DBName == GlobalOptions.DataBaseName).ToList();
                    if (dd.Count > 0)
                    {
                        foreach (var d in dd)
                        {
                            var k = MainReferences.GetKontragent(d.DocCode);
                            if (k != null)
                                k.OrderCount = d.Count;
                        }
                    }
                }
                var ret = MainReferences.AllKontragents.Values.OrderByDescending(x => x.OrderCount)
                    .ThenBy(x => x.Name)
                    .ToList();
                return ret;
            }
            catch (Exception ex)
            {
               
                WindowManager.ShowError(ex);
                var f = File.Create((string)Application.Current.Properties["DataPath"]+"\\dblite3.err");
                using (FileStream fs = File.OpenWrite(f.Name))
                {
                    Byte[] content = new UTF8Encoding(true).GetBytes((string)Application.Current.Properties["DataPath"]);
                    fs.Write(content, 0, content.Length);
                }
            }
            return null;
        }
        public static void UpdateSelectCount(decimal dc)
        {
            try
            {
                using (var ctx = new LocalDbContext())
                {
                    var k = ctx.KontragentCashes.FirstOrDefault(_ => _.DocCode == dc && _.DBName == GlobalOptions.DataBaseName);
                    if (k != null)
                    {
                        k.Count = k.Count + 1;
                        k.LastUpdate = DateTime.Now;
                    }
                    else
                    {
                        int newId = 1;
                        var d = ctx.KontragentCashes.ToList().Count;
                        if (d > 0)
                            newId = ctx.KontragentCashes.Max(_ => _.Id) + 1;
                        ctx.KontragentCashes.Add(new KontragentCash
                        {
                            Id = newId,
                            DBName = GlobalOptions.DataBaseName,
                            DocCode = dc,
                            LastUpdate = DateTime.Now,
                            Count = 1
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