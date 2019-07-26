using System;
using System.IO;
using System.Reflection;
using System.Xml;
using log4net.Config;
using NUnit.Framework;

namespace Terradue.OpenSearch.Test
{

    [SetUpFixture()]
    public class Util
    {
        public static string TestBaseDir;

        [OneTimeSetUp]
        public static void OneTimeSetUp()
        {
            BasicConfigurator.Configure();
            var baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            TestBaseDir = Path.Combine(baseDir, "../../..");
        }

    }
}

