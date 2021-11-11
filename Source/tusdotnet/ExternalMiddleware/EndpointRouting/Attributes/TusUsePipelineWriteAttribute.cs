using System;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    /// <summary>
    /// Specifies that tus should use <see cref="System.IO.Pipelines.PipeReader"/> instead of <see cref="System.IO.Stream"/> when writing to a file
    /// </summary>
    public class TusUsePipelineWriteAttribute : Attribute
    {
        public TusUsePipelineWriteAttribute(bool usePipelines = true)
        {
            UsePipelines = usePipelines;
        }

        /// <summary>
        /// Use the incoming request's PipeReader instead of the stream to read data from the client.
        /// This is only available on .NET Core 3.1 or later and if the store supports it through the ITusPipelineStore interface.
        /// </summary>
        public bool UsePipelines { get; }
    }
}
