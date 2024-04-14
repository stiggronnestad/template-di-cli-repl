using Usignert.App.Models;

namespace Usignert.App.Repositories
{
    public interface IRepository<TEntity>
        where TEntity : Model
    {
        IQueryable<TEntity> GetAll();
        TEntity? GetById(string id);
        bool Add(TEntity entity);
        bool Update(TEntity entity);
        bool Delete(string id);
    }
}
