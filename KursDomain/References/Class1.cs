using System;
using System.Collections;
using Core.ViewModel.Base;
using KursDomain.ICommon;

namespace KursDomain.References;

public abstract class NomenklTree : IName, ITreeReference<decimal, decimal?>
{
    public virtual string Name { get; set; }
    public virtual string Notes { get; set; }
    public virtual string Description => "Не определен";

    public abstract void Add(NomenklTree node);
    public abstract void Remove(NomenklTree node);
    public abstract NomenklTree GetChild(int index);
    public abstract int GetNomenklCount();
    public decimal Id { get; set; }
    public decimal? ParentId { get; set; }
}

public class NomenklTreeGroup : NomenklTree
{
    private NomenklGroup group;
    private ArrayList nodes = new ArrayList();

    public NomenklTreeGroup(NomenklGroup grp)
    {
        group = grp;
    }

    public override void Add(NomenklTree node)
    {
        throw new NotImplementedException();
    }

    public override void Remove(NomenklTree node)
    {
        throw new NotImplementedException();
    }

    public override NomenklTree GetChild(int index)
    {
        throw new NotImplementedException();
    }

    public override int GetNomenklCount()
    {
        throw new NotImplementedException();
    }
}

public class NomenklTreeOne : NomenklTree
{
    private readonly Nomenkl nomenkl;

    public NomenklTreeOne(Nomenkl nom)
    {
        nomenkl = nom;
    }

    public override string Name => nomenkl.Name;
    public override string Notes => nomenkl.Notes;
    public override string Description => nomenkl.Description;

    public override void Add(NomenklTree node)
    {
        throw new NotImplementedException();
    }

    public override void Remove(NomenklTree node)
    {
        throw new NotImplementedException();
    }

    public override NomenklTree GetChild(int index)
    {
        throw new NotImplementedException();
    }

    public override int GetNomenklCount()
    {
        return 1;
    }
}
