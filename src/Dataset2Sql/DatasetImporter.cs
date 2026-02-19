using System.Data;
using Microsoft.Data.SqlClient;

namespace Develix.Dataset2Sql;

public class DatasetImporter(SqlCommandBuilder commandBuilder)
{
    private readonly SqlCommandBuilder commandBuilder = commandBuilder;

    public bool ImportDatasetToSqlServer(DataSet dataSet, SqlConnectionStringBuilder connectionStringBuilder, string dbName, Func<string, bool> confirmDropCallback)
    {
        var connectionString = connectionStringBuilder.ToString();
        var masterConnectionString = new SqlConnectionStringBuilder(connectionString) { InitialCatalog = "master" }.ToString();

        if (!CreateDatabase(dbName, masterConnectionString, confirmDropCallback))
            return false;

        using SqlConnection connection = new(connectionString);
        connection.Open();

        foreach (DataTable table in dataSet.Tables)
        {
            CreateTable(table, connection);
            ImportTableData(table, connection);
        }

        return true;
    }

    private bool CreateDatabase(string dbName, string masterConnectionString, Func<string, bool> confirmDropCallback)
    {
        using SqlConnection masterConnection = new(masterConnectionString);
        masterConnection.Open();

        var checkDbQuery = "SELECT COUNT(*) FROM sys.databases WHERE name = @name";
        using var checkDbCmd = new SqlCommand(checkDbQuery, masterConnection);
        checkDbCmd.Parameters.AddWithValue("@name", dbName);
        var dbCount = (int)checkDbCmd.ExecuteScalar();

        if (dbCount > 0)
        {
            if (!confirmDropCallback(dbName))
                return false;

            var safeDbName = SanitizeIdentifier(dbName);
            var dropDbQuery = $"ALTER DATABASE {safeDbName} SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE {safeDbName}";
            using var dropDbCmd = new SqlCommand(dropDbQuery, masterConnection);
            dropDbCmd.ExecuteNonQuery();
        }

        var safeDbNameForCreate = SanitizeIdentifier(dbName);
        var createDbQuery = $"CREATE DATABASE {safeDbNameForCreate}";
        using var createDbCmd = new SqlCommand(createDbQuery, masterConnection);
        createDbCmd.ExecuteNonQuery();

        Log.Info($"Database '{dbName}' created.");
        return true;
    }

    private void CreateTable(DataTable table, SqlConnection connection)
    {
        var createTableQuery = CreateTableSqlBuilder.Build(table, SanitizeIdentifier, SqlTypeMapper.Map);

        using SqlCommand command = new(createTableQuery, connection);
        command.ExecuteNonQuery();
        Log.Info($"Table '{table.TableName}' created.");
    }

    private string SanitizeIdentifier(string identifier)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(identifier);
        return commandBuilder.QuoteIdentifier(identifier);
    }

    private static void ImportTableData(DataTable table, SqlConnection connection)
    {
        using SqlBulkCopy bulkCopy = new(connection);
        bulkCopy.DestinationTableName = table.TableName;

        foreach (DataColumn column in table.Columns)
            bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);

        bulkCopy.WriteToServer(table);
        Log.Info($"Imported {table.Rows.Count} rows to table '{table.TableName}'.");
    }
}
