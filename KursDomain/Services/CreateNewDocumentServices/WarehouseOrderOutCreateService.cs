using System;
using AutoMapper;
using Data;
using KursDomain.ICommon;
using KursDomain.IDocuments;
using KursDomain.IReferences;
using KursDomain.Wrapper.Nomenkl.WarehouseOut;
using Prism.Events;

namespace KursDomain.Services.CreateNewDocumentServices;

public class WarehouseOrderOutCreateService : ICreateNewDocument<WarehouseOutWrapper>
{
    private ALFAMEDIAEntities myContext;
    IReferencesCache myReferencesCache;
    private IEventAggregator myEventAggregator;
    IMessageDialogService myMessageDialogService;

    public WarehouseOrderOutCreateService(ALFAMEDIAEntities context, 
        IReferencesCache referencesCache, 
        IEventAggregator eventAggregator, 
        IMessageDialogService messageDialogService)
    {
        myReferencesCache = referencesCache;
        myEventAggregator = eventAggregator;
        myMessageDialogService = messageDialogService;
        myContext = context;
    }

    public WarehouseOutWrapper NewDocument()
    {
        var model = new SD_24
        {
            DOC_CODE = -1,
            Id = Guid.NewGuid(),
            DD_DATE = DateTime.Today,
            DD_TYPE_DC = (decimal)MaterialDocumentTypeEnum.WarehouseOut,
            CREATOR = GlobalOptions.UserInfo.NickName,
            DD_IN_NUM = -1,
        };
        return new WarehouseOutWrapper(model, myReferencesCache, myContext, myEventAggregator ?? new EventAggregator(), 
            myMessageDialogService ?? new MessageDialogService());
    }

    public WarehouseOutWrapper NewDocument(MaterialDocumentTypeEnum docType)
    {
        throw new NotImplementedException();
    }

    public WarehouseOutWrapper NewCopyDocument(WarehouseOutWrapper oldDocument)
    {
        throw new System.NotImplementedException();
    }

    public WarehouseOutWrapper NewRequisiteDocument(WarehouseOutWrapper oldDocument)
    {
        throw new System.NotImplementedException();
    }
}
