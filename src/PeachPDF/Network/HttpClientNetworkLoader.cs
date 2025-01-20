#nullable enable

using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace PeachPDF.Network
{
    public class HttpClientNetworkLoader(HttpClient httpClient, Uri? primaryContentsUri) : RNetworkLoader
    {
        public override async Task<string> GetPrimaryContents()
        {
            if (primaryContentsUri is null)
            {
                throw new InvalidOperationException("Primary contents URL is not set.");
            }

            var stream = await GetResourceStream(primaryContentsUri);

            if (stream is null)
            {
                throw new InvalidOperationException("Primary contents stream is null.");
            }

            using var streamReader = new StreamReader(stream);
            return await streamReader.ReadToEndAsync();
        }

        public override async Task<Stream?> GetResourceStream(Uri uri)
        {
            return await httpClient.GetStreamAsync(uri);
        }
    }
}
