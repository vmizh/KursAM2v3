using System;
using Data;

namespace Core.Helper
{
    public class DocumentHistoryHelper
    {
        public static void SaveHistory(string docType, Guid? docId, decimal? docDC, int? code,
            string json, ALFAMEDIAEntities context = null)
        {
            if (context == null)
            {
                using var ctx = GlobalOptions.GetEntities();
                ctx.DocHistory.Add(new DocHistory
                {
                    Id = Guid.NewGuid(),
                    Date = DateTime.Now,
                    UserName = GlobalOptions.UserInfo.NickName,
                    DocType = docType,
                    DocId = docId,
                    DocDC = docDC,
                    Code = code,
                    DocData = json
                });
                ctx.SaveChanges();
            }
            else
                context.DocHistory.Add(new DocHistory
                {
                    Id = Guid.NewGuid(),
                    Date = DateTime.Now,
                    UserName = GlobalOptions.UserInfo.NickName,
                    DocType = docType,
                    DocId = docId,
                    DocDC = docDC,
                    Code = code,
                    DocData = json
                });
        }
    }
}