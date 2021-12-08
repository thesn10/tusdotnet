using System;
using tusdotnet.Models;

namespace tusdotnet.Controllers
{
    /// <summary>
    /// Set the strategy to use when parsing metadata. Defaults to <see cref="MetadataParsingStrategy.AllowEmptyValues"/> for better compatibility with tus clients.
    /// Change to <see cref="MetadataParsingStrategy.Original"/> to use the old format.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class MetadataParsingAttribute : Attribute
    {
        /// <summary>
        /// Set the strategy to use when parsing metadata. Defaults to <see cref="MetadataParsingStrategy.AllowEmptyValues"/> for better compatibility with tus clients.
        /// Change to <see cref="MetadataParsingStrategy.Original"/> to use the old format.
        /// </summary>
        public MetadataParsingAttribute(MetadataParsingStrategy strategy)
        {
            Strategy = strategy;
        }

        /// <summary>
        /// The metadata parsing strategy
        /// </summary>
        public MetadataParsingStrategy Strategy { get; }
    }
}
