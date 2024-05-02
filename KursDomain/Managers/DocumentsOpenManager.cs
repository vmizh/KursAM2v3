using System;
using System.Linq;
using Core.WindowsManager;
using Data;
using Helper;
using KursDomain;
using KursDomain.Documents.CommonReferences;

namespace KursAM2.Managers;

public class DocumentsOpenManager
{
    public static void SaveLastOpenInfo(DocumentType docType, Guid? docId, decimal? docDC,
        string creator, string lastChanger,
        string desc)
    {
        try
        {
            using (var ctx = GlobalOptions.KursSystem())
            {
                if (docDC != null)
                    ctx.Database.ExecuteSqlCommand(
                        $"DELETE FROM LastDocument WHERE DocDC = {CustomFormat.DecimalToSqlDecimal(docDC)} " +
                        $"AND UserId = '{CustomFormat.GuidToSqlString(GlobalOptions.UserInfo.KursId)}' " +
                        $"AND DbId = '{CustomFormat.GuidToSqlString(GlobalOptions.DataBaseId)}' ");
                if (docId != null && docId != Guid.Empty)
                    ctx.Database.ExecuteSqlCommand(
                        $"DELETE FROM LastDocument WHERE DocId = '{CustomFormat.GuidToSqlString(docId.Value)}' " +
                        $"AND UserId = '{CustomFormat.GuidToSqlString(GlobalOptions.UserInfo.KursId)}' " +
                        $"AND DbId = '{CustomFormat.GuidToSqlString(GlobalOptions.DataBaseId)}' ");

                ctx.Database.ExecuteSqlCommand(
                    $"DELETE FROM LastDocument WHERE UserId = '{CustomFormat.GuidToSqlString(GlobalOptions.UserInfo.KursId)}' " +
                    $"AND DbId = '{CustomFormat.GuidToSqlString(GlobalOptions.DataBaseId)}' " +
                    $"AND LastOpen < '{CustomFormat.DateToString(DateTime.Today.AddDays(-60))}'");

                var newItem = new LastDocument
                {
                    Id = Guid.NewGuid(),
                    UserId = GlobalOptions.UserInfo.KursId,
                    DbId = GlobalOptions.DataBaseId,
                    DocId = docId,
                    Creator = creator ?? "не указан",
                    DocDC = docDC,
                    DocType = (int)docType,
                    LastChanger = lastChanger,
                    LastOpen = DateTime.Now,
                    Description = desc
                };
                ctx.LastDocument.Add(newItem);
                ctx.SaveChanges();
            }
        }
        catch (Exception ex)
        {
            WindowManager.ShowError(ex);
        }
    }

    public static void DeleteFromLastDocument(Guid? id, decimal? dc)
    {
        using (var ctx = GlobalOptions.KursSystem())
        {
            if (id != null && id != Guid.Empty)
            {
                var old = ctx.LastDocument.FirstOrDefault(_ => _.DocId == id);
                if (old != null)
                {
                    ctx.LastDocument.Remove(old);
                    ctx.SaveChanges();
                }
            }

            if (dc == null) return;
            {
                var old = ctx.LastDocument
                    .FirstOrDefault(_ => _.DocDC == dc && _.DbId == GlobalOptions.DataBaseId);
                if (old != null)
                {
                    ctx.LastDocument.Remove(old);
                    ctx.SaveChanges();
                }
            }
        }
    }
}
