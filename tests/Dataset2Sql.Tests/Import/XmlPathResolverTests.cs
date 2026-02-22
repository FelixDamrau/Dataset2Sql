using System.Diagnostics;

namespace Develix.Dataset2Sql.Tests.Import;

public class XmlPathResolverTests
{
    [Test]
    public async Task Resolve_WhenPathProvidedInSettings_ThenReturnsTrimmedPath()
    {
        var path = XmlPathResolver.Resolve(
            xmlFilePath: "\"./dump.xml\"",
            isInputRedirected: false,
            promptForXmlPath: () => throw new UnreachableException("Prompting should not be used"));

        await Assert.That(path).IsEqualTo("./dump.xml");
    }

    [Test]
    public async Task Resolve_WhenPathMissingAndInputIsRedirected_ThenThrows()
    {
        static string action() => XmlPathResolver.Resolve(
            xmlFilePath: null,
            isInputRedirected: true,
            promptForXmlPath: () => throw new UnreachableException("Prompting should not be used"));

        await Assert.That(action).Throws<InvalidOperationException>();
    }

    [Test]
    public async Task Resolve_WhenPathMissingAndInteractive_ThenUsesPrompt()
    {
        var path = XmlPathResolver.Resolve(
            xmlFilePath: null,
            isInputRedirected: false,
            promptForXmlPath: () => "'./prompted.xml'");

        await Assert.That(path).IsEqualTo("./prompted.xml");
    }
}
