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

            // Build our services
            var serviceProvider = services.BuildServiceProvider ();

            var httpFileDownloader = serviceProvider.GetService<IHttpFileDownloader> ();
            var foo= await httpFileDownloader.GetFileStreamAsync (new System.Uri("http://apple.com")).ConfigureAwait(false);

            // Displasy Hello World
            var helloWorld = serviceProvider.GetService<IHelloWorld> ();
            helloWorld.PrintHelloWorld ();

            // Remove the serviceces
            serviceProvider.Dispose ();

        }
    }
}
