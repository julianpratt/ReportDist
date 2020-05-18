using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ReportDist.Data;

namespace ReportDist.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrgTreeController : ControllerBase
    {
        protected readonly DataContext _context;
        private readonly ILogger<OrgTreeController> _logger;

        public OrgTreeController(DataContext context, ILogger<OrgTreeController> logger)
        {
            _context = context;
            _logger  = logger;
        }

        [HttpGet]
        public OrgTree Get()
        {
            return _context.OrgTreeRepo.Load();
        }
    }
}