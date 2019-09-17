using System.Text;

namespace KursAM2.ViewModel.Splash
{
    public class DebitorCreditorCalcKontrSplashViewModel : SplashBaseViewModel
    {
        private string myLabelState;

        public override string Text
        {
            get
            {
                var sb = new StringBuilder();
                sb.Append($"\tРассчитываю ... {Progress + 1} из {MaxProgress}");
                if (!string.IsNullOrWhiteSpace(ExtendedText))
                    sb.Append("\n\t" + ExtendedText);
                return sb.ToString();
            }
        }

        public override double Progress
        {
            get
            {
                RaisePropertyChanged(nameof(LabelState));
                return base.Progress;
            }
        }

        public virtual string LabelState
        {
            // ReSharper disable once ValueParameterNotUsed
            set
            {
                myLabelState = (int) Progress + "%";
                RaisePropertyChanged();
            }
            get => myLabelState;
        }

        //private int myWidth;
        //public virtual int Width
        //{
        //    set
        //    {
        //        if( myWidth == value) return;
        //        RaisePropertyChanged();
        //    }
        //    get => myWidth;
        //}
    }
}