#if endpointrouting
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tusdotnet.Adapters;
using tusdotnet.Constants;
using tusdotnet.Models;
using tusdotnet.Parsers;

namespace tusdotnet.ExternalMiddleware.EndpointRouting.RequestHandlers
{
    internal class CreateRequestHandler : RequestHandler
    {
        internal CreateRequestHandler(HttpContext context, TusControllerBase controller, TusEndpointOptions options)
            : base(context, controller, options)
        {

        }

        internal override async Task<IActionResult> Invoke()
        {
            if (!await _controller.AuthorizeForAction(_context, nameof(_controller.Create)))
                return new ForbidResult();

            // TODO: Replace with typed headers
            var metadata = _context.Request.Headers[HeaderConstants.UploadMetadata].FirstOrDefault();

            long uploadLength;
            if (_context.Request.Headers.ContainsKey(HeaderConstants.UploadLength))
            {
                uploadLength = long.Parse(_context.Request.Headers[HeaderConstants.UploadLength].First());
            }
            else
            {
                uploadLength = 0;
            }

            var createContext = new CreateContext
            {
                UploadMetadata = metadata,
                Metadata = MetadataParser.ParseAndValidate(_options.MetadataParsingStrategy, metadata).Metadata,
                UploadLength = uploadLength,
            };

            ITusCreateActionResult createResult;
            try
            {
                createResult = await _controller.Create(createContext, _context.RequestAborted);
            }
            catch (TusStoreException ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }

            if (createResult is TusFail fail)
            {
                return new BadRequestObjectResult(fail.Error);
            }

            if (createResult is TusForbidden forbidden)
            {
                return new ForbidResult();
            }

            var createOk = createResult as TusCreateOk;

            if (createOk == null)
            {
                throw new InvalidOperationException($"Unknown action result: {createResult.GetType().FullName}");
            }

            var isEmptyFile = createContext.UploadLength == 0;

            if (isEmptyFile)
            {
                SetTusResumableHeader();
                var completedResult = await _controller.FileCompleted(new() { FileId = createOk.FileId }, _context.RequestAborted);

                if (completedResult is TusFail completeFail)
                    return new BadRequestObjectResult(completeFail.Error);

                var createdResult = new CreatedResult($"{_context.Request.GetDisplayUrl().TrimEnd('/')}/{createOk.FileId}", null);

                SetCreateHeaders(createOk.Expires, null);
                return createdResult;
            }
            else
            {
                if (_context.Request.Headers.ContentLength > 0)
                {
                    // creation-with-upload
                    var writeHandler = new WriteRequestHandler(_context, _controller, _options, createOk.FileId);
                    await writeHandler.Invoke();
                }

                var createdResult = new CreatedResult($"{_context.Request.GetDisplayUrl().TrimEnd('/')}/{createOk.FileId}", null);

                return createdResult;
            }
        }
    }
}
#endif
