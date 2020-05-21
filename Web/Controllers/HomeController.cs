using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using ReportDist.Models;
using ReportDist.Data;
using Mistware.Utils;

namespace ReportDist.Controllers
{
    public class HomeController : BaseController
    {
        public HomeController(DataContext context) : base(context) {}
       
        public IActionResult Index(string sortOrder, string searchString, string currentFilter, int? pageNumber)
        {
            string user = CheckIdentity();
            if (user == null) return RedirectToAction("Error", "Home");
            Log.Me.Debug("Home/Index - User: " + user + ", SortOrder: " + sortOrder + ", SearchString: " + searchString + ", CurrentFilter: " + currentFilter + ", PageNumber: " + pageNumber.ToString());
            ApplicationVersion();

            string userid = HttpContext.Session.GetString("UserId");
            
            // Sorting
            if (sortOrder == null) sortOrder = HttpContext.Session.GetString("CurrentSort");
            if (sortOrder == null) sortOrder = "creationdate desc"; // default sort order
            ViewData["CurrentSort"] = sortOrder;
            if (sortOrder != null) HttpContext.Session.SetString("CurrentSort", sortOrder);
           
            // Filtering
            if (currentFilter == null)      currentFilter = HttpContext.Session.GetString("CurrentFilter");
            if (searchString != null)       pageNumber = 1;  // filter just got updated, so reset the page number
            else if (currentFilter == null) searchString = "uncommitted-" + userid; // default filter
            else                            searchString = currentFilter;
            ViewData["CurrentFilter"] = searchString;
            if (searchString != null) HttpContext.Session.SetString("CurrentFilter", searchString);

            int pageSize = 10;

            return View(PaginatedList<PendingReport>.Create(
                        _context.PendingReportRepo.List(sortOrder, searchString), pageNumber ?? 1, pageSize));
        }

        // POST: /Home/Search
        [HttpPost]
        public IActionResult Search(HomeIndexSearchModel vm)
        {
            string method = "Home/Search[Post]";

            try
            {
                if (vm.Number != null && !vm.Number.IsInteger()) return RedirectToAction("Error", "Home"); 
                string start = vm.Company + "/" + vm.Year;
                if (vm.Code != null) start += "/" + vm.Code;
                string filter = "search-" + start + "-" + (vm.Number ?? "") + "-" + (vm.ReportType ?? "");
                HttpContext.Session.SetString("CurrentFilter", filter);

                return RedirectToAction("Index", "Home");   
            }
            catch (Exception ex)
            {
                Log.Me.Error("Exception in " + method + ": " + ex.Message);
                return RedirectToAction("Error", "Home"); 
            }     
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
