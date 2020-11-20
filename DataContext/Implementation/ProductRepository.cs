using DataContext.Abstractions;
using DataContext.DataContext;
using DataContext.Models;
using Microsoft.EntityFrameworkCore;

namespace DataContext.Implementation
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private EFDBContext context
        {
            get
            {
                return db as EFDBContext;
            }
        }

        public ProductRepository(DbContext db)
        {
            this.db = db;
        }
    }
}
