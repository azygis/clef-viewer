using ClefViewer.API.Business;

namespace ClefViewer.API.Tests.Unit.Business;

public sealed class CustomMemberNameResolverTests
{
    private CustomMemberNameResolver _sut;

    [SetUp]
    public void SetUp() =>
        _sut = new CustomMemberNameResolver();

    [TestCaseSource(nameof(GetTestData))]
    public void TryResolveBuiltInPropertyName_Should_ProcessAlias(string alias, string? expectedTarget)
    {
        // ARRANGE
        var expectedResult = expectedTarget is not null;

        // ACT
        var result = _sut.TryResolveBuiltInPropertyName(alias, out var actualTarget);

        // ASSERT
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(expectedResult));
            Assert.That(actualTarget, Is.EqualTo(expectedTarget));
        });
    }

    public static IEnumerable<object?[]> GetTestData() =>
        CustomMemberNameResolver.PropertyMap
            .Select(x => new object?[] { x.Key, x.Value })
            .Append(["Random", null]);
}