namespace Develix.Dataset2Sql.Importing;

internal sealed class ProgressReadStream(Stream innerStream, Action<long> reportProgress, CancellationToken cancellationToken) : Stream
{
    private readonly Stream innerStream = innerStream;
    private readonly Action<long> reportProgress = reportProgress;
    private readonly CancellationToken cancellationToken = cancellationToken;

    private long totalBytesRead;

    public override int Read(byte[] buffer, int offset, int count)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var bytesRead = innerStream.Read(buffer, offset, count);
        return OnBytesRead(bytesRead);
    }

    public override int Read(Span<byte> buffer)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var bytesRead = innerStream.Read(buffer);
        return OnBytesRead(bytesRead);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            innerStream.Dispose();

        base.Dispose(disposing);
    }

    public override async ValueTask DisposeAsync()
    {
        await innerStream.DisposeAsync();
        await base.DisposeAsync();
    }

    private int OnBytesRead(int bytesRead)
    {
        if (bytesRead <= 0)
            return bytesRead;

        totalBytesRead += bytesRead;
        reportProgress(totalBytesRead);
        return bytesRead;
    }

    #region Stream wrapper

    public override bool CanRead => innerStream.CanRead;
    public override bool CanSeek => innerStream.CanSeek;
    public override bool CanWrite => innerStream.CanWrite;
    public override long Length => innerStream.Length;
    public override long Position { get => innerStream.Position; set => innerStream.Position = value; }
    public override void Flush() => innerStream.Flush();
    public override long Seek(long offset, SeekOrigin origin) => innerStream.Seek(offset, origin);
    public override void SetLength(long value) => innerStream.SetLength(value);
    public override void Write(byte[] buffer, int offset, int count) => innerStream.Write(buffer, offset, count);
    public override void Write(ReadOnlySpan<byte> buffer) => innerStream.Write(buffer);

    #endregion
}
