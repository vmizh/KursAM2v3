using System.Collections.ObjectModel;

namespace KursDomain.IReferences;

public interface IProjectGroup
{
    ObservableCollection<IProject> Projects { get; set; }
}
