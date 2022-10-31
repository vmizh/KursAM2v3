using KursDomain.Tests.Fakes;
using System.Linq;
using Xunit;

namespace KursDomain.Tests;

public class KursConnetcTests
{
    [Fact]
    public void ConnectToDB()
    {
        var context = new LocalKursDBContext();
        var data = context.Context.SD_301.ToList();
        Assert.Equal(7, data.Count);
    }
}
