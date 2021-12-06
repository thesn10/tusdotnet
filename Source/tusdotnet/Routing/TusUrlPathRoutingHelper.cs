using Microsoft.AspNetCore.Http;
using System;
using System.Runtime.CompilerServices;

namespace tusdotnet.Routing
{
    /// <summary>
    /// Provides helper methods for simple url path routing
    /// </summary>
    internal class TusUrlPathRoutingHelper : ITusRoutingHelper
    {
        private readonly string _urlPath;
        private readonly HttpContext _httpContext;

        public TusUrlPathRoutingHelper(string urlPath, HttpContext context)
        {
            _urlPath = urlPath;
            _httpContext = context;
        }

        /// <inheritdoc />
        public string? GenerateFilePath(string fileId)
        {
            return $"{_urlPath.TrimEnd('/')}/{fileId}";
        }

        /// <inheritdoc />
        public string? GetFileId()
        {
            var startIndex = _httpContext.Request.Path.Value.IndexOf(_urlPath, StringComparison.OrdinalIgnoreCase) + _urlPath.Length;
            string fileId = _httpContext.Request.Path.Value.Substring(startIndex).Trim('/');

            if (string.IsNullOrWhiteSpace(fileId))
            {
                fileId = null;
            }
            return fileId;
        }

        /// <inheritdoc />
        public bool IsMatchingRoute()
        {
            return _httpContext.Request.Path.Value.TrimEnd('/').Equals(_urlPath.TrimEnd('/'), StringComparison.OrdinalIgnoreCase);
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <inheritdoc />
        public string? ParseFileId(string url)
        {
            var localPath = GetLocalPath(url);

            if (localPath.IsEmpty || !localPath.StartsWith(_urlPath))
            {
                return null;
            }

            return ExtractFileId(localPath, _urlPath).ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ReadOnlySpan<char> ExtractFileId(ReadOnlySpan<char> localPath, ReadOnlySpan<char> urlPath)
        {
            var fileId = localPath[urlPath.Length..].Trim('/');
            var indexOfQuestionMarkOrHash = fileId.IndexOfAny('?', '#');

            return indexOfQuestionMarkOrHash != -1
                ? fileId[0..indexOfQuestionMarkOrHash]
                : fileId;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ReadOnlySpan<char> GetLocalPath(ReadOnlySpan<char> fileUri)
        {
            if (fileUri.IsEmpty)
                return ReadOnlySpan<char>.Empty;

            var result = TryParseLocalPathFromFullUri(fileUri);

            if (!result.IsEmpty)
                return result;

            return fileUri;
        }

        private static readonly ReadOnlyMemory<char> _httpProtocol = "http://".AsMemory();
        private static readonly ReadOnlyMemory<char> _httpsProtocol = "https://".AsMemory();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ReadOnlySpan<char> TryParseLocalPathFromFullUri(ReadOnlySpan<char> fileUri)
        {
            var isHttp = fileUri.StartsWith(_httpProtocol.Span);
            bool isHttps = false;

            if (!isHttp)
            {
                isHttps = fileUri.StartsWith(_httpsProtocol.Span);
            }

            if (isHttp || isHttps)
            {
                var protocolLength = isHttp ? 7 : 8; // "http://" == 7
                var httpUri = fileUri[protocolLength..];
                var indexOfSlash = httpUri.IndexOf('/');
                if (indexOfSlash == -1)
                    return ReadOnlySpan<char>.Empty;

                httpUri = httpUri[indexOfSlash..];
                return httpUri;
            }

            return ReadOnlySpan<char>.Empty;
        }
#else
        /// <inheritdoc />
        public string? ParseFileId(string url)
        {
            if (string.IsNullOrWhiteSpace(url) || !Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out Uri uri))
            {
                return null;
            }

            var localPath = uri.IsAbsoluteUri
                ? uri.LocalPath
                : uri.ToString();

            if (!localPath.StartsWith(_urlPath, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return localPath.Substring(_urlPath.Length).Trim('/');
        }
#endif
    }
}
