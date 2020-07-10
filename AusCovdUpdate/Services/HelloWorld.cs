using AusCovdUpdate.ServiceInterfaces;

namespace AusCovdUpdate.Services
{
    public class HelloWorld : IHelloWorld
    {
        private readonly IConsoleWrapper console;

        public HelloWorld (IConsoleWrapper console)
        {
            this.console = console;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage ("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Temporary development")]
        public void PrintHelloWorld ()
        {
            this.console.WriteLine ("Hello World!");
        }
    }
}
