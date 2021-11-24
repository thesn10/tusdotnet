using System;
using System.Threading;
using System.Threading.Tasks;
using tusdotnet.Interfaces;
using tusdotnet.Models;
using tusdotnet.Models.Expiration;

namespace tusdotnet.Helpers
{
    internal class ExpirationHelper
    {
        private readonly ITusExpirationStore _expirationStore;
        private readonly ExpirationBase _expiration;
        private readonly bool _isSupported;
        private readonly Func<DateTimeOffset> _getSystemTime;

        public bool IsSlidingExpiration => _expiration is SlidingExpiration;

        internal ExpirationHelper(ITusExpirationStore store, ExpirationBase expiration, Func<DateTimeOffset> getSystemTime)
        {
            _expirationStore = store;
            _expiration = expiration;
            _isSupported = _expirationStore != null && _expiration != null;
            _getSystemTime = getSystemTime;
        }

        internal async Task<DateTimeOffset?> SetExpirationIfSupported(string fileId, CancellationToken cancellationToken)
        {
            if (!_isSupported)
            {
                return null;
            }

            var expires = _getSystemTime().Add(_expiration.Timeout);
            await _expirationStore.SetExpirationAsync(fileId, expires, cancellationToken);

            return expires;
        }

        internal Task<DateTimeOffset?> GetExpirationIfSupported(string fileId, CancellationToken cancellationToken)
        {
            if (!_isSupported)
            {
                return Task.FromResult<DateTimeOffset?>(null);
            }

            return _expirationStore.GetExpirationAsync(fileId, cancellationToken);
        }

        internal static string FormatHeader(DateTimeOffset? expires)
        {
            return expires?.ToString("R");
        }
    }
}