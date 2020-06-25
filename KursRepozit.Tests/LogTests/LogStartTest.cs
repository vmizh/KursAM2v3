using System;
using Core.Logger;
using NUnit.Framework;

namespace KursRepozit.Tests.LogTests
{
    [TestFixture]
    public class LogStartTest
    {
        [Test]
        public void LogFirstTest()
        {
            Exception ex = new Exception("75984357347");
            LoggerHelper.WriteError(ex);
            LoggerHelper.WriteMessage(LogLevelEnum.Warning,"Warning");
            LoggerHelper.WriteMessage(LogLevelEnum.Debug,"Debug`");
            LoggerHelper.WriteMessage(LogLevelEnum.None,"None");
            LoggerHelper.WriteMessage(LogLevelEnum.Error,"Error");
            LoggerHelper.WriteMessage(LogLevelEnum.Trace, "Trace");
            LoggerHelper.WriteMessage(LogLevelEnum.Fatal, "Fatal");
        }
    }
}