namespace KursDomain.Base;

public interface IStartEditingService
{
    void StartEditing(object dataItem);
    void StartEditing(object dataItem, string fieldName);
}
