using System;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Core;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Editors;
using Helper;
using KursAM2.ViewModel.StartLogin;
using Microsoft.Win32;
using Brush = System.Windows.Media.Brush;
using Image = System.Drawing.Image;

// ReSharper disable InconsistentNaming
namespace KursAM2.View
{
    /// <summary>
    ///     Interaction logic for StartLogin.xaml
    /// </summary>
    
    public partial class StartLogin
    {
        private Brush buttonOkColor;
        private readonly StartLoginViewModel dtx;
        public bool IsConnectSuccess;

        public StartLogin()
        {
            InitializeComponent();

            buttonOkColor = ButtonNewOk.Background;
            DataContext = new StartLoginViewModel(this);
            pwdText.Focus();
            dtx = (StartLoginViewModel)DataContext;
        }

        private void MenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            //Не переносить в ViewModel
            var form = new OpenFileDialog
            {
                Filter = @"Картинки(*.JPG;*.PNG)|*.JPG;*.PNG",
                CheckFileExists = true,
                Multiselect = false
            };
            form.ShowDialog();
            if (!string.IsNullOrEmpty(form.FileName))
            {
                var source = Image.FromFile(form.FileName);
                if (source.Height != source.Width)
                {
                    var cropSource = (Bitmap)source.Crop(new Rectangle(source.Width / 2 - source.Height / 2,
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

        private void dataSources_EditValueChanged(object sender, EditValueChangedEventArgs e)
        {
            if (dtx != null)
                dtx.SelectedDataSource = e.NewValue as DataSource;
        }

        private void ButtonNewOk_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ((Button)sender).Background= (SolidColorBrush)new BrushConverter().ConvertFrom("#93b3bd");
        }

        private void ButtonNewOk_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ((Button)sender).Background= buttonOkColor;
        }

        private void ButtonNewOk_Click(object sender, RoutedEventArgs e)
        {
            ((Button)sender).Background= (SolidColorBrush)new BrushConverter().ConvertFrom("#9ae4ff");
        }

        private void pwdText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ButtonNewOk.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#9ae4ff");
            }
        }
    }
}