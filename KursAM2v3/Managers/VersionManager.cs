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

        public Version CheckVersion()
        {
            var xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load("Version.xml");

                // ReSharper disable once NotAccessedVariable
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
                                    // ReSharper disable once RedundantAssignment
                                    copytype = node.InnerText;
                                    break;
                            }
                    }
#if (!DEBUG)
                    if (Version.Sver != Version.Ver)
                    {
                        ShowMsgResult = MessageBox.Show(
                            $"Версия программы {Version.Ver} отличается от новой версии {Version.Sver}. \nОбновить программу?",
                            "Запрос на обновление программы", MessageBoxButton.YesNo);
                        if (ShowMsgResult == MessageBoxResult.Yes)
                        {
                            var procId = Process.GetCurrentProcess().Id;
                            Process.Start("Updater.exe",
                                "KursAM2v4.exe " + Version.Serverpath + " " + "\"" + Directory.GetCurrentDirectory() + "\"" +
                                $" {copytype}" + " " + procId);
                            Process.GetCurrentProcess().Kill();
                        }
                    }
#endif
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
