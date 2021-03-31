using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Mail;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Mistware.Utils;
using Mistware.Files;
using Mistware.Postman;

namespace ReportDist.Data
{
	/// <summary>
    /// Repository for CRUD operations on Pending Report records in a database
    /// </summary>
    public class PendingReportRepository : Repository<PendingReport>      
    {
    	/// <summary>
        /// Initializes a new instance of the PendingReportRepository class
        /// <param name="context">The data context.</param>
        /// </summary>
        public PendingReportRepository(DataContext context) : base(context) {}

        public int CreateNonNDT(PendingReport r)
        {
            if (r == null) throw new Exception("Cannot CreateNonNDT PendingReport with null entity!");
            if (r.ReportYear == null) throw new Exception("Cannot CreateNonNDT PendingReport with null ReportYear!");
            if (!r.ReportYear.IsInteger()) throw new Exception("Cannot CreateNonNDT PendingReport with non integer ReportYear!");

            int year = r.ReportYear.ToInteger();
            int nextNum = 1;

            // Get Next Number
            IQueryable<NextNumbers> q = _context.NextNumberSet.Where(n => n.ReportYear == year);
            int rows = q.Count();
            if (rows == 0) 
            {
                NextNumbers n = new NextNumbers();
                n.ReportYear = year;
                n.NextNumber = nextNum;
                _context.NextNumberSet.Add(n);
                _context.SaveChanges();
            }
            else if (rows > 1) 
            {
                throw new Exception("There are multiple rows in the NextNumber table for year " + r.ReportYear );  
            }
            else
            {
                NextNumbers n = q.First();
                nextNum = n.NextNumber + 1;
                n.NextNumber = nextNum;

                var local = _context.NextNumberSet.Local.FirstOrDefault(e => e.ReportYear.Equals(year));
                if (local != null)
                {
                    _context.Entry(local).State = EntityState.Detached;
                }
                _context.Entry(n).State = EntityState.Modified;
                _context.NextNumberSet.Update(n);
                _context.SaveChanges();
            }

            r.ReportNo = r.ReportNo + nextNum.ToString() + "/";

            return base.Create(r);

        }
      
        public new void Delete(int? id)
        {
            if (id == null) throw new Exception("Cannot delete PendingReport with null id!");
   
            PendingReport r = base.Read(id);
            if (r == null) throw new Exception("PendingReport " + id.ToString() + "Not found");

            r.Deleted = true;
            base.Update(r);
        }

        public IQueryable<PendingReport> List(string sort, string filter)
        {
            IQueryable<PendingReport> reports = _context.PendingReports.Where(r => (!r.Deleted));

            if (!String.IsNullOrEmpty(filter))
            {
                string[] parts = filter.Split('-'); 
                string mode = parts[0];
                if (mode == "search")
                {
                    if (parts.Count() != 4) throw new Exception("filter search-start-num-type badly formed in PendingReportRepo.List");

                    string start = parts[1];
                    if (String.IsNullOrEmpty(start)) throw new Exception("filter search-start-num-type start is missing in PendingReportRepo.List");
                    reports = reports.Where(r => r.ReportNo.StartsWith(start));
 
                    string num   = parts[2];
                    if (!String.IsNullOrEmpty(num))
                    {
                        if (!num.IsInteger()) throw new Exception("filter search-start-num-type num is not an integer in PendingReportRepo.List");
                        reports = reports.Where(r => r.ReportNo.EndsWith("/" + num + "/"));
                    }
                    string type  = parts[3];
                    if (!String.IsNullOrEmpty(type))
                    {
                        reports = reports.Where(r => r.ReportType == type);
                    }
                }
                else
                {
                    if (parts.Count() > 2) throw new Exception("filter badly formed in PendingReportRepo.List");
                    string option = null;
                    if (parts.Count() == 2) 
                    {
                        option=parts[1];
                        if (!option.IsInteger()) throw new Exception("filter option is not an integer in PendingReportRepo.List");
                        int userid = option.ToInteger();
                        reports = reports.Where(r => (r.RecipientID == userid) );
                    }
                    
                    if (mode == "uncommitted")
                    {
                        reports = reports.Where(r => r.State == 0);
                    }
                    else if (mode == "committed")
                    {
                        reports = reports.Where(r => (r.State == 1 && r.CID <= 0)); 
                    }
                    else if (mode == "unsent")
                    {
                        reports = reports.Where(r => (r.State == 1 && r.CID > 0)); 
                    }
                    else if (mode == "sent")
                    {
                        reports = reports.Where(r => r.State == 2); 
                    }
                    else if (mode == "poisoned")
                    {
                        reports = reports.Where(r => r.State == 3); 
                    }
                }
            }

            if (!String.IsNullOrEmpty(sort))
            {
                List<string> sortTerms = sort.Wordise();
                string sortColumn = null;
                string sortOrder  = null;
                if (sortTerms.Count() == 1)
                {
                    sortColumn = sortTerms[0];
                    sortOrder  = "asc";
                }
                if (sortTerms.Count() == 2)
                {
                    sortColumn = sortTerms[0];
                    sortOrder  = sortTerms[1];
                    if (sortOrder != "asc" && sortOrder != "desc") 
                        throw new Exception("Sort order not recognised: " + sortOrder);
                }

                if (sortColumn != null)
                {
                    Expression<Func<PendingReport,string>>   fs = null; 
                    Expression<Func<PendingReport,DateTime>> fd = null; 

                    switch (sortColumn.ToLower())
                    {
                        case "author":
                            fs = (r => r.Author);
                            break;
                        case "creationdate":
                            fd = (r => r.CreationDate);
                            break;   
                        case "title":
                            fs = (r => r.Title);
                            break;
                        case "reportno":
                            fs = (r => r.ReportNo);
                            break;
                    }
                    if (fs != null)
                    {
                        if (sortOrder == "desc") reports = reports.OrderByDescending(fs);
                        else                     reports = reports.OrderBy(fs);
                    }
                    else if (fd != null)
                    {
                        if (sortOrder == "desc") reports = reports.OrderByDescending(fd);
                        else                     reports = reports.OrderBy(fd);
                    }
                }
            }

            return reports;

        }

