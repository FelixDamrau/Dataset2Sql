using System.Data;
using Microsoft.Data.SqlClient;
using System.Text;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using System.Data.Common;

namespace Develix.Dataset2Sql;

public class Program
{
    public static void Main(string[] args)
    {
        ShowVersionScreen();

        var configuration = new ConfigurationBuilder()
           .SetBasePath(Directory.GetCurrentDirectory())
           .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
               .Build();

        var dbSettings = new DatabaseSettings();
        configuration.GetSection("DatabaseSettings").Bind(dbSettings);


        Console.Write("Enter the path to the XML file: ");
        var inputPath = Console.ReadLine() ?? throw new InvalidOperationException("Invalid input");
        var xmlFilePath = inputPath.Trim('"', '\'');

        if (!File.Exists(xmlFilePath))
        {
            Console.WriteLine($"Error: File not found at '{xmlFilePath}'");
            return;
        }

        Console.Write($"Enter DB Name [{dbSettings.Name}]: ");
        var dbNameInput = Console.ReadLine();
        var dbName = string.IsNullOrWhiteSpace(dbNameInput) ? dbSettings.Name : dbNameInput;

        var connectionStringBuilder = new SqlConnectionStringBuilder
        {
            DataSource = dbSettings.Server,
            InitialCatalog = dbName,
            TrustServerCertificate = true,
            UserID = dbSettings.Username,
            Password = dbSettings.Password
        };
        try
        {
            var dataSet = new DataSet();
            dataSet.ReadXml(xmlFilePath);

            using var commandBuilder = new SqlCommandBuilder();
            ImportDatasetToSqlServer(dataSet, connectionStringBuilder, dbName, commandBuilder);

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

    private static void ImportDatasetToSqlServer(DataSet dataSet, SqlConnectionStringBuilder connectionStringBuilder, string dbName, SqlCommandBuilder commandBuilder)
    {
        var connectionString = connectionStringBuilder.ToString();
        var masterConnectionString = new SqlConnectionStringBuilder(connectionString) { InitialCatalog = "master" }.ToString();

        CreateDatabase(dbName, masterConnectionString, commandBuilder);

        using SqlConnection connection = new(connectionString);
        connection.Open();

        foreach (DataTable table in dataSet.Tables)
        {
            CreateTable(table, connection, commandBuilder);
            ImportTableData(table, connection);
        }
    }

    private static void CreateDatabase(string dbName, string masterConnectionString, SqlCommandBuilder commandBuilder)
    {
        using SqlConnection masterConnection = new(masterConnectionString);
        masterConnection.Open();

        var checkDbQuery = "SELECT COUNT(*) FROM sys.databases WHERE name = @name";
        using var checkDbCmd = new SqlCommand(checkDbQuery, masterConnection);
        checkDbCmd.Parameters.AddWithValue("@name", dbName);
        var dbCount = (int)checkDbCmd.ExecuteScalar();

        if (dbCount > 0)
        {
            Console.WriteLine($"DB {dbName} already exists. Database must be dropped press 'y' to continue: ");
            var response = Console.ReadLine();
            if (!"y".Equals(response, StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Tschüss.");
                Environment.Exit(0);
            }

            var safeDbName = SanitizeIdentifier(dbName, commandBuilder);
            var dropDbQuery = $"ALTER DATABASE {safeDbName} SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE {safeDbName}";
            using var dropDbCmd = new SqlCommand(dropDbQuery, masterConnection);
            dropDbCmd.ExecuteNonQuery();
        }

        var safeDbNameForCreate = SanitizeIdentifier(dbName, commandBuilder);
        var createDbQuery = $"CREATE DATABASE {safeDbNameForCreate}";
        using var createDbCmd = new SqlCommand(createDbQuery, masterConnection);
        createDbCmd.ExecuteNonQuery();

        Console.WriteLine($"Database '{dbName}' created.");
    }

    private static void CreateTable(DataTable table, SqlConnection connection, SqlCommandBuilder commandBuilder)
    {
        var safeTableName = SanitizeIdentifier(table.TableName, commandBuilder);
        var createTableQuery = new StringBuilder();
        createTableQuery.AppendLine($"CREATE TABLE {safeTableName} (");

        for (var i = 0; i < table.Columns.Count; i++)
        {
            var column = table.Columns[i];
            var sqlType = GetSqlType(column.DataType);
            var safeColumnName = SanitizeIdentifier(column.ColumnName, commandBuilder);

            createTableQuery.Append($"    {safeColumnName} {sqlType}");

            if (i < table.Columns.Count - 1)
                createTableQuery.AppendLine(",");
            else
                createTableQuery.AppendLine();
        }

        createTableQuery.AppendLine(")");

        using SqlCommand command = new(createTableQuery.ToString(), connection);
        command.ExecuteNonQuery();
        Console.WriteLine($"Table '{table.TableName}' created.");
    }

    private static string SanitizeIdentifier(string identifier, SqlCommandBuilder commandBuilder)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            throw new ArgumentException("Identifier cannot be null or empty.", nameof(identifier));

        return commandBuilder.QuoteIdentifier(identifier);
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
        Console.WriteLine($"Imported {table.Rows.Count} rows to table '{table.TableName}'.");
    }

    static void ShowVersionScreen()
    {
        var version = Assembly.GetExecutingAssembly()!.GetName().Version;
        var title = "Dataset2Sql";
        var versionText = $"Version: {version}";
        var width = versionText.Length + 6;
        var topBorder = "┌" + new string('─', width - 2) + "┐";
        var bottomBorder = "└" + new string('─', width - 2) + "┘";

        Console.WriteLine(topBorder);
        Console.WriteLine(FormatLine(title, width));
        Console.WriteLine(FormatLine(versionText, width));
        Console.WriteLine(bottomBorder);

        static string FormatLine(string text, int width)
        {
            var padding = (width - 2 - text.Length) / 2;
            var extraSpace = (width - 2 - text.Length) % 2;
            return "│" + new string(' ', padding) + text + new string(' ', padding + extraSpace) + "│";
        }
    }
}
