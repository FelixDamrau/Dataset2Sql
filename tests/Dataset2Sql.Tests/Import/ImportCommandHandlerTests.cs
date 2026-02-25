using Develix.Dataset2Sql.Importing;

namespace Develix.Dataset2Sql.Tests.Import;

public class ImportCommandHandlerTests
{
    [Test]
    public async Task Execute_WhenConfigFileMissing_ThenReturnsErrorCodeOne()
    {
        var handler = new ImportCommandHandler(
            () => "./missing.settings.json",
            () => "dataset2sql",
            _ => false,
            _ => throw new InvalidOperationException("should not load config"),
            _ => throw new InvalidOperationException("should not run workflow"));

        var exitCode = handler.Execute(
            new ImportExecutionOptions("./dump.xml", "DumpDb", AutoConfirmDrop: true),
            BuildCallbacks());

        await Assert.That(exitCode).IsEqualTo(1);
    }

    [Test]
    public async Task Execute_WhenWorkflowReturnsFileNotFound_ThenReturnsErrorCodeOne()
    {
        var handler = new ImportCommandHandler(
            () => "./dataset2sql.settings.json",
            () => "dataset2sql",
            _ => true,
            _ => BuildDbSettings(),
            request => new DatasetImportResult
            {
                Status = DatasetImportStatus.FileNotFound,
                MissingFilePath = request.XmlFilePath
            });

        var exitCode = handler.Execute(
            new ImportExecutionOptions("./missing.xml", "DumpDb", AutoConfirmDrop: false),
            BuildCallbacks());

        await Assert.That(exitCode).IsEqualTo(1);
    }

    [Test]
    public async Task Execute_WhenWorkflowReturnsCancelled_ThenReturnsZero()
    {
        var handler = new ImportCommandHandler(
            () => "./dataset2sql.settings.json",
            () => "dataset2sql",
            _ => true,
            _ => BuildDbSettings(),
            _ => new DatasetImportResult { Status = DatasetImportStatus.Cancelled });

        var exitCode = handler.Execute(
            new ImportExecutionOptions("./dump.xml", "DumpDb", AutoConfirmDrop: false),
            BuildCallbacks());

        await Assert.That(exitCode).IsEqualTo(0);
    }

    [Test]
    public async Task Execute_WhenWorkflowReturnsCompleted_ThenReturnsZero()
    {
        DatasetImportRequest? capturedRequest = null;
        var handler = new ImportCommandHandler(
            () => "./dataset2sql.settings.json",
            () => "dataset2sql",
            _ => true,
            _ => BuildDbSettings(),
            request =>
            {
                capturedRequest = request;
                return new DatasetImportResult { Status = DatasetImportStatus.Completed };
            });

        var exitCode = handler.Execute(
            new ImportExecutionOptions("./dump.xml", "DumpDb", AutoConfirmDrop: true),
            BuildCallbacks());

        await Assert.That(exitCode).IsEqualTo(0);
        await Assert.That(capturedRequest).IsNotNull();
        await Assert.That(capturedRequest!.XmlFilePath).IsEqualTo("./dump.xml");
        await Assert.That(capturedRequest.DatabaseName).IsEqualTo("DumpDb");
    }

    private static DatabaseSettings BuildDbSettings() => new() { Name = "ConfigDb", };

    private static ImportExecutionCallbacks BuildCallbacks()
    {
        return new(
            IsInputRedirected: true,
            PromptForXmlPath: () => "./prompt.xml",
            PromptForDatabaseName: defaultName => defaultName ?? "PromptDb",
            ConfirmDatabaseDrop: (_, _) => true);
    }
}
