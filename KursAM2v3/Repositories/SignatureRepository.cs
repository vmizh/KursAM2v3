using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Core;
using Data;
using Data.Repository;
using KursAM2.ViewModel.Signatures;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KursAM2.Repositories
{
    public interface ISignatureRepository
    {
        List<SignatureViewModel> ResetSignes(int docType);
        List<SignatureViewModel> CreateSignes(int docType, Guid docId, out bool isSign, out bool isNew);
        void SaveSignes();
        string LoadSignes();
        UnitOfWork<ALFAMEDIAEntities> UnitOfWork { set; get; }
    }

    public class SignatureRepository : GenericKursDBRepository<DocumentSignatures>,
        ISignatureRepository
    {
        public UnitOfWork<ALFAMEDIAEntities> UnitOfWork { set; get; }

        public SignatureRepository(IUnitOfWork<ALFAMEDIAEntities> unitOfWork) : base(unitOfWork)
        {
            UnitOfWork = (UnitOfWork<ALFAMEDIAEntities>)unitOfWork;
        }

        public SignatureRepository(ALFAMEDIAEntities context) : base(context)
        {
        }

        private List<SignatureViewModel> createSigns(KursSystemEntities ctx, int docType)
        {
            var res = new List<SignatureViewModel>();
            var signes = ctx.SignatureSchemesInfo
                .Include(_ => _.SignatureSchemes)
                .Include(_ => _.SignatureType)
                .Where(_ => _.SignatureSchemes.DocumentTYpeId == docType
                            && _.SignatureSchemes.DbId == GlobalOptions.DataBaseId).ToList();
            foreach (var sign in signes)
            {
                var newItem = new SignatureViewModel
                {
                    Id = sign.Id,
                    ParentId = sign.ParentId,
                    IsRequired = sign.IsRequired,
                    DocumentType = 72,
                    SignTypeId = sign.SignId,
                    SignTypeName = sign.SignatureType.Name
                };
                res.Add(newItem);
            }

            return res;
        }



        public List<SignatureViewModel> CreateSignes(int docType, Guid docId, out bool isSign, out bool isNew)
        {
            isSign = false;
            isNew = false;
            var res = new List<SignatureViewModel>();
            using (var ctx = GlobalOptions.KursSystem())
            {
                var data = UnitOfWork.Context.DocumentSignatures.FirstOrDefault(_ => _.DocId == docId);
                if (data == null)
                {
                    res.AddRange(createSigns(ctx, docType));
                    isNew = true;
                }
                else
                {
                    if (data.Signatures == "[]")
                    {
                        res.AddRange(createSigns(ctx, docType));
                        isNew = true;
                    }
                    else
                    {
                        isSign = data.IsSign;
                        var signs = (JArray)JsonConvert.DeserializeObject(data.Signatures);
                        // ReSharper disable once PossibleNullReferenceException
                        foreach (var jToken in signs)
                        {
                            var s = (JObject)jToken;
                            var newItem = new SignatureViewModel();
                            foreach (var p in s.Properties())
                                switch (p.Name)
                                {
                                    case "Id":
                                        newItem.Id = (Guid)p.Value;
                                        break;
                                    case "DocumentType":
                                        newItem.DocumentType = (int)p.Value;
                                        break;
                                    case "ParentId":
                                        newItem.ParentId = (Guid?)p.Value;
                                        break;
                                    case "IsRequired":
                                        newItem.IsRequired = (bool)p.Value;
                                        break;
                                    case "Note":
                                        newItem.Note = (string)p.Value;
                                        break;
                                    case "SignTypeId":
                                        newItem.SignTypeId = (Guid)p.Value;
                                        break;
                                    case "SignTypeName":
                                        newItem.SignTypeName = (string)p.Value;
                                        break;
                                    case "UserId":
                                        newItem.UserId = (Guid?)p.Value;
                                        break;
                                    case "SignUserName":
                                        newItem.SignUserName = (string)p.Value;
                                        break;
                                    case "SignUserFullName":
                                        newItem.SignUserFullName = (string)p.Value;
                                        break;
                                }

                            res.Add(newItem);
                        }
                    }

                    foreach (var r in res)
                    {
                        var ss = ctx.SignatureType.Include(_ => _.Users)
                            .FirstOrDefault(_ => _.DbId == GlobalOptions.DataBaseId && _.Id == r.SignTypeId);
                        if (ss != null)
                            foreach (var u in ss.Users)
                                r.UsersCanSigned.Add(new UserSignViewModel
                                {
                                    Id = u.Id,
                                    SignUserName = u.Name
                                });
                    }

                }

                return res;
            }
        }

        public List<SignatureViewModel> ResetSignes(int docType)
        {
            List<SignatureViewModel> res;
            using (var ctx = GlobalOptions.KursSystem())
            {
                    res = createSigns(ctx, docType);
            }

            return res;
        }

        public void SaveSignes()
        {
            throw new NotImplementedException();
        }

        public string LoadSignes()
        {
            throw new NotImplementedException();
        }
    }
}