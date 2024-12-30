using Microsoft.AspNetCore.Http;
using System;
using tusdotnet.Constants;

namespace tusdotnet.Helpers
{
    internal class HeaderHelper
    {
        internal static void SetCommonHeaders(HttpContext context, DateTimeOffset? expires, long? uploadOffset)
        {
            if (expires != null)
            {
                context.Response.Headers.Add(HeaderConstants.UploadExpires, expires.Value.ToString("R"));
            }

            if (uploadOffset != null)
            {
                context.Response.Headers.Add(HeaderConstants.UploadOffset, uploadOffset.Value.ToString());
            }
        }

        internal static void SetTusResumableHeader(HttpContext context)
        {
            context.Response.Headers.Add(HeaderConstants.TusResumable, HeaderConstants.TusResumableValue);
        }

        internal static void SetCacheNoStoreHeader(HttpContext context)
        {
            context.Response.Headers.Add(HeaderConstants.CacheControl, HeaderConstants.NoStore);
        }

        internal static void SetCacheNoCacheHeader(HttpContext context)
        {
            context.Response.Headers.Add(HeaderConstants.CacheControl, HeaderConstants.NoCache);
        }
    }
}
