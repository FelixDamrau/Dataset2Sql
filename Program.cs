using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Reflection;

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
            var importer = new DatasetImporter(commandBuilder);

            importer.ImportDatasetToSqlServer(dataSet, connectionStringBuilder, dbName, ConfirmDatabaseDrop);

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

    private static bool ConfirmDatabaseDrop(string dbName)
    {
        Console.WriteLine($"DB {dbName} already exists. Database must be dropped press 'y' to continue: ");
        var response = Console.ReadLine();
        if (!"y".Equals(response, StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("Tschüss.");
            Environment.Exit(0);
            return false;
        }
        return true;
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
