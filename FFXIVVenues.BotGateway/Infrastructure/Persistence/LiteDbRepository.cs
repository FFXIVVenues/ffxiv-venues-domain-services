using System;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Infrastructure.Persistence.Abstraction;
using LiteDB;

namespace FFXIVVenues.Veni.Infrastructure.Persistence
{

    // This repository should remain singleton. While it will work fine none-singleton, LiteDb opens,
    // locks, unlocks and closes the target database file as the LiteDatabase object is instantiated
    // and disposed, all of which are very expensive. Page caching is also kept at LiteDatabase level,
    // so is lost on disposing it. Additionally, LiteDb is thread-safe. So, it's considerably more 
    // performant to keep a continual instance of LiteDatabase. 
    // For more information, see here: https://github.com/mbdavid/LiteDB/wiki/Concurrency
    public class LiteDbRepository : IRepository, IDisposable
    {

        private readonly LiteDatabase _repository;

        public LiteDbRepository(string strConnectionString)
        {
            var connectionString = new ConnectionString(strConnectionString);
            if (!Path.IsPathRooted(connectionString.Filename))
            {
                var assembliesPath = Path.GetDirectoryName(new Uri(System.Reflection.Assembly.GetExecutingAssembly().Location!).LocalPath);
                if (assembliesPath != null)
                {
                    connectionString.Filename = Path.Combine(assembliesPath, connectionString.Filename);
                }
            }

            var directory = Path.GetDirectoryName(connectionString.Filename);
            if (directory != null)
            {
                _ = Directory.CreateDirectory(directory);
            }

            _repository = new LiteDatabase(connectionString);
        }

        public Task UpsertAsync<T>(T entity) where T : class, IEntity =>
            Task.FromResult(_repository.GetCollection<T>().Upsert(entity));

        public Task DeleteAsync<T>(T entity) where T : class, IEntity =>
            Task.FromResult(_repository.GetCollection<T>().Delete(entity.id));

        public Task DeleteAsync<T>(string id) where T : class, IEntity =>
            Task.FromResult(_repository.GetCollection<T>().Delete(id));

        public Task<IQueryable<T>> GetWhereAsync<T>(Expression<Func<T, bool>> predicate) where T : class, IEntity =>
            Task.FromResult(_repository.GetCollection<T>().Find(predicate).AsQueryable());

        public Task<IQueryable<T>> QueryAsync<T>() where T : class, IEntity =>
            Task.FromResult(_repository.GetCollection<T>().FindAll().AsQueryable());

        public Task<T> GetByIdAsync<T>(string id) where T : class, IEntity =>
            Task.FromResult(_repository.GetCollection<T>().FindById(id));

        public Task<bool> ExistsAsync<T>(string id) where T : class, IEntity =>
            Task.FromResult(_repository.GetCollection<T>().FindById(id) != null);

        public void Dispose() =>
            Dispose(true);

        public void Dispose(bool disposing)
        {
            if (disposing)
            {
                _repository.Dispose();
            }
            GC.SuppressFinalize(this);
        }

        ~LiteDbRepository() =>
            Dispose(false);

    }
}