        /// Generate a filename based on the report number

        public string GenerateFilename(int pendingId)
        {
            return GenerateFilename(this.Read(pendingId), ".pdf");
        }

        /// Generate a filename for a report

        public string GenerateFilename(PendingReport report, string ext)
        {

            string reportYear;
            string filename;

            string[] split = report.ReportNo.Split('/');
            string org = split[0].ToUpper();

            // Check that the second number relates to a valid year number
            reportYear = LongYear(split[1]);

            // Check that we start with PT
            if (org == "PT")
            {
                // Its a PT Report so make PT file name
                // Check param 3 - should start with 2 characters then some numbers
                filename = reportYear + "_" + "DX" + split[2].Substring(0, 2) + "_" + split[2] + ext;
            }
            else if (org == "EEN" || org == "ENT" || org == "ETG" || org == "UTG")
            {
                // Its a EEN or ENT report so make EEN or ENT file name
                if (split.Length == 6)
                {
                    // Report number contains a team code so we need to deal with the /
                    filename = reportYear + "_" + split[2] + "_" + split[3] + "_" + split[4] + ext;
                }
                else
                {
                    // Report number does not contain a team code so we dont need to deal with the /
                    filename = reportYear + "_" + split[2] + "_" + split[3] + ext;
                }

            }
            else
            {
                // No idea what kinda report error out
                throw new Exception("Only PT, EEN, ENT, ETG or UTG reports are valid");
            }

            return filename;
        }    
        private string LongYear(string shortYear)
        {
            int year = shortYear.ToInteger(); 
            int longYear;

            if (year < 40) longYear = year + 2000;
            else           longYear = year + 1900;
            if (longYear > 2099) throw new Exception("Year is probably invalid");

            return longYear.ToString();
        }

        public string CheckFileSize(IFile filesys, int pendingId)
        {
            string upload = Config.Get("UploadDirectory");

            PendingReport report = this.Read(pendingId);
            if (report == null)  throw new Exception("Cannot CheckFileSize - Pending Report not found!");

            return CheckFileSize(filesys, report, upload);
        }

        public string CheckFileSize(IFile filesys, PendingReport report, string upload)
        {
            long maxFileSize = 0L;
            if (!long.TryParse(Config.Get("AttachmentSizeLimit"), out maxFileSize)) 
                throw new Exception("Unable to parse FileSizeLimit as a long");

            filesys.ChangeDirectory(upload);
            if (filesys.FileLength(report.eFileName) > maxFileSize)
            {
                return "The Report's PDF File is too large to send as an email attachment. Please amend the circulation list and amend all recipients that have been marked for Full Electronic distribution. Full Electronic distribution is NOT available for this report.";
            }
            return null;
        }

