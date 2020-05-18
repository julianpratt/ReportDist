using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using ReportDist.Data;

namespace ReportDist.Models
{
    public class CirculationViewModel : Circulation
    {
        public CirculationViewModel(Circulation c)
        {
            if (c != null)
            {
                this.Id          = c.Id;
                this.Name        = c.Name;
                this.Email       = c.Email;
                this.Address     = c.Address;
                this.ToCcBccDB   = c.ToCcBccDB;
                this.Delivery    = c.Delivery;
                this.PendingId   = c.PendingId;
                this.RecipientID = c.RecipientID;
                this.State       = c.State;
                

                this.ElecDeliveryTypes   = new List<SelectListItem>();
                this.ElecDeliveryTypes.Add(new SelectListItem("None",                       ""));
                this.ElecDeliveryTypes.Add(new SelectListItem("Summary Only",               "ES"));
                this.ElecDeliveryTypes.Add(new SelectListItem("Full Report",                "EF"));
                this.ElecDeliveryTypes.Add(new SelectListItem("Summary with CD to follow",  "EC"));

                this.PaperDeliveryTypes = new List<SelectListItem>(); 
                this.PaperDeliveryTypes.Add(new SelectListItem("None",                       ""));
                this.PaperDeliveryTypes.Add(new SelectListItem("Summary Only",               "PS"));
                this.PaperDeliveryTypes.Add(new SelectListItem("Full Report",                "PF"));

            }
            
        }
        public List<SelectListItem> ElecDeliveryTypes  { get; set; } 

        public List<SelectListItem> PaperDeliveryTypes { get; set; } 
 
    }
}