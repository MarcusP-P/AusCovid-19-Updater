using System.Threading.Tasks;

using AusCovdUpdate.ServiceInterfaces;
using AusCovdUpdate.Services;

using Microsoft.Extensions.DependencyInjection;

namespace AusCovdUpdate
{
    public static class Program
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage ("Style", "IDE0060:Remove unused parameter", Justification = "Main function")]
        public static async Task Main (string[] args)
        {
            // Resgister our services
            var services = new ServiceCollection ();
            _ = services.AddHttpClient ();
            _ = services.AddScoped<IHelloWorld, HelloWorld> ();
            _ = services.AddScoped<IConsoleWrapper, ConsoleWrapper> ();
            _ = services.AddScoped<IHttpFileDownloader, HttpFileDownloader> ();
            _ = services.AddScoped<ICovid19AuDownloader, Covid19AuDownloader> ();
            _ = services.AddScoped<IJohnsHopkinsDownloader, JohnsHopkinsDownloader> ();
            _ = services.AddScoped<IExcelDocument, ExcelDocument> ();

            // Build our services
            var serviceProvider = services.BuildServiceProvider ();

            // Displasy Hello World
            var helloWorld = serviceProvider.GetService<IHelloWorld> ();
            helloWorld.PrintHelloWorld ();

            var covid19AuReader = serviceProvider.GetService<ICovid19AuDownloader> ();
            await covid19AuReader.DownloadFile ().ConfigureAwait (false);
            var dailyStatsTask = covid19AuReader.DeserialiseDownloadedData ();

            var johnsHopkinsDownloader = serviceProvider.GetService<IJohnsHopkinsDownloader> ();
            await johnsHopkinsDownloader.DownloadFile ().ConfigureAwait (false);
            var internationalStatsTask = johnsHopkinsDownloader.DeserialiseDownloadedData ();

            var excelReader = serviceProvider.GetService<IExcelDocument> ();
            excelReader.OpenDocument (@"c:\Users\Marcus\Source\Repos\AusCovid-19\COVID-19 Australia.xlsx");
            excelReader.UpdateDailyData (await dailyStatsTask.ConfigureAwait (false));
            excelReader.UpdateInternationalData (await internationalStatsTask.ConfigureAwait (false));

            // Remove the serviceces
            serviceProvider.Dispose ();

        }
    }
}
