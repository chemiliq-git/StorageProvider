using System.Collections.Generic;

namespace DataContext.Abstractions
{
    public interface IRepository<TEntity> where TEntity : class
    {
        void Add(TEntity model);

        IEnumerable<TEntity> GetAll();

        TEntity GetById(object Id);

        void Modify(TEntity model);

        void Delete(TEntity model);

        void DeleteById(object Id);
    }
}
