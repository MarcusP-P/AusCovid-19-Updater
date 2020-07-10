using AusCovdUpdate.ServiceInterfaces;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

namespace AusCovdUpdate.Services.Tests
{
    [TestClass ()]
    public class HelloWorldTests
    {
        // A Dummy test to make sure the test frameworks are working.
        [TestMethod ()]
        public void PrintHelloWorldTest ()
        {
            // Create the console
            var fakeConsole = new Mock<IConsoleWrapper> ();

            // Use the mock in a hello world
            var printHelloWorld = new HelloWorld (fakeConsole.Object);

            // test the Hello World
            printHelloWorld.PrintHelloWorld ();

            // Verify that it prints what we want
            fakeConsole.Verify (x => x.WriteLine ("Hello World!"), Times.Once ());
        }
    }
}