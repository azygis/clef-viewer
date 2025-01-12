using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using Serilog.Expressions;

namespace ClefViewer.API.Business;

public sealed class CustomMemberNameResolver : NameResolver
{
    internal static readonly FrozenDictionary<string, string> PropertyMap = new Dictionary<string, string>
    {
        { "Exception", "x" },
        { "Level", "l" },
        { "Message", "m" },
        { "MessageTemplate", "mt" },
        { "Properties", "p" },
        { "Timestamp", "t" }
    }.ToFrozenDictionary();

    public override bool TryResolveBuiltInPropertyName(string alias, [NotNullWhen(true)] out string? target) =>
        PropertyMap.TryGetValue(alias, out target);
}