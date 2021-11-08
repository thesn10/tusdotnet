using System;
using tusdotnet.Adapters;
using tusdotnet.Constants;
using tusdotnet.IntentHandlers;
using tusdotnet.Interfaces;
using tusdotnet.Models;
using tusdotnet.Stores;

namespace tusdotnet
{
    internal static class IntentAnalyzer
    {
        public static IntentType DetermineIntent(ContextAdapter context)
        {
            var httpMethod = GetHttpMethod(context.Request);

            if (RequestRequiresTusResumableHeader(httpMethod))
            {
                if (context.Request.GetHeader(HeaderConstants.TusResumable) == null)
                {
                    return IntentType.NotApplicable;
                }
            }

            if (MethodRequiresFileIdUrl(httpMethod))
            {
                if (!UrlMatchesFileIdUrl(context.Request.RequestUri, context.Configuration.UrlPath))
                {
                    return IntentType.NotApplicable;
                }
            }
            else if (!UrlMatchesUrlPath(context.Request.RequestUri, context.Configuration.UrlPath))
            {
                return IntentType.NotApplicable;
            }

            return httpMethod switch
            {
                "post" => DetermineIntentForPost(context),
                "patch" => DetermineIntentForPatch(),
                "head" => DetermineIntentForHead(),
                "options" => DetermineIntentForOptions(),
                "delete" => DetermineIntentForDelete(context),
                _ => IntentType.NotApplicable,
            };
        }

        public static IntentType DetermineIntent(ContextAdapter context, StoreExtensions extensions)
        {
            var httpMethod = GetHttpMethod(context.Request);

            if (RequestRequiresTusResumableHeader(httpMethod))
            {
                if (context.Request.GetHeader(HeaderConstants.TusResumable) == null)
                {
                    return IntentType.NotApplicable;
                }
            }

            if (MethodRequiresFileIdUrl(httpMethod))
            {
                if (!UrlMatchesFileIdUrl(context.Request.RequestUri, context.Configuration.UrlPath))
                {
                    return IntentType.NotApplicable;
                }
            }
            else if (!UrlMatchesUrlPath(context.Request.RequestUri, context.Configuration.UrlPath))
            {
                return IntentType.NotApplicable;
            }

            return httpMethod switch
            {
                "post" => DetermineIntentForPost(context, extensions),
                "patch" => DetermineIntentForPatch(),
                "head" => DetermineIntentForHead(),
                "options" => DetermineIntentForOptions(),
                "delete" => DetermineIntentForDelete(extensions),
                _ => IntentType.NotApplicable,
            };
        }

        /// <summary>
        /// Returns the request method taking X-Http-Method-Override into account.
        /// </summary>
        /// <param name="request">The request to get the method for</param>
        /// <returns>The request method</returns>
        private static string GetHttpMethod(RequestAdapter request)
        {
            var method = request.GetHeader(HeaderConstants.XHttpMethodOveride);

            if (string.IsNullOrWhiteSpace(method))
            {
                method = request.Method;
            }

            return method.ToLower();
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

        private static IntentType DetermineIntentForPost(ContextAdapter context)
        {
            if (!(context.Configuration.Store is ITusCreationStore creationStore))
                return IntentType.NotApplicable;

            var hasUploadConcatHeader = context.Request.Headers.ContainsKey(HeaderConstants.UploadConcat);

            if (context.Configuration.Store is ITusConcatenationStore tusConcatenationStore
                && hasUploadConcatHeader)
            {
                return IntentType.ConcatenateFiles;
            }

            return IntentType.CreateFile;
        }

        private static IntentType DetermineIntentForPost(ContextAdapter context, StoreExtensions extensions)
        {
            if (!extensions.Creation)
                return IntentType.NotApplicable;

            var hasUploadConcatHeader = context.Request.Headers.ContainsKey(HeaderConstants.UploadConcat);

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

        private static IntentType DetermineIntentForDelete(ContextAdapter context)
        {
            if (!(context.Configuration.Store is ITusTerminationStore terminationStore))
                return IntentType.NotApplicable;

            return IntentType.DeleteFile;
        }

        private static IntentType DetermineIntentForDelete(StoreExtensions extensions)
        {
            if (!extensions.Termination)
                return IntentType.NotApplicable;

            return IntentType.DeleteFile;
        }

        public static IntentHandler GetHandler(IntentType type, ContextAdapter context)
        {
            switch (type)
            {
                case IntentType.NotApplicable:
                    return IntentHandler.NotApplicable;
                case IntentType.CreateFile:
                    return new CreateFileHandler(context, context.Configuration.Store as ITusCreationStore);
                case IntentType.ConcatenateFiles:
                    return new ConcatenateFilesHandler(context, context.Configuration.Store as ITusConcatenationStore);
                case IntentType.WriteFile:
                    return new WriteFileHandler(context, initiatedFromCreationWithUpload: false);
                case IntentType.DeleteFile:
                    return new DeleteFileHandler(context, context.Configuration.Store as ITusTerminationStore);
                case IntentType.GetFileInfo:
                    return new GetFileInfoHandler(context);
                case IntentType.GetOptions:
                    return new GetOptionsHandler(context);
                default:
                    throw new InvalidOperationException();
            }
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
