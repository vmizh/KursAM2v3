using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Xml;

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
        public MessageBoxResult ShowMsgResult;
        private Version Version { get; } = new Version();

        public void RunShowMsg()
        {
            new Thread(() =>
            {
                // запустить отдельный поток
                ShowMsg(Version);
            }).Start();
        }

        public void ShowMsg(Version vers)
        {
            ShowMsgResult = MessageBox.Show(
                $"Версия программы {vers.Ver} отличается от новой версии {vers.Sver}. \nОбновить программу?",
                "Запрос на обновление программы", MessageBoxButton.YesNo);
        }

        public void KursUpdate()
        {
//            ShowMsgResult = MessageBox.Show(
//                $" Идет обновление версии {Version.Ver} ..... ",
//                "Процесс обновления", MessageBoxButton.OK);

#if (!DEBUG)
                            var procId = Process.GetCurrentProcess().Id;
                            string copytype = null ;
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
        {
            string FullVersion;
            string SFullVersion;
            var xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load("Version.xml");

                 string copytype = null; 

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

                    FullVersion = $"{Version.Major}.{Version.Minor}.{Version.Ver}";
                    SFullVersion = $"{Version.Smajor}.{Version.Sminor}.{Version.Sver}";

                    if (Version.Sver != Version.Ver)
                    {
                        if (int.Parse(Version.SCrVersion) > int.Parse(Version.Ver))
                            //проверка на критическое обновление
                        {
                            ShowMsgResult = MessageBox.Show(
                                $"Для вашей версии {FullVersion} выпущено критическое обновление!\n Для дальнейшей работы необходимо обновить программу.\n Выполнить обновление?",
                                "Запрос на обновление программы", MessageBoxButton.YesNo);

                            if (ShowMsgResult == MessageBoxResult.Yes) KursUpdate();
                        }
                        else
                        {
                            if (typeOfcall == 1)
                            {
                                ShowMsgResult = MessageBox.Show(
                                    $"Версия программы {FullVersion} отличается от новой версии {SFullVersion}. \nОбновить программу?",
                                    "Запрос на обновление программы", MessageBoxButton.YesNo);
                                if (ShowMsgResult == MessageBoxResult.Yes) KursUpdate();
                            }
                        }
                    }
                    else
                    {
                        if (typeOfcall == 1)
                            ShowMsgResult = MessageBox.Show(
                                $"Версия программы {FullVersion} является актуальной. Обновления не требуется ",
                                "Обновление программы", MessageBoxButton.OK);
                    }
                }

                return Version;
            }
            catch (Exception ex)
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
