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
using Mistware.Files.Upload;
using Mistware.Files.Upload.Filters;
using Mistware.Utils;

namespace ReportDist.Controllers
{
    public class DocumentController : BaseController
    {
        protected readonly IFile       _fsys;
        private readonly long          _sizeLimit;
        private readonly string[]      _extensions = { ".pdf" };

        public DocumentController(DataContext context, IFile filesys) : base(context)
        {
            _fsys      = filesys;
            _sizeLimit = Config.Get("FileSizeLimit").ToLong();
        }

        [GenerateAntiforgeryTokenCookie]
        public IActionResult Index(int? id)
        {
            string method = "Document/Index";
            Log.Me.Debug(method + " - User: " + CheckIdentity() + ", PendingId: " + (id ?? 0).ToString());
            ApplicationVersion();

            if (NullId(id, "PendingId", method)) return RedirectToAction("Error", "Home"); 
            PendingReport report = _context.PendingReportRepo.Read(id);

            if (IsNull(report, "PendingReport", method)) return RedirectToAction("Error", "Home");
           
            HttpContext.Session.SetString("PendingId", report.PendingId.ToString());
            ViewData["PendingId"] = report.PendingId.ToString();

            return View();
        }

        [HttpPost]
        [DisableFormValueModelBinding]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(52428800)]        // Handle requests up to 50 MB
        public async Task<IActionResult> UploadDocument()
        {
            // Generate Filename
            int pendingId = HttpContext.Session.GetString("PendingId").ToInteger();
            string filename = _context.PendingReportRepo.GenerateFilename(pendingId);
            Log.Me.Debug("DocumentController.UploadDocument: Filename is " + filename);

            string uploadDir = Config.Get("UploadDirectory");
            _fsys.ChangeDirectory(uploadDir);

            try
            {
                int reason = await Uploader.Upload(Request, _fsys, uploadDir, filename, _sizeLimit, _extensions);

                if (reason > 0)
                {
                    string message = null;
                    if (reason == 415) message = "Type of file not accepted";
                    if (reason == 413) message = "File too large";
                    Log.Me.Warn(message);
                    return StatusCode(reason, message);
                }
                else
                {
                    // Save filename
                    PendingReport report = _context.PendingReportRepo.Read(pendingId);
                    report.eFileName = filename;
                    string check = _context.PendingReportRepo.CheckFileSize(_fsys, report, Config.Get("UploadDirectory"));
                    if (check != null) report.eFilePath = "OVERSIZE";
                    _context.PendingReportRepo.Update(report);
                    Log.Me.Info(CheckIdentity() + " uploaded file " + filename + " to report " + pendingId.ToString());
                }
            }
            catch (Exception ex)
            {
                Log.Me.Error(ex.Message);
                ModelState.AddModelError("File", ex.Message); 
                BadRequest(ModelState);
            }

            return Created(nameof(DocumentController), null);
        }
    }

    /// String extensions
    public static class Extensions
    {
        /// Convert string to long
        public static long ToLong(this string value, long deflt = 0L)
        {
            if (value == null) return deflt;

            long result = deflt;
            if (!long.TryParse(value, out result)) 
                throw new Exception("Unable to parse '" + value + "' as a long");
            
            return result;
        }

    }
}