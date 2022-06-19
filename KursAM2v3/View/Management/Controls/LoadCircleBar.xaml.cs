using System;
using System.Windows;
using System.Windows.Media;
using DevExpress.Xpf.Core;
using static System.Math;

namespace KursAM2.View.Management.Controls
{
    /// <summary>
    ///     Interaction logic for LoadCircleBar.xaml
    /// </summary>
    public partial class LoadCircleBar
    {
        //public static readonly DependencyProperty AnimatioTwoProperty =
        //    DependencyProperty.Register("AnimationTwo", typeof(bool), typeof(LoadCircleBar));

        //public static readonly DependencyProperty AnimatioOneProperty =
        //    DependencyProperty.Register("AnimationOne", typeof(bool), typeof(LoadCircleBar));

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double), typeof(LoadCircleBar),
                new UIPropertyMetadata(CurrentNumberChanged), ValidateCurrentNumber);

        public new static readonly DependencyProperty FontSizeProperty =
            DependencyProperty.Register("FontSize", typeof(int), typeof(LoadCircleBar));

        public static readonly DependencyProperty IndicatorBrushProperty =
            DependencyProperty.Register("IndicatorBrush", typeof(Brush), typeof(LoadCircleBar));

        public static readonly DependencyProperty BackCircleProperty =
            DependencyProperty.Register("BackCircle", typeof(Brush), typeof(LoadCircleBar));

        public static readonly DependencyProperty IndicatorSecondBrushProperty =
            DependencyProperty.Register("IndicatorSecondBrush", typeof(Brush), typeof(LoadCircleBar));

        public static readonly DependencyProperty ProgressLabelBrusProperty =
            DependencyProperty.Register("ProgressLabelBrus", typeof(Brush), typeof(LoadCircleBar));

        public LoadCircleBar()
        {
            InitializeComponent(); 
            ApplicationThemeHelper.ApplicationThemeName = Theme.MetropolisLightName;
        }

        public new int FontSize
        {
            set => SetValue(FontSizeProperty, value);
            get => (int) GetValue(FontSizeProperty);
        }

        public Brush IndicatorBrush
        {
            set => SetValue(IndicatorBrushProperty, value);
            get => (Brush) GetValue(IndicatorBrushProperty);
        }

        public Brush BackCircle
        {
            set => SetValue(BackCircleProperty, value);
            get => (Brush) GetValue(BackCircleProperty);
        }

        public Brush IndicatorSecondBrush
        {
            set => SetValue(IndicatorSecondBrushProperty, value);
            get => (Brush) GetValue(IndicatorSecondBrushProperty);
        }

        public Brush ProgressLabelBrus
        {
            set => SetValue(ProgressLabelBrusProperty, value);
            get => (Brush) GetValue(ProgressLabelBrusProperty);
        }

        //public bool AnimationOne { set; get; }
        //public bool AnimationTwo { set; get; }

        public double Value
        {
            set => SetValue(ValueProperty, value);
            get => (double) GetValue(ValueProperty);
        }

        public static bool ValidateCurrentNumber(object value)
        {
            if (Convert.ToDouble(value) >= 0.0 && Convert.ToDouble(value) <= 100.00)
                return true;
            return false;
        }

        private static void CurrentNumberChanged(DependencyObject depObj,
            DependencyPropertyChangedEventArgs args)
        {
            var s = (LoadCircleBar) depObj;
            var theValuel = s.ProgressLabel;
            theValuel.Content = Floor((double) args.NewValue + 0.05) + "%";
            var theValue2 = s.Indicator;
            theValue2.EndAngle = (double) args.NewValue * 0.01 * 360;
        }
        //    IndicatorSecond.StartAngle = 0;
        //{

        //private void SecondAnimationSecondIndicatorOne(object sender, EventArgs e)
        //}
        //    }
        //        IndicatorSecond.BeginAnimation(Arc.StartAngleProperty, animation);
        //        animation.Completed += SecondAnimationSecondIndicatorOne;
        //        animation.Duration = TimeSpan.FromSeconds(1);
        //        animation.From = 0;
        //        animation.To = 359;
        //        var animation = new DoubleAnimation();
        //        IndicatorSecond.EndAngle = 0;
        //    {
        //    if (AnimationOne)
        //{

        //    var animation = new DoubleAnimation();

        //private void AnimationSecondIndicatorOne(object sender, EventArgs e)
        //    animation.From = 0;
        //    animation.To = 359;
        //    animation.Duration = TimeSpan.FromSeconds(1);
        //    animation.Completed += AnimationSecondIndicatorOne;
        //    IndicatorSecond.BeginAnimation(Arc.EndAngleProperty, animation);
        //}

        //private void SecondAnimationSecondIndicatorTwo(object sender, EventArgs e)
        //{
        //    {
        //        var animation = new DoubleAnimation();
        //        animation.From = IndicatorSecond.StartAngle;
        //        animation.To = IndicatorSecond.StartAngle + 50;
        //        animation.Duration = TimeSpan.FromSeconds(0.3);
        //        animation.Completed += AnimationSecondIndicatorTwo;
        //        IndicatorSecond.BeginAnimation(Arc.StartAngleProperty, animation);
        //    }
        //}

        //private void AnimationSecondIndicatorTwo(object sender, EventArgs e)
        //{
        //    if (AnimationTwo)
        //    {
        //        var animation = new DoubleAnimation();
        //        animation.From = IndicatorSecond.EndAngle;
        //        animation.To = IndicatorSecond.EndAngle + 50;
        //        animation.Duration = TimeSpan.FromSeconds(0.3);
        //        animation.Completed += SecondAnimationSecondIndicatorTwo;
        //        IndicatorSecond.BeginAnimation(Arc.EndAngleProperty, animation);
        //    }
        //}
    }
}
