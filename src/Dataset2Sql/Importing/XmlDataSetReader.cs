using System.Data;

namespace Develix.Dataset2Sql.Importing;

public sealed class XmlDataSetReader : IDataSetXmlReader
{
    public Task<DataSet> ReadAsync(string xmlPath, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var totalBytes = new FileInfo(xmlPath).Length;

        var dataSet = Log.ProgressBytes("Parsing XML...", totalBytes, reportProgress => Read(xmlPath, reportProgress, cancellationToken));

        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(dataSet);
    }

    private static DataSet Read(string xmlPath, Action<long> reportProgress, CancellationToken cancellationToken)
    {
        using var fileStream = new FileStream(
            xmlPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 81920,
            options: FileOptions.SequentialScan);
        using var progressStream = new ProgressReadStream(fileStream, reportProgress, cancellationToken);
        var dataSet = new DataSet();
        dataSet.ReadXml(progressStream);
        return dataSet;
    }
}
