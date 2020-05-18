using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Mistware.Utils;

namespace ReportDist.Data
{
	/// <summary>
    /// Repository for CRUD operations on Recipient records in a database
    /// </summary>
    public class RecipientRepository : Repository<Recipient>      
    {
    	/// <summary>
        /// Initializes a new instance of the RecipientRepository class
        /// <param name="context">The data context.</param>
        /// </summary>
        public RecipientRepository(DataContext context) : base(context) {}

        public override IEnumerable<Recipient> Search(Recipient recipient)
        {
            string firstName = recipient.FirstName;
            string lastName  = recipient.LastName;

            return _context.Set<Recipient>().ToList();
        }

        public override int Find(string email)
        {
            IQueryable<Recipient> recips = _context.Recipients.Where(r => r.Email == email);
            if (recips.Count() > 0) return recips.Select(e => e.RecipientID).FirstOrDefault();
            else return 0;
        }

        public IQueryable<Recipient> List(string sort, string filter, string option = null )
        {
            IQueryable<Recipient> recips = _context.Recipients;

            if (!String.IsNullOrEmpty(filter))
            {
                recips = recips.Where(r => (r.FirstName.Contains(filter)    || r.LastName.Contains(filter) ||
                                            r.Email.Contains(filter)        || r.AddressLine1.Contains(filter) ||
                                            r.AddressLine2.Contains(filter) || r.AddressLine3.Contains(filter) ||
                                            r.AddressLine4.Contains(filter) || r.AddressLine5.Contains(filter)));
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
                    Expression<Func<Recipient,string>>   fs = null; 

                    switch (sortColumn.ToLower())
                    {
                        case "firstname":
                            fs = (r => r.FirstName);
                            break;
                        case "lastname":
                            fs = (r => r.LastName);
                            break;   
                        case "email":
                            fs = (r => r.Email);
                            break;
                        case "jobtitle":
                            fs = (r => r.JobTitle);
                            break;
                        case "address":
                            fs = (r => r.AddressLine1);
                            break;
                    }
                    if (fs != null)
                    {
                        if (sortOrder == "desc") recips = recips.OrderByDescending(fs);
                        else                     recips = recips.OrderBy(fs);
                    }
                }
            }

            return recips;

        }

    }
}