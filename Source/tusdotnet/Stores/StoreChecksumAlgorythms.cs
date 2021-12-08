
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using tusdotnet.Interfaces;

namespace tusdotnet.Stores
{
    /// <summary>
    /// Supported checksum algorithms of a <see cref="ITusStore"/>
    /// </summary>
    public class StoreChecksumAlgorithms
    {
        private readonly Lazy<Task<IEnumerable<string>>> _getSupportedChecksumAlgorithms;

        /// <summary>
        /// Initialize a new instance of <see cref="StoreChecksumAlgorithms"/>
        /// </summary>
        public StoreChecksumAlgorithms(Func<CancellationToken, Task<IEnumerable<string>>> getSupportedChecksumAlgorithmsFunc)
        {
            _getSupportedChecksumAlgorithms = new Lazy<Task<IEnumerable<string>>>(() => getSupportedChecksumAlgorithmsFunc(default));
        }

        /// <summary>
        /// Initialize a new instance of <see cref="StoreChecksumAlgorithms"/>
        /// </summary>
        public StoreChecksumAlgorithms()
        {
            _getSupportedChecksumAlgorithms = new Lazy<Task<IEnumerable<string>>>(() => Task.FromResult<IEnumerable<string>>(new List<string>(0)));
        }

        /// <summary>
        /// Returns an enumerable of all supported checksum algorithms
        /// </summary>
        public Task<IEnumerable<string>> AsEnumerableAsync()
        {
            return _getSupportedChecksumAlgorithms.Value;
        }

        /// <summary>
        /// Returns a list of all supported checksum algorithms
        /// </summary>
        public async Task<List<string>> ToListAsync()
        {
            return (await AsEnumerableAsync()).ToList();
        }
    }
}
