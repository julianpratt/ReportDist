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
    public class StandingDataController : ControllerBase
    {
        protected readonly DataContext _context;
        private readonly ILogger<StandingDataController> _logger;

        public StandingDataController(DataContext context, ILogger<StandingDataController> logger)
        {
            _context = context;
            _logger  = logger;
        }

        [HttpGet]
        public StandingData Get()
        {
            return _context.StandingDataRepo.Load();
        }
    }
}
