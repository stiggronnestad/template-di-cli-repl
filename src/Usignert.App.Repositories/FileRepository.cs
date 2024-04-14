using Usignert.App.Data.Context;
using Usignert.App.Models;

namespace Usignert.App.Repositories
{
    public sealed class FileRepository<TEntity> : IRepository<TEntity>
        where TEntity : Model
    {
        private readonly FileSystemDbContext<TEntity> _fileSystemDbContext;

        public FileRepository(FileSystemDbContext<TEntity> fileSystemDbContext)
        {
            _fileSystemDbContext = fileSystemDbContext;
        }

        public IQueryable<TEntity> GetAll() => _fileSystemDbContext.ReadAll();

        public TEntity? GetById(string id)
        {
            if (_fileSystemDbContext.TryRead(id, out var entity))
            {
                return entity;
            }

            return default;
        }

        public bool Add(TEntity entity) => _fileSystemDbContext.TryWrite(entity);

        public bool Update(TEntity entity)
        {
            if (_fileSystemDbContext.TryRead(entity.Id, out _))
            {
                return _fileSystemDbContext.TryWrite(entity);
            }

            return false;
        }

        public bool Delete(string id) => _fileSystemDbContext.TryDelete(id);
    }
}
