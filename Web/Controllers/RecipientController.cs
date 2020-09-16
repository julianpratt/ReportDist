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
            User user = CheckIdentity();
            if (user == null) return RedirectToAction("Error", "Home");
            Log.Me.Debug("Recipient/Index - User: " + user.UserName);
            ApplicationVersion();

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

        // GET: /Recipient/Create
        public IActionResult Create()
        {
            string method = "Recipient/Create";
            Log.Me.Debug(method + " - User: " + CheckIdentity());
            ApplicationVersion();
    
            return View("Edit", new Recipient());
        }

        // POST: /Recipient/Create
        [HttpPost]
        public ActionResult Create(Recipient r)
        {
            string method = "Recipient/Create[Post]";

            try
            {   
                _context.RecipientRepo.Create(r);

                Log.Me.Info(CheckIdentity().UserName + " added recipient " + r.Name + " (" + r.Email + ")");

                return RedirectToAction("ShowGrid", "Recipient");   
            }
            catch (Exception ex)
            {
                Log.Me.Error("Exception in " + method + ": " + ex.Message);
                return RedirectToAction("Error", "Home"); 
            }
        }

        // GET: /Recipient/Edit/n
        public IActionResult Edit(int? id)
        {
            string method = "Recipient/Edit";

            Log.Me.Debug(method + " - User: " + CheckIdentity().UserName + ", RecipientID: " + (id ?? 0).ToString());
            ApplicationVersion();

            if (NullId(id, "RecipientID", method)) return RedirectToAction("Error", "Home"); 

            try
            {
                Recipient r = null;
                if ((r = Read(id.Value, method)) == null) return RedirectToAction("Error", "Home"); 
               
                return View(r);
            }
            catch (Exception ex)
            {
                Log.Me.Error("Exception in " + method + ": " + ex.Message);
                return RedirectToAction("Error", "Home"); 
            }
        }

        // POST: /Recipient/Edit
        [HttpPost]
        public ActionResult Edit(Recipient update)
        {
            string method = "Recipient/Edit[Post]";

            if (IsNull(update,    "Recipient",   method)) return RedirectToAction("Error", "Home");     
            if (NullId(update.Id, "RecipientID", method)) return RedirectToAction("Error", "Home"); 
           
            try
            {
                Recipient r = null;
                if ((r = Read(update.Id, method)) == null)    return RedirectToAction("Error", "Home");

                r.FirstName    = update.FirstName;
                r.LastName     = update.LastName;
                r.Email        = update.Email;
                r.JobTitle     = update.JobTitle;
                r.AddressLine1 = update.AddressLine1;
                r.AddressLine2 = update.AddressLine2;
                r.AddressLine3 = update.AddressLine3;
                r.AddressLine4 = update.AddressLine4;
                r.AddressLine5 = update.AddressLine5;
                r.PostCode     = update.PostCode;
                
                _context.RecipientRepo.Update(r);

                Log.Me.Info(CheckIdentity().UserName + " updated recipient " + r.Name + " (" + r.Email + ")");

                return RedirectToAction("ShowGrid", "Recipient");
            }
           catch (Exception ex)
            {
                Log.Me.Error("Exception in " + method + ": " + ex.Message);
                return RedirectToAction("Error", "Home"); 
            }
        }

        // POST: /Recipient/Cancel
        [HttpPost]
        public ActionResult Cancel()
        {                      
            return RedirectToAction("ShowGrid", "Recipient");
        }

        // GET: /Recipient/Delete?pendingId=x&recipientId=y
        public IActionResult Delete(int? recipientId, int? pendingId)
        {
             try
            {
                _context.RecipientRepo.Delete(recipientId);
    
                if (pendingId.HasValue && pendingId > 0) return RedirectToAction("Index", "Circulation", new { id = pendingId });
                else return RedirectToAction("ShowGrid", "Recipient");
            }
            catch (Exception ex)
            {
                Log.Me.Error("Failed to delete recipient " + recipientId.ToString() + ". Error was: " + ex.Message);
                return RedirectToAction("Error", "Home"); 
            } 
        } 

        public Recipient Read(int id, string method)
        {
           Recipient r = _context.RecipientRepo.Read(id);
            if (r==null)
            {
                Log.Me.Fatal("Recipient with id " + id.ToString() + " could not be read in " + method); 
            }
            return r;
        }

    }
}
     