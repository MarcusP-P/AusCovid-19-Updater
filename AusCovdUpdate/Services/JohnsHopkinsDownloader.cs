using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using AusCovdUpdate.ServiceInterfaces;

using CsvHelper;

namespace AusCovdUpdate.Services
{
    public class JohnsHopkinsDownloader : IJohnsHopkinsDownloader, IDisposable

    {
        private readonly IHttpFileDownloader httpFileDownloader;
        private bool DisposedValue;

        public Uri Uri { get; set; } = new Uri ("https://raw.githubusercontent.com/CSSEGISandData/COVID-19/master/csse_covid_19_data/csse_covid_19_time_series/time_series_covid19_confirmed_global.csv");
        public Stream CsvStream { get; set; }

        public JohnsHopkinsDownloader (IHttpFileDownloader httpFileDownloader)
        {
            this.httpFileDownloader = httpFileDownloader;
        }

        public async Task DownloadFile ()
        {
            this.CsvStream = await this.httpFileDownloader.GetFileStreamAsync (this.Uri).ConfigureAwait (false);
        }

        public async Task<Dictionary<DateTime, Dictionary<string, int>>> DeserialiseDownloadedData ()
        {
            using var csvStreamReader = new StreamReader (this.CsvStream);

            using var csv = new CsvReader (csvStreamReader, CultureInfo.InvariantCulture);
            _ = await csv.ReadAsync ().ConfigureAwait (false);
            _ = csv.ReadHeader ();
            var headerRow = csv.Context.HeaderRecord;

            var headerDates = headerRow
                .Where (x => int.TryParse (x.Substring (0, 1), out _))
                .Select (x => (
                    Date: DateTime.Parse (x, CultureInfo.CreateSpecificCulture ("en-US")),
                    Header: x
                ))
                .OrderBy (x => x.Date)
                .ToList ();

            var countries = new Dictionary<string, Dictionary<DateTime, int>> ();

            while (await csv.ReadAsync ().ConfigureAwait (false))
            {
                var country = csv.GetField<string> ("Country/Region");
                var province = csv.GetField<string> ("Province/State");

                if (!string.IsNullOrWhiteSpace (province))
                {
                    continue;
                }

                var dataPoints = new Dictionary<DateTime, int> ();

                foreach (var (Date, Header) in headerDates)
                {
                    var value = csv.GetField<int> (Header);
                    dataPoints.Add (Date, value);
                }
                countries.Add (country, dataPoints);
            }

            var casesByDateAndCountry = new Dictionary<DateTime, Dictionary<string, int>> ();

            foreach (var (Date, _) in headerDates)
            {
                casesByDateAndCountry.Add (Date, new Dictionary<string, int> ());
            }

            foreach (var country in countries.Keys)
            {
                foreach (var date in countries[country].Keys)
                {
                    casesByDateAndCountry[date].Add (country, countries[country][date]);
                }
            }

            return casesByDateAndCountry;
        }

        protected virtual void Dispose (bool disposing)
        {
            if (!this.DisposedValue)
            {
                if (disposing)
                {
                    this.CsvStream.Dispose ();
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
