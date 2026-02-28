namespace Develix.DataSet2Sql.Importing;

public sealed class PhysicalFileSystem : IFileSystem
{
    public bool FileExists(string path) => File.Exists(path);
}
