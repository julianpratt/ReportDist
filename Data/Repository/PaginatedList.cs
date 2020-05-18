using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ReportDist.Data
{
    public class PaginatedList<T> : List<T>
    {
        public int PageIndex  { get; private set; }
        public int TotalPages { get; private set; }
        public int TotalRows  { get; private set; }

        public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
        {
            TotalRows = count;
            PageIndex = pageIndex;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);

            this.AddRange(items);
        }

        public bool HasPreviousPage
        {
            get
            {
                return (PageIndex > 1);
            }
        }

        public bool HasNextPage
        {
            get
            {
                return (PageIndex < TotalPages);
            }
        }

        public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize)
        {
            int count = await source.CountAsync();
            List<T> items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PaginatedList<T>(items, count, pageIndex, pageSize);
        }

        public static PaginatedList<T> Create(IQueryable<T> source, int pageIndex, int pageSize)
        {
            int count = source.Count();
            List<T> items = source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            return new PaginatedList<T>(items, count, pageIndex, pageSize);
        }

        public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, DataTableHelper helper)
        {
            int pageSize = helper.PageSize;
            int pageIndex = ( helper.Skip / pageSize ) + 1;
            int count = await source.CountAsync();
            List<T> items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PaginatedList<T>(items, count, pageIndex, pageSize);
        }

        public static PaginatedList<T> Create(IQueryable<T> source, DataTableHelper helper)
        {
            int pageSize = helper.PageSize;
            int pageIndex = ( helper.Skip / pageSize ) + 1;
            int count = source.Count();
            List<T> items = source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            return new PaginatedList<T>(items, count, pageIndex, pageSize);
        }

    }
}