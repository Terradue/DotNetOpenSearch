using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace Terradue.OpenSearch.Request
{
    public class PaginatedList<T> : List<T>
    {
        #region Properties
        // these properties need to be set
        public int StartIndex { get; set; }
        public int PageNo { get; set; }
        public int PageSize { get; set; }
        public int Adjacents { get; set; } // number of adjacent pages (to the current page) to display. default is set below

        // all of these properties are derived from the above
        public int TotalNoOfPages { get { return PageSize == 0 ? 0 : (int)Math.Ceiling((double)Count / PageSize); } }
        public int TotalNoOfPagesDisplay { get { return TotalNoOfPages == 0 ? 1 : TotalNoOfPages; } }
        public int PrevPageNo { get { return PageNo == 1 ? 1 : PageNo - 1; } }
        public int NextPageNo { get { return PageNo >= TotalNoOfPages ? TotalNoOfPages : PageNo + 1; } }
        public bool HasPrev { get { return PageNo > 1; } }
        public bool HasNext { get { return PageNo < TotalNoOfPages; } }
        #endregion

        #region Initialization/Finalization
        public PaginatedList()
        {
            Adjacents = 1;
        }
        #endregion

        public List<T> GetCurrentPage(){
            return this.Skip<T>((StartIndex -1) + ((PageNo - 1) * PageSize)).Take<T>(PageSize).ToList();
        }
    }
}