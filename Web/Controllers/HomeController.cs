using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using ReportDist.Models;
using ReportDist.Data;
using Mistware.Utils;

namespace ReportDist.Controllers
{
    public class HomeController : Controller
    {
        protected readonly DataContext _context;

        public HomeController(DataContext context)
        {
            _context = context;
        }
        public IActionResult Index(string sortOrder, string searchString, string currentFilter, int? pageNumber)
        {
            Log.Me.Info("In HomeController.Index");
            string userid = "16"; // TODO!!!
            var userClaims = User.Identity as System.Security.Claims.ClaimsIdentity;
            // string firstName = userClaims?.FindFirst("name")?.Value;
            // string lastName  = userClaims?.FindFirst("preferred_username")?.Value; 
            string emailAddress  = userClaims?.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            string firstName     = userClaims?.FindFirst(System.Security.Claims.ClaimTypes.GivenName)?.Value; 
            string lastName      = userClaims?.FindFirst(System.Security.Claims.ClaimTypes.Surname)?.Value; 
            //userid = emailAddress;

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
            ViewData["UserId"] = userid; 
            return View(PaginatedList<PendingReport>.Create(
                        _context.PendingReportRepo.List(sortOrder, searchString), pageNumber ?? 1, pageSize));
        }

        // POST: /Home/Search
        [HttpPost]
        public IActionResult Search(HomeIndexSearchModel vm)
        {
            try
            {
                if (vm.Number != null && !vm.Number.IsInteger()) return RedirectToAction("Error", "Home"); 
                string start = vm.Company + "/" + vm.Year;
                if (vm.Code != null) start += "/" + vm.Code;
                string filter = "search-" + start + "-" + (vm.Number ?? "") + "-" + (vm.ReportType ?? "");
                HttpContext.Session.SetString("CurrentFilter", filter);
            }
            catch
            {
                return RedirectToAction("Error", "Home"); 
            }
            
            return RedirectToAction("Index", "Home");        
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
