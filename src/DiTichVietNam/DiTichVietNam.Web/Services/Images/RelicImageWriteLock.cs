using System.Collections.Concurrent;

namespace DiTichVietNam.Web.Services.Images;

public class RelicImageWriteLock
{
    private readonly ConcurrentDictionary<int, SemaphoreSlim> _gates = new();

    public async Task<IDisposable> AcquireAsync(int relicId)
    {
        var gate = _gates.GetOrAdd(relicId, _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync();

        return new Release(gate);
    }

    private sealed class Release : IDisposable
    {
        private readonly SemaphoreSlim _gate;
        private bool _released;

        public Release(SemaphoreSlim gate)
        {
            _gate = gate;
        }

        public void Dispose()
        {
            if (_released)
            {
                return;
            }

            _released = true;
            _gate.Release();
        }
    }
}
