﻿using System;
using System.ComponentModel.DataAnnotations;
using KursDomain;
using KursDomain.Documents.CommonReferences;

namespace KursAM2.Repositories.RedisRepository
{
    public class RedisMessage
    {
        public DocumentType DocumentType { get; set; }
        public RedisMessageDocumentOperationTypeEnum OperationType { get; set; }
        public Guid UserId { get; set; } = GlobalOptions.UserInfo.KursId;
        public Guid DbId { get; set; } = GlobalOptions.DataBaseId;
        public string UserName { get; set; } = GlobalOptions.UserInfo.NickName;
        public bool IsDocument { get; set; }
        public decimal? DocCode { get; set; }
        public DateTime? DocDate { get; set; }
        public Guid? Id { get; set; }
        public string Message { get; set; }
    }

    public enum RedisMessageDocumentOperationTypeEnum
    {
        [Display(Name = "Открытие")]
        Open = 0,
        [Display(Name = "Создание")]
        Create = 1,
        [Display(Name = "Обновление")]
        Update = 2,
        [Display(Name = "Удаление")]
        Delete = 3,
    }
}