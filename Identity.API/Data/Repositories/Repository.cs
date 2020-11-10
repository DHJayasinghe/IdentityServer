using CommonUtil;
using System.Collections.Generic;

namespace Identity.API.Data.Repositories
{
    /// <summary>
    /// A generic repository
    /// </summary>
    /// <typeparam name="T">Entity class</typeparam>
    public class Repository<T> : IRepository<T> where T : class
    {
        /// <summary>
        /// shared UOW instance accessible across inherited repositories
        /// </summary>
        protected readonly UnitOfWork _unitOfWork;

        protected Repository(UnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        public Maybe<T> GetById(object id) => _unitOfWork.GetById<T>(id);

        public void Insert(T obj) => _unitOfWork.Insert<T>(obj);

        public void Insert(IEnumerable<T> objs) => _unitOfWork.Insert<T>(objs);

        public void Update(T obj) => _unitOfWork.Update<T>(obj);

        public void Delete(object id) => _unitOfWork.Delete<T>(id);
    }

    public interface IRepository<T> where T : class
    {
        Maybe<T> GetById(object id);

        void Insert(T obj);

        void Insert(IEnumerable<T> objs);

        void Update(T obj);

        void Delete(object id);
    }
}
