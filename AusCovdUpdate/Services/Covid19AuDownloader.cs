using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using AusCovdUpdate.ServiceInterfaces;

namespace AusCovdUpdate.Services
{
    public class Covid19AuDownloader : ICovid19AuDownloader
    {
        private readonly IHttpFileDownloader httpFileDownloader;
        public Uri Uri { get; set; } = new Uri("https://raw.githubusercontent.com/covid-19-au/covid-19-au.github.io/prod/src/data/state.json");
        public Stream JsonStream { get; set; }

        public Covid19AuDownloader (IHttpFileDownloader httpFileDownloader)
        {
            this.httpFileDownloader = httpFileDownloader;
        }

        public async Task DownloadFile ()
        {
            this.JsonStream = await this.httpFileDownloader.GetFileStreamAsync (this.Uri).ConfigureAwait (false);
        }
    }
}
