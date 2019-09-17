using System;
using System.Windows;
using Core.ViewModel.Base;

namespace KursAM2.ViewModel.Splash
{
    public abstract class SplashBaseViewModel : RSViewModelBase
    {
        private string myExtendedText;
        private Visibility myExtendedTextVisibility;
        private double myMaxProgress;
        private double myMinimum;
        private double myProgress;

        public virtual double Progress
        {
            get => myProgress;
            set
            {
                if (Math.Abs(myProgress - value) < 0.01) return;
                myProgress = value;
                RaisePropertyChanged();
                RaisePropertyChanged("Text");
                RaisePropertyChanged("ExtendedText");
            }
        }

        public virtual double MaxProgress
        {
            get => myMaxProgress;
            set
            {
                if (Math.Abs(myMaxProgress - value) < 0.01) return;
                myMaxProgress = value;
                RaisePropertyChanged();
            }
        }

        public double Minimum
        {
            get => myMinimum;
            set
            {
                if (Math.Abs(myMinimum - value) < 0.01) return;
                myMinimum = value;
                RaisePropertyChanged();
            }
        }

        // ReSharper disable once FunctionRecursiveOnAllPaths
        public virtual string Text => Text;

        public Visibility ExtendExtendedTextVisibility
        {
            get => myExtendedTextVisibility;
            set
            {
                if (myExtendedTextVisibility == value) return;
                myExtendedTextVisibility = value;
                RaisePropertyChanged();
            }
        }

        public virtual string ExtendedText
        {
            get => myExtendedText;
            set
            {
                if (myExtendedText == value) return;
                myExtendedText = value;
                RaisePropertyChanged();
                RaisePropertyChanged("Text");
            }
        }
    }
}