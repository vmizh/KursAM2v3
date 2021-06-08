using DevExpress.Mvvm.UI.Interactivity;
using DevExpress.Xpf.Editors;

namespace KursAM2.View.Behaviors
{
    public sealed class SelectAllOnGotFocusBehavior : Behavior<TextEdit> {
        TextEdit TextEdit => AssociatedObject;

        protected override void OnAttached() {
            base.OnAttached();
            TextEdit.GotFocus += OnGotFocus;
        }

        protected override void OnDetaching() {
            TextEdit.GotFocus -= OnGotFocus;
            base.OnDetaching();
        }

        void OnGotFocus(object sender, System.Windows.RoutedEventArgs e) {
            TextEdit.SelectAll();
        }
    }
}