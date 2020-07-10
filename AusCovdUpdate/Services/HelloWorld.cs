using System;

using AusCovdUpdate.ServiceInterfaces;

namespace AusCovdUpdate.Services
{
    public class HelloWorld : IHelloWorld
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage ("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Temporary development")]
        public void PrintHelloWorld ()
        {
            Console.WriteLine ("Hello World!");
        }
    }
}
