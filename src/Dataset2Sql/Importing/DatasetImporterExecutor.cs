using System.Data;
using Microsoft.Data.SqlClient;

namespace Develix.Dataset2Sql.Importing;

public sealed class DatasetImporterExecutor : IDatasetImportExecutor
{
    public bool Import(DataSet dataSet, SqlConnectionStringBuilder connectionStringBuilder, string dbName, Func<string, bool> confirmDropCallback)
    {
        using var commandBuilder = new SqlCommandBuilder();
        var importer = new DatasetImporter(commandBuilder);
        return importer.ImportDatasetToSqlServer(dataSet, connectionStringBuilder, dbName, confirmDropCallback);
    }
}
