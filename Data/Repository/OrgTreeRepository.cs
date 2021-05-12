using System.Collections.Generic;
using System.Linq;

namespace ReportDist.Data
{
	/// <summary>
    /// Repository to load up Organisation Tree data from the database
    /// </summary>
    public class OrgTreeRepository     
    {
        private readonly DataContext _context;

    	/// <summary>
        /// Initializes a new instance of the OrgTreeRepository class
        /// <param name="context">The data context.</param>
        /// </summary>
        public OrgTreeRepository(DataContext context)
        {
            _context = context;
        }

        public OrgTree Load()
        {
            OrgTree ot = new OrgTree();

            List<OrgTreeBase> divs = _context.Set<OrgTreeBase>().Where(d => d.OrgTypeCode == "Div").OrderBy(d => d.Name).ToList();
            foreach (OrgTreeBase div in divs)
            {
                ot.Divisions.Add(new Division(div.Id, div.Code, div.Name + " (" + div.Code + ")"));
            }

            List<OrgTreeBase> deps = _context.Set<OrgTreeBase>().Where(d => d.OrgTypeCode == "Dep").OrderBy(d => d.Name).ToList();
            foreach (OrgTreeBase dep in deps)
            {
                if (ot.Divisions.Exists(d => d.Id == dep.ParentID))
                {
                    Division div = ot.Divisions.Find(d => d.Id == dep.ParentID);
                    div.Departments.Add(new Department(dep.Id, dep.Code, dep.Name + " (" + dep.Code + ")"));
                }
            }
            
            List<OrgTreeBase> teams = _context.Set<OrgTreeBase>().Where(d => d.OrgTypeCode == "Tem").OrderBy(d => d.Name).ToList();
            foreach (OrgTreeBase team in teams)
            {
                foreach (Division div in ot.Divisions)
                {
                    if (div.Departments.Exists(d => d.Id == team.ParentID))
                    {
                        Department dep = div.Departments.Find(d => d.Id == team.ParentID);
                        dep.Teams.Add(new Team(team.Id, team.Code, team.Name + " (" + team.Code + ")"));
                    }
                }
            }

            return ot;
        }
    }
}