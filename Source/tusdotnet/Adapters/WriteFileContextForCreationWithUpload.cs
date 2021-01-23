﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using tusdotnet.Constants;
using tusdotnet.Helpers;
using tusdotnet.IntentHandlers;
using tusdotnet.Models;
using tusdotnet.Models.Concatenation;
using tusdotnet.Models.Configuration;

namespace tusdotnet.Adapters
{
    internal sealed class WriteFileContextForCreationWithUpload
    {
        internal DateTimeOffset? UploadExpires => GetUploadExpires();

        internal bool FileContentIsAvailable { get; }

        private readonly ContextAdapter _context;

        private readonly Dictionary<string, string> _responseHeaders;

        private readonly Stream _responseStream;
        private readonly string _fileId;

#if NETCOREAPP3_0
        internal static Task<WriteFileContextForCreationWithUpload> FromCreationContext(ContextAdapter creationContext, string fileId)
        {
            // Check if there is any file content available in the request.
            bool hasData;
            if (creationContext.Request.BodyReader.TryRead(out var result))
            {
                hasData = !result.IsCanceled && result.Buffer.Length > 0;

                creationContext.Request.BodyReader.AdvanceTo(result.Buffer.Start);
            }
            else hasData = false;

            return Task.FromResult(new WriteFileContextForCreationWithUpload(creationContext, fileId, hasData));
        }

#else
        internal static async Task<WriteFileContextForCreationWithUpload> FromCreationContext(ContextAdapter creationContext, string fileId)
        {
            // Check if there is any file content available in the request.
            var buffer = new byte[1];
            var hasData = await creationContext.Request.Body.ReadAsync(buffer, 0, 1) > 0;

            return new WriteFileContextForCreationWithUpload(creationContext, fileId, hasData, buffer[0]);
        }
#endif

        /// <summary>
        /// Creates an internal WriteFile request and runs it. Returns the Upload-Offset for the file or 0 if something failed.
        /// The <code>UploadExpires</code> property is also updated if sliding expiration is used.
        /// </summary>
        /// <param name="fileConcat">Null for regular files or FileConcatPartial if the file is a partial file</param>
        /// <returns>The Upload-Offset for the file or 0 if something failed.</returns>
        internal async Task<long> SaveFileContent(FileConcat fileConcat = null)
        {
            var authResponse = await EventHelper.Validate<AuthorizeContext>(_context, ctx =>
            {
                ctx.Intent = IntentType.WriteFile;
                ctx.FileConcatenation = fileConcat;
            });

            if (authResponse == ResultType.StopExecution)
            {
                return 0;
            }

            var writeFileHandler = new WriteFileHandler(_context, initiatedFromCreationWithUpload: true);

            if (!await writeFileHandler.Validate())
            {
                return 0;
            }

            try
            {
                await writeFileHandler.Invoke();
            }
            catch
            {
                // Left blank
            }

            return GetUploadOffset() ?? (await _context.Configuration.Store.GetUploadOffsetAsync(_fileId, _context.CancellationToken));
        }

        private WriteFileContextForCreationWithUpload(ContextAdapter creationContext, string fileId, bool hasData, byte? preReadByte = null)
        {
            _responseHeaders = new Dictionary<string, string>();
            _responseStream = new MemoryStream();
            _fileId = fileId;

            FileContentIsAvailable = hasData;

            _context = new ContextAdapter
            {
                CancellationToken = creationContext.CancellationToken,
                Configuration = creationContext.Configuration,
                HttpContext = creationContext.HttpContext,
                Request = CreateWriteFileRequest(creationContext, fileId, preReadByte),
                Response = CreateWriteFileResponse(),
#if netfull
                OwinContext = creationContext.OwinContext
#endif
            };
        }

        private ResponseAdapter CreateWriteFileResponse()
        {
            return new ResponseAdapter
            {
                Body = _responseStream,
                SetHeader = (key, value) => _responseHeaders[key] = value,
                SetStatus = _ => { }
            };
        }

        private RequestAdapter CreateWriteFileRequest(ContextAdapter context, string fileId, byte? preReadByte = null)
        {
            var uriBuilder = new UriBuilder(context.Request.RequestUri);
            uriBuilder.Path = uriBuilder.Path + "/" + fileId;

            RequestAdapter writeFileRequest;
            if (preReadByte.HasValue)
            {
                writeFileRequest = new RequestAdapter(context.Configuration.UrlPath)
                {
                    Body = new ReadOnlyStreamWithPreReadByte(context.Request.Body, preReadByte.Value),
                    Method = context.Request.Method,
                    RequestUri = uriBuilder.Uri,
                    Headers = context.Request.Headers
                };
            }
            else
            {
                writeFileRequest = new RequestAdapter(context.Configuration.UrlPath)
                {
                    Body = context.Request.Body,
#if NETCOREAPP3_0
                    BodyReader = context.Request.BodyReader,
#endif
                    Method = context.Request.Method,
                    RequestUri = uriBuilder.Uri,
                    Headers = context.Request.Headers
                };
            }

            writeFileRequest.Headers[HeaderConstants.UploadOffset] = new List<string>(1) { "0" };
            writeFileRequest.Headers.Remove(HeaderConstants.UploadLength);

            return writeFileRequest;
        }

        private long? GetUploadOffset()
        {
            if (!_responseHeaders.TryGetValue(HeaderConstants.UploadOffset, out var uploadOffset))
            {
                return null;
            }

            if (long.TryParse(uploadOffset, out var i))
            {
                return i;
            }

            return null;
        }

        private DateTimeOffset? GetUploadExpires()
        {
            if (!_responseHeaders.TryGetValue(HeaderConstants.UploadExpires, out var uploadExpires))
            {
                return null;
            }

            if (DateTimeOffset.TryParseExact(uploadExpires, "R", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
            {
                return d;
            }

            return null;
        }
    }
}
