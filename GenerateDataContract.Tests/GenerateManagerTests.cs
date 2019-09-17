using NUnit.Framework;

namespace GenerateDataContract.Tests
{
    [TestFixture]
    public class GenerateManagerTests
    {
        [Test]
        public void LoadConfigTest()
        {
            var cfg = new GenerateManager();
            cfg.LoadConfig();

        }
    }
}