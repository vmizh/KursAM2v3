using System;

namespace KursAM2.View.Behaviors
{
    public class FrameBalansNavigator : INavigator
    {
        public void Navigate(Type source)
        {
            FrameBalansNavigation.Navigate(source.Name);
        }

        public void Navigate(Type source, object param)
        {
            FrameBalansNavigation.Navigate(source.Name, param);
        }

        public void NavigateToInstance(object source)
        {
            FrameBalansNavigation.Navigate(source);
        }
    }

//    public class FrameProfitNavigator : INavigator
//    {
//        public void Navigate(Type source)
//        {
//            FrameProfitNavigation.Navigate(source.Name);
//        }

//        public void Navigate(Type source, object param)
//        {
//            FrameProfitNavigation.Navigate(source.Name, param);
//        }

//        public void NavigateToInstance(object source)
//        {
//            FrameProfitNavigation.Navigate(source);
//        }
//    }
}
