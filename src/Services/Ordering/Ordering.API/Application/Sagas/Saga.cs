using Microsoft.EntityFrameworkCore;
using Ordering.Domain.SagaData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ordering.API.Application.Sagas
{
    public abstract class Saga<TEntity> where TEntity : class, ISagaEntity
    {
        public Saga()
        {
        }

        protected TEntity FindSagaById(int id, DbContext context)
        {
            return context.Set<TEntity>().Where(x => x.CorrelationId == id).SingleOrDefault();
        }

        protected bool ExistSaga(int sagaId, DbContext context)
        {
            var saga = FindSagaById(sagaId, context);
            return saga != null;
        }

        protected void MarkSagaAsCompleted(TEntity item)
        {
            item.Completed = true;
        }

        protected void MarkAsCancelled(TEntity item)
        {
            item.Cancelled = true;
        }

        protected void AddSaga(TEntity item, DbContext context)
        {
            context.Add(item);
        }

        protected void UpdateSaga(TEntity item, DbContext context)
        {
            context.Update(item);
        }

        protected async Task<bool> SaveChangesAsync(DbContext context)
        {
            var result = await context.SaveChangesAsync();
            return result > 0;
        }
    }
}
