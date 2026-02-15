using System.Data;
using System.Text;
using Microsoft.Data.SqlClient;

namespace Develix.Dataset2Sql;

public class DatasetImporter
{
    private readonly SqlCommandBuilder _commandBuilder;

    public DatasetImporter(SqlCommandBuilder commandBuilder)
    {
        _commandBuilder = commandBuilder;
    }

    public void ImportDatasetToSqlServer(DataSet dataSet, SqlConnectionStringBuilder connectionStringBuilder, string dbName, Func<string, bool> confirmDropCallback)
    {
        var connectionString = connectionStringBuilder.ToString();
        var masterConnectionString = new SqlConnectionStringBuilder(connectionString) { InitialCatalog = "master" }.ToString();

        CreateDatabase(dbName, masterConnectionString, confirmDropCallback);

        using SqlConnection connection = new(connectionString);
        connection.Open();

        foreach (DataTable table in dataSet.Tables)
        {
            CreateTable(table, connection);
            ImportTableData(table, connection);
        }
    }

    private void CreateDatabase(string dbName, string masterConnectionString, Func<string, bool> confirmDropCallback)
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
                return;

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
    }

    private void CreateTable(DataTable table, SqlConnection connection)
    {
        var safeTableName = SanitizeIdentifier(table.TableName);
        var createTableQuery = new StringBuilder();
        createTableQuery.AppendLine($"CREATE TABLE {safeTableName} (");

        for (var i = 0; i < table.Columns.Count; i++)
        {
            var column = table.Columns[i];
            var sqlType = GetSqlType(column.DataType);
            var safeColumnName = SanitizeIdentifier(column.ColumnName);

            createTableQuery.Append($"    {safeColumnName} {sqlType}");

            if (i < table.Columns.Count - 1)
                createTableQuery.AppendLine(",");
            else
                createTableQuery.AppendLine();
        }

        createTableQuery.AppendLine(")");

        using SqlCommand command = new(createTableQuery.ToString(), connection);
        command.ExecuteNonQuery();
        Log.Info($"Table '{table.TableName}' created.");
    }

    private string SanitizeIdentifier(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            throw new ArgumentException("Identifier cannot be null or empty.", nameof(identifier));

        return _commandBuilder.QuoteIdentifier(identifier);
    }

    private static string GetSqlType(Type dotNetType)
    {
        return dotNetType switch
        {
            _ when dotNetType == typeof(string) => "NVARCHAR(MAX)",
            _ when dotNetType == typeof(int) => "INT",
            _ when dotNetType == typeof(long) => "BIGINT",
            _ when dotNetType == typeof(decimal) => "DECIMAL(18, 6)",
            _ when dotNetType == typeof(double) => "FLOAT",
            _ when dotNetType == typeof(float) => "REAL",
            _ when dotNetType == typeof(DateTime) => "DATETIME",
            _ when dotNetType == typeof(bool) => "BIT",
            _ when dotNetType == typeof(byte) => "TINYINT",
            _ when dotNetType == typeof(Guid) => "UNIQUEIDENTIFIER",
            _ => "NVARCHAR(MAX)"
        };
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
