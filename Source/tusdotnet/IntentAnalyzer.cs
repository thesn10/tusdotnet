using Microsoft.AspNetCore.Http;
using System;
using tusdotnet.Constants;
using tusdotnet.Models;
using tusdotnet.Stores;
using tusdotnet.Routing;

namespace tusdotnet
{
    internal static class IntentAnalyzer
    {
        public static IntentType DetermineIntent(TusContext context)
        {
            var httpMethod = GetHttpMethod(context.HttpContext.Request);

            if (RequestRequiresTusResumableHeader(httpMethod))
            {
                if (!context.HttpContext.Request.Headers.ContainsKey(HeaderConstants.TusResumable))
                {
                    return IntentType.NotApplicable;
                }
            }

            if (MethodRequiresFileIdUrl(httpMethod))
            {
                if (context.RoutingHelper.GetFileId() == null)
                {
                    return IntentType.NotApplicable;
                }
            }
            else if (!context.RoutingHelper.IsMatchingRoute())
            {
                return IntentType.NotApplicable;
            }

            return httpMethod switch
            {
                "post" => DetermineIntentForPost(context),
                "patch" => DetermineIntentForPatch(),
                "head" => DetermineIntentForHead(),
                "options" => DetermineIntentForOptions(),
                "delete" => DetermineIntentForDelete(context.FeatureSupportContext.SupportedExtensions),
                _ => IntentType.NotApplicable,
            };
        }

        /// <summary>
        /// Returns the request method taking X-Http-Method-Override into account.
        /// </summary>
        /// <param name="request">The request to get the method for</param>
        /// <returns>The request method</returns>
        private static string GetHttpMethod(HttpRequest request)
        {
            if (!request.Headers.TryGetValue(HeaderConstants.XHttpMethodOveride, out var method))
            {
                method = request.Method;
            }

            return method.ToString().ToLower();
        }
        
        private static bool MethodRequiresFileIdUrl(string httpMethod)
        {
            switch (httpMethod)
            {
                case "head":
                case "patch":
                case "delete":
                    return true;
                default:
                    return false;
            }
        }

        private static IntentType DetermineIntentForOptions()
        {
            return IntentType.GetOptions;
        }

        private static IntentType DetermineIntentForHead()
        {
            return IntentType.GetFileInfo;
        }
        
        private static IntentType DetermineIntentForPost(TusContext context)
        {
            var extensions = context.FeatureSupportContext.SupportedExtensions;

            if (!extensions.Creation)
                return IntentType.NotApplicable;

            var hasUploadConcatHeader = context.HttpContext.Request.Headers.ContainsKey(HeaderConstants.UploadConcat);

            if (extensions.Concatenation && hasUploadConcatHeader)
            {
                return IntentType.ConcatenateFiles;
            }

            return IntentType.CreateFile;
        }

        private static IntentType DetermineIntentForPatch()
        {
            return IntentType.WriteFile;
        }

        private static IntentType DetermineIntentForDelete(StoreExtensions extensions)
        {
            if (!extensions.Termination)
                return IntentType.NotApplicable;

            return IntentType.DeleteFile;
        }

        private static bool RequestRequiresTusResumableHeader(string httpMethod)
        {
            return httpMethod != "options";
        }

        private static bool UrlMatchesUrlPath(Uri requestUri, string configUrlPath)
        {
            return requestUri.LocalPath.TrimEnd('/').Equals(configUrlPath.TrimEnd('/'), StringComparison.OrdinalIgnoreCase);
        }

        private static bool UrlMatchesFileIdUrl(Uri requestUri, string configUrlPath)
        {
            return !UrlMatchesUrlPath(requestUri, configUrlPath)
                   && requestUri.LocalPath.StartsWith(configUrlPath, StringComparison.OrdinalIgnoreCase);
        }
    }
}
