﻿using System;
using System.ComponentModel.DataAnnotations;
using Core.Helper;
using Core.ViewModel.Base;
using DevExpress.Mvvm.DataAnnotations;
using KursDomain.Documents.CommonReferences;

namespace KursDomain.Documents.Dogovora;

public class LinkDocumentInfo_FluentAPI : DataAnnotationForFluentApiBase,
    IMetadataProvider<LinkDocumentInfo>
{
    void IMetadataProvider<LinkDocumentInfo>.BuildMetadata(
        MetadataBuilder<LinkDocumentInfo> builder)
    {
        SetNotAutoGenerated(builder);
        builder.Property(_ => _.DocumentType).AutoGenerated().DisplayName("Тип документа").ReadOnly();
        builder.Property(_ => _.DocNumber).AutoGenerated().DisplayName("№").ReadOnly();
        builder.Property(_ => _.DocDate).AutoGenerated().DisplayName("Дата").ReadOnly();
        builder.Property(_ => _.DocInfo).AutoGenerated().DisplayName("Описание").ReadOnly();
    }
}

/// <summary>
///     Связанный документ
/// </summary>
[MetadataType(typeof(LinkDocumentInfo_FluentAPI))]
public class LinkDocumentInfo : RSViewModelBase
{
    private DateTime myDocDate;
    private string myDocInfo;
    private string myDocNumber;
    private DocumentType myDocumentType;


    private decimal mySumma;

    public DocumentType DocumentType
    {
        get => myDocumentType;
        set
        {
            if (myDocumentType == value) return;
            myDocumentType = value;
            RaisePropertyChanged();
        }
    }

    public string DocNumber
    {
        get => myDocNumber;
        set
        {
            if (myDocNumber == value) return;
            myDocNumber = value;
            RaisePropertyChanged();
        }
    }

    public DateTime DocDate
    {
        get => myDocDate;
        set
        {
            if (myDocDate == value) return;
            myDocDate = value;
            RaisePropertyChanged();
        }
    }

    public string DocInfo
    {
        get => myDocInfo;
        set
        {
            if (myDocInfo == value) return;
            myDocInfo = value;
            RaisePropertyChanged();
        }
    }

    public decimal Summa
    {
        get => mySumma;
        set
        {
            if (mySumma == value) return;
            mySumma = value;
            RaisePropertyChanged();
        }
    }
}