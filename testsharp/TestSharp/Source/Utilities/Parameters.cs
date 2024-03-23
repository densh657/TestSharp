using NUnit.Framework;

namespace TestSharp.Source.Utilities
{
    /// <summary>
    /// Handles parameters for NUnit Console Runner
    /// </summary>
    public class Parameters
    {
        public static string headless = TestContext.Parameters.Get("headless");
        public static string browser = TestContext.Parameters.Get("browser");
        public static string wait = TestContext.Parameters.Get("wait");
        public static string env = TestContext.Parameters.Get("env");
    }
}