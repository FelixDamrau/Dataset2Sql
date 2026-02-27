using System.Data;

namespace Develix.Dataset2Sql.Importing;

public interface IDataSetXmlReader
{
    Task<DataSet> ReadAsync(string xmlPath, CancellationToken cancellationToken);
}
