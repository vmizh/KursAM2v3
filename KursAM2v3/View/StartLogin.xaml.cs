using System;
using System.Drawing;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Core;
using DevExpress.Xpf.Editors;
using Helper;
using KursAM2.Repositories.RedisRepository;
using KursAM2.ViewModel.StartLogin;
using KursDomain.Documents.CommonReferences;
using Newtonsoft.Json;
using StackExchange.Redis;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;


// ReSharper disable InconsistentNaming
namespace KursAM2.View
{
    /// <summary>
    ///     Interaction logic for StartLogin.xaml
    /// </summary>
    public partial class StartLogin
    {
        private readonly StartLoginViewModel dtx;
        private readonly IDatabase myRedis = RedisStore.RedisCache;
        private readonly ISubscriber mySubscriber;
        public bool IsConnectSuccess;

        public StartLogin()
        {
            InitializeComponent();

            mySubscriber = myRedis.Multiplexer.GetSubscriber();
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

        private void pwdText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && ((StartLoginViewModel)DataContext).IsConnectNotExecute)
            {
                ((StartLoginViewModel)DataContext).IsConnectNotExecute = false;
                ButtonOK.Tag = "Active";
                if (mySubscriber != null && mySubscriber.IsConnected())
                {
                    var message = new RedisMessage
                    {
                        DocumentType = DocumentType.StartLogin,
                        DocCode = 0,
                        DocDate = DateTime.Now,
                        IsDocument = false,
                        OperationType = RedisMessageDocumentOperationTypeEnum.Execute,
                        Message = $"{((StartLoginViewModel)DataContext).CurrentUser}"
                    };
                    var jsonSerializerSettings = new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.All
                    };
                    var json = JsonConvert.SerializeObject(message, jsonSerializerSettings);
                    mySubscriber.Publish("StartLogin", json);
                }
            }
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
