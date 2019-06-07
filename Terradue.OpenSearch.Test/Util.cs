using System;
using System.IO;
using System.Reflection;
using System.Xml;
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
            var baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            TestBaseDir = Path.Combine(baseDir, "../..");
        }

    }
}

