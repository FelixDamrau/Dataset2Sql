using System.Data;
using Microsoft.Data.SqlClient;

namespace Develix.Dataset2Sql;

public class DatasetImporter(SqlCommandBuilder commandBuilder)
{
    private readonly SqlCommandBuilder commandBuilder = commandBuilder;
    private static readonly string[] systemDatabases = ["master", "model", "msdb", "tempdb"];

    public async Task<bool> ImportDatasetToSqlServerAsync(DataSet dataSet, SqlConnectionStringBuilder connectionStringBuilder, string dbName, Func<string, bool> confirmDropCallback, CancellationToken cancellationToken)
    {
        var connectionString = connectionStringBuilder.ToString();
        var masterConnectionString = new SqlConnectionStringBuilder(connectionString) { InitialCatalog = "master" }.ToString();

        return await CreateDatabaseAsync(dbName, masterConnectionString, confirmDropCallback, cancellationToken)
            && await Log.StatusAsync($"Importing {dataSet.Tables.Count} tables to database '{dbName}'...", ImportTableAsync);

        async Task<bool> ImportTableAsync()
        {
            using SqlConnection connection = new(connectionString);
            await connection.OpenAsync(cancellationToken);

            foreach (DataTable table in dataSet.Tables)
            {
                await CreateTableAsync(table, connection, cancellationToken);
                await ImportTableDataAsync(table, connection, cancellationToken);
            }

            return true;
        }
    }

    private async Task<bool> CreateDatabaseAsync(string dbName, string masterConnectionString, Func<string, bool> confirmDropCallback, CancellationToken cancellationToken)
    {
        if (systemDatabases.Contains(dbName, StringComparer.OrdinalIgnoreCase))
        {
            Log.Error($"Cannot drop or recreate system database '{dbName}'.");
            return false;
        }

        using SqlConnection masterConnection = new(masterConnectionString);
        await masterConnection.OpenAsync(cancellationToken);

        var checkDbQuery = "SELECT COUNT(*) FROM sys.databases WHERE name = @name";
        using var checkDbCmd = new SqlCommand(checkDbQuery, masterConnection);
        checkDbCmd.Parameters.AddWithValue("@name", dbName);
        var dbCount = (int)(await checkDbCmd.ExecuteScalarAsync(cancellationToken))!;

        if (dbCount > 0)
        {
            if (!confirmDropCallback(dbName))
                return false;

            var safeDbName = SanitizeIdentifier(dbName);
            var dropDbQuery = $"ALTER DATABASE {safeDbName} SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE {safeDbName}";
            using var dropDbCmd = new SqlCommand(dropDbQuery, masterConnection);
            await Log.StatusAsync($"Dropping database '{dbName}'...", async () => await dropDbCmd.ExecuteNonQueryAsync(cancellationToken));
        }

        var safeDbNameForCreate = SanitizeIdentifier(dbName);
        var createDbQuery = $"CREATE DATABASE {safeDbNameForCreate}";
        using var createDbCmd = new SqlCommand(createDbQuery, masterConnection);
        await Log.StatusAsync($"Creating database '{dbName}'...", async () => await createDbCmd.ExecuteNonQueryAsync(cancellationToken));

        Log.Info($"Database '{dbName}' created.");
        return true;
    }

    private async Task CreateTableAsync(DataTable table, SqlConnection connection, CancellationToken cancellationToken)
    {
        var createTableQuery = CreateTableSqlBuilder.Build(table, SanitizeIdentifier, SqlTypeMapper.Map);

        using SqlCommand command = new(createTableQuery, connection);
        await command.ExecuteNonQueryAsync(cancellationToken);
        Log.Info($"Table '{table.TableName}' created.");
    }

    private string SanitizeIdentifier(string identifier)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(identifier);
        return commandBuilder.QuoteIdentifier(identifier);
    }

    private static async Task ImportTableDataAsync(DataTable table, SqlConnection connection, CancellationToken cancellationToken)
    {
        using SqlBulkCopy bulkCopy = new(connection);
        bulkCopy.DestinationTableName = table.TableName;

        foreach (DataColumn column in table.Columns)
            bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);

        await bulkCopy.WriteToServerAsync(table, cancellationToken);
        Log.Info($"Imported {table.Rows.Count} rows to table '{table.TableName}'.");
    }
}
