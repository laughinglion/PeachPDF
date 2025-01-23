#nullable enable

using System;
using System.IO;
using System.Threading.Tasks;

namespace PeachPDF.Network
{
    public abstract class RNetworkLoader
    {
        public abstract Task<string> GetPrimaryContents();
        public abstract Task<Stream?> GetResourceStream(Uri uri);
        public abstract Uri? BaseUri { get; }
    }
}
