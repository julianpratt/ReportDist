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
    public class CirculationController : Controller
    {
        protected readonly DataContext _context;

        public CirculationController(DataContext context)
        {
            _context = context;
        }

        // GET: /Circulation/Index/n
        public IActionResult Index(int? id)
        {
            if (!id.HasValue || id == null) throw new Exception("No id passed to Circulation/Index");
            ViewData["pendingId"] = (id.Value.ToString());
            return View();
        }

        // POST: /Circulation/LoadData
        [HttpPost]
        public IActionResult LoadData()  
        {  
            try  
            { 
                DataTableHelper helper = new DataTableHelper(Request);
                int pendingId = helper.Id;
                if (pendingId == 0) pendingId = 20;
      
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
                Log.Me.Error("Exception in /Circulation/LoadData[Post]: " + ex.Message);
                return RedirectToAction("Error", "Home"); 
            }

        }  


        // GET: /Circulation/Create/pendingId
        public IActionResult Create(int? id)
        {
            if (id == null) return NotFound();

            Circulation circ = new Circulation();
            circ.Id          = 0;
            circ.RecipientID = 0;
            circ.PendingId   = id ?? 0;
            circ.Name        = "";
            circ.Email       = "";
            circ.Address     = "";

            return View("Edit", new CirculationViewModel(circ));
        }

        // POST: /Circulation/Create
        [HttpPost]
        public ActionResult Create(Circulation circ)
        {

            int pendingId = circ.PendingId;
            /* TODO
            if (!ModelState.IsValid) return View(report);
            */
            try
            {
                // TODO Test Id is valid (i.e. not zero)!       
                _context.CirculationRepo.Create(circ);
            }
            catch (Exception ex)
            {
            
                Log.Me.Error("Exception in /Circulation/Create[Post]: " + ex.Message);
                return RedirectToAction("Error", "Home"); 
            }
            
            if (pendingId > 0 ) return RedirectToAction("Index", "Circulation", new { id = pendingId });   
            else                return RedirectToAction("Index", "Home");   
        }


        // GET: /Circulation/Add?pendingId=x&recipientId=y
        public IActionResult Add(int? recipientId, int? pendingId)
        {
            if (recipientId == null || pendingId == null) return NotFound();

            Recipient r = _context.RecipientRepo.Read(recipientId);

            Circulation circ = new Circulation();
            circ.RecipientID = recipientId;
            circ.PendingId   = pendingId ?? 0;
            circ.Name        = r.Name;
            circ.Email       = r.Email;
            circ.Address     = r.Address;
            int circId = _context.CirculationRepo.Create(circ);

            return RedirectToAction("Edit", "Circulation", new { id = circId });
        }


        // GET: /Circulation/Edit/n
        public IActionResult Edit(int? id)
        {
            Circulation circ = _context.CirculationRepo.Read(id);
            if (circ == null) return NotFound();        
            return View(new CirculationViewModel(circ));
        }

        // POST: /Circulation/Edit
        [HttpPost]
        public ActionResult Edit(Circulation update)
        {

            int pendingId = 0;
            /*
            if (!ModelState.IsValid) return View(report);
            */
            try
            {
                // TODO Test Id is valid (i.e. not zero)!
                Circulation circ = _context.CirculationRepo.Read(update.Id);
                circ.Name     = update.Name;
                circ.Email    = update.Email;
                circ.Address  = update.Address;
                circ.ToCcBcc  = update.ToCcBcc;
                circ.Delivery = update.Delivery;
            
                _context.CirculationRepo.Update(circ);

                pendingId = circ.PendingId;
            }
            catch
            {
                return RedirectToAction("Error", "Home"); 
            }
            
            if (pendingId > 0 ) return RedirectToAction("Index", "Circulation", new { id = pendingId });   
            else                return RedirectToAction("Index", "Home");   
        }

        // POST: /Circulation/Cancel
        [HttpPost]
        public ActionResult Cancel(Circulation update)
        {

            int pendingId = update.PendingId;
                        
            if (pendingId > 0 ) return RedirectToAction("Index", "Circulation", new { id = pendingId });   
            else                return RedirectToAction("Index", "Home");   
        }


        // GET: /Circulation/Delete/n
        public IActionResult Delete(int? id)
        {
            Circulation circ = _context.CirculationRepo.Read(id);
            int pendingId = circ.PendingId;

            _context.CirculationRepo.Delete(id);
    
            return RedirectToAction("Index", "Circulation", new { id = pendingId });  
        }

    }
}
     