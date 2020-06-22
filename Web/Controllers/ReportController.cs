using System;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using ReportDist.Models;
using ReportDist.Data;
using Mistware.Files;
using Mistware.Utils;

namespace ReportDist.Controllers
{
    public class ReportController : BaseController
    {
        private   readonly IFile _filesys;
    
        public ReportController(DataContext context, IFile filesys) : base(context) 
        {
            _filesys = filesys;
        }

        // GET: /Report/Edit/n
        public IActionResult Edit(int? id, string commiterror = null)
        {
            string method = "Report/Edit";

            Log.Me.Debug(method + " - User: " + CheckIdentity() + ", PendingId: " + (id ?? 0).ToString());
            ApplicationVersion();

            if (NullId(id, "PendingId", method))  return RedirectToAction("Error", "Home");

            try
            {
                PendingReport pr = null;
                if ((pr = Read(id.Value, method)) == null) return RedirectToAction("Error", "Home");
            
                StandingData sd = _context.StandingDataRepo.Load();

                ViewData["RecipientMessage"] = "- " + _context.CirculationRepo.RecipientMessage(id.Value);
                if (commiterror != null) ViewData["CommitError"] = commiterror;

                PendingReportViewModel report = new PendingReportViewModel(pr, sd);

                //Log.Me.Info("ReportType='" + report.ReportType + "'");
                //foreach (SelectListItem item in report.ReportTypes) Log.Me.Info("ReportTypeSelect: '" + item.Text + "' = '" + item.Value + "'");
            
                return View(report);
            }
            catch (Exception ex)
            {
                Log.Me.Error("Exception in " + method + ": " + ex.Message);
                return RedirectToAction("Error", "Home"); 
            }  
        }

        private bool SaveChanges(PendingReport update, string method)
        {
            if (IsNull(update,    "PendingReport(update)", method)) return false;
            if (ZeroId(update.Id, "PendingId",             method)) return false;

            try
            {
                PendingReport report = _context.PendingReportRepo.Read(update.Id);
                if (IsNull(report, "PendingReport(read))", method)) return false;

                if (report.State > 0) return true; // If we're committed, then not saving changes is the OK path

                if (report.RecipientID <= 0) 
                {
                    Log.Me.Warn("Report had zero RecipientId in ReportController.SaveChanges(). Report: " + update.Id.ToString());
                }
                string userid = HttpContext.Session.GetString("UserId");
                if (!userid.HasValue() || userid.ToInteger() <= 0)
                {
                    Log.Me.Warn("Null or Zero userid from Session in ReportController.SaveChanges(). Report: " + update.Id.ToString());
                }
                else
                {
                    report.RecipientID = userid.ToInteger();
                }
                
                report.Abstract      = update.Abstract;
                report.Author        = update.Author;
                report.Axess         = update.Axess;
                report.JobNo         = update.JobNo;
                report.ReportType    = update.ReportType;
                report.SecurityLevel = update.SecurityLevel;
                report.Software      = update.Software; 
                report.Title         = update.Title;

                _context.PendingReportRepo.Update(report);

                Log.Me.Info(CheckIdentity() + " updated report " + update.Id.ToString());

                return true;     
            }
            catch (Exception ex)
            {
                Log.Me.Error("Exception in " + method + ": " + ex.Message);
                return false; 
            } 

        }

        // POST: /Report/Edit/n
        [HttpPost]
        public ActionResult Edit(PendingReport update)
        {
            string method = "Report/Edit[Post]";

            if (SaveChanges(update, method)) return RedirectToAction("Index", "Home");  
            else                             return RedirectToAction("Error", "Home"); 
        }

        // POST: /Report/Commit
        [HttpPost]
        public ActionResult Commit(PendingReport update)
        {
            if (update.State > 0) 
            {
                Log.Me.Error("There shouldn't even be a Commit button on the form!");
                return RedirectToAction("Error", "Home"); 
            }

            string method = "Report/Commit[Post]";

            if (!SaveChanges(update, method)) return RedirectToAction("Error", "Home"); 
          
            try
            {
                string commiterror = _context.PendingReportRepo.CommitReport(_filesys, update.Id);
                
                if (commiterror == null)
                {
                    Log.Me.Info(CheckIdentity() + " committed report " + update.Id.ToString());

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    return RedirectToAction("Edit", "Report", new { id = update.PendingId, commiterror = commiterror });
                }

            }
            catch (Exception ex)
            {
                Log.Me.Error("Exception in " + method + " (at stage Commit): " + ex.Message);
                return RedirectToAction("Error", "Home"); 
            }
        }

