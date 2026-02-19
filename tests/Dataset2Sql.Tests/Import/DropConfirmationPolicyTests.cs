namespace Develix.Dataset2Sql.Tests.Import;

public class DropConfirmationPolicyTests
{
    [Test]
    public async Task ShouldDrop_ReturnsTrue_WhenAutoConfirmIsEnabled()
    {
        var shouldDrop = DropConfirmationPolicy.ShouldDrop("DumpDb", autoConfirmDrop: true, () => "wrong");

        await Assert.That(shouldDrop).IsTrue();
    }

    [Test]
    public async Task ShouldDrop_ReturnsTrue_WhenResponseMatchesExactly()
    {
        var shouldDrop = DropConfirmationPolicy.ShouldDrop("DumpDb", autoConfirmDrop: false, () => "DumpDb");

        await Assert.That(shouldDrop).IsTrue();
    }

    [Test]
    public async Task ShouldDrop_ReturnsFalse_WhenResponseDoesNotMatch()
    {
        var shouldDrop = DropConfirmationPolicy.ShouldDrop("DumpDb", autoConfirmDrop: false, () => "dumpdb");

        await Assert.That(shouldDrop).IsFalse();
    }
}
