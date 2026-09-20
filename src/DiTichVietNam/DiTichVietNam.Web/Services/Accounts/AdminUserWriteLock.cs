namespace DiTichVietNam.Web.Services.Accounts;

public class AdminUserWriteLock
{
    private readonly SemaphoreSlim _gate = new(1, 1);

    public async Task<IDisposable> AcquireAsync()
    {
        await _gate.WaitAsync();

        return new Release(_gate);
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
