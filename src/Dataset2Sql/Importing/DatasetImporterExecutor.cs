using System.Data;
using Microsoft.Data.SqlClient;

namespace Develix.Dataset2Sql.Importing;

public sealed class DatasetImporterExecutor : IDatasetImportExecutor
{
    public async Task<bool> ImportAsync(DataSet dataSet, SqlConnectionStringBuilder connectionStringBuilder, string dbName, Func<string, bool> confirmDropCallback, CancellationToken cancellationToken)
    {
        using var commandBuilder = new SqlCommandBuilder();
        var importer = new DatasetImporter(commandBuilder);
        return await importer.ImportDatasetToSqlServerAsync(dataSet, connectionStringBuilder, dbName, confirmDropCallback, cancellationToken);
    }
}
