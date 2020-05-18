using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using ReportDist.Models;
using ReportDist.Data;
using Mistware.Files;
using Mistware.Utils;

namespace ReportDist.Controllers
{
    public class ReportController : Controller
    {
        protected readonly DataContext _context;
        private   readonly IFile       _filesys;

        public ReportController(DataContext context, IFile filesys)
        {
            _context = context;
            _filesys = filesys;
        }

        // GET: /Report/Edit/n
        public IActionResult Edit(int? id)
        {
            if (id == null) return RedirectToAction("Error", "Home"); 
            PendingReport pr = _context.PendingReportRepo.Read(id);
            if (pr == null) return NotFound();
            StandingData sd = _context.StandingDataRepo.Load();

            ViewData["RecipientMessage"] = "- " + _context.CirculationRepo.RecipientMessage(id.Value);

            PendingReportViewModel report = new PendingReportViewModel(pr, sd);
        
            return View(report);
        }

        // POST: /Report/Edit/n
        [HttpPost]
        public ActionResult Edit(PendingReport update)
        {
            /*
            if (!ModelState.IsValid) return View(report);
            */
            try
            {
                // TODO Test Id is valid (i.e. not zero)!
                PendingReport report = _context.PendingReportRepo.Read(update.Id);
                report.Abstract      = update.Abstract;
                report.Author        = update.Author;
                report.Axess         = update.Axess;
                report.JobNo         = update.JobNo;
                report.ReportType    = update.ReportType;
                report.SecurityLevel = update.SecurityLevel;
                report.Software      = update.Software; 
                report.Title         = update.Title;

                _context.PendingReportRepo.Update(report);
            }
            catch
            {
                return RedirectToAction("Error", "Home"); 
            }
            
            return RedirectToAction("Index", "Home");        
        }

        // POST: /Report/Commit
        [HttpPost]
        public ActionResult Commit(PendingReport update)
        {
            /*
            if (!ModelState.IsValid) return View(report);
            */
            try
            {
                // TODO Test Id is valid (i.e. not zero)!
                PendingReport report = _context.PendingReportRepo.Read(update.Id);
                report.Abstract      = update.Abstract;
                report.Author        = update.Author;
                report.Axess         = update.Axess;
                report.JobNo         = update.JobNo;
                report.ReportType    = update.ReportType;
                report.SecurityLevel = update.SecurityLevel;
                report.Software      = update.Software; 
                report.Title         = update.Title;

                _context.PendingReportRepo.Update(report);

                string dump = _context.PendingReportRepo.CommitReport(_filesys, update.Id);
            }
            catch (Exception ex)
            {
                Log.Me.Error("Exception in ReportController.Commit: " + ex.Message);
                return RedirectToAction("Error", "Home"); 
            }
            
            return RedirectToAction("Index", "Home");        
        }

        // GET: /Report/Create
        public IActionResult Create()
        {
            ViewData["Feature"] = "Create Report";
            return View();
        }

        // POST: /Report/Create
        [HttpPost]
        public ActionResult Create(CreateReportPostModel vm)
        {
            /*
            if (!ModelState.IsValid) return View(report);
            */

            int pendingId = 0;

            try
            {
                PendingReport report = new PendingReport();
                report.ReportNo      = vm.ReportNo;
                report.ReportYear    = "20" + vm.Year;
                report.ReportType    = vm.ReportType;
                report.RecipientID   = 16; // TODO!!
   
                if (vm.CheckNDT) pendingId = _context.PendingReportRepo.Create(report);
                else             pendingId = _context.PendingReportRepo.CreateNonNDT(report);
            }
            catch (Exception ex)
            {
                Log.Me.Error("/Report/Create(post): " + ex.Message);
                return RedirectToAction("Error", "Home"); 
            }
            
            return RedirectToAction("Edit", "Report", new { id = pendingId });        
        }


        // GET: /Report/Delete/n
        public IActionResult Delete(int? id)
        {
            _context.PendingReportRepo.Delete(id);

            return RedirectToAction("Index", "Home");
        }

        // GET: /Report/DocView/n
        public IActionResult DocView(int? id)
        {
            try
            {
                PendingReport report = _context.PendingReportRepo.Read(id);
                if (report == null) 
                {
                    Log.Me.Error("ReportController.DocView: Cannot find report id " + id.ToString());
                    return RedirectToAction("Error", "Home");
                }
                string filename = report.eFileName;
                string ext = Path.GetExtension(filename);
                _filesys.ChangeDirectory(Config.Get("UploadDirectory"));
                byte[] file = _filesys.FileDownload(filename);
            
                return new FileContentResult(file, MIME.GetMimeType(ext));
            }
            catch (Exception ex)
            {
                Log.Me.Error("Error in ReportController.DocView: " + ex.Message);
                return RedirectToAction("Error", "Home");
            }
        }

        // GET: /Report/Send/n
        public IActionResult Send(int? id)
        {
            if (!id.HasValue) 
            {
                Log.Me.Error("Error in ReportController.Send: id had no value.");
                return RedirectToAction("Error", "Home");   
            }
            _context.PendingReportRepo.Send(id.Value, _filesys);

            return RedirectToAction("Index", "Home");
        }

        // GET: /Report/SendAll
        public IActionResult SendAll()
        {
            _context.PendingReportRepo.SendAll(_filesys);
            return RedirectToAction("Index", "Home");
        }
    }
}
     