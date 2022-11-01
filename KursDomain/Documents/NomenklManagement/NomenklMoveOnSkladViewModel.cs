using Core.ViewModel.Base;
using KursDomain.ICommon;
using KursDomain.References;

namespace KursDomain.Documents.NomenklManagement;

public class NomenklMoveOnSkladViewModel : RSViewModelBase
{
    private Nomenkl myNomenkl;
    private decimal myPriceEnd;
    private decimal myPriceStart;
    private decimal myQuantityEnd;
    private decimal myQuantityIn;
    private decimal myQuantityOut;
    private decimal myQuantityStart;
    private decimal mySummaAllEnd;
    private decimal mySummaAllIn;
    private decimal mySummaAllOut;
    private decimal mySummaAllStart;
    private decimal mySummaEUREnd;
    private decimal mySummaEURIn;
    private decimal mySummaEUROut;
    private decimal mySummaEURStart;
    private decimal mySummaRUBEnd;
    private decimal mySummaRUBIn;
    private decimal mySummaRUBOut;
    private decimal mySummaRUBStart;
    private decimal mySummaUSDEnd;
    private decimal mySummaUSDIn;
    private decimal mySummaUSDOut;
    private decimal mySummaUSDStart;

    public override decimal DocCode => myNomenkl?.DocCode ?? 0;

    public Nomenkl Nomenkl
    {
        get => myNomenkl;
        set
        {
            if (myNomenkl != null && myNomenkl.Equals(value)) return;
            myNomenkl = value;
            RaisePropertyChanged();
        }
    }

    public new string Name => Nomenkl?.Name;
    public string NomenklNumber => Nomenkl?.NomenklNumber;
    public string CurrencyName => ((IName)Nomenkl?.Currency)?.Name;
    public new string Note => ((IName)Nomenkl)?.Notes;

    public decimal QuantityStart
    {
        get => myQuantityStart;
        set
        {
            if (myQuantityStart == value) return;
            myQuantityStart = value;
            RaisePropertyChanged();
        }
    }

    public decimal QuantityEnd
    {
        get => myQuantityEnd;
        set
        {
            if (myQuantityEnd == value) return;
            myQuantityEnd = value;
            RaisePropertyChanged();
        }
    }

    public decimal QuantityIn
    {
        get => myQuantityIn;
        set
        {
            if (myQuantityIn == value) return;
            myQuantityIn = value;
            RaisePropertyChanged();
        }
    }

    public decimal QuantityOut
    {
        get => myQuantityOut;
        set
        {
            if (myQuantityOut == value) return;
            myQuantityOut = value;
            RaisePropertyChanged();
        }
    }

    public decimal PriceStart
    {
        get => myPriceStart;
        set
        {
            if (myPriceStart == value) return;
            myPriceStart = value;
            RaisePropertyChanged();
        }
    }

    public decimal PriceEnd
    {
        get => myPriceEnd;
        set
        {
            if (myPriceEnd == value) return;
            myPriceEnd = value;
            RaisePropertyChanged();
        }
    }

    public decimal SummaRUBStart
    {
        get => mySummaRUBStart;
        set
        {
            if (mySummaRUBStart == value) return;
            mySummaRUBStart = value;
            RaisePropertyChanged();
        }
    }

    public decimal SummaRUBEnd
    {
        get => mySummaRUBEnd;
        set
        {
            if (mySummaRUBEnd == value) return;
            mySummaRUBEnd = value;
            RaisePropertyChanged();
        }
    }

    public decimal SummaRUBIn
    {
        get => mySummaRUBIn;
        set
        {
            if (mySummaRUBIn == value) return;
            mySummaRUBIn = value;
            RaisePropertyChanged();
        }
    }

    public decimal SummaRUBOut
    {
        get => mySummaRUBOut;
        set
        {
            if (mySummaRUBOut == value) return;
            mySummaRUBOut = value;
            RaisePropertyChanged();
        }
    }

    public decimal SummaUSDStart
    {
        get => mySummaUSDStart;
        set
        {
            if (mySummaUSDStart == value) return;
            mySummaUSDStart = value;
            RaisePropertyChanged();
        }
    }

    public decimal SummaUSDEnd
    {
        get => mySummaUSDEnd;
        set
        {
            if (mySummaUSDEnd == value) return;
            mySummaUSDEnd = value;
            RaisePropertyChanged();
        }
    }

    public decimal SummaUSDIn
    {
        get => mySummaUSDIn;
        set
        {
            if (mySummaUSDIn == value) return;
            mySummaUSDIn = value;
            RaisePropertyChanged();
        }
    }

    public decimal SummaUSDOut
    {
        get => mySummaUSDOut;
        set
        {
            if (mySummaUSDOut == value) return;
            mySummaUSDOut = value;
            RaisePropertyChanged();
        }
    }

    public decimal SummaEURStart
    {
        get => mySummaEURStart;
        set
        {
            if (mySummaEURStart == value) return;
            mySummaEURStart = value;
            RaisePropertyChanged();
        }
    }

    public decimal SummaEUREnd
    {
        get => mySummaEUREnd;
        set
        {
            if (mySummaEUREnd == value) return;
            mySummaEUREnd = value;
            RaisePropertyChanged();
        }
    }

    public decimal SummaEURIn
    {
        get => mySummaEURIn;
        set
        {
            if (mySummaEURIn == value) return;
            mySummaEURIn = value;
            RaisePropertyChanged();
        }
    }

    public decimal SummaEUROut
    {
        get => mySummaEUROut;
        set
        {
            if (mySummaEUROut == value) return;
            mySummaEUROut = value;
            RaisePropertyChanged();
        }
    }

    public decimal SummaAllStart
    {
        get => mySummaAllStart;
        set
        {
            if (mySummaAllStart == value) return;
            mySummaAllStart = value;
            RaisePropertyChanged();
        }
    }

    public decimal SummaAllEnd
    {
        get => mySummaAllEnd;
        set
        {
            if (mySummaAllEnd == value) return;
            mySummaAllEnd = value;
            RaisePropertyChanged();
        }
    }

    public decimal SummaAllIn
    {
        get => mySummaAllIn;
        set
        {
            if (mySummaAllIn == value) return;
            mySummaAllIn = value;
            RaisePropertyChanged();
        }
    }

    public decimal SummaAllOut
    {
        get => mySummaAllOut;
        set
        {
            if (mySummaAllOut == value) return;
            mySummaAllOut = value;
            RaisePropertyChanged();
        }
    }
}
