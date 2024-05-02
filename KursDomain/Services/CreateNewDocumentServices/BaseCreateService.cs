using Data;

namespace KursDomain.Services.CreateNewDocumentServices;

public class BaseCreateService
{
    protected ALFAMEDIAEntities Context;
    public BaseCreateService(ALFAMEDIAEntities context)
    {
        Context = context;
    }
}
