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
       
        public IActionResult Index(string sortOrder, string currentFilter, int? pageNumber)
        {
            User user = CheckIdentity();
            if (user == null) return RedirectToAction("Error", "Home");
            Log.Me.Debug("Home/Index - User: " + user.UserName + ", SortOrder: '" + sortOrder + "', CurrentFilter: '" + currentFilter + "', PageNumber: " + pageNumber.ToString());
            ApplicationVersion();

            // current is what's cached, or the defaults 
            SortFilter def = new SortFilter("creationdate desc", "uncommitted-" + user.UserId, 1);
            string key = "HomeIndex-" + user.UserId;
            SortFilter current = Cache<SortFilter>.GetCache().Get(key, () => def);      

            // Inputs override previous or default values  
            if (sortOrder     != null) current.Sort   = sortOrder;
            if (pageNumber.HasValue)   current.Page   = pageNumber;
            if (currentFilter != null && current.Filter != currentFilter) 
            {
                current.Filter = currentFilter;
                current.Page   = 1;  // filter just got updated, so reset the page number
            }

            // Cache the current settings, so that we come back to the same page from Report/Edit
            Cache<SortFilter>.GetCache().Set(key, () => current);

            ViewData["CurrentSort"]   = current.Sort;
            ViewData["CurrentFilter"] = current.Filter;

            int pageSize = 25;

            return View(PaginatedList<PendingReport>.Create(
                        _context.PendingReportRepo.List(current.Sort, current.Filter), current.Page ?? 1, pageSize));
        }

        private SortFilter GetSortFilter(SortFilter def, SortFilter inp)
        {
            SortFilter sf = new SortFilter();
            sf.Sort   = (inp.Sort   == null) ? def.Sort   : inp.Sort;
            sf.Filter = (inp.Filter == null) ? def.Filter : inp.Filter;
            sf.Page   = (!inp.Page.HasValue) ? def.Page   : inp.Page;

            return sf;
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

                Log.Me.Debug("Redirecting to Home/Index/?sortOrder=creationdate desc&pageNumber=1&currentFilter=" + filter);

                return RedirectToAction("Index", "Home", new { sortOrder="creationdate desc", pageNumber=1, currentFilter=filter} );   
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
