using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using DevExpress.Xpf.Core;
using Microsoft.Expression.Shapes;

namespace KursAM2.View.SplashWindow
{
    /// <summary>
    ///     Interaction logic for DebitorCreditorKontrCalcSplashView.xaml
    /// </summary>
    public partial class DebitorCreditorKontrCalcSplashView
    {
        public new static readonly DependencyProperty FontSizeProperty =
            DependencyProperty.Register("FontSize", typeof(int), typeof(DebitorCreditorKontrCalcSplashView));

        public static readonly DependencyProperty IndicatorBrushProperty =
            DependencyProperty.Register("IndicatorBrush", typeof(Brush), typeof(DebitorCreditorKontrCalcSplashView));

        public static readonly DependencyProperty BackCircleProperty =
            DependencyProperty.Register("BackCircle", typeof(Brush), typeof(DebitorCreditorKontrCalcSplashView));

        public static readonly DependencyProperty IndicatorSecondBrushProperty =
            DependencyProperty.Register("IndicatorSecondBrush", typeof(Brush),
                typeof(DebitorCreditorKontrCalcSplashView));

        public static readonly DependencyProperty ProgressLabelBrusProperty =
            DependencyProperty.Register("ProgressLabelBrus", typeof(Brush), typeof(DebitorCreditorKontrCalcSplashView));

        public static readonly DependencyProperty AnimatioOneProperty =
            DependencyProperty.Register("AnimationOne", typeof(bool), typeof(DebitorCreditorKontrCalcSplashView));

        public DebitorCreditorKontrCalcSplashView()
        {
            InitializeComponent(); 
         //   ApplicationThemeHelper.ApplicationThemeName = Theme.MetropolisLightName;
            Loaded += AnimationSecondIndicatorOne;
            ProgressLabelRepit.Opacity = 0;
        }

        public bool AnimationOne
        {
            set => SetValue(AnimatioOneProperty, value);
            get => (bool) GetValue(AnimatioOneProperty);
        }

        public string Animation
        {
            set => SetValue(FontSizeProperty, value);
            get => (string) GetValue(FontSizeProperty);
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

        private void AnimationSecondIndicatorOne(object sender, EventArgs e)
        {
            if (AnimationOne)
            {
                MainBorder.Width = 400;
                MainBorder.Height = 670;
                MainBorder.Opacity = 0.9;
                MainBorder.Fill = Brushes.White;
                ProgressLabel.Opacity = 0;
                ProgressLabelRepit.Opacity = 1;
                var animation = new DoubleAnimation();
                IndicatorSecond.EndAngle = 0;
                animation.From = 0;
                animation.To = 359;
                animation.Duration = TimeSpan.FromSeconds(1);
                animation.Completed += SecondAnimationSecondIndicatorOne;
                IndicatorSecond.BeginAnimation(Arc.StartAngleProperty, animation);
            }
        }

        private void SecondAnimationSecondIndicatorOne(object sender, EventArgs e)
        {
            var animation = new DoubleAnimation();
            IndicatorSecond.StartAngle = 360;
            animation.From = 0;
            animation.To = 359;
            animation.Duration = TimeSpan.FromSeconds(1);
            animation.Completed += AnimationSecondIndicatorOne;
            IndicatorSecond.BeginAnimation(Arc.EndAngleProperty, animation);
        }
        //public bool AnimationTwo { set; get; }

        //public bool AnimationOne { set; get; }
    }
}
