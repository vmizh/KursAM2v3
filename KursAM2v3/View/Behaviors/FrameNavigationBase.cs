using DevExpress.Mvvm.UI.Interactivity;
using DevExpress.Xpf.WindowsUI;

namespace KursAM2.View.Behaviors
{
    public class FrameBalansNavigation : Behavior<NavigationFrame>
    {
        private static NavigationFrame myCurrentNavigationFrame;

        protected override void OnAttached()
        {
            base.OnAttached();
            myCurrentNavigationFrame = AssociatedObject;
        }

        public static void Navigate(object source)
        {
            myCurrentNavigationFrame?.Navigate(source);
        }

        public static void Navigate(object source, object param)
        {
            myCurrentNavigationFrame?.Navigate(source, param);
        }
    }

    public class FrameProfitNavigation : Behavior<NavigationFrame>
    {
        private static NavigationFrame myCurrentNavigationFrame;

        protected override void OnAttached()
        {
            base.OnAttached();
            myCurrentNavigationFrame = AssociatedObject;
        }

        public static void Navigate(object source)
        {
            myCurrentNavigationFrame?.Navigate(source);
        }

        public static void Navigate(object source, object param)
        {
            myCurrentNavigationFrame?.Navigate(source, param);
        }
    }
}