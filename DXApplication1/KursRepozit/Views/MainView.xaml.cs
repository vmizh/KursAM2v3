using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using DevExpress.Xpf.LayoutControl;

namespace KursRepozit.Views
{
    /// <summary>
    ///     Interaction logic for View1.xaml
    /// </summary>
    public partial class MainView
    {
        public MainView()
        {
            InitializeComponent();
        }

        // ReSharper disable once UnusedType.Global
        
    }

    public class ScalablePaddingConverter : IValueConverter
    {
        public ScalablePaddingConverter()
        {
            MinPadding = 35;
        }

        public double MinPadding { get; set; }

        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // ReSharper disable once PossibleNullReferenceException
            var controlHeight = (double)value;
            var desiredContentHeight = 3 * Tile.LargeHeight + 2 * TileLayoutControl.DefaultItemSpace + 20;
            var paddingY = Math.Floor(Math.Max(0d, controlHeight - desiredContentHeight) / 2d);
            paddingY = Math.Max(MinPadding, Math.Min(paddingY, TileLayoutControl.DefaultPadding.Top));
            var relativePadding = (paddingY - MinPadding) / (TileLayoutControl.DefaultPadding.Top - MinPadding);
            var paddingX =
                Math.Floor(MinPadding + relativePadding * (TileLayoutControl.DefaultPadding.Left - MinPadding));
            return new Thickness(paddingX, paddingY, paddingX, paddingY);
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}