using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

using AusCovdUpdate.Model;
using AusCovdUpdate.Model.DTO;
using AusCovdUpdate.ServiceInterfaces;

namespace AusCovdUpdate.Services
{
    public class Covid19AuDownloader : ICovid19AuDownloader, IDisposable
    {
        private readonly IHttpFileDownloader httpFileDownloader;
        private bool DisposedValue;

        public Uri Uri { get; set; } = new Uri ("https://raw.githubusercontent.com/covid-19-au/covid-19-au.github.io/prod/src/data/state.json");
        public Stream JsonStream { get; set; }

        public Covid19AuDownloader (IHttpFileDownloader httpFileDownloader)
        {
            this.httpFileDownloader = httpFileDownloader;
        }

        public async Task DownloadFile ()
        {
            this.JsonStream = await this.httpFileDownloader.GetFileStreamAsync (this.Uri).ConfigureAwait (false);
        }

        public async Task<Dictionary<DateTime, Covid19Aus>> DeserialiseDownloadedData ()
        {
            var deserialised = await JsonSerializer.DeserializeAsync<Dictionary<string, AusCovid19DataDTO>> (this.JsonStream).ConfigureAwait (false);

            var cumulative = new List<Covid19Aus> ();
            var daily = new Dictionary<DateTime, Covid19Aus> ();
            Covid19Aus previous = null;
            foreach (var day in deserialised.Keys)
            {
                var currentDay = deserialised[day];

                var datePart = day.Split ('-');

                var newData = new Covid19Aus
                {
                    Date = new DateTime (int.Parse (datePart[0], CultureInfo.InvariantCulture), int.Parse (datePart[1], CultureInfo.InvariantCulture), int.Parse (datePart[2], CultureInfo.InvariantCulture)),
                    ACT = ConvertArrayToOurData (currentDay.ACT),
                    NSW = ConvertArrayToOurData (currentDay.NSW),
                    NT = ConvertArrayToOurData (currentDay.NT),
                    QLD = ConvertArrayToOurData (currentDay.QLD),
                    SA = ConvertArrayToOurData (currentDay.SA),
                    TAS = ConvertArrayToOurData (currentDay.TAS),
                    VIC = ConvertArrayToOurData (currentDay.VIC),
                    WA = ConvertArrayToOurData (currentDay.WA),
                };
                if (previous != null)
                {
                    var todayData = newData - previous;
                    daily.Add (newData.Date, todayData);
                }
                cumulative.Add (newData);
                previous = newData;
            }
            return daily;
        }

        public static AusCovid19State ConvertArrayToOurData (int[] input)
        {
            Contract.Assert (input != null);
            Contract.Assert (input.Length <= 7);


            return new AusCovid19State
            {
                Cases = input.Length >= 1 ? input[0] : 0,
                Deaths = input.Length >= 2 ? input[1] : 0,
                Recovered = input.Length >= 3 ? input[2] : 0,
                Tested = input.Length >= 4 ? input[3] : 0,
                Active = input.Length >= 5 ? input[4] : 0,
                InHospital = input.Length >= 6 ? input[5] : 0,
                InIcu = input.Length >= 7 ? input[6] : 0,
            };
        }

        protected virtual void Dispose (bool disposing)
        {
            if (!this.DisposedValue)
            {
                if (disposing)
                {
                    this.JsonStream.Dispose ();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                this.DisposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~Covid19AuDownloader()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose ()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            this.Dispose (disposing: true);
            GC.SuppressFinalize (this);
        }
    }
}
