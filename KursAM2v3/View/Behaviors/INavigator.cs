using System;

namespace KursAM2.View.Behaviors
{
    public interface INavigator
    {
        void Navigate(Type source);
        void Navigate(Type source, object param);
        void NavigateToInstance(object source);
    }
}