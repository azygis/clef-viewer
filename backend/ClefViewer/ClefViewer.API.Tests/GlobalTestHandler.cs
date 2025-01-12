[SetUpFixture]
// ReSharper disable once CheckNamespace
public class GlobalTestHandler
{
    [OneTimeTearDown]
    public static void TearDown()
    {
        var directoryPath = Path.GetFullPath("Unit/TestData/temp");
        if (Directory.Exists(directoryPath))
        {
            Directory.Delete(directoryPath, true);
        }
    }
}