namespace Develix.Dataset2Sql.Tests;

public class ProgramTests
{
    [Test]
    [MethodDataSource(nameof(GetShouldPauseOnExitTestData))]
    public async Task ShouldPauseOnExit(string[] args, bool isInputRedirected, bool isOutputRedirected, bool expectedShouldPauseOnExit)
    {
        var shouldPause = Program.ShouldPauseOnExit(args, isInputRedirected, isOutputRedirected);

        await Assert.That(shouldPause).IsEqualTo(expectedShouldPauseOnExit);
    }

    public static IEnumerable<Func<(string[], bool, bool, bool)>> GetShouldPauseOnExitTestData()
    {
        yield return () => ([], false, false, true);
        yield return () => (["--xml", "./dump.xml"], false, false, false);
        yield return () => ([], true, false, false);
        yield return () => ([], false, true, false);
    }
}
