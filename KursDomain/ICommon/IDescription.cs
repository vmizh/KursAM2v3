using System.Dynamic;

namespace KursDomain.ICommon;

public interface IDescription
{
    string Description { get; }
    string Notes { get; set; }
}
