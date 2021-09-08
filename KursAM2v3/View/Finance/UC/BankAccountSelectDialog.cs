using System.Windows;
using Core.ViewModel.Base;

namespace KursAM2.View.Finance.UC
{
    public sealed class BankAccountSelectDialog : RSWindowViewModelBase, IDataUserControl

    {
        #region Fields

        #endregion

        #region Constructors

        public BankAccountSelectDialog()
        {
            RefreshData(null);
            WindowName = "Выбор расчетных счетов";
        }

        #endregion

        #region Properties

        #endregion

        #region Commands

        #endregion

        // ReSharper disable once UnassignedGetOnlyAutoProperty
        public DependencyObject LayoutControl { get; }
    }
}