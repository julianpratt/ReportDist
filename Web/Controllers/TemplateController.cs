using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ReportDist.Data;
using Mistware.Utils;
using Mistware.Postman;

namespace ReportDist.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TemplateController : ControllerBase
    {
        protected readonly DataContext _context;
    
        public TemplateController(DataContext context)
        {
            _context = context;
        }

        // GET: /Template?code=code
        [HttpGet]
        public ContentResult Get(string code)
        {  
            string sepchar = Path.DirectorySeparatorChar.ToString();
            MailMerge.Me.LoadTemplates(Config.ContentRoot + sepchar + "Config" + sepchar, "EmailTemplates.txt");

            string subject = MailMerge.Me.GetSubject(code, new Dictionary<string, string>());
            string body    = MailMerge.Me.GetBody(code, new Dictionary<string, string>());

            StringBuilder sOutput = new StringBuilder();
            if (subject.HasValue() && body.HasValue())
            {
                sOutput.Append("<html><body><pre>");
                sOutput.Append(subject);
                sOutput.Append(body);
                sOutput.Append("</pre></body></html>");
            }
            else sOutput.Append("No Preview available for the current Electronic template");

            return new ContentResult {
                ContentType = "text/html",
                StatusCode = (int) HttpStatusCode.OK,
                Content = sOutput.ToString()
                };
        }
    }
}
