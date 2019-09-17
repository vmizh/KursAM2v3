using System.Windows;

namespace LayoutManager
{
    public class LayoutManager : LayoutManagerBase
    {
        public LayoutManager(string fname, Window win, DependencyObject ctrl)
        {
            FileName = fname;
            Win = win;
            LayoutControl = ctrl;
        }

        public LayoutManager(string fname, DependencyObject ctrl)
        {
            FileName = fname;
            Win = null;
            LayoutControl = ctrl;
        }
    }
}