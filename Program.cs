using System.Data;
using Microsoft.Data.SqlClient;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace Develix.Dataset2Sql;

public class Program
{
    public static void Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
           .SetBasePath(Directory.GetCurrentDirectory())
           .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
               .Build();

        var dbSettings = new DatabaseSettings();
        configuration.GetSection("DatabaseSettings").Bind(dbSettings);


        Console.Write("Enter the path to the XML file: ");
        var xmlFilePath = Console.ReadLine()?.Trim('"') ?? throw new InvalidOperationException("Invalid file path");

        Console.Write($"Enter DB Name [{dbSettings.Name}]: ");
        var dbNameInput = Console.ReadLine();
        var dbName = string.IsNullOrWhiteSpace(dbNameInput) ? dbSettings.Name : dbNameInput;

        var connectionString = $"Data Source={dbSettings.Server};Initial Catalog ={ dbName}; TrustServerCertificate = true; User ID = { dbSettings.Username };password ={ dbSettings.Password}        ";

        try
        {
            var dataSet = new DataSet();
            dataSet.ReadXml(xmlFilePath);

            ImportDatasetToSqlServer(dataSet, connectionString, dbName);

            Console.WriteLine("Import completed successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
        }

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    private static void ImportDatasetToSqlServer(DataSet dataSet, string connectionString, string dbName)
    {
        var masterConnectionString = connectionString.Replace(dbName, "master");

        CreateDatabase(dbName, masterConnectionString);

        using var connection = new SqlConnection(connectionString);
        connection.Open();

        foreach (DataTable table in dataSet.Tables)
        {
            CreateTable(table, connection);
            ImportTableData(table, connection);
        }
    }
    private static void CreateDatabase(string dbName, string masterConnectionString)
    {
        using SqlConnection masterConnection = new SqlConnection(masterConnectionString);
        masterConnection.Open();

        var checkDbQuery = $"SELECT COUNT(*) FROM sys.databases WHERE name = '{dbName}'";
        var checkDbCmd = new SqlCommand(checkDbQuery, masterConnection);
        var dbCount = (int)checkDbCmd.ExecuteScalar();

        if (dbCount > 0)
        {
            Console.WriteLine($"DB {dbName} already exists. Database must be dropped, OK? (Y/N)");
            var response = Console.ReadLine();
            if (response != "Y" && response != "y")
            {
                Console.WriteLine("Tschüss.");
                Environment.Exit(0);
            }

            var dropDbQuery = $"ALTER DATABASE [{dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{dbName}]";
            var dropDbCmd = new SqlCommand(dropDbQuery, masterConnection);
            dropDbCmd.ExecuteNonQuery();
        }

        var createDbQuery = $"CREATE DATABASE [{dbName}]";
        var createDbCmd = new SqlCommand(createDbQuery, masterConnection);
        createDbCmd.ExecuteNonQuery();

        Console.WriteLine($"Database '{dbName}' created.");
    }

    private static void CreateTable(DataTable table, SqlConnection connection)
    {
        var createTableQuery = new StringBuilder();
        createTableQuery.AppendLine($"CREATE TABLE [{table.TableName}] (");

        for (var i = 0; i < table.Columns.Count; i++)
        {
            var column = table.Columns[i];
            var sqlType = GetSqlType(column.DataType);

            createTableQuery.Append($"    [{column.ColumnName}] {sqlType}");

            if (i < table.Columns.Count - 1)
                createTableQuery.AppendLine(",");
            else
                createTableQuery.AppendLine();
        }

        createTableQuery.AppendLine(")");

        using var command = new SqlCommand(createTableQuery.ToString(), connection);
        command.ExecuteNonQuery();
        Console.WriteLine($"Table '{table.TableName}' created.");
    }

    private static string GetSqlType(Type dotNetType)
    {
        if (dotNetType == typeof(string))
            return "NVARCHAR(MAX)";
        else if (dotNetType == typeof(int))
            return "INT";
        else if (dotNetType == typeof(long))
            return "BIGINT";
        else if (dotNetType == typeof(decimal))
            return "DECIMAL(18, 6)";
        else if (dotNetType == typeof(double))
            return "FLOAT";
        else if (dotNetType == typeof(float))
            return "REAL";
        else if (dotNetType == typeof(DateTime))
            return "DATETIME";
        else if (dotNetType == typeof(bool))
            return "BIT";
        else if (dotNetType == typeof(byte))
            return "TINYINT";
        else if (dotNetType == typeof(Guid))
            return "UNIQUEIDENTIFIER";
        else
            return "NVARCHAR(MAX)";
    }

    private static void ImportTableData(DataTable table, SqlConnection connection)
    {
        using var bulkCopy = new SqlBulkCopy(connection);
        bulkCopy.DestinationTableName = table.TableName;

        foreach (DataColumn column in table.Columns)
            bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);

        bulkCopy.WriteToServer(table);
        Console.WriteLine($"Imported {table.Rows.Count} rows to table '{table.TableName}'.");
    }
}
