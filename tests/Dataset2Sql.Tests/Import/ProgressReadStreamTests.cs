using Develix.Dataset2Sql.Importing;

namespace Develix.Dataset2Sql.Tests.Import;

public class ProgressReadStreamTests
{
    [Test]
    public async Task Read_WhenReadingChunks_ThenReportsCumulativeBytes()
    {
        using var innerStream = new MemoryStream(new byte[10]);
        var reportedBytes = new List<long>();
        using var stream = new ProgressReadStream(innerStream, reportedBytes.Add, CancellationToken.None);

        var buffer = new byte[4];
        var firstRead = stream.Read(buffer, 0, buffer.Length);
        var secondRead = stream.Read(buffer, 0, buffer.Length);
        var thirdRead = stream.Read(buffer, 0, buffer.Length);
        var endOfStreamRead = stream.Read(buffer, 0, buffer.Length);

        await Assert.That(firstRead).IsEqualTo(4);
        await Assert.That(secondRead).IsEqualTo(4);
        await Assert.That(thirdRead).IsEqualTo(2);
        await Assert.That(endOfStreamRead).IsEqualTo(0);
        await Assert.That(reportedBytes.Count).IsEqualTo(3);
        await Assert.That(reportedBytes[0]).IsEqualTo(4);
        await Assert.That(reportedBytes[1]).IsEqualTo(8);
        await Assert.That(reportedBytes[2]).IsEqualTo(10);
    }

    [Test]
    public async Task Read_WhenCancellationRequested_ThenThrowsOperationCanceledException()
    {
        using var innerStream = new MemoryStream([1, 2, 3]);
        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        using var stream = new ProgressReadStream(innerStream, _ => { }, cancellationTokenSource.Token);
        void ReadAction() => stream.ReadByte();

        await Assert.That(ReadAction).Throws<OperationCanceledException>();
    }
}
