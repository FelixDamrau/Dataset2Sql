namespace Develix.Dataset2Sql.Tests.Import;

public class XmlPathResolverTests
{
    [Test]
    public async Task Resolve_ReturnsTrimmedPath_WhenProvidedInSettings()
    {
        var path = XmlPathResolver.Resolve("\"./dump.xml\"", isInputRedirected: false, () => "unused");

        await Assert.That(path).IsEqualTo("./dump.xml");
    }

    [Test]
    public async Task Resolve_Throws_WhenMissingPathAndInputIsRedirected()
    {
        var action = () => XmlPathResolver.Resolve(null, isInputRedirected: true, () => "unused");

        await Assert.That(action).Throws<InvalidOperationException>();
    }

    [Test]
    public async Task Resolve_UsesPrompt_WhenMissingPathAndInteractive()
    {
        var path = XmlPathResolver.Resolve(null, isInputRedirected: false, () => "'./prompted.xml'");

        await Assert.That(path).IsEqualTo("./prompted.xml");
    }
}
