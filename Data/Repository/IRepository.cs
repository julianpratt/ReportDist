using System;
using System.Collections.Generic;
using System.Linq;

namespace ReportDist.Data
{
    /// Standard Interface for repositories - CRUD, Any() and All().  
    public interface IRepository<TEntity> where TEntity : class, IID
    {
        /// True if there are any records
        bool Any();

        /// Creates the specified entity in the database
	    int Create(TEntity entity); 

        /// Reads a single record of specified identity
        TEntity Read(int? id);

        /// Update a record of specified identity
        void Update(TEntity entity);

        /// Deletes the specified entitiy
        void Delete(int? id);

        /// All records
        IQueryable<TEntity> All();
    }
}