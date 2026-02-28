using System.Data;
using Develix.DataSet2Sql.Importing;
using Microsoft.Data.SqlClient;

namespace Develix.DataSet2Sql.Tests.Workflow;

public class DataSetImportWorkflowTests
{
    [Test]
    public async Task Run_WhenXmlFileDoesNotExist_ThenReturnsFileNotFound()
    {
        var fileSystem = new FakeFileSystem(exists: false);
        var reader = new FakeDataSetXmlReader();
        var importer = new FakeDataSetImportExecutor();
        var workflow = new DataSetImportWorkflow(fileSystem, reader, importer);

        var result = workflow.Run(BuildRequest(xmlPath: "./missing.xml"));

        await Assert.That(result.Status).IsEqualTo(DataSetImportStatus.FileNotFound);
        await Assert.That(result.MissingFilePath).IsEqualTo("./missing.xml");
        await Assert.That(reader.CallCount).IsEqualTo(0);
        await Assert.That(importer.CallCount).IsEqualTo(0);
    }

    [Test]
    public async Task Run_WhenImportExecutorReturnsTrue_ThenReturnsCompleted()
    {
        var fileSystem = new FakeFileSystem(exists: true);
        var reader = new FakeDataSetXmlReader();
        var importer = new FakeDataSetImportExecutor { ReturnValue = true };
        var workflow = new DataSetImportWorkflow(fileSystem, reader, importer);

        var result = workflow.Run(BuildRequest(dbName: "DumpDb"));

        await Assert.That(result.Status).IsEqualTo(DataSetImportStatus.Completed);
        await Assert.That(importer.CallCount).IsEqualTo(1);
        await Assert.That(importer.LastConnectionStringBuilder!.InitialCatalog).IsEqualTo("DumpDb");
        await Assert.That(importer.LastConnectionStringBuilder.DataSource).IsEqualTo("localhost");
        await Assert.That(importer.LastConnectionStringBuilder.UserID).IsEqualTo("dbUser");
    }

    [Test]
    public async Task Run_WhenImportExecutorReturnsFalse_ThenReturnsCancelled()
    {
        var fileSystem = new FakeFileSystem(exists: true);
        var reader = new FakeDataSetXmlReader();
        var importer = new FakeDataSetImportExecutor { ReturnValue = false };
        var workflow = new DataSetImportWorkflow(fileSystem, reader, importer);

        var result = workflow.Run(BuildRequest());

        await Assert.That(result.Status).IsEqualTo(DataSetImportStatus.Cancelled);
    }

    [Test]
    public async Task Run_WhenDataSetReaderThrows_ThenPropagatesException()
    {
        var fileSystem = new FakeFileSystem(exists: true);
        var reader = new FakeDataSetXmlReader { ExceptionToThrow = new IOException("invalid xml") };
        var importer = new FakeDataSetImportExecutor();
        var workflow = new DataSetImportWorkflow(fileSystem, reader, importer);

        DataSetImportResult workflowAction() => workflow.Run(BuildRequest());

        await Assert.That(workflowAction).Throws<IOException>();
    }

    private static DataSetImportRequest BuildRequest(string xmlPath = "./dump.xml", string dbName = "DumpDb")
    {
        var settings = new DatabaseSettings
        {
            Server = "localhost",
            Username = "dbUser",
            Password = "dbPass",
            Name = "ConfigDb"
        };

        return new DataSetImportRequest(xmlPath, dbName, settings, _ => true);
    }

    private sealed class FakeFileSystem(bool exists) : IFileSystem
    {
        public bool FileExists(string path) => exists;
    }

    private sealed class FakeDataSetXmlReader : IDataSetXmlReader
    {
        public int CallCount { get; private set; }
        public Exception? ExceptionToThrow { get; init; }

        public DataSet Read(string xmlPath)
        {
            CallCount++;
            if (ExceptionToThrow != null)
            {
                throw ExceptionToThrow;
            }

            var dataSet = new DataSet();
            var table = new DataTable("Sample");
            table.Columns.Add("Id", typeof(int));
            table.Rows.Add(1);
            dataSet.Tables.Add(table);
            return dataSet;
        }
    }

    private sealed class FakeDataSetImportExecutor : IDataSetImportExecutor
    {
        public bool ReturnValue { get; init; }
        public int CallCount { get; private set; }
        public SqlConnectionStringBuilder? LastConnectionStringBuilder { get; private set; }

        public bool Import(
            DataSet dataSet,
            SqlConnectionStringBuilder connectionStringBuilder,
            string dbName,
            Func<string, bool> confirmDropCallback)
        {
            CallCount++;
            LastConnectionStringBuilder = connectionStringBuilder;
            return ReturnValue;
        }
    }
}
