namespace ClefViewer.API.Business;

public sealed record PathsToProcess(IEnumerable<string> Files, IEnumerable<string> Directories);

public interface IFileSystemHandler
{
    PathsToProcess GetPathsToProcess(string[] paths, ISet<string> allowedExtensionsInDirectory);
    Task<long> ProcessStreamAsync(string path, long streamPosition, Action<StreamReader> reader);
}

public sealed class FileSystemHandler : IFileSystemHandler
{
    public PathsToProcess GetPathsToProcess(string[] paths, ISet<string> allowedExtensionsInDirectory)
    {
        var filePaths = paths.Where(File.Exists);
        var directoryPaths = paths.Where(Directory.Exists).SelectMany(x => GetFilesByExtensions(x, allowedExtensionsInDirectory));
        return new PathsToProcess(filePaths, directoryPaths);
    }

    public async Task<long> ProcessStreamAsync(string path, long streamPosition, Action<StreamReader> reader)
    {
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        stream.Seek(streamPosition, SeekOrigin.Begin);
        using var streamReader = new StreamReader(stream, leaveOpen: true);
        reader(streamReader);
        return stream.Position;
    }

    private static IEnumerable<string> GetFilesByExtensions(string directoryPath, IEnumerable<string> extensions) =>
        Directory.GetFiles(directoryPath, "*")
            .Where(filePath => extensions.Contains(Path.GetExtension(filePath)))
            .OrderBy(filePath => filePath);
}