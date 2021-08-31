using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Windows;
using System.Xml;
using Data;
using DevExpress.Mvvm;
using JetBrains.Annotations;

namespace Helper
{
    public interface IKursLayoutManager
    {
        LayoutManager LayoutManager { set; get; }
    }

    [DataContract]
    public class WindowsScreenState
    {
        [DataMember]
        public double FormTop { set; get; }

        [DataMember]
        public double FormLeft { set; get; }

        [DataMember]
        public double FormHeight { set; get; }

        [DataMember]
        public double FormWidth { set; get; }

        [DataMember]
        public WindowStartupLocation FormStartLocation { set; get; }

        [DataMember]
        public WindowState FormState { set; get; }
    }

    public class LayoutManager
    {
        public readonly string FormName;
        public readonly string ControlName;
        private readonly Window window;

        private readonly ILayoutSerializationService layoutService;
        public LayoutManager(ILayoutSerializationService service, string formName, string ctrlName)
        {
            layoutService = service;
            FormName = formName;
            ControlName = ctrlName;
            //window = win;
            //StartWinState = new WindowsScreenState
            //{
            //    FormHeight = window.Height,
            //    FormWidth = window.Width,
            //    FormLeft = window.WindowState == WindowState.Maximized ? 0 : window.Left,
            //    FormTop = window.WindowState == WindowState.Maximized ? 0 : window.Top,
            //    FormStartLocation = window.WindowStartupLocation,
            //    FormState = window.WindowState
            //};
            StartLayout = layoutService.Serialize();
        }
        public LayoutManager([NotNull]Window win,ILayoutSerializationService service, string formName, string ctrlName)
        {
            layoutService = service;
            FormName = formName;
            ControlName = ctrlName;
            window = win;
            StartWinState = new WindowsScreenState
            {
                FormHeight = window.Height,
                FormWidth = window.Width,
                FormLeft = window.WindowState == WindowState.Maximized ? 0 : window.Left,
                FormTop = window.WindowState == WindowState.Maximized ? 0 : window.Top,
                FormStartLocation = window.WindowStartupLocation,
                FormState = window.WindowState
            };
            StartLayout = layoutService.Serialize();
        }

        public string StartLayout { set; get; } = null;
        public WindowsScreenState StartWinState { set; get; }
        public void Save()
        {
            var connString = new SqlConnectionStringBuilder
            {
                DataSource = "172.16.0.1",
                InitialCatalog = "KursSystem",
                UserID = "sa",
                Password = "CbvrfFhntvrf65"
            }.ToString();
            using (var ctx = new KursSystemEntities(connString))
            {
                if (CurrentUser.UserInfo == null) return;
                var l = ctx.FormLayout.FirstOrDefault(_ => _.UserId == CurrentUser.UserInfo.KursId
                                                           && _.FormName == FormName
                                                           && _.ControlName == FormName);
                try
                {
                    StringBuilder sb = null;
                    if (window != null)
                    {
                        sb = new StringBuilder();
                        var winState = new WindowsScreenState
                        {
                            FormHeight = window.Height,
                            FormWidth = window.Width,
                            FormLeft = window.WindowState == WindowState.Maximized ? 0 : window.Left,
                            FormTop = window.WindowState == WindowState.Maximized ? 0 : window.Top,
                            FormStartLocation = window.WindowStartupLocation,
                            FormState = window.WindowState
                        };
                        var ser1 =
                            new DataContractSerializer(typeof(WindowsScreenState));
                        using (var writer = XmlWriter.Create(sb))
                        {
                            ser1.WriteObject(writer, winState);
                            writer.Flush();
                        }
                    }

                    var s = layoutService.Serialize();
                    if (l == null)
                    {
                        ctx.FormLayout.Add(new FormLayout
                        {
                            Id = Guid.NewGuid(),
                            UpdateDate = DateTime.Now,
                            UserId = CurrentUser.UserInfo.KursId,
                            FormName = FormName,
                            ControlName = FormName,
                            Layout = layoutService.Serialize(),
                            WindowState = sb?.ToString()
                        });
                    }
                    else
                    {
                        l.UpdateDate = DateTime.Now;
                        l.UserId = CurrentUser.UserInfo.KursId;
                        l.FormName = FormName;
                        l.ControlName = FormName;
                        l.Layout = layoutService.Serialize();
                        l.WindowState = sb?.ToString();
                    }

                    ctx.SaveChanges();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка сохранения разметки {FormName} / {ControlName}" + $"{ex}");
                }
            }
        }

        public void Load()
        {
            if (CurrentUser.UserInfo == null) return;
            var connString = new SqlConnectionStringBuilder
            {
                DataSource = "172.16.0.1",
                InitialCatalog = "KursSystem",
                UserID = "sa",
                Password = "CbvrfFhntvrf65"
            }.ToString();
            using (var ctx = new KursSystemEntities(connString))
            {
                try
                {
                    var l = ctx.FormLayout.FirstOrDefault(_ => _.UserId == CurrentUser.UserInfo.KursId
                                                               && _.FormName == FormName
                                                               && _.ControlName == FormName);
                    if (l != null)
                    {
                        var d = new DataContractSerializer(typeof(WindowsScreenState));
                        if (l.WindowState != null && window != null &&
                            d.ReadObject(XmlReader.Create(new StringReader(l.WindowState))) is WindowsScreenState p)
                        {
                            window.WindowStartupLocation = p.FormStartLocation;
                            window.WindowState = p.FormState;
                            window.Height = p.FormHeight;
                            window.Width = p.FormWidth;
                            window.Left = p.FormLeft < 0 ? 0 : StartWinState.FormLeft;
                            window.Top = p.FormTop < 0 ? 0 : StartWinState.FormTop;
                        }
                        layoutService.Deserialize(l.Layout);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки разметки {FormName} " + ex.Message);
                    //layoutService.Deserialize(null);
                }
            }
            
        }

        public void ResetLayout()
        {
            if (window != null)
            {
                window.WindowStartupLocation = StartWinState.FormStartLocation;
                window.WindowState = StartWinState.FormState;
                window.Height = StartWinState.FormHeight;
                window.Width = StartWinState.FormWidth;
                window.Left = StartWinState.FormLeft < 0 ? 0 : StartWinState.FormLeft;
                window.Top = StartWinState.FormTop < 0 ? 0 : StartWinState.FormTop;
            }

            layoutService.Deserialize(StartLayout);
        }
    }
}