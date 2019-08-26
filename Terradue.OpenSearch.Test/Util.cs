using System;
using System.IO;
using System.Reflection;
using log4net;
using log4net.Config;

namespace Terradue.OpenSearch.Test
{

    public class TestFixture
    {
        private static string testBaseDir;

        public TestFixture()
        {
            var logRepository = LogManager.GetRepository(Assembly.GetExecutingAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo(TestFixture.TestBaseDir + "/log4net.config"));
        }

        public static string TestBaseDir
        {
            get
            {
                if (string.IsNullOrEmpty(testBaseDir))
                {
                    var codeBaseUrl = new Uri(Assembly.GetExecutingAssembly().CodeBase);
                    var codeBasePath = Uri.UnescapeDataString(codeBaseUrl.AbsolutePath);
                    var dirPath = Path.GetDirectoryName(codeBasePath);
                    testBaseDir = Path.Combine(dirPath, "../../..");
                }
                return testBaseDir;
            }
        }

    }

}

