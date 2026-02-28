using System.Data;
using Microsoft.Data.SqlClient;

namespace Develix.DataSet2Sql.Importing;

public sealed class DataSetImporterExecutor : IDataSetImportExecutor
{
    public bool Import(DataSet dataSet, SqlConnectionStringBuilder connectionStringBuilder, string dbName, Func<string, bool> confirmDropCallback)
    {
        using var commandBuilder = new SqlCommandBuilder();
        var importer = new DataSetImporter(commandBuilder);
        return importer.ImportDataSetToSqlServer(dataSet, connectionStringBuilder, dbName, confirmDropCallback);
    }
}
