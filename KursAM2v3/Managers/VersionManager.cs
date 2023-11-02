using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Management;
using System.Text;
using System.Threading;
using System.Windows;
using System.Xml;

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
        public string CrVersion { set; get; }
        public string SCopyType { set; get; }
        public string SCrVersion { set; get; }
    }

    public class VersionManager
    {
        public const string processName = "KursAM2v4";

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

        public int GetCanUpdate(int callType)

            // 0 - нельзя обновляться
            // 1 - можно 

            // 0 - вызов местный
            // 1 - вызов из меню система
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
                    if (ownerName != currentUserName) return 0;
                }
                catch (Exception ex)
                {
                }

            if (callType == 1)
                // проверка на наличие версии для обновлеия
            {
                var Vers = new VersionManager();
                var Ver = Vers.CheckVersion(3);

                if (Ver.Sver == Ver.Ver) return 0;
            }

            return 1;
        }


        public void KursUpdate()
        {
            var procs = Process.GetProcessesByName(processName);
            if (procs.Length > 1)
            {
                var ShowMsgResult = MessageBox.Show(
                    "В системе обнаружены работающие версии приложения. Все приложения будут автоматически закрыты." +
                    "Возможна потеря несохраненных данных. Продолжить обновление?",
                    "Запрос на обновление программы", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (ShowMsgResult == MessageBoxResult.No) return;
            }


            //            ShowMsgResult = MessageBox.Show(
            //                $" Идет обновление версии {Version.Ver} ..... ",
            //                "Процесс обновления", MessageBoxButton.OK);

#if (!DEBUG)
            var procId = Process.GetCurrentProcess().Id;
            string copytype = null;
            File.Copy($@"{Version.Serverpath}\Updater.exe", $@"{Directory.GetCurrentDirectory()}\Updater.exe", true);
            Process.Start("Updater.exe",
                "KursAM2v4.exe " + Version.Serverpath + " " + "\"" + Directory.GetCurrentDirectory() + "\"" +
                $" {0}" + " " + procId);
            //Process.GetCurrentProcess().Kill();
#endif
        }

        public Version CheckVersion(int typeOfcall)
            // 0 - вызов из StartLogin
            // 1 - вызов из меню Система
            // 2 - вызов по таймеру
            // 3 - полурекурсия
        {
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
                            case "CrVersion":
                                Version.CrVersion = node.InnerText;
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
                                case "CrVersion":
                                    Version.SCrVersion = node.InnerText;
                                    break;
                            }
                    }

                    if (typeOfcall == 3) return Version;
                    var fullVersion = $"{Version.Major}.{Version.Minor}.{Version.Ver}";
                    var sFullVersion = $"{Version.Smajor}.{Version.Sminor}.{Version.Sver}";

                    if (Version.Sver != Version.Ver)
                        if (int.Parse(Version.SCrVersion) > int.Parse(Version.Ver))
                            //проверка на критическое обновление
                        {
                            if (GetCanUpdate(0) == 1)
                            {
                                var ShowMsgResult = MessageBox.Show(
                                    $"Для вашей версии {fullVersion} выпущено критическое обновление!\n Для дальнейшей работы необходимо обновить программу.\n Выполнить обновление?",
                                    "Запрос на обновление программы", MessageBoxButton.YesNo, MessageBoxImage.Question);

                                if (ShowMsgResult == MessageBoxResult.Yes) KursUpdate();
                            }
                        }
                        else
                        {
                            if (typeOfcall == 1)
                            {
                                var ShowMsgResult = MessageBox.Show(
                                    $"Версия программы {fullVersion} отличается от новой версии {sFullVersion}. \nОбновить программу?",
                                    "Запрос на обновление программы", MessageBoxButton.YesNo, MessageBoxImage.Question);
                                if (ShowMsgResult == MessageBoxResult.Yes) KursUpdate();
                            }
                        }
                }

                return Version;
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
    }
}
