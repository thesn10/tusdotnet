#if endpointrouting
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using tusdotnet.Constants;
using tusdotnet.Interfaces;
using tusdotnet.Models;
using System.Threading;
using tusdotnet.Extensions;

#if trailingheaders

using Microsoft.AspNetCore.Http;
using tusdotnet.Extensions.Internal;

#endif

namespace tusdotnet.Helpers
{
    internal class ChecksumHelper
    {
        private static ChecksumHelperResult Ok { get; } = new(HttpStatusCode.OK, null);

        private readonly HttpContext _context;
        //private readonly ITusChecksumStore _checksumStore;
        public Lazy<Task<List<string>>> _supportedAlgorithms;

#if trailingheaders

        private bool _checksumOriginatesFromTrailer;
        private ChecksumHelperResult _checksumTrailerParseResult;
        private Checksum? _checksum;

#else

        private readonly Checksum _checksum;

#endif

        public ChecksumHelper(HttpContext context)
        {
            _context = context;

            //_checksumStore = _context.Configuration.Store as ITusChecksumStore;

            //if (_checksumStore == null)
            //    return;

            //_supportedAlgorithms = new Lazy<Task<List<string>>, ITusChecksumStore>(LoadSupportAlgorithms(store));

            var checksumHeader = _context.Request.GetHeader(HeaderConstants.UploadChecksum);

            if (checksumHeader != null)
            {
                _checksum = new Checksum(checksumHeader);
            }
        }
        
        internal bool IsSupported(ITusStore store) => store is ITusChecksumStore;

#if trailingheaders && false

        private static async Task SetChecksumFromTrailingHeader(HttpContext context)
        {
            if (_checksum != null)
                return;

            var checksumHeader = context.Request.GetTrailingUploadChecksumHeader();

            if (string.IsNullOrEmpty(checksumHeader))
            {
                // Fallback to force the store to discard the chunk.
                if (clientDisconnected && _context.Request.HasDeclaredTrailingUploadChecksumHeader())
                {
                    _checksumOriginatesFromTrailer = true;
                    _checksumTrailerParseResult = Ok;
                    _checksum = ChecksumTrailerHelper.TrailingChecksumToUseIfRealTrailerIsFaulty;
                }

                return;
            }

            var tempChecksum = new Checksum(checksumHeader);

            _checksumOriginatesFromTrailer = true;
            _checksumTrailerParseResult = VerifyHeader(tempChecksum);

            // Fallback to force the store to discard the chunk.
            if (_checksumTrailerParseResult.IsFailure())
            {
                tempChecksum = ChecksumTrailerHelper.TrailingChecksumToUseIfRealTrailerIsFaulty;
            }

            _checksumOriginatesFromTrailer = true;
            _checksum = tempChecksum;
        }

        internal bool SupportsChecksumTrailer() => true;
#else 

        internal ChecksumHelperResult VerifyStateForChecksumTrailer() => Ok;

        internal bool SupportsChecksumTrailer() => false;

#endif

        internal async Task<ChecksumHelperResult> MatchChecksum(ITusChecksumStore? store, string fileId, CancellationToken cancellationToken = default)
        {
            if (store == null)
            {
                return Ok;
            }

            var errorResponse = new ChecksumHelperResult((HttpStatusCode)460, "Header Upload-Checksum does not match the checksum of the file");

#if trailingheaders && false

            await SetChecksumFromTrailingHeader(clientDisconnected);

            if (_checksumOriginatesFromTrailer && _checksumTrailerParseResult.IsFailure())
            {
                errorResponse = _checksumTrailerParseResult;
            }
#endif

            if (_checksum == null)
            {
                return Ok;
            }

            var result = await store.VerifyChecksumAsync(fileId, _checksum.Algorithm, _checksum.Hash, cancellationToken);

            if (!result)
            {
                return errorResponse;
            }

            return Ok;
        }

        internal struct ChecksumHelperResult
        {
            internal HttpStatusCode Status { get; }

            internal string ErrorMessage { get; }

            internal ChecksumHelperResult(HttpStatusCode status, string errorMessage)
            {
                Status = status;
                ErrorMessage = errorMessage;
            }

            internal bool IsSuccess() => Status == HttpStatusCode.OK;

            internal bool IsFailure() => !IsSuccess();
        }
    }
}
#endif