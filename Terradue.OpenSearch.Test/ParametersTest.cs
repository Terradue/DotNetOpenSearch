using System;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using NUnit.Framework;
using Terradue.OpenSearch;
using Terradue.OpenSearch.Request;
using Terradue.OpenSearch.Schema;

namespace Terradue.OpenSearch.Test
{

    [TestFixture]
    public class PaginatedListTest
    {

        public PaginatedListTest()
        {
        }

        //---------------------------------------------------------------------------------------------------------------------

        [Test]
        public void PaginatedListTest1()
        {

            PaginatedList<int> pds = new PaginatedList<int>();
            pds.StartIndex = 1;
            pds.PageNo = 1;
            pds.PageSize = 5;

            pds.AddRange(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 });

            Assert.AreEqual(new int[] { 1, 2, 3, 4, 5 }, pds.GetCurrentPage().ToArray());

            pds.StartIndex = 2;
            pds.PageNo = 1;
            pds.PageSize = 5;
            Assert.AreEqual(new int[] { 2, 3, 4, 5, 6 }, pds.GetCurrentPage().ToArray());

            pds.StartIndex = 5;
            pds.PageNo = 3;
            pds.PageSize = 4;
            Assert.AreEqual(new int[] { 13, 14, 15, 16 }, pds.GetCurrentPage().ToArray());

        }
    }
}

