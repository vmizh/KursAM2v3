using System.Threading.Tasks;

namespace ServiceStack;

public interface IAsyncCommand<in T> : IAsyncCommand
{
    Task ExecuteAsync(T request);
}