        // GET: /Report/Create
        public IActionResult Create()
        {
            string method = "Report/Create";
            Log.Me.Debug(method + " - User: " + CheckIdentity());
            ApplicationVersion();

            ViewData["Feature"] = "Create Report";
            return View();
        }

        // POST: /Report/Create
        [HttpPost]
        public ActionResult Create(CreateReportPostModel vm)
        {
            string method = "Report/Create[Post]";

            string userid = HttpContext.Session.GetString("UserId");
            if (!userid.HasValue() || userid.ToInteger() <= 0)
            {
                Log.Me.Warn("Null or Zero userid in ReportController.Create() for Report " + vm.ReportNo);
            }
            
            try
            {
                PendingReport report = new PendingReport();
                report.ReportNo      = vm.ReportNo;
                report.ReportYear    = "20" + vm.Year;
                report.ReportType    = vm.ReportType;
                report.RecipientID   = userid.ToInteger();

                //if (!ModelState.IsValid) return View(report);
   
                int pendingId = 0;
                if (vm.CheckNDT) pendingId = _context.PendingReportRepo.Create(report);
                else             pendingId = _context.PendingReportRepo.CreateNonNDT(report);

                if (ZeroId(pendingId, "PendingId", method)) return RedirectToAction("Error", "Home"); 

                Log.Me.Info(CheckIdentity() + " created report " + pendingId.ToString());

                return RedirectToAction("Edit", "Report", new { id = pendingId });
            }
            catch (Exception ex)
            {
                Log.Me.Error("Exception in " + method + ": " + ex.Message);
                return RedirectToAction("Error", "Home");
            }       
        }


        // GET: /Report/Delete/n
        public IActionResult Delete(int? id)
        {
            string method = "Report/Delete";

            if (NullId(id, "PendingId", method)) return RedirectToAction("Error", "Home");  

            try
            {
                _context.PendingReportRepo.Delete(id);

                Log.Me.Info(CheckIdentity() + " deleted report " + id.ToString());

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                Log.Me.Error("Exception in " + method + ": " + ex.Message);
                return RedirectToAction("Error", "Home");
            }
        }

        // POST: /Report/CirculationEdit
        [HttpPost]
        public ActionResult CirculationEdit(PendingReport update)
        {
            string method = "Report/CirculationEdit";

            if (SaveChanges(update, method)) return RedirectToAction("Index", "Circulation", new { id = update.Id });
            else                             return RedirectToAction("Error", "Home");
        }

        // POST: /Report/DocUpload
        [HttpPost]
        public ActionResult DocUpload(PendingReport update)
        {
            string method = "Report/DocUpload";

            if (SaveChanges(update, method)) return RedirectToAction("Index", "Document", new { id = update.Id });
            else                             return RedirectToAction("Error", "Home");
        }


        // GET: /Report/DocView/n
        public IActionResult DocView(int? id)
        {
            string method = "Report/DocView";
            if (NullId(id, "PendingId", method)) return RedirectToAction("Error", "Home");   

            try
            {
                PendingReport report = null;
                if ((report = Read(id.Value, method)) == null) return RedirectToAction("Error", "Home");
            
                string filename = report.eFileName;
                if (IsNull(filename, "eFileName", method)) return RedirectToAction("Error", "Home");

                _filesys.ChangeDirectory(Config.Get("UploadDirectory"));
                byte[] file = _filesys.FileDownload(filename);
            
                return new FileContentResult(file, MIME.GetMimeType(Path.GetExtension(filename)));
            }
            catch (Exception ex)
            {
                Log.Me.Error("Exception in " + method + ": " + ex.Message);
                return RedirectToAction("Error", "Home");
            }
        }

        // GET: /Report/Send/n
        public IActionResult Send(int? id)
        {
            if (NullId(id, "PendingId", "Report/Send")) return RedirectToAction("Error", "Home");   
            
            _context.PendingReportRepo.Send(id.Value, _filesys);

            Log.Me.Info(CheckIdentity() + " sent report " + id.ToString());

            return RedirectToAction("Index", "Home");
        }

        // GET: /Report/SendAll
        public IActionResult SendAll()
        {
            _context.PendingReportRepo.SendAll(_filesys);

            Log.Me.Info(CheckIdentity() + " sent all reports");

            return RedirectToAction("Index", "Home");
        }

        public PendingReport Read(int id, string method)
        {
            PendingReport report = _context.PendingReportRepo.Read(id);
            if (report==null)
            {
                Log.Me.Fatal("PendingReport with id " + id.ToString() + " could not be read in " + method); 
            }
            return report;
        }
    }
}
     