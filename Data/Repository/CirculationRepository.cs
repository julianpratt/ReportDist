using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Mistware.Utils;

namespace ReportDist.Data
{
	/// <summary>
    /// Repository for CRUD operations on Circulation records in a database
    /// </summary>
    public class CirculationRepository : Repository<Circulation>      
    {
    	/// <summary>
        /// Initializes a new instance of the CirculationRepository class
        /// <param name="context">The data context.</param>
        /// </summary>
        public CirculationRepository(DataContext context) : base(context) {}

        private int? CheckUser(Circulation c)
        {
            int? recipientId = c.RecipientID;

            if (recipientId == null || recipientId == 0) recipientId = _context.RecipientRepo.Find(c.Email);
            /* if (recipientId != 0)
            {
                Recipient r = new Recipient();
                r.Name    = c.Name;
                r.Email   = c.Email;
                r.Address = c.Address;
                recipientId = _context.RecipientRepo.Create(r);
            }
            else 
            { 
                Recipient r = _context.RecipientRepo.Read(recipientId);
                r.Name    = c.Name;
                r.Email   = c.Email;
                r.Address = c.Address;
                _context.RecipientRepo.Update(r);
            } */
            return recipientId;
        } 

         public new int Create(Circulation circulation)
         {
            circulation.RecipientID = CheckUser(circulation);
            return base.Create(circulation);
         } 

         public new void Update(Circulation circulation)
         {
            circulation.RecipientID = CheckUser(circulation);
            base.Update(circulation);
         }

        public IQueryable<Circulation> List(int pendingId)
        {
            return _context.Circulations.Where(c => c.PendingId==pendingId).OrderBy(c => c.ToCcBccDB);
        }

        public void SetState(int pendingId, int state)
        {
            IQueryable<Circulation> list = _context.Circulations.Where(c => c.PendingId==pendingId);
            foreach (Circulation circ in list.ToList())
            {
                circ.State = state;
                _context.Circulations.Update(circ);
            }
        }

        public string RecipientMessage(int pendingId)
        {
            int addressees = List(pendingId).Where(c => c.ToCcBcc == Circulation.eToCcBcc.TO).Count();
            int recipients = List(pendingId).Count();
            if      (recipients == 0) return "No recipients specified";
            else if (addressees == 0) return "No Addressee specified for report.";
            else if (recipients == 1) return "1 recipient for this report.";
            else                      return recipients.ToString() + " recipients for this report.";
        }
    }
}