using System.Data;
using Microsoft.Data.SqlClient;

namespace Develix.Dataset2Sql.Importing;

public interface IDatasetImportExecutor
{
    bool Import(
        DataSet dataSet,
        SqlConnectionStringBuilder connectionStringBuilder,
        string dbName,
        Func<string, bool> confirmDropCallback);
}
