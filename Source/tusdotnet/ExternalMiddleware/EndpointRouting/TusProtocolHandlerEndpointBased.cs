#if endpointrouting

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using tusdotnet.Adapters;
using tusdotnet.Constants;
using tusdotnet.ExternalMiddleware.Core;
using tusdotnet.Models;
using tusdotnet.Models.Concatenation;
using tusdotnet.Parsers;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    internal class TusProtocolHandlerEndpointBased<TController> : TusProtocolHandlerEndpointBased
        where TController : TusControllerBase
    {
        internal TusProtocolHandlerEndpointBased(TusEndpointOptions options)
            : base(options)
        {
        }

        internal Task Invoke(HttpContext context)
        {
            var controller = (TusControllerBase)context.RequestServices.GetRequiredService<TController>();
            return Invoke(context, controller);
        }
    }

    internal class TusProtocolHandlerEndpointBased<TController, TControllerOptions> : TusProtocolHandlerEndpointBased
        where TController : TusControllerBase
    {
        private readonly TControllerOptions _controllerOptions;

        internal TusProtocolHandlerEndpointBased(TusEndpointOptions options, TControllerOptions controllerOptions) 
            : base(options)
        {
            _controllerOptions = controllerOptions;
        }

        internal Task Invoke(HttpContext context)
        {
            var controller = (TusControllerBase)context.RequestServices.GetRequiredService<TController>();

            if (controller is IControllerWithOptions<TControllerOptions> controllerWithOptions)
            {
                controllerWithOptions.Options = _controllerOptions;
            }

            return Invoke(context, controller);
        }
    }

    internal abstract class TusProtocolHandlerEndpointBased
    {
        private readonly TusEndpointOptions _options;

        internal TusProtocolHandlerEndpointBased(TusEndpointOptions options)
        {
            _options = options;
        }


        internal async Task Invoke(HttpContext context, TusControllerBase controller)
        {
            // Inject HttpContext into the controller
            controller.HttpContext = context;

            var contextAdapter = CreateFakeContextAdapter(context);
            var responseStream = new MemoryStream();
            var responseHeaders = new Dictionary<string, string>();
            HttpStatusCode? responseStatus = null;
            contextAdapter.Response = new ResponseAdapter
            {
                Body = responseStream,
                SetHeader = (key, value) => responseHeaders[key] = value,
                SetStatus = status => responseStatus = status
            };

            var controllerCapabilities = await controller.GetCapabilities();
            var intentType = IntentAnalyzer.DetermineIntent(contextAdapter, controllerCapabilities.SupportedExtensions);

            if (intentType == IntentType.NotApplicable)
            {
                // Cannot determine intent so return not found.
                context.Response.StatusCode = 404;
                return;
            }

            // TODO:
            // 1. Seperate request validation from file storage validation
            // 2. Validate request here
            // 3. Validate file storage in StorageService

            //var valid = await intentHandler.Validate();

            /*if (!valid)
            {
                // TODO: Optimize as there is not much worth in writing to a stream and then piping it to the response.
                context.Response.StatusCode = (int)responseStatus.Value;
                responseStream.Seek(0, SeekOrigin.Begin);
                await context.Response.BodyWriter.WriteAsync(responseStream.GetBuffer(), context.RequestAborted);

                return;
            }*/

            IActionResult result = null;
            IDictionary<string, string> headers = null;

            switch (intentType)
            {
                case IntentType.CreateFile:
                    (result, headers) = await HandleCreate(context, controller);
                    break;
                case IntentType.WriteFile:
                    (result, headers) = await HandleWriteFile(context, controller);
                    break;
                case IntentType.GetFileInfo:
                    (result, headers) = await HandleGetFileInfo(context, controller);
                    break;
                case IntentType.GetOptions:
                    (result, headers) = await HandleGetOptions(context, controller);
                    break;
            }

            await context.Respond(result, headers);
        }

        private async Task<(IActionResult result, IDictionary<string, string> headers)> HandleGetOptions(HttpContext context, TusControllerBase controller)
        {
            var result = new Dictionary<string, string>
            {
                {HeaderConstants.TusResumable, HeaderConstants.TusResumableValue },
                {HeaderConstants.TusVersion, HeaderConstants.TusResumableValue }
            };

            var maximumAllowedSize = _options.MaxAllowedUploadSizeInBytes;

            if (maximumAllowedSize.HasValue)
            {
                result.Add(HeaderConstants.TusMaxSize, maximumAllowedSize.Value.ToString());
            }

            var capabilities = await controller.GetCapabilities();
            if (capabilities.SupportedExtensions.Count > 0)
            {
                result.Add(HeaderConstants.TusExtension, string.Join(",", capabilities.SupportedExtensions));
            }

            if (capabilities.SupportedChecksumAlgorithms.Count > 0)
            {
                result.Add(HeaderConstants.TusChecksumAlgorithm, string.Join(",", capabilities.SupportedChecksumAlgorithms));
            }

            return (new NoContentResult(), result);
        }

        private async Task<(IActionResult result, IDictionary<string, string> headers)> HandleGetFileInfo(HttpContext context, TusControllerBase controller)
        {
            if (!await controller.AuthorizeForAction(context, nameof(controller.GetFileInfo)))
                return (new ForbidResult(), null);

            var fileId = (string)context.GetRouteValue("TusFileId");

            var result = new Dictionary<string, string>
            {
                {HeaderConstants.TusResumable, HeaderConstants.TusResumableValue },
                {HeaderConstants.CacheControl, HeaderConstants.NoStore }
            };

            var getInfoContext = new GetFileInfoContext()
            {
                FileId = fileId,
            };

            ITusInfoActionResult getInfoResult;
            try
            {
                getInfoResult = await controller.GetFileInfo(getInfoContext, context.RequestAborted);
            }
            catch (TusStoreException ex)
            {
                return (new BadRequestObjectResult(ex.Message), null);
            }

            if (getInfoResult is TusFail fail)
            {
                return (new BadRequestObjectResult(fail.Error), null);
            }

            if (getInfoResult is TusForbidden forbidden)
            {
                return (new ForbidResult(), null);
            }

            var getInfoOk = getInfoResult as TusInfoOk;

            if (getInfoOk == null)
            {
                throw new InvalidOperationException($"Unknown action result: {getInfoResult.GetType().FullName}");
            }

            if (!string.IsNullOrEmpty(getInfoOk.UploadMetadata))
            {
                result.Add(HeaderConstants.UploadMetadata, getInfoOk.UploadMetadata);
            }
            
            if (getInfoOk.UploadDeferLength)
            {
                result.Add(HeaderConstants.UploadDeferLength, "1");
            }
            else if (getInfoOk.UploadLength != null)
            {
                result.Add(HeaderConstants.UploadLength, getInfoOk.UploadLength.Value.ToString());
            }

            var addUploadOffset = true;
            if (getInfoOk.UploadConcat != null)
            {
                // Only add Upload-Offset to final files if they are complete.
                if (getInfoOk.UploadConcat is FileConcatFinal && getInfoOk.UploadLength != getInfoOk.UploadOffset)
                {
                    addUploadOffset = false;
                }
            }

            if (addUploadOffset)
            {
                result.Add(HeaderConstants.UploadOffset, getInfoOk.UploadOffset.ToString());
            }

            if (getInfoOk.UploadConcat != null)
            {
                (getInfoOk.UploadConcat as FileConcatFinal)?.AddUrlPathToFiles(context.Request.GetDisplayUrl());
                result.Add(HeaderConstants.UploadConcat, getInfoOk.UploadConcat.GetHeader());
            }

            return (new NoContentResult(), result);
        }

        private async Task<(IActionResult content, IDictionary<string, string> headers)> HandleWriteFile(HttpContext context, TusControllerBase controller)
        {
            if (!await controller.AuthorizeForAction(context, nameof(controller.Write)))
                return (new ForbidResult(), null);

            long? uploadLength = null;
            if (context.Request.Headers.ContainsKey(HeaderConstants.UploadLength))
            {
                uploadLength = long.Parse(context.Request.Headers[HeaderConstants.UploadLength].First());
            }

            long? uploadOffset = null;
            if (context.Request.Headers.ContainsKey(HeaderConstants.UploadOffset))
            {
                uploadOffset = long.Parse(context.Request.Headers[HeaderConstants.UploadOffset].First());
            }
            else
            {
                // CreationWithUpload
                //uploadOffset = 0;
            }

            var writeContext = new WriteContext
            {
                FileId = (string)context.GetRouteValue("TusFileId"),
                // Callback to later support trailing checksum headers
                GetChecksumProvidedByClient = () => GetChecksumFromContext(context),
                RequestStream = context.Request.Body,
                UploadOffset = uploadOffset,
                UploadLength = uploadLength
            };

            ITusWriteActionResult writeResult = null;
            try
            {
                writeResult = await controller.Write(writeContext, context.RequestAborted);
            }
            catch (TusFileAlreadyInUseException ex)
            {
                return (new ConflictObjectResult(ex.Message), null);
            }
            catch (TusStoreException ex)
            {
                return (new BadRequestObjectResult(ex.Message), null);
            }

            if (writeResult is TusFail fail)
            {
                return (new BadRequestObjectResult(fail.Error), null);
            }
            else if (writeResult is TusForbidden forbidden)
            {
                return (new ForbidResult(), null);
            }

            var writeOk = writeResult as TusWriteOk;

            if (writeOk == null)
            {
                throw new InvalidOperationException($"Unknown action result: {writeResult.GetType().FullName}");
            }

            if (writeOk.ClientDisconnectedDuringRead)
            {
                return (new OkResult(), null);
            }

            if (writeOk.IsComplete && !writeContext.IsPartialFile)
            {
                await controller.FileCompleted(new() { FileId = writeContext.FileId }, context.RequestAborted);
            }

            return (new NoContentResult(), GetCreateHeaders(writeOk.FileExpires, writeOk.UploadOffset));
        }

        private Checksum GetChecksumFromContext(HttpContext context)
        {
            var header = context.Request.Headers[HeaderConstants.UploadChecksum].FirstOrDefault();

            return header != null ? new Checksum(header) : null;
        }

        private async Task<(IActionResult content, IDictionary<string, string> headers)> HandleCreate(HttpContext context, TusControllerBase controller)
        {
            if (!await controller.AuthorizeForAction(context, nameof(controller.Create)))
                return (new ForbidResult(), null);

            // TODO: Replace with typed headers
            var metadata = context.Request.Headers[HeaderConstants.UploadMetadata].FirstOrDefault();
            var uploadLength = context.Request.Headers[HeaderConstants.UploadLength].FirstOrDefault();

            var createContext = new CreateContext
            {
                UploadMetadata = metadata,
                Metadata = MetadataParser.ParseAndValidate(_options.MetadataParsingStrategy, metadata).Metadata,
                UploadLength = long.Parse(uploadLength),
            };

            ITusCreateActionResult createResult;
            try
            {
                createResult = await controller.Create(createContext, context.RequestAborted);
            }
            catch (TusStoreException ex)
            {
                return (new BadRequestObjectResult(ex.Message), null);
            }

            if (createResult is TusFail fail)
            {
                return (new BadRequestObjectResult(fail.Error), null);
            }

            if (createResult is TusForbidden forbidden)
            {
                return (new ForbidResult(), null);
            }

            var createOk = createResult as TusCreateOk;

            if (createOk == null)
            {
                throw new InvalidOperationException($"Unknown action result: {createResult.GetType().FullName}");
            }

            var isEmptyFile = createContext.UploadLength == 0;

            if (isEmptyFile)
            {
                var completedResult = await controller.FileCompleted(new() { FileId = createOk.FileId }, context.RequestAborted);

                if (completedResult is TusFail completeFail)
                    return (new BadRequestObjectResult(completeFail.Error), null);

                var createdResult = new CreatedResult($"{context.Request.GetDisplayUrl().TrimEnd('/')}/{createOk.FileId}", null);

                return (createdResult, GetCreateHeaders(createOk.Expires, null));
            }
            else
            {
                // Creation with upload
                var (writeResult, headers) = await HandleWriteFile(context, controller);

                var createdResult = new CreatedResult($"{context.Request.GetDisplayUrl().TrimEnd('/')}/{createOk.FileId}", null);

                return (createdResult, headers);
            }
        }

        private Dictionary<string, string> GetCreateHeaders(DateTimeOffset? expires, long? uploadOffset)
        {
            var result = new Dictionary<string, string>();
            if (expires != null)
            {
                result.Add(HeaderConstants.UploadExpires, expires.Value.ToString("R"));
            }

            if (uploadOffset != null)
            {
                result.Add(HeaderConstants.UploadOffset, uploadOffset.Value.ToString());
            }

            result.Add(HeaderConstants.TusResumable, HeaderConstants.TusResumableValue);

            return result;
        }

        private ContextAdapter CreateFakeContextAdapter(HttpContext context)
        {
            var urlPath = (string)context.GetRouteValue("TusFileId");

            if (string.IsNullOrWhiteSpace(urlPath))
            {
                urlPath = context.Request.Path;
            }
            else
            {
                var span = context.Request.Path.ToString().TrimEnd('/').AsSpan();
                urlPath = span.Slice(0, span.LastIndexOf('/')).ToString();
            }

            var config = new DefaultTusConfiguration
            {
                UrlPath = urlPath
            };

            var adapter = ContextAdapterBuilder.FromHttpContext(context, config);

            return adapter;
        }
    }
}

#endif