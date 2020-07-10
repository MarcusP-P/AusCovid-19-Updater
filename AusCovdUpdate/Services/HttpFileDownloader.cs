using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

using AusCovdUpdate.ServiceInterfaces;

namespace AusCovdUpdate.Services
{
    public class HttpFileDownloader : IHttpFileDownloader
    {
        private readonly IHttpClientFactory httpClientFactory;
        public HttpFileDownloader (IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
        }

        public async Task<Stream> GetFileStreamAsync (Uri uri)
        {
            using var httpClient = this.httpClientFactory.CreateClient ("Downloader");
            var reultStream = await httpClient.GetStreamAsync (uri).ConfigureAwait (false);

            return reultStream;
        }
    }
}
