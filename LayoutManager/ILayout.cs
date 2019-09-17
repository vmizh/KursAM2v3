
namespace LayoutManager
{
    public interface ILayout
    {
        LayoutManagerBase LayoutManager { set; get; }
        string LayoutManagerName { set; get; }
    }
}