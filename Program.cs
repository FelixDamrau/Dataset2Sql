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

            ImportDatasetToSqlServer(dataSet, connectionStringBuilder, dbName);

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

    private static void ImportDatasetToSqlServer(DataSet dataSet, SqlConnectionStringBuilder connectionStringBuilder, string dbName)
    {
        var connectionString = connectionStringBuilder.ToString();
        var masterConnectionString = new SqlConnectionStringBuilder(connectionString) { InitialCatalog = "master" }.ToString();

        CreateDatabase(dbName, masterConnectionString);

        using SqlConnection connection = new(connectionString);
        connection.Open();

        foreach (DataTable table in dataSet.Tables)
        {
            CreateTable(table, connection);
            ImportTableData(table, connection);
        }
    }

    private static void CreateDatabase(string dbName, string masterConnectionString)
    {
        using SqlConnection masterConnection = new(masterConnectionString);
        masterConnection.Open();

        var checkDbQuery = $"SELECT COUNT(*) FROM sys.databases WHERE name = '{dbName}'";
        var checkDbCmd = new SqlCommand(checkDbQuery, masterConnection);
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

        using SqlCommand command = new(createTableQuery.ToString(), connection);
        command.ExecuteNonQuery();
        Console.WriteLine($"Table '{table.TableName}' created.");
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
