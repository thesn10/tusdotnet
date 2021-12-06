#if !NETCOREAPP3_1_OR_GREATER

using System;
using System.Collections.Generic;
using tusdotnet.Models.Concatenation;
using tusdotnet.Routing;

namespace tusdotnet.Parsers.UploadConcatParserHelpers
{
    internal class UploadConcatParserStringBased
    {
        internal static UploadConcatParserResult ParseAndValidate(string uploadConcatHeader, ITusRoutingHelper routingHelper)
        {
            var temp = uploadConcatHeader.Split(';');

            // Unable to parse Upload-Concat header
            var type = temp[0].ToLower();
            return type switch
            {
                "partial" => UploadConcatParserResult.FromResult(new FileConcatPartial()),
                "final" => ParseFinal(temp, routingHelper),
                _ => UploadConcatParserResult.FromError(UploadConcatParserErrorTexts.HEADER_IS_INVALID),
            };
        }

        /// <summary>
        /// Parses the "final" concatenation type based on the parts provided.
        /// Will validate and strip the url path provided to make sure that all files are in the same store.
        /// </summary>
        /// <param name="parts">The separated parts of the Upload-Concat header</param>
        /// <param name="routingHelper">The RoutingHelper to parse url route and extract the file id parameter</param>
        /// <returns>THe parse final concatenation</returns>
        // ReSharper disable once SuggestBaseTypeForParameter
        private static UploadConcatParserResult ParseFinal(string[] parts, ITusRoutingHelper routingHelper)
        {
            if (parts.Length < 2)
            {
                return UploadConcatParserResult.FromError(UploadConcatParserErrorTexts.HEADER_IS_INVALID);
            }

            var fileUris = parts[1].Split(' ');
            var fileIds = new List<string>(fileUris.Length);

            foreach (var fileUri in fileUris)
            {
                var fileId = routingHelper.ParseFileId(fileUri);
                if (fileId == null)
                {
                    return UploadConcatParserResult.FromError(UploadConcatParserErrorTexts.HEADER_IS_INVALID);
                }

                fileIds.Add(fileId);
            }

            return UploadConcatParserResult.FromResult(new FileConcatFinal(fileIds.ToArray()));
        }
    }
}

#endif