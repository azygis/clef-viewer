using System.Diagnostics.CodeAnalysis;
using Serilog.Expressions;

namespace ClefViewer.API.Business;

public sealed class CustomMemberNameResolver : NameResolver
{
    public override bool TryResolveBuiltInPropertyName(string alias, [NotNullWhen(true)] out string? target)
    {
        target = alias switch
        {
            "Exception" => "x",
            "Level" => "l",
            "Message" => "m",
            "MessageTemplate" => "mt",
            "Properties" => "p",
            "Timestamp" => "t",
            _ => null
        };
        return target is not null;
    }
}