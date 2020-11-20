using DataContext.Abstractions;
using DataContext.DataContext;
using DataContext.Implementation;
using Microsoft.EntityFrameworkCore;

namespace DataContext
{
    public class UnitOfWork : IUnitOfWork
    {
        private DbContext db;
        public UnitOfWork()
        {
            db = new EFDBContext();
        }      

        private IProductRepository _ProductRepo;
        public IProductRepository ProductRepo
        {
            get
            {
                if (_ProductRepo == null)
                    _ProductRepo = new ProductRepository(db);

                return _ProductRepo;
            }
        }

        public int SaveChanges()
        {
            return db.SaveChanges();
        }
    }
}
