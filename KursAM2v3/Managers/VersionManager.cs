using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Management;
using System.Text;
using System.Threading;
using System.Windows;
using System.Xml;
using Core.WindowsManager;
using DevExpress.Skins;
using DevExpress.XtraSpreadsheet.DocumentFormats.Xlsb;
using KursAM2.View;
using KursAM2.ViewModel;

//using MessageBox = System.Windows.MessageBox;

namespace KursAM2.Managers
{
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class Version
    {
        public string Major { set; get; }
        public string Minor { set; get; }
        public string Ver { set; get; }
        public string Serverpath { set; get; }
        public string Smajor { set; get; }
        public string Sminor { set; get; }
        public string Sver { set; get; }

        public string CopyType { set; get; }

        // public string CrVersion { set; get; }
        public string SCopyType { set; get; }
        public int UpdateStatus { set; get; }
        public string fullVersion { set; get; }
        public string sfullVersion { set; get; }
    }

    public class VersionManager
    {
        public const string processName = "KursAM2v4";
        private readonly MainWindowViewModel _windowsViewModel;
        private readonly WindowManager winManager = new WindowManager();

        public VersionManager(MainWindowViewModel model)
        {
            _windowsViewModel = model;
        }

//        public MessageBoxResult ShowMsgResult;
        private Version Version { get; } = new();

        public void RunShowMsg()
        {
            new Thread(() =>
            {
                // запустить отдельный поток
//                ShowMsg(Version);
            }).Start();
        }

        public bool GetCanUpdate()

            // false - нельзя обновляться
            // true - можно 
        {
            string GetProcessOwner(int processId)
            {
                var query = "Select * From Win32_Process Where ProcessID = " + processId;
                var searcher = new ManagementObjectSearcher(query);
                var processList = searcher.Get();
                foreach (ManagementObject obj in processList)
                {
                    string[] argList = { string.Empty, string.Empty };
                    var returnVal = Convert.ToInt32(obj.InvokeMethod("GetOwner", argList));
                    if (returnVal == 0)
                        return argList[0];
                }

                return "NO OWNER";
            }

            // проверка наналичие других пользователей в системе
            var processes = Process.GetProcessesByName(processName);
            var currentUserName = Environment.UserName;

            foreach (var process in processes)
                try
                {
                    var processIId = process.Id;
                    var ownerName = GetProcessOwner(processIId);
                    if (ownerName != currentUserName) return false;
                }
                catch (Exception ex)
                {
                }

            return true;
        }


