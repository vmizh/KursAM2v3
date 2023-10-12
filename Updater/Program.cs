using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using MessageBox = System.Windows.Forms.MessageBox;

namespace Updater
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var process = args[0].Replace(".exe", "");
            Console.WriteLine($"Terminate process {args[0]}! ");
            try
            {
                var pr = Process.GetProcessById(Convert.ToInt32(args[4]));
                pr.Kill();
                Thread.Sleep(5000);
                
                var procs = Process.GetProcessesByName(process);
                if (procs.Length > 0)
                {
                    var ShowMsgResult = MessageBox.Show(
                        "Есть открытые экземпляры KursAM2v4. Закройте их вручную и продолжите обновление, " +
                        "при подтверждении они будут закрыты автоматически. Продолжить обновление?",
                        "Запрос на обновление программы", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                    if (ShowMsgResult == DialogResult.Cancel)
                        return;

                    foreach (var p in procs)
                    {
                        p.Kill();
                    }
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }
            Console.WriteLine(args[2]);
            var programpath = @"" + args[2]; //@"c:\Users\Vadim\WorkProject\KursAM2v3\KursAM2v3\bin\Release";
            Console.WriteLine(programpath);
            var serverbasepath = args[1]; //@"\\172.16.0.1\kurs\KursAM2v3";
            var serverdir = new DirectoryInfo(serverbasepath);
            var programdir = new DirectoryInfo(programpath);
            var exList = new List<string> { "Updater.exe" };
            if (args[3] == "short")
            {
                exList.Add("DevExpress");
                exList.Add(".pdb");
            }
            CopyFilesRecursively(serverdir, programdir, exList);
            Console.WriteLine("Стартую программу. Нажмите любую клавишу " + args[0]);
            Process.Start(programpath + "\\" + args[0]);
        }

        public static void CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target, List<string> excludeNames)
        {
            foreach (DirectoryInfo dir in source.GetDirectories())
                CopyFilesRecursively(dir, target.CreateSubdirectory(dir.Name), excludeNames);
            foreach (FileInfo file in source.GetFiles())
            {
                if (excludeNames.Any(file.Name.Contains)) continue;
                Console.WriteLine(file.Name + " -> " + Path.Combine(target.FullName, file.Name));
                file.CopyTo(Path.Combine(target.FullName, file.Name), true);
            }
        }
    }
}
