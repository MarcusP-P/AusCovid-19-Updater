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

            // Build our services
            var serviceProvider = services.BuildServiceProvider ();



            // Displasy Hello World
            var helloWorld = serviceProvider.GetService<IHelloWorld> ();
            helloWorld.PrintHelloWorld ();

            var covid19AuReader = serviceProvider.GetService<ICovid19AuDownloader> ();
            await covid19AuReader.DownloadFile ().ConfigureAwait (false);
            await covid19AuReader.DeserialiseDownloadedData ().ConfigureAwait (false);


            // Remove the serviceces
            serviceProvider.Dispose ();

        }
    }
}
