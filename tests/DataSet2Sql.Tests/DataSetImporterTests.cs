using System.Data;
using Microsoft.Data.SqlClient;

namespace Develix.DataSet2Sql.Tests;

public class DataSetImporterTests
{
    [Test]
    [Arguments("master")]
    [Arguments("MASTER")]
    [Arguments("MaStEr")]
    [Arguments("model")]
    [Arguments("msdb")]
    [Arguments("tempdb")]
    public async Task ImportDataSetToSqlServer_WhenTargetIsSystemDatabase_ThenReturnsFalse(string dbName)
    {
        var commandBuilder = new SqlCommandBuilder();
        var importer = new DataSetImporter(commandBuilder);
        var dataSet = new DataSet();

        var result = importer.ImportDataSetToSqlServer(dataSet, connectionStringBuilder: new(), dbName, confirmDropCallback: _ => true);

        await Assert.That(result).IsFalse();
    }
}
