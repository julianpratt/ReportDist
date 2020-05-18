/// Standard Interface for repositories. Includes: CRUD and query methods 
using System;
using System.Collections.Generic;

namespace ReportDist.Data 
{
    public interface IID
    {
       int Id { get; set; }
    }
}