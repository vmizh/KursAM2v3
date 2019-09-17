using System;
using System.Collections.Generic;
using Data;

namespace KursAM2.Managers.OffBalansOperationManagers
{
    public class OffBalansOutManager
    {
        #region Fields

        #endregion

        #region Constructors

        #endregion

        #region Properties

        #endregion

        #region Commands

        #endregion

        #region Static Methods

        /// <summary>
        ///     Загрузить начисление дебиторов
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static OffBalanceSheetOutDoc GetOffBalansOutDoc(Guid id)
        {
            return new OffBalanceSheetOutDoc();
        }

        public static OffBalanceSheetOutDoc NewOffBalansOutDoc(Guid id)
        {
            return new OffBalanceSheetOutDoc();
        }

        public static OffBalanceSheetOutDoc NewOffBalansOutDocCopy(Guid id)
        {
            return new OffBalanceSheetOutDoc();
        }

        public static OffBalanceSheetInDoc NewOffBalansInDocCopy(OffBalanceSheetOutDoc doc)
        {
            return new OffBalanceSheetInDoc();
        }

        public static OffBalanceSheetOutDoc NewOffBalansOutDocRequisite(OffBalanceSheetOutDoc doc)
        {
            return new OffBalanceSheetOutDoc();
        }

        public static OffBalanceSheetOutDoc NewOffBalansOutDocRequisite(Guid id)
        {
            return new OffBalanceSheetOutDoc();
        }

        public static OffBalanceSheetOutDoc NewOffBalansOutDocSave(OffBalanceSheetOutDoc doc)
        {
            return new OffBalanceSheetOutDoc();
        }

        public static OffBalanceSheetOutDoc NewOffBalansOutDocDelete(OffBalanceSheetOutDoc doc)
        {
            return new OffBalanceSheetOutDoc();
        }

        public static OffBalanceSheetOutDoc NewOffBalansOutDocDelete(Guid id)
        {
            return new OffBalanceSheetOutDoc();
        }

        public static List<OffBalanceSheetOutDoc> GetOffBalansOutDocs(DateTime start, DateTime end)
        {
            return new List<OffBalanceSheetOutDoc>();
        }

        #endregion
    }
}