        public string CommitReport(IFile filesys, int pendingId)
        {
            if (filesys == null) throw new Exception("Cannot Commit Report - no file system!");
            if (pendingId <= 0)  throw new Exception("Cannot Commit Report - invalid report id!");
            PendingReport report = this.Read(pendingId);
            if (report == null)  throw new Exception("Cannot Commit Report - Pending Report not found!");
            if (report.RecipientID <= 0)  throw new Exception("Cannot Commit Report - Zero RecipientId!");

            string upload = Config.Get("UploadDirectory");
            string outbox = Config.Get("OutboxDirectory");

            IQueryable<Circulation> circlist = _context.CirculationRepo.List(pendingId)
                                                    .Where(c => c.ElecDeliveryCode == "EF");
            if (circlist.Count() > 0)
            {
                if (report.eFileName == null) return "Cannot Commit Report - no pdf for EF circulation";

                //-------------------------------------------
                // We have some EF recipients (check the file size of the PDF file - if it's Too big - Tell the user)
                //-------------------------------------------
                string msg = CheckFileSize(filesys, report, upload);
                if (msg != null) return msg;
            }

            //--------------------------------------------------------------------------------
            // Create the xml metadata file and upload it to Outbox. 
            //--------------------------------------------------------------------------------
            string metadatafile = GenerateFilename(report, ".xml");
            TransferReport transfer = new TransferReport(report);
            string xmlout = transfer.ToXML();
            if (xmlout == null) throw new Exception("Cannot Commit Report - failed to create xml metadata");
            filesys.ChangeDirectory(outbox);
            filesys.FileUpload(metadatafile, xmlout.ToStream());
            Log.Me.Info("Uploaded XML");

            //--------------------------------------------------------------------------------
            // Copy pdf to Outbox
            //--------------------------------------------------------------------------------
            filesys.ChangeDirectory(upload);
            if (report.eFileName != null) filesys.FileCopy(report.eFileName, outbox);

            //--------------------------------------------------------------------------------
            // Change State to Committed
            //--------------------------------------------------------------------------------
            report.State = 1;
            this.Update(report);

            return null;
        }  

        /// <summary>
        /// Check which reports have arrived in the Catalogue and have CIDs
        /// </summary>
        /// <param name="api">API urls and xpaths</param>
        public string GetCIDs()
        {
            try
            {
                 List<PendingReport> reports = _context.PendingReports.
                                    Where(r => (!r.Deleted) && (r.State == 1) && (r.CID <= 0)).ToList();

                int todo = reports.Count();
                if (todo <= 0) return "Nothing to check";

                int updated = 0;
                foreach (PendingReport report in reports)
                {
                    // Get CID
                    report.CID = GetCID(report.FullReportNo);
                    if (report.CID.HasValue && report.CID > 0)
                    {
                        // Update the record to record the CID.
                        this.Update(report);
                        ++updated;
                    }
                }
                return todo.ToString() + " reports checked. " + updated.ToString() + " CIDs found.";
            }
            catch (Exception ex)
            {
                return "WARNING! GetCIDs() failed this time because of exception: " + ex.Message; 
            }
           
        }
        
        public static int? GetCID(string reportNumber)
        {
            CatalogueAPI api = CatalogueAPI.Me;

            if (!api.IsValid()) throw new Exception("Missing configuration required by GetCID.");

            string searchAPI = String.Format(api.SearchCataloguesAPI, reportNumber, api.CatalogueUserId);

            string xml1 = WebLoadSync(searchAPI, api.Authorization);
            if (xml1 == null) return null;

            //Console.WriteLine(xml1); 

            // Check that the ReportNumber we searched on is in the XML returned 
            string cid = null;
            string s = null;
            int i = 1;
            if (api.Authorization == null)
            {
                // Old style API potentially returns multiple results from search => must find right one
                while (true)
                {
                    cid = XML.ReadXmlAttribute(xml1, String.Format(api.CIDXpath,i));
                    s = XML.ReadXmlNode(xml1, String.Format(api.ReportNoXpath,i));
                    if (s == null || s.Length == 0) { cid=null; break; }    
                    if (s == reportNumber.Trim()) break;
                    ++i;
                }
            }
            else
            {
                // New style finds the report or it doesn't
                cid = XML.ReadXmlAttribute(xml1, api.CIDXpath);
            }
            
            if (!cid.HasValue()) return null; // Report has not yet arrived in the catalogue

            // Check the attached document has also arrived.
            string xml2 = WebLoadSync(String.Format(api.GetCatalogueAPI, cid), api.Authorization);
            if (xml2 == null) return null;

            //Console.WriteLine("======================================================================================"); 
            //Console.WriteLine(xml2); 

            // Check ReportNumber again
            s = XML.ReadXmlNode(xml2, String.Format(api.ReportNoGetXpath,i));
            if (s == null || s.Length == 0) return null;   
            if (s != reportNumber.Trim())   return null; 

            //string summary = XML.ReadXmlNode(xml2, api.AbstractXpath);
            //Console.WriteLine(ToHTML(ToTextList(summary)));

            //string eFileName  = XML.ReadXmlNode(xml2, api.eFileNameXpath);
            string attachment = XML.ReadXmlNode(xml2, api.AttachmentXpath);
            // We'll only accept reports with an attachment (but we don't care about what it is called)
            if (!attachment.HasValue()) return null;
            // For the time being, we'll stop trying to get a match on the filename
            // if (eFileName == null || attachment == null) return null;
            // if (eFileName.Trim().ToLower() != attachment.Trim().ToLower()) return null;

            return cid.ToInteger(0); 
        }

