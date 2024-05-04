using System;
using AutoMapper;
using Data;
using KursDomain.IDocuments;

namespace KursDomain.Services.CreateNewDocumentServices;

public class SD24CreateService : BaseCreateService, ICreateNewDocument<SD_24>
{
    public SD_24 NewDocument()
    {
        var model = new SD_24
        {
            DOC_CODE = -1,
            Id = Guid.NewGuid(),
            DD_DATE = DateTime.Today,
            DD_TYPE_DC = (decimal)MaterialDocumentTypeEnum.None,
            CREATOR = GlobalOptions.UserInfo.NickName,
            DD_IN_NUM = -1,
        };
        Context.SD_24.Add(model);
        return model;
    }

    public SD_24 NewDocument(MaterialDocumentTypeEnum docType)
    {
        var model = new SD_24
        {
            DOC_CODE = -1,
            Id = Guid.NewGuid(),
            DD_DATE = DateTime.Today,
            DD_TYPE_DC = (decimal)docType,
            CREATOR = GlobalOptions.UserInfo.NickName,
            DD_IN_NUM = -1,
        };
        Context.SD_24.Add(model);
        return model;
    }

    public SD_24 NewCopyDocument(SD_24 oldDocument)
    {
        var config = new MapperConfiguration(cfg => cfg.CreateMap<SD_24, SD_24>());
        var mapper = new Mapper(config);
        var model = mapper.Map<SD_24>(oldDocument);
        var id = Guid.NewGuid();
        model.DOC_CODE = -1;
        model.Id = id;
        model.DD_DATE = DateTime.Today;
        model.CREATOR = GlobalOptions.UserInfo.NickName;
        model.DD_IN_NUM = -1;
        int code = 1;
        if (model.TD_24 is { Count: > 0 })
        {
            foreach (var row in model.TD_24)
            {
                row.DOC_CODE = -1;
                row.CODE = code;
                row.Id = Guid.NewGuid();
                row.DocId = id;
                code++;
            }
        }
        Context.SD_24.Add(model);
        return model;
    }

    public SD_24 NewRequisiteDocument(SD_24 oldDocument)
    {
        var config = new MapperConfiguration(cfg => cfg.CreateMap<SD_24, SD_24>());
        var mapper = new Mapper(config);
        var model = mapper.Map<SD_24>(oldDocument);
        var id = Guid.NewGuid();
        model.DOC_CODE = -1;
        model.Id = id;
        model.DD_DATE = DateTime.Today;
        model.CREATOR = GlobalOptions.UserInfo.NickName;
        model.DD_IN_NUM = -1;
        model.TD_24.Clear();
        Context.SD_24.Add(model);
        return model;
    }

    public SD24CreateService(ALFAMEDIAEntities context) : base(context)
    {
    }
}
