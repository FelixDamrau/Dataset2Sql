using System.Data;
using Microsoft.Data.SqlClient;

namespace Develix.Dataset2Sql.Tests;

public class DatasetImporterTests
{
    [Test]
    [Arguments("master")]
    [Arguments("MASTER")]
    [Arguments("MaStEr")]
    [Arguments("model")]
    [Arguments("msdb")]
    [Arguments("tempdb")]
    public async Task ImportDatasetToSqlServer_WhenTargetIsSystemDatabase_ThenReturnsFalse(string dbName)
    {
        var commandBuilder = new SqlCommandBuilder();
        var importer = new DatasetImporter(commandBuilder);
        var dataSet = new DataSet();

        var result = importer.ImportDatasetToSqlServer(dataSet, connectionStringBuilder: new(), dbName, confirmDropCallback: _ => true);

        await Assert.That(result).IsFalse();
    }
}
