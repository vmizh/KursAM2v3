using System;
using System.Threading.Tasks;
using KursDomain.WindowsManager.WindowsManager;
using Data;
using Helper;
using KursDomain.Documents.CommonReferences;
using KursDomain.Repository.Base;

namespace KursDomain.Repository.LastDocumentRepository;

public class LastDocumentRepository : KursGenericRepository<LastDocument, KursSystemEntities, Guid>,
    ILastDocumentRepository
{
    public LastDocumentRepository(KursSystemEntities context) : base(context)
    {
    }

    public async Task SaveLastOpenInfoAsync(DocumentType docType, Guid? docId, decimal? docDC,
        string creator, string lastChanger,
        string desc)
    {
        try
        {
            if (docDC != null)
                await Context.Database.ExecuteSqlCommandAsync(
                    $"DELETE FROM LastDocument WHERE DocDC = {CustomFormat.DecimalToSqlDecimal(docDC)} " +
                    $"AND UserId = '{CustomFormat.GuidToSqlString(GlobalOptions.UserInfo.KursId)}' " +
                    $"AND DbId = '{CustomFormat.GuidToSqlString(GlobalOptions.DataBaseId)}' ");
            if (docId != null && docId != Guid.Empty)
                await Context.Database.ExecuteSqlCommandAsync(
                    $"DELETE FROM LastDocument WHERE DocId = '{CustomFormat.GuidToSqlString(docId.Value)}' " +
                    $"AND UserId = '{CustomFormat.GuidToSqlString(GlobalOptions.UserInfo.KursId)}' " +
                    $"AND DbId = '{CustomFormat.GuidToSqlString(GlobalOptions.DataBaseId)}' ");

            await Context.Database.ExecuteSqlCommandAsync(
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
            Context.LastDocument.Add(newItem);
            await Context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            WindowManager.ShowError(ex);
        }
    }
}
