using Microsoft.Data.SqlClient;

namespace Develix.DataSet2Sql.Importing;

public sealed class DataSetImportWorkflow(IFileSystem fileSystem, IDataSetXmlReader dataSetXmlReader, IDataSetImportExecutor dataSetImportExecutor)
{
    private readonly IFileSystem fileSystem = fileSystem;
    private readonly IDataSetXmlReader dataSetXmlReader = dataSetXmlReader;
    private readonly IDataSetImportExecutor dataSetImportExecutor = dataSetImportExecutor;

    public DataSetImportResult Run(DataSetImportRequest request)
    {
        if (!fileSystem.FileExists(request.XmlFilePath))
            return new DataSetImportResult { Status = DataSetImportStatus.FileNotFound, MissingFilePath = request.XmlFilePath };

        var connectionStringBuilder = new SqlConnectionStringBuilder
        {
            DataSource = request.DatabaseSettings.Server,
            InitialCatalog = request.DatabaseName,
            TrustServerCertificate = true,
            UserID = request.DatabaseSettings.Username,
            Password = request.DatabaseSettings.Password
        };

        var dataSet = dataSetXmlReader.Read(request.XmlFilePath);
        var importCompleted = dataSetImportExecutor.Import(dataSet, connectionStringBuilder, request.DatabaseName, request.ConfirmDropCallback);

        return importCompleted
            ? new DataSetImportResult { Status = DataSetImportStatus.Completed }
            : new DataSetImportResult { Status = DataSetImportStatus.Cancelled };
    }
}
