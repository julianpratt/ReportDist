using System;

namespace ReportDist.Models
{
    public class SortFilter
    {
        public SortFilter() {}
        public SortFilter(string sort, string filter, int? page)
        {
            Sort   = sort;
            Filter = filter;
            Page   = page;
        }
        public string Sort   { get; set; }
        public string Filter { get; set; }
        public int?   Page   { get; set; }
                    
    }
}