using Terradue.OpenSearch.Request;
using Xunit;

namespace Terradue.OpenSearch.Test
{

    public class PaginatedListTest : IClassFixture<TestFixture>
    {

        public PaginatedListTest()
        {
        }

        //---------------------------------------------------------------------------------------------------------------------

        [Fact(DisplayName = "Paginated List Test #1")]
        [Trait("Category", "unit")]
        public void PaginatedListTest1()
        {

            PaginatedList<int> pds = new PaginatedList<int>();
            pds.StartIndex = 1;
            pds.PageNo = 1;
            pds.PageSize = 5;

            pds.AddRange(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 });

            Assert.Equal(new int[] { 1, 2, 3, 4, 5 }, pds.GetCurrentPage().ToArray());

            pds.StartIndex = 2;
            pds.PageNo = 1;
            pds.PageSize = 5;
            Assert.Equal(new int[] { 2, 3, 4, 5, 6 }, pds.GetCurrentPage().ToArray());

            pds.StartIndex = 5;
            pds.PageNo = 3;
            pds.PageSize = 4;
            Assert.Equal(new int[] { 13, 14, 15, 16 }, pds.GetCurrentPage().ToArray());

        }
    }
}

