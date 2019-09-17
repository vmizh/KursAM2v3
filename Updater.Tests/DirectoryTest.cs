using System.IO;
using Helper;
using NUnit.Framework;

namespace Updater.Tests
{
    [TestFixture]
    public class DirectoryTest
    {
        [Test]
        public void GetFilesTest()
        {
            var programpath = @"c:\Users\Vadim\WorkProject\KursAM2v3\KursAM2v3\bin\Release";
            var serverbasepath = @"\\172.16.0.1\kurs\KursAM2v3";
            var serverdir = new DirectoryInfo(serverbasepath);
            var programdir = new DirectoryInfo(programpath);
            CopyFiles.CopyFilesRecursively(serverdir, programdir);
        }
    }
}