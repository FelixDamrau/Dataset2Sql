using System.Data;
using Microsoft.Data.SqlClient;

namespace Develix.DataSet2Sql.Importing;

public interface IDataSetImportExecutor
{
    bool Import(
        DataSet dataSet,
        SqlConnectionStringBuilder connectionStringBuilder,
        string dbName,
        Func<string, bool> confirmDropCallback);
}
