namespace Helper.Interface
{
    public interface ITree<Key, PKey>
        where Key : struct
        where PKey : struct
    {
        Key Id { set; get; }
        PKey? ParentId { set; get; }
        ITree<Key, PKey>  Item { set; get; }
    }
    public interface ITreeWithSumCalculateSimple<Key, PKey> : ITree<Key, PKey>
        where Key : struct
        where PKey : struct
    {
        decimal CalcSumma();
    }
    public interface ITreeWithSumCalculate<Key, PKey> : ITree<Key, PKey>
        where Key : struct
        where PKey : struct
    {
        decimal CalcSumma();
        void CalcSumma(decimal par1);
        void CalcSumma(decimal par1, decimal par2);
        void CalcSumma(decimal par1, decimal par2, decimal par3);
        void CalcSumma(decimal par1, decimal par2, decimal par3, decimal par4);

    }
}