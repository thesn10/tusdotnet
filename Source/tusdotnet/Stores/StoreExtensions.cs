
using System.Collections.Generic;
using tusdotnet.Constants;

namespace tusdotnet.Stores
{
    public class StoreExtensions
    {
        public bool Creation { get; set; }

        public bool Expiration { get; set; }

        public bool Checksum { get; set; }
        public bool Concatenation { get; set; }
        public bool CreationDeferLength { get; set; }
        public bool Termination { get; set; }

        public List<string> ToList()
        {
            List<string> extensionList = new List<string>();

            if (Creation)
            {
                extensionList.Add(ExtensionConstants.Creation);
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

        public bool Any()
        {
            return Creation || Expiration || Checksum || Concatenation || CreationDeferLength || Termination;
        }
    }
}
