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
    public class CirculationController : BaseController
    {
        public CirculationController(DataContext context) : base(context) {}

        // GET: /Circulation/Index/n
        public IActionResult Index(int? id)
        {
            string method = "Circulation/Index";
            Log.Me.Debug(method + " - User: " + CheckIdentity() + ", PendingId: " + (id ?? 0).ToString());

            if (NullId(id, "PendingId", "Circulation/Index")) return RedirectToAction("Error", "Home"); 
            
            ViewData["pendingId"] = (id.Value.ToString());
            return View();
        }

        // POST: /Circulation/LoadData
        [HttpPost]
        public IActionResult LoadData()  
        {  
            string method = "Circulation/LoadData[Post]";
            try  
            { 
                DataTableHelper helper = new DataTableHelper(Request);
                int pendingId = helper.Id;
                if (ZeroId(pendingId, "PendingId", method)) return RedirectToAction("Error", "Home");
      
                PaginatedList<Circulation> list = PaginatedList<Circulation>.Create(
                                    _context.CirculationRepo.List(pendingId), helper);

                //total number of rows count   
                int recordsTotal = list.TotalRows;  
                //Paging   
                IEnumerable<Circulation> data = list as List<Circulation>;   
                //Returning Json Data  
                return Json(new { draw = helper.Draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });  

            }  
            catch (Exception ex)  
            {                
                Log.Me.Error("Exception in " + method + ": " + ex.Message);
                return RedirectToAction("Error", "Home"); 
            }
        }  


        // GET: /Circulation/Create/pendingId
        public IActionResult Create(int? id)
        {
            string method = "Circulation/Create";
            Log.Me.Debug(method + " - User: " + CheckIdentity() + ", PendingId: " + (id ?? 0).ToString());

            if (NullId(id, "PendingId", method)) return RedirectToAction("Error", "Home"); 

            Circulation circ = new Circulation();
            circ.Id          = 0;
            circ.RecipientID = 0;
            circ.PendingId   = id.Value;
            circ.Name        = "";
            circ.Email       = "";
            circ.Address     = "";

            return View("Edit", new CirculationViewModel(circ));
        }

        // POST: /Circulation/Create
        [HttpPost]
        public ActionResult Create(Circulation circ)
        {
            string method = "Circulation/Create[Post]";

            if (ZeroId(circ.PendingId, "PendingId", method)) return RedirectToAction("Error", "Home");

            //if (!ModelState.IsValid) return View(new CirculationViewModel(circ));
           
            try
            {   
                _context.CirculationRepo.Create(circ);

                Log.Me.Info(CheckIdentity() + " added circulation to report " + circ.PendingId.ToString());

                return RedirectToAction("Index", "Circulation", new { id = circ.PendingId });   
            }
            catch (Exception ex)
            {
                Log.Me.Error("Exception in " + method + ": " + ex.Message);
                return RedirectToAction("Error", "Home"); 
            }
        }


        // GET: /Circulation/Add?pendingId=x&recipientId=y
        public IActionResult Add(int? recipientId, int? pendingId)
        {
            string method = "Circulation/Add";

            if (NullId(recipientId, "RecipientId", method)) return RedirectToAction("Error", "Home"); 
            if (NullId(pendingId,   "PendingId",   method)) return RedirectToAction("Error", "Home"); 
        
            Recipient r = _context.RecipientRepo.Read(recipientId);
            if (r==null)
            {
                Log.Me.Fatal("Recipient with id " + recipientId.ToString() + " could not be read in Circulation/Add");
                return RedirectToAction("Error", "Home"); 
            }

            try
            {    
                Circulation circ = new Circulation();
                circ.RecipientID = recipientId;
                circ.PendingId   = pendingId ?? 0;
                circ.Name        = r.Name;
                circ.Email       = r.Email;
                circ.Address     = r.Address;
                int? circId = _context.CirculationRepo.Create(circ);

                Log.Me.Info(CheckIdentity() + " added circulation to report " + circ.PendingId.ToString());

                return RedirectToAction("Edit", "Circulation", new { id = circId });
            }
            catch (Exception ex)
            {
                Log.Me.Error("Exception in " + method + ": " + ex.Message);
                return RedirectToAction("Error", "Home"); 
            }
        }


        // GET: /Circulation/Edit/n
        public IActionResult Edit(int? id)
        {
            string method = "Circulation/Edit";

            Log.Me.Debug(method + " - User: " + CheckIdentity() + ", CirculationId: " + (id ?? 0).ToString());

            if (NullId(id, "CirculationId", method)) return RedirectToAction("Error", "Home"); 

            try
            {
                Circulation circ = null;
                if ((circ = Read(id.Value, method)) == null) return RedirectToAction("Error", "Home"); 
               
                return View(new CirculationViewModel(circ));
            }
            catch (Exception ex)
            {
                Log.Me.Error("Exception in " + method + ": " + ex.Message);
                return RedirectToAction("Error", "Home"); 
            }
        }

        // POST: /Circulation/Edit
        [HttpPost]
        public ActionResult Edit(Circulation update)
        {
            string method = "Circulation/Edit[Post]";

            if (IsNull(update,    "Circulation",   method)) return RedirectToAction("Error", "Home");     
            if (NullId(update.Id, "CirculationId", method)) return RedirectToAction("Error", "Home"); 
           
            //if (!ModelState.IsValid) return View(new CirculationViewModel(update));

            try
            {
                Circulation circ = null;
                if ((circ = Read(update.Id, method)) == null)    return RedirectToAction("Error", "Home");
                if (ZeroId(circ.PendingId, "PendingId", method)) return RedirectToAction("Error", "Home");

                circ.Name     = update.Name;
                circ.Email    = update.Email;
                circ.Address  = update.Address;
                circ.ToCcBcc  = update.ToCcBcc;
                circ.Delivery = update.Delivery;
                _context.CirculationRepo.Update(circ);

                Log.Me.Info(CheckIdentity() + " updated circulation on report " + circ.PendingId.ToString());

                return RedirectToAction("Index", "Circulation", new { id = circ.PendingId });
            }
           catch (Exception ex)
            {
                Log.Me.Error("Exception in " + method + ": " + ex.Message);
                return RedirectToAction("Error", "Home"); 
            }
        }

        // POST: /Circulation/Cancel
        [HttpPost]
        public ActionResult Cancel(Circulation update)
        {
            string method = "Circulation/Cancel";
            
            if (IsNull(update,           "Circulation", method)) return RedirectToAction("Error", "Home"); 
            if (ZeroId(update.PendingId, "PendingId",   method)) return RedirectToAction("Error", "Home");
                        
            return RedirectToAction("Index", "Circulation", new { id = update.PendingId });
        }


        // GET: /Circulation/Delete/n
        public IActionResult Delete(int? id)
        {
            string method = "Circulation/Delete";

            if (NullId(id, "CirculationId", method)) return RedirectToAction("Error", "Home"); 

            try
            {
                Circulation circ = null;
                if ((circ = Read(id.Value, method)) == null) return RedirectToAction("Error", "Home"); 
               
                if (ZeroId(circ.PendingId, "PendingId", method)) return RedirectToAction("Error", "Home");

                _context.CirculationRepo.Delete(id);

                Log.Me.Info(CheckIdentity() + " deleted circulation from report " + circ.PendingId.ToString());
    
                return RedirectToAction("Index", "Circulation", new { id = circ.PendingId });
            }
            catch (Exception ex)
            {
                Log.Me.Error("Exception in " + method + ": " + ex.Message);
                return RedirectToAction("Error", "Home"); 
            }
        }

        public Circulation Read(int id, string method)
        {
            Circulation circ = _context.CirculationRepo.Read(id);
            if (circ==null)
            {
                Log.Me.Fatal("Circulation with id " + id.ToString() + " could not be read in " + method); 
            }
            return circ;
        }
    }
}
     