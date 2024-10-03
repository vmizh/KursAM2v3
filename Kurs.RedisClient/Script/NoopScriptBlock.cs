using System.Threading;
using System.Threading.Tasks;

namespace ServiceStack.Script;

public class NoopScriptBlock : ScriptBlock
{
    public override string Name => "noop";
    public override ScriptLanguage Body => ScriptVerbatim.Language;

    public override Task WriteAsync(ScriptScopeContext scope, PageBlockFragment block, CancellationToken token) => 
        TypeConstants.EmptyTask;
}
