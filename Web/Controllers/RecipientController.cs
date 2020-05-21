using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using ReportDist.Models;
using ReportDist.Data;
using Mistware.Utils;

namespace ReportDist.Controllers
{
    public class RecipientController : BaseController
    {
        public RecipientController(DataContext context) : base(context) {}
            
        // GET: /Recipient/ShowGrid 
        public IActionResult ShowGrid()  
        {  
            return View("Grid");  
        } 

        // POST: /Recipient/LoadData
        [HttpPost]
        public IActionResult LoadData()  
        {  
            try  
            { 
                DataTableHelper helper = new DataTableHelper(Request);
      
                PaginatedList<Recipient> list = PaginatedList<Recipient>.Create(
                                    _context.RecipientRepo.List(helper.SortColumn, helper.SearchValue), helper);

                //total number of rows count   
                int recordsTotal = list.TotalRows;  
                //Paging   
                IEnumerable<Recipient> data = list as List<Recipient>;   
                //Returning Json Data  
                return Json(new { draw = helper.Draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });  

            }  
            catch (Exception)  
            {  
                throw;  
            }  

        }

        // GET: /Recipient/Delete?pendingId=x&recipientId=y
        public IActionResult Delete(int? recipientId, int? pendingId)
        {
            _context.RecipientRepo.Delete(recipientId);
    
            return RedirectToAction("Index", "Circulation", new { id = pendingId });  
        } 


    }
}
     