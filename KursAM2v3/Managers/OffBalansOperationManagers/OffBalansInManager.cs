using System;
using System.Collections.Generic;
using Data;

namespace KursAM2.Managers.OffBalansOperationManagers
{
    public class OffBalansInManager
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
        public static OffBalanceSheetInDoc GetOffBalansInDoc(Guid id)
        {
            return new OffBalanceSheetInDoc();
        }

        public static OffBalanceSheetInDoc NewOffBalansInDoc(Guid id)
        {
            return new OffBalanceSheetInDoc();
        }

        public static OffBalanceSheetInDoc NewOffBalansInDocCopy(Guid id)
        {
            return new OffBalanceSheetInDoc();
        }

        public static OffBalanceSheetInDoc NewOffBalansInDocCopy(OffBalanceSheetInDoc doc)
        {
            return new OffBalanceSheetInDoc();
        }

        public static OffBalanceSheetInDoc NewOffBalansInDocRequisite(OffBalanceSheetInDoc doc)
        {
            return new OffBalanceSheetInDoc();
        }

        public static OffBalanceSheetInDoc NewOffBalansInDocRequisite(Guid id)
        {
            return new OffBalanceSheetInDoc();
        }

        public static OffBalanceSheetInDoc NewOffBalansInDocSave(OffBalanceSheetInDoc doc)
        {
            return new OffBalanceSheetInDoc();
        }

        public static OffBalanceSheetInDoc NewOffBalansInDocDelete(OffBalanceSheetInDoc doc)
        {
            return new OffBalanceSheetInDoc();
        }

        public static OffBalanceSheetInDoc NewOffBalansInDocDelete(Guid id)
        {
            return new OffBalanceSheetInDoc();
        }

        public static List<OffBalanceSheetInDoc> GetOffBalansInDocs(DateTime start, DateTime end)
        {
            return new List<OffBalanceSheetInDoc>();
        }

        #endregion
    }
}