        private static string WebLoadSync(string fullpath, KeyList header)
        {
            return Task.Run(()=>WebLoad(fullpath, header)).GetAwaiter().GetResult();
        }

        private static async Task<string> WebLoad(string fullpath, KeyList header)
        {
            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = delegate ( HttpRequestMessage msg,
                X509Certificate2 certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
            var client = new HttpClient(handler);

            HttpResponseMessage response = null;    
            var request = new HttpRequestMessage();
            request.RequestUri = new Uri(fullpath);    
            request.Method = HttpMethod.Get;    
            if (header != null) foreach (KeyPair p in header) request.Headers.Add(p.Key, p.Value);
            response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else 
            {
                return null;
            } 
        }

        public void SendAll(IFile filesys)
        {
            IQueryable<PendingReport> reports = _context.PendingReports.Where(r => (!r.Deleted));
            reports = reports.Where(r => r.State == 1).Where(r => r.CID != null);

            List<EmailBatch> batches = new List<EmailBatch>();

            foreach (PendingReport report in reports.ToList())
            {
                EmailBatch batch = CreateEmailBatch(report.PendingId); 
                if (batch != null) batches.Add(batch);
            }

            if (batches.Count > 0) SendEmailBatches(batches, filesys);
        }

        public void Send(int pendingId, IFile filesys)
        {
            List<EmailBatch> batches = new List<EmailBatch>();

            EmailBatch batch = CreateEmailBatch(pendingId); 
            if (batch != null) 
            {
                batches.Add(batch);
                SendEmailBatches(batches, filesys);
            }
            
        }

        private EmailBatch CreateEmailBatch(int pendingId)
        {
            EmailBatch batch = new EmailBatch();
            batch.Recipients = new List<EmailRecipient>();
            string emailDomain = EmailConfig.Me.InternalEmailDomain;
            batch.Postmaster   = EmailConfig.Me.Postmaster;

            PendingReport pr = this.Read(pendingId);

            batch.Name       = pr.FullReportNo.Trim();

            if (pr.State != 1)
            {
                Log.Me.Error("State != 1 for " + pr.FullReportNo + ", so report send was blocked.");
                return null; 
            }
    
            if (!pr.CID.HasValue || pr.CID <= 0) 
            {  
                Log.Me.Error("Could not get CID for " + pr.FullReportNo);
                return null;              
            }

            int cid = pr.CID.Value;
            string attachment = pr.eFileName;

            Log.Me.Debug("Getting the From email address");
            Recipient r = null;
            if (!pr.RecipientID.HasValue || pr.RecipientID <= 0) 
            {
                Log.Me.Error("RecipientID not set.");
                return null;
            }
            else
            {
                r = _context.RecipientRepo.Read(pr.RecipientID);
                
                if (r == null ||  r.Email == null || r.Name == null)
                {
                    Log.Me.Error("Recipient data not set. RecipientID was " + pr.RecipientID.ToString());
                    return null;
                }
            }
            batch.From = r.ToMailAddress();
                
            Log.Me.Debug("From: " + batch.From.ToString());

            Log.Me.Debug("-------------------------------------------------------------------------------------");
            Log.Me.Debug("Email Circulation Log for CID : " + cid.ToString());
            Log.Me.Debug("-------------------------------------------------------------------------------------");
            Log.Me.Debug("");

            //------------------------------------------------------------------------------------------------
            // If we find a ReportLink tag we need to replace it with a link to the PTOPAC_Link project
            //------------------------------------------------------------------------------------------------
            string company = EmailConfig.Me.Company;
            string support = EmailConfig.Me.CatalogueSupport;

            string linkPath = EmailConfig.Me.PortalLinkURL;
            if (linkPath != null) linkPath += cid.ToString();
            Log.Me.Debug("Portal Link = " + (linkPath ?? "(empty)"));

            //-----------------------------------------------------------------------
            // Loop through all the recipients - 
            //-----------------------------------------------------------------------
            string IntExt;

            Log.Me.Debug("Getting the list of Email Recipients from the database");
            IQueryable<Circulation> circs = _context.Circulations;
            circs = circs.Where(c => c.PendingId == pendingId);
                
            foreach (Circulation circ in circs.ToList())
            {
                //---------------------------------------------------
                // What type of Delivery is this? ES, EF or EC ?
                //---------------------------------------------------
                string deliveryType = circ.ElecDeliveryCode;
                if (deliveryType != null)
                { 
                    Log.Me.Debug("------------------------------------------------------------");
                    Log.Me.Debug("Building Email Message");
                    EmailRecipient recipient = new EmailRecipient();
                    recipient.To = new MailAddress(circ.Email, circ.Name);
                    //--------------------------------------------------------------------
                    // Figure out if the current recipient is internal or external
                    //--------------------------------------------------------------------
                    if (circ.Email.Trim().Right(emailDomain.Length).ToLower() == emailDomain) IntExt = "I";
                    else                                                                      IntExt = "X";

                    Log.Me.Debug("  To            : " + recipient.To.ToString());
                    Log.Me.Debug("  From          : " + batch.Postmaster.ToString());
                                            
                    string deliveryMsg = "";
                    if      (deliveryType == "EF") deliveryMsg = "Electronic FULL";
                    else if (deliveryType == "EC") deliveryMsg = "Electronic SUMMARY with CD to follow";
                    else                           deliveryMsg = "Electronic SUMMARY";

                    recipient.DeliveryType = deliveryType + IntExt;

                    if (deliveryType == "EF") recipient.Attachment = attachment;

                    Log.Me.Debug("  Delivery Type : " + deliveryMsg);

                    recipient.MailMergeFields = new Dictionary<string, string>();
                    recipient.MailMergeFields.Add("FromName",         batch.From.DisplayName);
                    recipient.MailMergeFields.Add("FromEmail",        batch.From.Address);
                    recipient.MailMergeFields.Add("ReportNumber",     pr.FullReportNo ?? "");
                    recipient.MailMergeFields.Add("Title",            pr.Title        ?? "");
                    recipient.MailMergeFields.Add("Authors",          pr.Author       ?? "");
                    recipient.MailMergeFields.Add("Summary",          pr.Abstract     ?? "");
                    recipient.MailMergeFields.Add("ReportLink",       linkPath        ?? "");
                    recipient.MailMergeFields.Add("Company",          company         ?? "");
                    recipient.MailMergeFields.Add("CatalogueSupport", support         ?? "");

                    batch.Recipients.Add(recipient);
                }
            }

            Log.Me.Debug(batch.Recipients.Count().ToString() + " recipients");
            if (batch.Recipients.Count() == 0)
            {
                // No recipients, so nothing to send, we can just set the Status to 2.
                // TODO Set status to 2 
                return null;
            }     
            
            PendingReport pr2 = this.Read(pendingId);
            if (pr2.State != 1)
            {
                Log.Me.Error("Possible race condition. Right at the end of CreateEmailBatch State != 1 for " + pr2.FullReportNo + ", so report send was blocked.");
                return null; 
            }

            // Mark the report as sent  
            pr2.State = 2; // Set State to "Sent" 
            pr2.DateSent = DateTime.Now;               
            this.Update(pr2);  // Update Record
            // Also update the state of Circulation
            //_context.CirculationRepo.SetState(pr.PendingId, 2); not needed any more

            return batch;
        }

        private void SendEmailBatches(List<EmailBatch> batches, IFile filesys)
        {
            try
            {
                //--------------------------------------------------------------------------------------
                // Kick off a Thread to send emails out to all electronic users
                //--------------------------------------------------------------------------------------
                Log.Me.Info("About to kick off the Emailing Thread");
                filesys.ChangeDirectory(Config.Get("UploadDirectory"));
                MailEngine engine = new MailEngine();
                engine.SendGridKey = EmailConfig.Me.SendGridKey;
                engine.FileSys = filesys;
                string sepchar = Path.DirectorySeparatorChar.ToString();
                engine.LoadTemplates(Config.ContentRoot + sepchar + "Config" + sepchar, "EmailTemplates.txt");
                engine.Start(batches);
                Log.Me.Info("Email Thread started!");
            }
            catch (Exception ex)
            {
                Log.Me.Error("EXCEPTION - " + ex.Message);
            }   
        }
    }
}