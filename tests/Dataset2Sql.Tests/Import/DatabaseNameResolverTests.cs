using System.Diagnostics;

namespace Develix.Dataset2Sql.Tests.Import;

public class DatabaseNameResolverTests
{
    [Test]
    public async Task Resolve_WhenDatabaseNameIsProvided_ThenReturnsDatabaseName()
    {
        var name = DatabaseNameResolver.Resolve(
            databaseName: "CliDb",
            defaultDatabaseName: "ConfigDb",
            isInputRedirected: false,
            promptForDbName: _ => throw new UnreachableException("Prompting should not be used"));

        await Assert.That(name).IsEqualTo("CliDb");
    }

    [Test]
    public async Task Resolve_WhenInputIsRedirectedAndDefaultExists_ThenReturnsDefault()
    {
        var name = DatabaseNameResolver.Resolve(
            databaseName: null,
            defaultDatabaseName: "ConfigDb",
            isInputRedirected: true,
            promptForDbName: _ => throw new UnreachableException("Prompting should not be used"));

        await Assert.That(name).IsEqualTo("ConfigDb");
    }

    [Test]
    public async Task Resolve_WhenInputIsRedirectedAndDefaultMissing_ThenThrows()
    {
        static string resolveAction() => DatabaseNameResolver.Resolve(
            databaseName: null,
            defaultDatabaseName: string.Empty,
            isInputRedirected: true,
            promptForDbName: _ => throw new UnreachableException("Prompting should not be used"));

        await Assert.That((Func<string>)resolveAction).Throws<InvalidOperationException>();
    }

    [Test]
    public async Task Resolve_WhenInteractiveAndDefaultMissing_ThenPromptsWithNullDefault()
    {
        string? receivedDefaultName = "initial";
        var name = DatabaseNameResolver.Resolve(
            databaseName: null,
            defaultDatabaseName: string.Empty,
            isInputRedirected: false,
            promptForDbName: defaultName =>
            {
                receivedDefaultName = defaultName;
                return "PromptDb";
            });

        await Assert.That(name).IsEqualTo("PromptDb");
        await Assert.That(receivedDefaultName).IsNull();
    }

    [Test]
    public async Task Resolve_WhenInteractiveAndDefaultExists_ThenPromptsWithDefault()
    {
        string? receivedDefaultName = null;
        var name = DatabaseNameResolver.Resolve(
            databaseName: null,
            defaultDatabaseName: "ConfigDb",
            isInputRedirected: false,
            promptForDbName: defaultValue =>
            {
                receivedDefaultName = defaultValue;
                return "ChosenDb";
            });

        await Assert.That(name).IsEqualTo("ChosenDb");
        await Assert.That(receivedDefaultName).IsEqualTo("ConfigDb");
    }
}
