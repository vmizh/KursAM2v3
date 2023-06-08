using System;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Core;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Editors;
using Helper;
using KursAM2.ViewModel.StartLogin;
using MessageBox = System.Windows.MessageBox;

// ReSharper disable InconsistentNaming
namespace KursAM2.View
{
    /// <summary>
    ///     Interaction logic for StartLogin.xaml
    /// </summary>
    public partial class StartLogin
    {
        private readonly StartLoginViewModel dtx;
        public bool IsConnectSuccess;

        public StartLogin()
        {
            InitializeComponent(); ApplicationThemeHelper.ApplicationThemeName = Theme.MetropolisLightName;
            DataContext = new StartLoginViewModel(this);
            pwdText.Focus();
            dtx = (StartLoginViewModel) DataContext;
        }

        private void MenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            //Не переносить в ViewModel
            using (var form = new OpenFileDialog())
            {
                form.Filter = @"Картинки(*.JPG;*.PNG)|*.JPG;*.PNG";
                form.CheckFileExists = true;
                form.Multiselect = false;
                form.ShowDialog();
                if (!string.IsNullOrEmpty(form.FileName))
                {
                    var source = Image.FromFile(form.FileName);
                    if (source.Height != source.Width)
                    {
                        var cropSource = (Bitmap) source.Crop(new Rectangle(source.Width / 2 - source.Height / 2,
                            0, source.Height, source.Height));
                        var b =
                            Imaging.CreateBitmapSourceFromHBitmap(
                                cropSource.GetHbitmap(),
                                IntPtr.Zero,
                                Int32Rect.Empty,
                                BitmapSizeOptions.FromEmptyOptions());
                        AvatarObj.Source = b;
                    }
                    else
                    {
                        AvatarObj.Source = new BitmapImage(new Uri(form.FileName, UriKind.RelativeOrAbsolute));
                    }
                }
            }
        }

        private void dataSources_EditValueChanged(object sender, EditValueChangedEventArgs e)
        {
            if( dtx != null)
                dtx.SelectedDataSource = e.NewValue as DataSource;
        }
    }
}
