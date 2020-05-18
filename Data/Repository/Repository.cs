using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace ReportDist.Data
{
       public class Repository<TEntity> : IRepository<TEntity> where TEntity : class, IID
    {
        protected readonly DataContext _context;

        public Repository(DataContext context)
        {
            _context = context;
        }

        public bool Any()
        {
            return _context.Set<TEntity>().Count() > 0;
        }
        public IQueryable<TEntity> All()
        {
            return _context.Set<TEntity>();
        }

        public virtual IEnumerable<TEntity> ListById(int id)
        {
            throw new Exception("Not implemented");
        }

        public virtual IEnumerable<TEntity> ListByState(int id)
        {
            throw new Exception("Not implemented");
        }

        public virtual IEnumerable<TEntity> Search(TEntity entity) 
        {
            throw new Exception("Not implemented");
        }

        public virtual int Find(string s) 
        {
            throw new Exception("Not implemented");
        }

        public int Create(TEntity entity)
        {
            int id;

            try
            {  
                //Determine the next ID
                DbSet<TEntity> set = _context.Set<TEntity>();
                //if (set.Any()) id = (set.Select(e => e.Id).Max()) + 1;
                //else           id = 1;
                //entity.Id = id;
                entity.Id = 0; // Defensive against calling code accidentally setting an id
                set.Add(entity);
                _context.SaveChanges();
                DetachLocal(entity.Id);
                id = entity.Id;
            }
            catch (Exception ex)
            {
                throw new Exception("Exception in Repository.Create. Entity:" + typeof(TEntity).ToString() + ", id:" + entity.Id.ToString() + ", exception:" + ex.ToString());
            }
            return id;
        }

        public TEntity Read(int? id)
        {
            if (id == null) return null;

            DbSet<TEntity> set = _context.Set<TEntity>();
            return set.FirstOrDefault(e => e.Id == id);
        }

        public void Update(TEntity entity)
        {
            if (entity == null) throw new Exception("Cannot update " + typeof(TEntity).ToString() + " with null entity!");
            if (Read(entity.Id) == null)
                throw new Exception("Exception in Repository.Update. Entity:" + typeof(TEntity).ToString() + ", id:" + entity.Id.ToString() + " not found");
         
            try
            {
                DetachLocal(entity.Id);
                DbSet<TEntity> set = _context.Set<TEntity>();
                _context.Entry(entity).State = EntityState.Modified;
                set.Update(entity);
                _context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (Read(entity.Id) == null)
                {
                    throw new Exception("Exception in Repository.Update. Entity:" + typeof(TEntity).ToString() + ", id:" + entity.Id.ToString() + " not found");
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Exception in Repository.Update. Entity:" + typeof(TEntity).ToString() + ", id:" + entity.Id.ToString() + ", exception:" + ex.ToString());
            }
        }

        public void Delete(int? id)
        {
            if (id == null) throw new Exception("Cannot delete " + typeof(TEntity).ToString() + " with null id!");
  
            TEntity entity = Read(id);
            if (entity == null) throw new Exception(typeof(TEntity).ToString() + " " + id.ToString() + "Not found");
            else
            {
                try
                {
                    DbSet<TEntity> set = _context.Set<TEntity>();
                    set.Remove(entity);
                    _context.SaveChanges();
                }
                catch (Exception ex)
                {
                    throw new Exception("Exception in Repository.Delete. Entity:" + typeof(TEntity).ToString() + ", id:" + id.ToString() + ", exception:" + ex.ToString());
                }
            }
        }

        private void DetachLocal(int? id)
        {
            DbSet<TEntity> set = _context.Set<TEntity>();
            var local = set.Local.FirstOrDefault(e => e.Id.Equals(id));
            if (local != null)
            {
                _context.Entry(local).State = EntityState.Detached;
            }
        }
    }
}
