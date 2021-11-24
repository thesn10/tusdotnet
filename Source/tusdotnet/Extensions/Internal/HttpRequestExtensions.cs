using Microsoft.AspNetCore.Http;
using System.Linq;
using tusdotnet.Constants;

namespace tusdotnet.Extensions
{
    internal static class HttpRequestExtensions
    {
        internal static string GetHeader(this HttpRequest request, string name)
        {
            return request.Headers.ContainsKey(name) == true ? request.Headers[name].ToString() : null;
        }

#if trailingheaders

        internal static string? GetTrailingUploadChecksumHeader(this HttpRequest request)
        {
            if (!request.SupportsTrailers() || !request.CheckTrailersAvailable())
                return null;

            if (!request.HasDeclaredTrailingUploadChecksumHeader())
                return null;

            return request.GetTrailer(HeaderConstants.UploadChecksum).FirstOrDefault();
        }

        internal static bool HasDeclaredTrailingUploadChecksumHeader(this HttpRequest request)
        {
            return request.GetDeclaredTrailers().Any(x => x == HeaderConstants.UploadChecksum);
        }

#endif
    }
}
