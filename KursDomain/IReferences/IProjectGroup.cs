using System.Collections.ObjectModel;
using KursDomain.References;

namespace KursDomain.IReferences;

public interface IProjectGroup
{
    ObservableCollection<ProjectViewModel> Projects { get; set; }
}
