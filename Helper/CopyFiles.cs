using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Helper
{
    public class CopyFiles
    {
        public static void CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target)
        {
            foreach (DirectoryInfo dir in source.GetDirectories())
                CopyFilesRecursively(dir, target.CreateSubdirectory(dir.Name));
            foreach (FileInfo file in source.GetFiles())
            {
                Console.ReadKey();
                Console.WriteLine(file.Name);
                file.CopyTo(Path.Combine(target.FullName, file.Name), true);
            }
        }

        //private static bool StringContainsStrings([NotNull] string name,  List<string> excludeNames)
        //{
        //    return excludeNames.Any(name.Contains);
        //}

        public static void CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target, List<string>excludeNames)
        {
            foreach (DirectoryInfo dir in source.GetDirectories())
                CopyFilesRecursively(dir, target.CreateSubdirectory(dir.Name), excludeNames);
            foreach (FileInfo file in source.GetFiles())
            {
                if (!excludeNames.Any(file.Name.Contains))
                {
                    Console.WriteLine(file.Name + " -> " + Path.Combine(target.FullName, file.Name));
                    file.CopyTo(Path.Combine(target.FullName, file.Name), true);
                }
            }
        }
    }
}