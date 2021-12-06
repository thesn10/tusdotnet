
using System.Collections.Generic;
using tusdotnet.Constants;
using tusdotnet.Interfaces;

namespace tusdotnet.Stores
{
    /// <summary>
    /// Supported extensions of a <see cref="ITusStore"/>
    /// </summary>
    public class StoreExtensions
    {
        /// <summary>
        /// Supports creation
        /// </summary>
        public bool Creation { get; set; }

        /// <summary>
        /// Supports creation-with-upload
        /// </summary>
        public bool CreationWithUpload { get; set; }

        /// <summary>
        /// Supports expiration
        /// </summary>
        public bool Expiration { get; set; }

        /// <summary>
        /// Supports checksum
        /// </summary>
        public bool Checksum { get; set; }

        /// <summary>
        /// Supports checksum-trailer
        /// </summary>
        public bool ChecksumTrailer { get; set; }

        /// <summary>
        /// Supports concatenation
        /// </summary>
        public bool Concatenation { get; set; }

        /// <summary>
        /// Supports creation-defer-length
        /// </summary>
        public bool CreationDeferLength { get; set; }

        /// <summary>
        /// Supports termination
        /// </summary>
        public bool Termination { get; set; }

        /// <summary>
        /// Returns a list of all supported extensions
        /// </summary>
        public List<string> ToList()
        {
            List<string> extensionList = new List<string>();

            if (Creation)
            {
                extensionList.Add(ExtensionConstants.Creation);
            }
            if (CreationWithUpload)
            {
                extensionList.Add(ExtensionConstants.CreationWithUpload);
            }
            if (Termination)
            {
                extensionList.Add(ExtensionConstants.Termination);
            }
            if (Checksum)
            {
                extensionList.Add(ExtensionConstants.Checksum);
            }
            if (ChecksumTrailer)
            {
                extensionList.Add(ExtensionConstants.ChecksumTrailer);
            }
            if (Concatenation)
            {
                extensionList.Add(ExtensionConstants.Concatenation);
            }
            if (Expiration)
            {
                extensionList.Add(ExtensionConstants.Expiration);
            }
            if (CreationDeferLength)
            {
                extensionList.Add(ExtensionConstants.CreationDeferLength);
            }

            return extensionList;
        }

        /// <summary>
        /// True if any extension is supported
        /// </summary>
        public bool Any()
        {
            return Creation || CreationWithUpload || Expiration || Checksum || ChecksumTrailer || Concatenation || CreationDeferLength || Termination;
        }

        /// <summary>
        /// Disable an extension
        /// </summary>
        public void Disable(string extensionName)
        {
            switch (extensionName)
            {
                case ExtensionConstants.Creation:
                    Creation = false;
                    break;
                case ExtensionConstants.CreationWithUpload:
                    CreationWithUpload = false;
                    break;
                case ExtensionConstants.Termination:
                    Termination = false;
                    break;
                case ExtensionConstants.Checksum:
                    Checksum = false;
                    break;
                case ExtensionConstants.ChecksumTrailer:
                    ChecksumTrailer = false;
                    break;
                case ExtensionConstants.Concatenation:
                    Concatenation = false;
                    break;
                case ExtensionConstants.Expiration:
                    Expiration = false;
                    break;
                case ExtensionConstants.CreationDeferLength:
                    CreationDeferLength = false;
                    break;
                default:
                    break;
            }
        }
    }
}
