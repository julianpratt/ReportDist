using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
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
            Log.Me.Debug(method + " - User: " + CheckIdentity().UserName + ", PendingId: " + (id ?? 0).ToString());
            ApplicationVersion();

            if (NullId(id, "PendingId", method)) return RedirectToAction("Error", "Home"); 
            PendingReport report = _context.PendingReportRepo.Read(id);

            if (IsNull(report, "PendingReport", method)) return RedirectToAction("Error", "Home");
           
            ViewData["PendingId"] = report.PendingId.ToString();

            return View();
        }

        [HttpPost]
        [RequestSizeLimit(52428800)]        // Handle requests up to 50 MB
        public async Task<IActionResult> Upload(IList<IFormFile> files, string pendingId)
        {
            string method = "Document/Upload";
            Log.Me.Debug(method + " - User: " + CheckIdentity().UserName + ", PendingId: " + (pendingId ?? "not provided"));
            ApplicationVersion();

            if (IsNull(pendingId, "PendingId", method)) return StatusCode(400, "No pendingId");

            try
            {                
                // Generate Filename
                string filename = _context.PendingReportRepo.GenerateFilename(pendingId.ToInteger());
                if (IsNull(filename, "filename", method)) return StatusCode(400, "No filename");
                Log.Me.Debug(method + ": Filename is " + filename);

                if (files.Count == 0)
                {
                    Log.Me.Error(method + " no document uploaded for pendingId: " + pendingId);
                    return StatusCode(400, "No document");
                }

                IFormFile source = files.FirstOrDefault();
                int progress = await UploadFile(source, _fsys, filename, _sizeLimit);       
                
                if (progress == 4)
                {
                    // Save filename
                    PendingReport report = _context.PendingReportRepo.Read(pendingId.ToInteger());
                    report.eFileName = filename;
                    string check = _context.PendingReportRepo.CheckFileSize(_fsys, report, Config.Get("UploadDirectory"));
                    if (check != null) report.eFilePath = "OVERSIZE";
                    _context.PendingReportRepo.Update(report);
                    Log.Me.Info(CheckIdentity().UserName + " uploaded file " + filename + " to report " + pendingId.ToString());
                }
                else
                {
                    string message = null;
                    int    code    = 0; 
                    if      (progress == 0) { message = "File empty or too large:   " + filename; code = 413; }
                    else if (progress == 1) { message = "Failed to upload file:     " + filename; code = 400; }
                    else if (progress == 2) { message = "Type of file not accepted: " + filename; code = 415; }
                    else if (progress == 3) { message = "Failed to save file:       " + filename; code = 500; }
                    Log.Me.Warn(message);
                    return StatusCode(code, message);
                }

            }
            catch (Exception ex)
            {
                Log.Me.Error("Exception in " + method + " :" + ex.Message);
                return StatusCode(500, "Internal Error. See logs.");
            }

            return this.Content("success");
        }

        private async Task<int> UploadFile(IFormFile source, IFile filesys, string filename, long sizeLimit)
        {
            byte[] file;
            int    progress = 0;

            try
            {
                long len = source.Length;
                // Check if the file is empty or exceeds the size limit.
                if (len == 0L || len > sizeLimit) return progress;
               
                string sourcename = ContentDispositionHeaderValue.Parse(source.ContentDisposition).FileName.ToString().Trim('"');

                progress = 1; 
                using (Stream input = source.OpenReadStream())
                {
                    file = await UploadStream(input, source.Length);
                }
              
                /* Check the file contents */
                progress = 2;
                if (CheckFileContent(file, sourcename,  _extensions) > 0) return progress;

                /* Upload the file */
                progress = 3;
                filesys.ChangeDirectory("Upload");
                filesys.FileUpload(WebUtility.HtmlEncode(filename), new MemoryStream(file));

                progress = 4;

            }
            catch (Exception ex)
            {
                Log.Me.Error("In UploadFile. Progress is " + progress.ToString() + ", exception: " + ex.Message);
            }

            return progress;
         }

        private async Task<byte[]> UploadStream(Stream input, long fileLength)
        {
            long bytesRead  = 0L;

            byte[] file = new byte[fileLength+1];
            byte[] buffer = new byte[16 * 1024];
       
            int readBytes;

            while ((readBytes = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                Array.Copy(buffer,0, file, bytesRead, readBytes);
                bytesRead += readBytes;
                await Task.Delay(5); // This is only to make the process slower
            }    
       
            return file;
        }

        private string EnsureCorrectFilename(string filename)
        {   
            if (filename.Contains("/"))
                filename = filename.Substring(filename.LastIndexOf("/") + 1);
            if (filename.Contains("\\"))
                filename = filename.Substring(filename.LastIndexOf("\\") + 1);  

            return filename;
        }

        [HttpPost]
        [DisableFormValueModelBinding]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(52428800)]        // Handle requests up to 50 MB
        public async Task<IActionResult> UploadDocument(string pendingId)
        {
            // Generate Filename
            string filename = _context.PendingReportRepo.GenerateFilename(pendingId.ToInteger());
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
                    PendingReport report = _context.PendingReportRepo.Read(pendingId.ToInteger());
                    report.eFileName = filename;
                    string check = _context.PendingReportRepo.CheckFileSize(_fsys, report, Config.Get("UploadDirectory"));
                    if (check != null) report.eFilePath = "OVERSIZE";
                    _context.PendingReportRepo.Update(report);
                    Log.Me.Info(CheckIdentity().UserName + " uploaded file " + filename + " to report " + pendingId);
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

        /// Check the contents of a byte array file.
        /// First checks the source file name extension against a list of permitted extensions.
        /// Second inspects the contents of the stream to confirm it matches the extension.
        /// Returns 0 if OK or 415 (Invalid File Type)
        public static int CheckFileContent(byte[] file, string sourceFilename, string[] permittedExtensions)
        {
            if (file == null || file.Length == 0)
            {
                throw new Exception("Cannot process an empty file in CheckFileContent");
            }
            if (string.IsNullOrEmpty(sourceFilename))
            {
                throw new Exception("No filename in CheckFileContent, so cannot determine file type.");
            }

            var ext = Path.GetExtension(sourceFilename).ToLowerInvariant();

            if (string.IsNullOrEmpty(ext))
            {
                throw new Exception("File extension missing in CheckFileContent.");
            }
            if (!permittedExtensions.Contains(ext)) return 415;
            
            if (ext.Equals(".txt") || ext.Equals(".csv") )
            {
                // Limits characters to ASCII encoding.
                for (var i = 0; i < file.Length; i++)
                {
                    if (file[i] > sbyte.MaxValue) return 415;
                } 
            }
            else
            {
                // File signature check
                // --------------------
                // With the file signatures provided in the _fileSignature dictionary, the following 
                // code tests the input content's file signature.
                var signatures = _fileSignature[ext];
                bool ok = signatures.Any(sig => file.Take(sig.Length).SequenceEqual(sig));
                if (!ok) return 415;
            }
            
            return 0;
        }

        // For more file signatures, see the File Signatures Database (https://www.filesignatures.net/)
        // and the official specifications for the file types you wish to add.
        private static readonly Dictionary<string, List<byte[]>> _fileSignature = new Dictionary<string, List<byte[]>>
        {
            { ".gif", new List<byte[]> { new byte[] { 0x47, 0x49, 0x46, 0x38 } } },
            { ".png", new List<byte[]> { new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } } },
            { ".jpeg", new List<byte[]>
                {
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE2 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE3 },
                }
            },
            { ".jpg", new List<byte[]>
                {
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE1 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE8 },
                }
            },  
            { ".pdf", new List<byte[]> { new byte[] { 0x25, 0x50, 0x44, 0x46 } } },
            { ".zip", new List<byte[]> 
                {
                    new byte[] { 0x50, 0x4B, 0x03, 0x04 }, 
                    new byte[] { 0x50, 0x4B, 0x4C, 0x49, 0x54, 0x45 },
                    new byte[] { 0x50, 0x4B, 0x53, 0x70, 0x58 },
                    new byte[] { 0x50, 0x4B, 0x05, 0x06 },
                    new byte[] { 0x50, 0x4B, 0x07, 0x08 },
                    new byte[] { 0x57, 0x69, 0x6E, 0x5A, 0x69, 0x70 },
                }
            },
        };



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