using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Data.SqlClient;
using System.Windows;
using Data;
using DevExpress.Xpf.Core;

namespace Core.WindowsManager
{
    public class ThemedWindowLayoutManager : WindowLayoutManager
    {
        public ThemedWindowLayoutManager(ThemedWindow window, string layoutName, Guid userId)
        {
            string hostName;
            var section = ConfigurationManager.GetSection("appSettings") as NameValueCollection;
#if  DEBUG
            hostName = section.Get("KursSystemDebugHost");
#else
            hostName = section.Get("KursSystemHost");
#endif
            if (window == null || string.IsNullOrWhiteSpace(layoutName) || userId == Guid.Empty)
                throw new Exception("Параметры не могут быть нулевыми");
            Window = window;
            Name = layoutName;
            UserId = userId;
            var connString = new SqlConnectionStringBuilder
            {
                DataSource = hostName,
                InitialCatalog = "KursSystem",
                UserID = "KursUser",
                Password = "KursUser"
            }.ToString();
            DBContext = new KursSystemEntities(connString);
            if (Window != null) window.SaveLayoutToStream(StartLayout);
            Window.Closing += Window_Closing;
            Window.Loaded += Window_Loaded;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Load();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Save();
        }
    }
}