        public void KursUpdate()
        {
            VersionManager vers = new(_windowsViewModel);

            var verCheck = vers.CheckVersion();

            switch (verCheck.UpdateStatus)
            {
                case 0:
                     var msgDialog = @"Установлена актуальная версия программы " + verCheck.fullVersion + ".\nОбновления не требуется.";
                    // winManager.ShowMessageBox(msgDialog, "Обновление версии", MessageBoxButton.OK,
                    //     MessageBoxImage.Information);
                    winManager.ShowKursDialog(msgDialog,"Обновление версии", null, WindowManager.Confirm);


                    return;
                case 1:
                    var ShowMsgResult = MessageBox.Show(
                        $"Версия программы {verCheck.fullVersion} отличается от новой версии {verCheck.sfullVersion}.\nОбновить программу?",
                        "Запрос на обновление программы", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (ShowMsgResult == MessageBoxResult.No) return;
                    break;
                case 2:
#if (!DEBUG)
                    if (_windowsViewModel != null && _windowsViewModel.Form != null)
                        ((MainWindow)_windowsViewModel.Form).VersionUpdateTimer.Stop();
#endif
                    var showMsgResult = MessageBox.Show(
                        $"Для вашей версии {verCheck.fullVersion} выпущено критическое обновление версии {verCheck.sfullVersion}.\n" +
                        "Для дальнейшей работы необходимо обновить программу.\n Выполнить обновление?",
                        "Запрос на обновление программы", MessageBoxButton.YesNo, MessageBoxImage.Warning);
#if (!DEBUG)
                    if (_windowsViewModel != null && _windowsViewModel.Form != null)
                        ((MainWindow)_windowsViewModel.Form).VersionUpdateTimer.Start();
#endif
                    if (showMsgResult == MessageBoxResult.No) return;
                    break;
            }

            var userCopyCheck = vers.CheckUserAppCopy();
            if (userCopyCheck)
            {
#if (!DEBUG)
                if (_windowsViewModel != null && _windowsViewModel.Form != null)
                    ((MainWindow)_windowsViewModel.Form).VersionUpdateTimer.Stop();
#endif
                var ShowMsgResult = MessageBox.Show(
                    "В системе обнаружены работающие версии приложения. Все приложения будут автоматически закрыты." +
                    "Возможна потеря несохраненных данных. Продолжить обновление?",
                    "Запрос на обновление программы", MessageBoxButton.YesNo, MessageBoxImage.Warning);
#if (!DEBUG)
                if (_windowsViewModel != null && _windowsViewModel.Form != null)
                    ((MainWindow)_windowsViewModel.Form).VersionUpdateTimer.Start();
#endif
                if (ShowMsgResult == MessageBoxResult.No) return;
            }

            var ver = vers.GetCanUpdate();
            if (_windowsViewModel != null) _windowsViewModel.IsVersionUpdateStatus = ver;
            if (ver == false)
            {
                var showMsgResult = MessageBox.Show(
                    "Внимание! Обновления в системе приостановлены.Зарегестрированы работающие пользователи!\n" +
                    "Повторите попытку позже или обратитесь к администратору.",
                    "Запрос на обновление программы", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }


            //            ShowMsgResult = MessageBox.Show(
            //                $" Идет обновление версии {Version.Ver} ..... ",
            //                "Процесс обновления", MessageBoxButton.OK);

#if (!DEBUG)
            var procId = Process.GetCurrentProcess().Id;
            string copytype = null;
            File.Copy($@"{verCheck.Serverpath}\Updater.exe", $@"{Directory.GetCurrentDirectory()}\Updater.exe", true);
            Process.Start("Updater.exe",
                "KursAM2v4.exe " + verCheck.Serverpath + " " + "\"" + Directory.GetCurrentDirectory() + "\"" +
                $" {0}" + " " + procId);
            //Process.GetCurrentProcess().Kill();
#endif
        }

        public Version CheckVersion()
            // 0 - нет обновлений
            // 1 - простое
            // 2 - критическое 
        {
            Version.UpdateStatus = 0;
            var xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load("Version.xml");
                if (xmlDoc.DocumentElement != null)
                {
                    var nodeList = xmlDoc.DocumentElement.ChildNodes;
                    foreach (XmlNode node in nodeList)
                        switch (node.Name)
                        {
                            case "Major":
                                Version.Major = node.InnerText;
                                break;
                            case "Minor":
                                Version.Minor = node.InnerText;
                                break;
                            case "Version":
                                Version.Ver = node.InnerText;
                                break;
                            case "ServerPath":
                                Version.Serverpath = node.InnerText;
                                break;
                            case "CopyType":
                                Version.CopyType = node.InnerText;
                                break;
                        }
                }

                if (Directory.Exists(Version.Serverpath))
                {
                    var xmlDocServer = new XmlDocument();
                    xmlDocServer.Load(Version.Serverpath + "\\" + "Version.xml");
                    if (xmlDocServer.DocumentElement != null)
                    {
                        var nodeList = xmlDocServer.DocumentElement.ChildNodes;
                        foreach (XmlNode node in nodeList)
                            switch (node.Name)
                            {
                                case "Major":
                                    Version.Smajor = node.InnerText;
                                    break;
                                case "Minor":
                                    Version.Sminor = node.InnerText;
                                    break;
                                case "Version":
                                    Version.Sver = node.InnerText;
                                    break;
                                case "CopyType":
                                    Version.SCopyType = node.InnerText;
                                    break;
                            }
                    }

                    Version.fullVersion = $"{Version.Major}.{Version.Minor}.{Version.Ver}";
                    Version.sfullVersion = $"{Version.Smajor}.{Version.Sminor}.{Version.Sver}";

                    while (true)
                    {
                        if (int.Parse(Version.Smajor) < int.Parse(Version.Major)) break;
                        if (int.Parse(Version.Smajor) > int.Parse(Version.Major))
                        {
                            Version.UpdateStatus = 2;
                            break;
                        }

                        if (int.Parse(Version.Sminor) < int.Parse(Version.Minor)) break;
                        if (int.Parse(Version.Sminor) > int.Parse(Version.Minor)) //проверка на критическое обновление
                        {
                            Version.UpdateStatus = 2;
                            break;
                        }

                        if (int.Parse(Version.Sver) > int.Parse(Version.Ver)) Version.UpdateStatus = 1;
                        break;
                    }

                    if (Version.UpdateStatus == 0)
                    {
                        if (_windowsViewModel != null) _windowsViewModel.IsVersionUpdateStatus = false;
                    }
                    else
                    {
                        if (_windowsViewModel != null) _windowsViewModel.IsVersionUpdateStatus = true;
                    }

                    return Version;
                }
            }
            catch
                (Exception ex)
            {
                var errText = new StringBuilder(ex.Message);
                while (ex.InnerException != null) errText.Append($"\n {ex.InnerException.Message}");

                MessageBox.Show("VersionControl error.\n" + errText);
                //WindowManager.ShowError(ex);
            }

            return null;
        }

        public bool CheckUserAppCopy()
            // определяет если еще копии программы в памяти запущенные текущим пользователем
            // false - нет копий приложения пользователя
            // true - есть копии 
        {
            string GetProcessOwner(int processId)
            {
                var query = "Select * From Win32_Process Where ProcessID = " + processId;
                var searcher = new ManagementObjectSearcher(query);
                var processList = searcher.Get();
                foreach (ManagementObject obj in processList)
                {
                    string[] argList = { string.Empty, string.Empty };
                    var returnVal = Convert.ToInt32(obj.InvokeMethod("GetOwner", argList));
                    if (returnVal == 0)
                        return argList[0];
                }

                return "NO OWNER";
            }

            var processCount = 0;

            // проверка наналичие нескольких копий приложения для текущего пользователя
            var processes = Process.GetProcessesByName(processName);
            var currentUserName = Environment.UserName;

            foreach (var process in processes)
                try
                {
                    var processIId = process.Id;
                    var ownerName = GetProcessOwner(processIId);
                    if (ownerName == currentUserName) processCount++;
                }
                catch (Exception ex)
                {
                }

            if (processCount > 1) return true;
            return false;
        }
    }
}