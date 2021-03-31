using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
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

        public Recipient Identify(ClaimsIdentity identity)
        {
            Recipient r = null;

            if (Config.Env == "Development")
            {
                string id = Config.Get("DefaultUserId");
                if (id == null) 
                {
                    Log.Me.Fatal("In Development (so no authentication). DefaultUserId not in configuration.");
                    return null;
                }
                if (!id.IsInteger()) 
                {
                    Log.Me.Fatal("In Development (so no authentication). DefaultUserId is not an integer.");
                    return null;
                }
                r = _context.RecipientRepo.Read(id.ToInteger(0));
                if (r == null) 
                {
                    Log.Me.Fatal("In Development (so no authentication). DefaultUserId does not match a Recipient.");
                }
                return r;
            } 
            else if (identity == null)
            {
                Log.Me.Fatal("User must be authenticated.");
                return null;
            }
            
            string username  = identity.FindFirst(System.Security.Claims.ClaimTypes.Upn)?.Value;
            string email     = identity.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            string firstName = identity.FindFirst(System.Security.Claims.ClaimTypes.GivenName)?.Value; 
            string lastName  = identity.FindFirst(System.Security.Claims.ClaimTypes.Surname)?.Value;

            if (!username.HasValue()) username=email; // AAD users have UPN set, but Microsoft accounts don't, so use email instead. 
            if (!username.HasValue()) 
            {
                Log.Me.Error("User " + firstName + " " + lastName + " has authenticated, but has neither UPN nor Email set.");
                return null;
            }

            r = FindRecipient(username, null);
            if (r != null)
            {
                // User Identity found. Check it
                if (r.FirstName != firstName || r.LastName != lastName)
                {
                    string tail = ", but First or Last Name do not match (from AAD " + firstName + " " + lastName;
                    tail += ", found in database " + r.FirstName + " " + r.LastName + ")";
                    Log.Me.Warn("User " + username + " has authenticated, and is identified as Recipient " + r.Id.ToString() + tail);
                } 
            }
            else
            {
                // Identity not found. So add it.
                r = new Recipient();
                r.UserName  = username; 
                r.FirstName = firstName;
                r.LastName  = lastName;   
                r.Email     = email;
                r.Id = _context.RecipientRepo.Create(r);
            }

            return r;
        }

        public override int Find(string email)
        {
            Recipient r = FindRecipient(null, email);
            return (r == null) ? 0 : r.RecipientID;
        }

        public Recipient FindRecipient(string username, string email)
        {
            if (email != null)
            {
                IQueryable<Recipient> recips = _context.Recipients.Where(r => r.Email == email);
                if (recips.Count() > 0) return recips.FirstOrDefault();
                else return null;
            }
            else if (username != null)
            {
                IQueryable<Recipient> recips = _context.Recipients.Where(r => r.UserName == username);
                if (recips.Count() > 0) return recips.FirstOrDefault();
                else return null;
            }
            else return null;
        }

        public IQueryable<Recipient> List(string sort, string filter, string option = null )
        {
            IQueryable<Recipient> recips = _context.Recipients;

            if (!String.IsNullOrEmpty(filter))
            {
                recips = recips.Where(r => (r.FirstName.Contains(filter)    || r.LastName.Contains(filter) ||
                                            r.Email.Contains(filter)        ));
                /*
                recips = recips.Where(r => (r.FirstName.Contains(filter)    || r.LastName.Contains(filter) ||
                                            r.Email.Contains(filter)        || r.AddressLine1.Contains(filter) ||
                                            r.AddressLine2.Contains(filter) || r.AddressLine3.Contains(filter) ||
                                            r.AddressLine4.Contains(filter) || r.AddressLine5.Contains(filter)));
                */                            

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