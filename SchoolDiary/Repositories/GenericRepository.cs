using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Linq.Expressions;
using System.Web;
using SchoolDiary.DAL;

namespace SchoolDiary.Repositories
{
    public class GenericRepository<T> : IRepository<T> where T : class
    {
        private SchoolContext db = null;

        IDbSet<T> _objectSet;
        public GenericRepository(SchoolContext _db)
        {
            db = _db;
            _objectSet = db.Set<T>();
        }
        public void Add(T entity)
        {
            _objectSet.Add(entity);
        }
        public void Delete(T entity)
        {
            _objectSet.Remove(entity);
        }
        public T GetDetail(Func<T, bool> predicate)
        {
            return _objectSet.First(predicate);
        }

        public IEnumerable<T> GetOverview(Func<T, bool> predicate = null)
        {
            if (predicate != null)
                return _objectSet.Where(predicate);
            return _objectSet.AsEnumerable();
        }
    }
}