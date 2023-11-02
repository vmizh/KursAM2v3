using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace Updater
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Updater Version 4.3.0");
            if (args.Length == 0)
            {
                Console.WriteLine("Updater: для запуска используйте приложение Kurs ");
                Thread.Sleep(4000);
                return;
            }

            var processName = args[0].Replace(".exe", "");
            var currentUserName = Environment.UserName;
            var procs = Process.GetProcessesByName(processName);
            try
            {
                Console.WriteLine("Запуск системы обновления");
                foreach (var p in procs) p.Kill();
                Thread.Sleep(5000);
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
            Console.WriteLine("Обновление завершено");
            Console.WriteLine("Запуск приложения " + args[0]);
            Process.Start(programpath + "\\" + args[0]);
        }

        public static void CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target, List<string> excludeNames)
        {
            foreach (var dir in source.GetDirectories())
                CopyFilesRecursively(dir, target.CreateSubdirectory(dir.Name), excludeNames);
            foreach (var file in source.GetFiles())
            {
                if (excludeNames.Any(file.Name.Contains)) continue;
                Console.WriteLine(file.Name + " -> " + Path.Combine(target.FullName, file.Name));
                file.CopyTo(Path.Combine(target.FullName, file.Name), true);
            }
        }
    }
}