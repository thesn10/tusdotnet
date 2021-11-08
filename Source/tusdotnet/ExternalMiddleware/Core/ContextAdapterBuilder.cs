using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tusdotnet.Adapters;
using tusdotnet.Models;

#if endpointrouting
using Microsoft.AspNetCore.Routing;
#endif

namespace tusdotnet.ExternalMiddleware.Core
{
    internal static class ContextAdapterBuilder
    {
        internal static ContextAdapter FromHttpContext(HttpContext context, DefaultTusConfiguration config)
        {
            var requestUri = GetRequestUri(context);

            var request = new RequestAdapter(config.UrlPath)
            {
                Headers =
                    context.Request.Headers.ToDictionary(
                        f => f.Key,
                        f => f.Value.ToList(),
                        StringComparer.OrdinalIgnoreCase),
                Body = context.Request.Body,
                Method = context.Request.Method,
                RequestUri = requestUri
            };

            var response = new ResponseAdapter
            {
                Body = context.Response.Body,
                SetHeader = (key, value) => context.Response.Headers[key] = value,
                SetStatus = status => context.Response.StatusCode = (int)status
            };

            return new ContextAdapter
            {
                Request = request,
                Response = response,
                Configuration = config,
                CancellationToken = context.RequestAborted,
                HttpContext = context
            };
        }

        internal static Uri GetRequestUri(HttpContext context)
        {
            return new Uri($"{context.Request.Scheme}://{context.Request.Host}{context.Request.PathBase}{context.Request.Path}");
        }

#if endpointrouting

        internal static ContextAdapter CreateFakeContextAdapter(HttpContext context, DefaultTusConfiguration config)
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

            config.UrlPath = urlPath;

            var adapter = ContextAdapterBuilder.FromHttpContext(context, config);

            return adapter;
        }

#endif
    }
}
