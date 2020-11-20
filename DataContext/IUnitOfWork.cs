using DataContext;
using DataContext.Abstractions;

namespace DataContext
{
    public interface IUnitOfWork
    {
        IProductRepository ProductRepo { get; }

        int SaveChanges();
    }
}
