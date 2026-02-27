using System.Data;

namespace Develix.Dataset2Sql.Importing;

public sealed class XmlDataSetReader : IDataSetXmlReader
{
    public async Task<DataSet> ReadAsync(string xmlPath, CancellationToken cancellationToken)
    {
        return await Task.Run(() =>
        {
            var dataSet = new DataSet();
            dataSet.ReadXml(xmlPath);
            return dataSet;
        }, cancellationToken);
    }
}
