#nullable enable
namespace KursDomain.ICommon;

public interface ITreeReference<I, P>
{
    I Id { set; get; }
    P ParentId { set; get; }
}
