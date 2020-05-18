using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Mistware.Utils;

namespace ReportDist.Data
{
    /// Extracts Request.Form data from jQuery DataTable AJAX call 
    public class DataTableHelper
    {
        public DataTableHelper(HttpRequest request)
        {
                if (request.HasFormContentType)
                {
                    IFormCollection form = request.Form;

                    this.Draw   = ((string)form["draw"]).IsNull("1").ToInteger();
                    // Skiping number of Rows count  
                    string start  = ((string)form["start"]).IsNull("0");  
                    // Paging Length 10,20  
                    string length = ((string)form["length"]).IsNull("10"); 
                    //Paging Size (10,20,50,100)  
                    this.PageSize = length != null ? Convert.ToInt32(length) : 0;  
                    this.Skip = start != null ? Convert.ToInt32(start) : 0;

                    // Sort Column 
                    string sortColumnId = form["order[0][column]"];
                    string sortColumnName      = null;
                    string sortColumnDirection = null;
                    if (sortColumnId != null)
                    {
                        // Sort Column Name  
                        sortColumnName = form["columns[" + sortColumnId + "][name]"];  
                        // Sort Column Direction ( asc ,desc)  
                        sortColumnDirection = form["order[0][dir]"];
                        this.SortColumn = sortColumnName + " " + sortColumnDirection;
                    }
                    else
                        this.SortColumn = null;
                     // Search Value from (Search box)  
                    this.SearchValue = form["search[value]"];
                    //  Id passed as data
                    this.Id = ((string)form["id"]).IsNull("0").ToInteger();

                }
                else
                {
                    // No Request.Form, to just use sensible defaults
                    this.Draw        = 1;
                    this.SortColumn  = null;
                    this.SearchValue = null;
                    this.Skip        = 0;
                    this.PageSize    = 10;
                    this.Id          = 0;
                }
        }
        /// Sequence Count
        public int    Draw        { get; set; }

        /// Name of Column to sort on, followd by asc or desc 
        public string SortColumn  { get; set; }

        /// Filter
        public string SearchValue { get; set; }

        /// Number of rows in a page
        public int    PageSize    { get; set; }

        /// Number of rows to skip
        public int    Skip        { get; set; }

        /// Optional Id passed as data
        public int    Id          { get; set; }

    }
}