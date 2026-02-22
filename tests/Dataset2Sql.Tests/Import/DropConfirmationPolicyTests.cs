namespace Develix.Dataset2Sql.Tests.Import;

public class DropConfirmationPolicyTests
{
    [Test]
    public async Task ShouldDrop_WhenAutoConfirmIsEnabled_ThenReturnsTrue()
    {
        var shouldDrop = DropConfirmationPolicy.ShouldDrop("DumpDb", autoConfirmDrop: true, () => "wrong");

        await Assert.That(shouldDrop).IsTrue();
    }

    [Test]
    public async Task ShouldDrop_WhenResponseMatchesExactly_ThenReturnsTrue()
    {
        var shouldDrop = DropConfirmationPolicy.ShouldDrop("DumpDb", autoConfirmDrop: false, () => "DumpDb");

        await Assert.That(shouldDrop).IsTrue();
    }

    [Test]
    public async Task ShouldDrop_WhenResponseDoesNotMatch_ThenReturnsFalse()
    {
        var shouldDrop = DropConfirmationPolicy.ShouldDrop("DumpDb", autoConfirmDrop: false, () => "dumpdb");

        await Assert.That(shouldDrop).IsFalse();
    }
}
