namespace Api.Services.AiProductMatch;

/// <summary>
/// Bounds expensive AI image analysis across every seller. The request rate
/// limiter protects each account; this gate prevents many valid accounts from
/// exhausting the process and the external model at the same time.
/// </summary>
public sealed class AiProductMatchExecutionGate
{
    private readonly SemaphoreSlim _slots = new(initialCount: 4, maxCount: 4);

    public IDisposable? TryAcquire()
    {
        if (!_slots.Wait(0))
            return null;

        return new Lease(_slots);
    }

    private sealed class Lease : IDisposable
    {
        private SemaphoreSlim? _slots;

        public Lease(SemaphoreSlim slots) => _slots = slots;

        public void Dispose() => Interlocked.Exchange(ref _slots, null)?.Release();
    }
}
