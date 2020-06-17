using System;
using System.Collections.Generic;
using System.Linq;
using Mistware.Utils;

namespace ReportDist.Data
{
	/// <summary>
    /// Repository to load up standing data from the database
    /// </summary>
    public class StandingDataRepository     
    {
        private readonly DataContext _context;

    	/// <summary>
        /// Initializes a new instance of the StandingDataRepository class
        /// <param name="context">The data context.</param>
        /// </summary>
        public StandingDataRepository(DataContext context)
        {
            _context = context;
        }

        public StandingData Load()
        {
            StandingData sd = new StandingData();
            sd.AccessCodes = _context.Set<AccessCode>().Where(a => !a.Disabled).OrderBy(a => a.Title).ToList();
            sd.Companies = _context.Set<Company>().OrderByDescending(c => c.StartYear).ToList();
            sd.ReportTypes = _context.Set<ReportType>().ToList();
            sd.SecurityLevels = _context.Set<SecurityLevel>().OrderBy(s => s.Code).ToList();
            List<string> years = new List<string>();
            List<string> shortYears = new List<string>();
            for (int iYr = DateTime.Today.Year; iYr > (DateTime.Today.Year - 6); --iYr)
            {
                years.Add(iYr.ToString());
                shortYears.Add(iYr.ToString().Right(2));
            }
            sd.Years = years;
            sd.ShortYears = shortYears;

            return sd;
        }
    }
}