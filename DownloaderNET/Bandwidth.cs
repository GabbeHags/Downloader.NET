namespace DownloaderNET;

internal class Bandwidth
{
    private Double _speed = 0;
    public Double Speed
    {
        get => _speed;
        set
        {
            Interlocked.Exchange(ref _speed, value);
        }
    }
    private const Double OneSecond = 1000;

    private Int64 _bandwidthLimit;

    private Int32 _lastSecondCheckpoint;
    private Int64 _lastTransferredBytesCount;
    private Int32 _speedRetrieveTime;

    public Bandwidth(Int32 bandwidthLimit)
    {
        _bandwidthLimit = bandwidthLimit > 0 ? bandwidthLimit : Int64.MaxValue;

        SecondCheckpoint();
    }

    public void SetBandwidthLimit(Int32 newBandwidthLimit)
    {
        Interlocked.Exchange(ref _bandwidthLimit, newBandwidthLimit > 0 ? newBandwidthLimit : Int64.MaxValue);
    }

    public void CalculateSpeed(Int64 receivedBytesCount)
    {
        var elapsedTime = Environment.TickCount - _lastSecondCheckpoint + 1;
        receivedBytesCount = Interlocked.Add(ref _lastTransferredBytesCount, receivedBytesCount);
        var momentSpeed = receivedBytesCount * OneSecond / elapsedTime;

        if (OneSecond < elapsedTime)
        {
            Speed = momentSpeed;
            SecondCheckpoint();
        }

        if (momentSpeed >= _bandwidthLimit)
        {
            var expectedTime = receivedBytesCount * OneSecond / _bandwidthLimit;
            Interlocked.Add(ref _speedRetrieveTime, (Int32)expectedTime - elapsedTime);
        }
    }

    public Int32 PopSpeedRetrieveTime()
    {
        return Interlocked.Exchange(ref _speedRetrieveTime, 0);
    }

    private void SecondCheckpoint()
    {
        Interlocked.Exchange(ref _lastSecondCheckpoint, Environment.TickCount);
        Interlocked.Exchange(ref _lastTransferredBytesCount, 0);
    }
}
