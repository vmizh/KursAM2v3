using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using DevExpress.Mvvm.UI.Interactivity;
using DevExpress.Xpf.Core;

namespace Helper
{
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class ThemeHelper : Behavior<FrameworkElement>
    {
        public string ThemeName { get; set; }

        protected override void OnAttached()
        {
            base.OnAttached();
            if (AssociatedObject.IsInitialized)
                ThemeManager.SetThemeName(AssociatedObject, ThemeName);
            AssociatedObject.Initialized += AssociatedObject_Initialized;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Initialized -= AssociatedObject_Initialized;
            base.OnDetaching();
        }

        private void AssociatedObject_Initialized(object sender, EventArgs e)
        {
            ThemeManager.SetThemeName(AssociatedObject, ThemeName);
        }

        #region to_remove

        public static readonly DependencyProperty TestProperty =
            DependencyProperty.RegisterAttached("Test", typeof(string), typeof(ThemeHelper),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits, (d, e) =>
                {
                    if (d is WaitIndicator)
                        ((WaitIndicator) d).Content = e.NewValue;
                }));

        public static string GetTest(DependencyObject obj)
        {
            return (string) obj.GetValue(TestProperty);
        }

        public static void SetTest(DependencyObject obj, string value)
        {
            obj.SetValue(TestProperty, value);
        }

        #endregion
    }
}