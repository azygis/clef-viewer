using ClefViewer.API.Business;
using ClefViewer.API.Data;

namespace ClefViewer.API.Tests.Unit.Business;

public sealed class LogSessionFilterFactoryTests
{
    private LogSessionFilterFactory _sut;

    [SetUp]
    public void SetUp() =>
        _sut = new LogSessionFilterFactory(new CustomMemberNameResolver());

    [Test]
    public void TryCreateFilter_When_ExpressionAndFiltersEmpty_ReturnsFalse()
    {
        // ACT
        var result = _sut.TryCreateFilter(string.Empty, [], out var output);

        // ASSERT
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.False);
            Assert.That(output, Is.Null);
        });
    }

    [TestCase("@mt = 'Loop {Counter} done'", 3)]
    [TestCase("@MessageTemplate = 'Loop {Counter} done'", 3)]
    [TestCase("@l = 'Warning'", 1)]
    [TestCase("@l='Error'", 1)]
    [TestCase("@Level = 'Warning'", 1)]
    [TestCase("loop", 3)]
    [TestCase("loop 1", 2)]
    [TestCase("@mt = 'Loop {Counter} done' and Counter > 0", 2)]
    public void TryCreateFilter_ByExpression_CreatesCorrectFilter(string? expression, int expectedCount)
    {
        // ACT
        var result = _sut.TryCreateFilter(expression, [], out var output);

        // ASSERT
        AssertCorrectResult(result, output, expectedCount);
    }

    [TestCase("@mt","Loop {Counter} done", 3)]
    [TestCase("@MessageTemplate", "Loop {Counter} done", 3)]
    [TestCase("@l", "Warning", 1)]
    [TestCase("@Level", "Warning", 1)]
    [TestCase("@Message", "Loop 1 done", 1)]
    public void TryCreateFilter_BySingleFilter_CreatesCorrectFilter(string property, string value, int expectedCount)
    {
        // ACT
        var result = _sut.TryCreateFilter(string.Empty, [new EventFilter(property, value)], out var output);

        // ASSERT
        AssertCorrectResult(result, output, expectedCount);
    }

    [Test]
    public void TryCreateFilter_ByMultipleFilters_CreatesCorrectFilters()
    {
        // ARRANGE
        EventFilter[] filters =
        [
            new("@MessageTemplate", "Loop {Counter} done"),
            new("@l", "Warning"),
        ];

        // ACT
        var result = _sut.TryCreateFilter(string.Empty, filters, out var output);

        // ASSERT
        AssertCorrectResult(result, output, 1);
    }

    [Test]
    public void TryCreateFilter_ByExpressionAndFilter_CreatesCorrectFilters()
    {
        // ARRANGE
        const string expression = "@MessageTemplate = 'Loop {Counter} done'";
        var filter = new EventFilter("@l", "Warning");

        // ACT
        var result = _sut.TryCreateFilter(expression, [filter], out var output);

        // ASSERT
        AssertCorrectResult(result, output, 1);
    }

    private static void AssertCorrectResult(bool filterCreated, EventFilterFunc? filter, int expectedCount)
    {
        Assert.Multiple(() =>
        {
            Assert.That(filterCreated, Is.True);
            Assert.That(filter, Is.Not.Null);
        });

        var events = EventStub.Events.Where(x => filter(x));
        Assert.That(events, Has.Exactly(expectedCount).Items);
    }
}