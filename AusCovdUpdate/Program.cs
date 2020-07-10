using AusCovdUpdate.ServiceInterfaces;
using AusCovdUpdate.Services;

using Microsoft.Extensions.DependencyInjection;

namespace AusCovdUpdate
{
    public static class Program
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage ("Style", "IDE0060:Remove unused parameter", Justification = "Main function")]
        public static void Main (string[] args)
        {
            // Resgister our services
            var services = new ServiceCollection ();
            _ = services.AddScoped<IHelloWorld, HelloWorld> ();

            // Build our services
            var serviceProvider = services.BuildServiceProvider ();

            // Displasy Hello World
            var helloWorld = serviceProvider.GetService<IHelloWorld> ();
            helloWorld.PrintHelloWorld ();

            // Remove the serviceces
            serviceProvider.Dispose ();

        }
    }
}
