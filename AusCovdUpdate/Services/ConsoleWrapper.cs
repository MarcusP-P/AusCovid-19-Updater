using System;

using AusCovdUpdate.ServiceInterfaces;

namespace AusCovdUpdate.Services
{
    public class ConsoleWrapper : IConsoleWrapper
    {
        public void WriteLine (string value)
        {
            Console.WriteLine (value);
        }
    }
}
