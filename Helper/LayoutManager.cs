using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Windows;
using System.Xml;
using Data;
using DevExpress.Mvvm;
using DevExpress.Xpf.Core;
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
        [DataMember] public double FormTop { set; get; }

        [DataMember] public double FormLeft { set; get; }

        [DataMember] public double FormHeight { set; get; }

        [DataMember] public double FormWidth { set; get; }

        [DataMember] public WindowStartupLocation FormStartLocation { set; get; }

        [DataMember] public WindowState FormState { set; get; }
    }

    public class LayoutManager
    {
        public const int Version = 1;
        private readonly KursSystemEntities context;
        public readonly string ControlName;
        public readonly string FormName;

        private readonly ILayoutSerializationService layoutService;
        private readonly MemoryStream StartLayoutStream = new MemoryStream();
        private readonly Window window;

        public LayoutManager(KursSystemEntities systemDBContext, ThemedWindow form, string formName)
        {
            context = systemDBContext;
            FormName = formName;
            window = form;
            form.SaveLayoutToStream(StartLayoutStream);
        }

        public LayoutManager(KursSystemEntities systemDBContext, ILayoutSerializationService service, string formName,
            string ctrlName)
        {
            context = systemDBContext;
            layoutService = service;
            FormName = formName;
            ControlName = ctrlName;
            StartLayout = layoutService.Serialize();
        }

        public LayoutManager([NotNull] Window win, ILayoutSerializationService service, string formName,
            string ctrlName,
            KursSystemEntities systemDBContext)
        {
            context = systemDBContext;
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

        public string StartLayout { set; get; }
        public WindowsScreenState StartWinState { set; get; }

        public void Save()
        {
            if (layoutService == null) return;

            if (CurrentUser.UserInfo == null) return;
            using (var tran = context.Database.BeginTransaction())
            {


                var l = context.FormLayout
                    .Include(_ => _.Users)
                    .Include(_ => _.KursMenuItem)
                    .FirstOrDefault(_ => _.UserId == CurrentUser.UserInfo.KursId
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
                        context.FormLayout.Add(new FormLayout
                        {
                            Id = Guid.NewGuid(),
                            UpdateDate = DateTime.Now,
                            UserId = CurrentUser.UserInfo.KursId,
                            FormName = FormName,
                            ControlName = FormName,
                            Layout = s,
                            WindowState = sb?.ToString()
                        });
                    }
                    else
                    {
                        l.UpdateDate = DateTime.Now;
                        //l.UserId = CurrentUser.UserInfo.KursId;
                        //l.FormName = FormName;
                        //l.ControlName = FormName;
                        l.Layout = s;
                        l.WindowState = sb?.ToString();
                    }

                    context.SaveChanges();
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    MessageBox.Show($"Ошибка сохранения разметки {FormName} / {ControlName}" + $"{ex}");
                }
            }
        }

        public bool IsLayoutExists()
        {
            if (CurrentUser.UserInfo == null) return false;
            return context.FormLayout.Any(_ => _.UserId == CurrentUser.UserInfo.KursId
                                               && _.FormName == FormName);
        }

        public void Load()
        {
            if (CurrentUser.UserInfo == null) return;
            try
            {
                var l = context.FormLayout.FirstOrDefault(_ => _.UserId == CurrentUser.UserInfo.KursId
                                                               && _.FormName == FormName
                                                               && _.ControlName == FormName);
                if (l == null) return;
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
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки разметки {FormName} " + ex.Message);
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
