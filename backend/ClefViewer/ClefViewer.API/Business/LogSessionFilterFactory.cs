using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using ClefViewer.API.Data;
using Serilog.Events;
using Serilog.Expressions;

namespace ClefViewer.API.Business;

public delegate bool EventFilterFunc(LogEvent logEvent);

public interface ILogSessionFilterFactory
{
    bool TryCreateFilter(string? expression, EventFilter[]? filters, [NotNullWhen(true)] out EventFilterFunc? filter);
}

public sealed class LogSessionFilterFactory(NameResolver nameResolver) : ILogSessionFilterFactory
{
    private static readonly FrozenSet<char> ExpressionOperators = "@()+=*<>%-".ToCharArray().ToFrozenSet();

    public bool TryCreateFilter(string? expression, EventFilter[]? filters, [NotNullWhen(true)] out EventFilterFunc? filter)
    {
        filter = null;
        if (filters is { Length: > 0 })
        {
            var filterExpression = string.Join(" and ", filters.Select(x => $"{x.Property} = '{SerilogExpression.EscapeStringContent(x.Value)}'"));
            if (string.IsNullOrWhiteSpace(expression))
            {
                expression = filterExpression;
            }
            else
            {
                expression += $" and {filterExpression}";
            }
        }

        if (!string.IsNullOrWhiteSpace(expression))
        {
            if (!expression.Contains(' ') && !expression.Any(x => ExpressionOperators.Contains(x)))
            {
                filter = MessageLike();
            }
            else if (SerilogExpression.TryCompile(expression, null, nameResolver, out var compiledExpression, out _))
            {
                filter = IsTrue(compiledExpression);
            }
            else
            {
                filter = MessageLike();
            }
        }

        return filter is not null;

        EventFilterFunc? MessageLike()
        {
            var filterSearch = $"@m like '%{SerilogExpression.EscapeLikeExpressionContent(expression)}%' ci";
            return SerilogExpression.TryCompile(filterSearch, out var compiledExpression, out _) ? IsTrue(compiledExpression) : null;
        }

        EventFilterFunc IsTrue(CompiledExpression compiledExpression) =>
            evt => ExpressionResult.IsTrue(compiledExpression(evt));
    }
}