using System.Data.Entity.Core.Metadata.Edm;

namespace SQLite.Base.Public
{
    public interface ISqlGenerator
    {
        string Generate(EdmModel storeModel);
    }
}