﻿using System.Linq;
using tusdotnet.Routing;

namespace tusdotnet.Models.Concatenation
{
	/// <summary>
	/// Represents the "final" file concatenation type.
	/// </summary>
	public class FileConcatFinal : FileConcat
	{
		/// <summary>
		/// The files that is included in this file concatenation.
		/// </summary>
		public string[] Files { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="FileConcatFinal"/> class.
		/// </summary>
		/// <param name="partialFiles">The files to include in the final concatenation. The files does not contain any URL only the file id.</param>
		public FileConcatFinal(params string[] partialFiles)
		{
			Files = partialFiles;
		}

		/// <summary>
		/// Returns the header value to send to the client for the specific file concatenation type.
		/// </summary>
		/// <returns>The header value to send to the client for the specific file concatenation type</returns>
		public override string GetHeader()
		{
			return $"final;{string.Join(" ", Files)}";
		}

		/// <summary>
		/// Appends the url path that tusdotnet is listening to to each file.
		/// This is done to give the client relative urls that can be used.
		/// </summary>
		/// <param name="urlPath">The UrlPath property of the ITusConfiguration</param>
		internal void AddUrlPathToFiles(string urlPath)
		{
			AddUrlPathToFiles(new TusUrlPathRoutingHelper(urlPath, null));
		}

		/// <summary>
		/// Appends the url path that tusdotnet is listening to to each file.
		/// This is done to give the client relative urls that can be used.
		/// </summary>
		internal void AddUrlPathToFiles(ITusRoutingHelper routingHelper)
		{
			Files = Files.Select(file => routingHelper.GenerateFilePath(file)).ToArray();
		}
	}
}
