namespace Develix.Dataset2Sql.Importing;

public sealed class PhysicalFileSystem : IFileSystem
{
    public bool FileExists(string path) => File.Exists(path);
}
