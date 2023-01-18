﻿using System;
using System.Net;
using System.Threading.Tasks;
using tusdotnet.Adapters;
using tusdotnet.Constants;
using tusdotnet.Extensions;
using tusdotnet.Extensions.Internal;
using tusdotnet.Helpers;
using tusdotnet.IntentHandlers;
using tusdotnet.Interfaces;
using tusdotnet.Models;
using tusdotnet.Models.Configuration;
using tusdotnet.Parsers;

namespace tusdotnet
{
    internal static class TusProtocolHandlerIntentBased
    {
        public static bool RequestIsForTusEndpoint(Uri requestUri, DefaultTusConfiguration configuration)
        {
            return requestUri.LocalPath.StartsWith(configuration.UrlPath, StringComparison.OrdinalIgnoreCase);
        }

        public static async Task<ResultType> Invoke(ContextAdapter context)
        {
            context.Configuration.Validate();

            var intentType = IntentAnalyzer.DetermineIntent(context);

            if (intentType == IntentType.NotApplicable)
            {
                return ResultType.ContinueExecution;
            }

            var intentHandler = IntentAnalyzer.GetHandler(intentType, context);

            // Hack for Upload-Challenge, fix a chain of authenticators (challenge + OnAuthorizeAsync)
            if (await VerifyUploadChallengeIfApplicable(context, intentHandler) == ResultType.StopExecution)
            {
                return ResultType.StopExecution;
            }

            var onAuhorizeResult = await EventHelper.Validate<AuthorizeContext>(context, ctx =>
            {
                ctx.Intent = intentType;
                ctx.FileConcatenation = GetFileConcatenationFromIntentHandler(intentHandler);
                // TODO: Add client-tag and resolved file id here or in a separate event/callback?
            });

            if (onAuhorizeResult == ResultType.StopExecution)
            {
                return ResultType.StopExecution;
            }

            if (await VerifyTusVersionIfApplicable(context, intentHandler) == ResultType.StopExecution)
            {
                return ResultType.StopExecution;
            }

            ITusFileLock fileLock = null;

            if (intentHandler.LockType == LockType.RequiresLock)
            {
                fileLock = await context.GetFileLock();

                var hasLock = await fileLock.Lock();
                if (!hasLock)
                {
                    await context.Response.Error(HttpStatusCode.Conflict, $"File {context.Request.FileId} is currently being updated. Please try again later");
                    return ResultType.StopExecution;
                }
            }

            try
            {
                if (!await intentHandler.Validate())
                {
                    return ResultType.StopExecution;
                }

                await intentHandler.Invoke();
                return ResultType.StopExecution;
            }
            catch (TusStoreException storeException)
            {
                await context.Response.Error(HttpStatusCode.BadRequest, storeException.Message);
                return ResultType.StopExecution;
            }
            finally
            {
                fileLock?.ReleaseIfHeld();
            }
        }

        private static async Task<ResultType> VerifyUploadChallengeIfApplicable(ContextAdapter context, IntentHandler intentHandler)
        {
            if (!(context.Configuration.Store is ITusChallengeStore challengeStore))
                return ResultType.ContinueExecution;

            UploadChallengeParserResult parsedUploadChallenge = null;
            ITusChallengeStoreHashFunction hashFunction = null;

            var uploadChallengeHeader = context.Request.GetHeader(HeaderConstants.UploadChallenge);

            if (!string.IsNullOrEmpty(uploadChallengeHeader))
            {
                parsedUploadChallenge = UploadChallengeParser.ParseAndValidate(uploadChallengeHeader, ChallengeChecksumCalculator.SupportedAlgorithms);
                if (!parsedUploadChallenge.Success)
                {
                    await context.Response.Error(HttpStatusCode.BadRequest, parsedUploadChallenge.ErrorMessage);
                    return ResultType.StopExecution;
                }

                hashFunction = ChallengeChecksumCalculator.Sha256;
            }

            var challengeResult = await intentHandler.Challenge(parsedUploadChallenge, hashFunction, challengeStore);
            if (challengeResult == ResultType.StopExecution)
            {
                context.Response.NotFound();
                return ResultType.StopExecution;
            }

            return ResultType.ContinueExecution;
        }

        private static Models.Concatenation.FileConcat GetFileConcatenationFromIntentHandler(IntentHandler intentHandler)
        {
            return intentHandler is ConcatenateFilesHandler concatFilesHandler ? concatFilesHandler.UploadConcat.Type : null;
        }

        private static async Task<ResultType> VerifyTusVersionIfApplicable(ContextAdapter context, IntentHandler intentHandler)
        {
            // Options does not require a correct tus resumable header.
            if (intentHandler.Intent == IntentType.GetOptions)
                return ResultType.ContinueExecution;

            var tusResumableHeader = context.Request.GetHeader(HeaderConstants.TusResumable);

            if (tusResumableHeader == HeaderConstants.TusResumableValue)
                return ResultType.ContinueExecution;

            context.Response.SetHeader(HeaderConstants.TusResumable, HeaderConstants.TusResumableValue);
            context.Response.SetHeader(HeaderConstants.TusVersion, HeaderConstants.TusResumableValue);
            await context.Response.Error(HttpStatusCode.PreconditionFailed, $"Tus version {tusResumableHeader} is not supported. Supported versions: {HeaderConstants.TusResumableValue}");

            return ResultType.StopExecution;
        }
    }
}