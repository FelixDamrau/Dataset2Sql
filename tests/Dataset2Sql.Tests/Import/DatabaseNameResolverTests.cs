using System.Diagnostics;

namespace Develix.Dataset2Sql.Tests.Import;

public class DatabaseNameResolverTests
{
    [Test]
    public async Task Resolve_ReturnsSettingsValue_WhenProvided()
    {
        var name = DatabaseNameResolver.Resolve(
            databaseName: "CliDb",
            defaultDatabaseName: "ConfigDb",
            isInputRedirected: false,
            promptForDbName: _ => throw new UnreachableException("Prompting should not be used"));

        await Assert.That(name).IsEqualTo("CliDb");
    }

    [Test]
    public async Task Resolve_ReturnsDefault_WhenInputRedirectedAndDefaultExists()
    {
        var name = DatabaseNameResolver.Resolve(
            databaseName: null,
            defaultDatabaseName: "ConfigDb",
            isInputRedirected: true,
            promptForDbName: _ => throw new UnreachableException("Prompting should not be used"));

        await Assert.That(name).IsEqualTo("ConfigDb");
    }

    [Test]
    public async Task Resolve_Throws_WhenInputRedirectedAndNoDefault()
    {
        static string resolveAction() => DatabaseNameResolver.Resolve(
            databaseName: null,
            defaultDatabaseName: string.Empty,
            isInputRedirected: true,
            promptForDbName: _ => throw new UnreachableException("Prompting should not be used"));

        await Assert.That((Func<string>)resolveAction).Throws<InvalidOperationException>();
    }

    [Test]
    public async Task Resolve_UsesPromptWithNullDefault_WhenInteractiveAndNoDefault()
    {
        var promptDbName = "PromptDb";
        string? receivedDefaultName = "initial";
        var name = DatabaseNameResolver.Resolve(
            databaseName: null,
            defaultDatabaseName: string.Empty,
            isInputRedirected: false,
            promptForDbName: defaultName =>
            {
                receivedDefaultName = defaultName;
                return promptDbName;
            });

        await Assert.That(name).IsEqualTo(promptDbName);
        await Assert.That(receivedDefaultName).IsNull();
    }

    [Test]
    public async Task Resolve_UsesPromptWithDefault_WhenInteractiveAndDefaultExists()
    {
        string? receivedDefaultName = null;
        var name = DatabaseNameResolver.Resolve(
            databaseName: null,
            defaultDatabaseName: "ConfigDb",
            isInputRedirected: false,
            promptForDbName: defaultValue =>
            {
                receivedDefaultName = defaultValue;
                return $"{defaultValue}_Chosen";
            });

        await Assert.That(name).IsEqualTo("ConfigDb_Chosen");
        await Assert.That(receivedDefaultName).IsEqualTo("ConfigDb");
    }
}
