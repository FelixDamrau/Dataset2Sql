using Microsoft.Data.SqlClient;

namespace Develix.Dataset2Sql.Importing;

public sealed class DatasetImportWorkflow(IFileSystem fileSystem, IDataSetXmlReader dataSetXmlReader, IDatasetImportExecutor datasetImportExecutor)
{
    private readonly IFileSystem fileSystem = fileSystem;
    private readonly IDataSetXmlReader dataSetXmlReader = dataSetXmlReader;
    private readonly IDatasetImportExecutor datasetImportExecutor = datasetImportExecutor;

    public async Task<DatasetImportResult> RunAsync(DatasetImportRequest request)
    {
        if (!fileSystem.FileExists(request.XmlFilePath))
            return new DatasetImportResult { Status = DatasetImportStatus.FileNotFound, MissingFilePath = request.XmlFilePath };

        var connectionStringBuilder = new SqlConnectionStringBuilder
        {
            DataSource = request.DatabaseSettings.Server,
            InitialCatalog = request.DatabaseName,
            TrustServerCertificate = true,
            UserID = request.DatabaseSettings.Username,
            Password = request.DatabaseSettings.Password
        };

        var dataSet = await Log.StatusAsync("Reading XML...", () => dataSetXmlReader.ReadAsync(request.XmlFilePath, request.CancellationToken));
        var importCompleted = await datasetImportExecutor.ImportAsync(dataSet, connectionStringBuilder, request.DatabaseName, request.ConfirmDropCallback, request.CancellationToken);

        return importCompleted
            ? new DatasetImportResult { Status = DatasetImportStatus.Completed }
            : new DatasetImportResult { Status = DatasetImportStatus.Cancelled };
    }
}
