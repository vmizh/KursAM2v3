﻿using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Core.Helper;
using Core.ViewModel.Base;
using DevExpress.Mvvm.DataAnnotations;
using KursDomain.ICommon;

namespace KursDomain.Documents.Bank;

[MetadataType(typeof(DataAnnotationsBankAndAccounts))]
[SuppressMessage("ReSharper", "VirtualMemberCallInConstructor")]
public class BankAndAccounts : RSViewModelBase
{
    private References.Bank myBank;
    private decimal? myBankDC;
    private int myCODE;
    private short? myDELETED;
    private short? myDISABLED;
    private string myRASCH_ACC;

    public BankAndAccounts()
    {
        State = RowStatus.NotEdited;
    }

    public override int Code
    {
        set
        {
            if (myCODE == value) return;
            myCODE = value;
            RaisePropertyChanged();
        }
        get => myCODE;
    }

    public string RASCH_ACC
    {
        set
        {
            if (myRASCH_ACC == value) return;
            myRASCH_ACC = value;
            RaisePropertyChanged();
        }
        get => myRASCH_ACC;
    }

    public bool IsDisabled
    {
        set
        {
            if (DISABLED == 1 == value) return;
            DISABLED = (short?)(value ? 1 : 0);
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(DISABLED));
        }
        get => DISABLED == 1;
    }

    public short? DISABLED
    {
        set
        {
            if (myDISABLED == value) return;
            myDISABLED = value;
            RaisePropertyChanged();
        }
        get => myDISABLED;
    }

    public bool IsDeleted
    {
        set
        {
            if (DELETED == 1 == value) return;
            DELETED = (short?)(value ? 1 : 0);
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(DELETED));
        }
        get => DISABLED == 1;
    }

    public short? DELETED
    {
        set
        {
            if (myDELETED == value) return;
            myDELETED = value;
            RaisePropertyChanged();
        }
        get => myDELETED;
    }

    public decimal? BankDC
    {
        set
        {
            if (myBankDC == value) return;
            myBankDC = value;
            RaisePropertyChanged();
        }
        get => myBankDC;
    }

    public References.Bank Bank
    {
        get => myBank;
        set
        {
            if (myBank != null && myBank.Equals(value)) return;
            myBank = value;
            if (myBank != null)
                BankDC = myBank.DocCode;
            RaisePropertyChanged();
        }
    }
}

public class DataAnnotationsBankAndAccounts : DataAnnotationForFluentApiBase, IMetadataProvider<BankAndAccounts>
{
    void IMetadataProvider<BankAndAccounts>.BuildMetadata(MetadataBuilder<BankAndAccounts> builder)
    {
        SetNotAutoGenerated(builder);
        builder.Property(_ => _.RASCH_ACC).AutoGenerated().DisplayName("Р/счет");
        builder.Property(_ => _.IsDisabled).AutoGenerated().DisplayName("Заблокирован");
        builder.Property(_ => _.Bank).AutoGenerated().DisplayName("Банк");
    }
}