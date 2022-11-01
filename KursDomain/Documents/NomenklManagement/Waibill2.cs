using System.ComponentModel;
using Core;
using Core.Helper;
using Data;
using KursDomain.Documents.CommonReferences.Kontragent;

namespace KursDomain.Documents.NomenklManagement;

public class Waibill2 : SD_24ViewModel, IDataErrorInfo
{
    #region Properties

    public Kontragent Client
    {
        get => MainReferences.GetKontragent(Entity.DD_KONTR_POL_DC);
        set
        {
            if (Entity.DD_KONTR_POL_DC == value?.DocCode) return;
            Entity.DD_KONTR_POL_DC = value?.DocCode;
            RaisePropertyChanged();
        }
    }

    #endregion

    #region Methods

    public new SD_24 DefaultValue()
    {
        return new SD_24
        {
            DOC_CODE = -1
        };
    }

    #endregion

    #region Fields

    #endregion

    #region Constructors

    #endregion

    #region Commands

    #endregion

    #region IDataErrorInfo

    public string this[string columnName]
    {
        get
        {
            switch (columnName)
            {
                case nameof(Client):
                    return Client == null ? ValidationError.FieldNotNull : null;
            }

            return null;
        }
    }

    public string Error { get; } = null;

    #endregion
}
