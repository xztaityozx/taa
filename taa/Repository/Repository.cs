using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using taa.Config;
using taa.Model;

namespace taa.Repository {

    public abstract class Repository<TEntity>  where TEntity : class {
        public abstract void Use(string db);
        public abstract void BulkUpsert(IList<TEntity> list);
        public abstract Tuple<string, long>[] Count(Func<TEntity, bool> whereFunc, Filter filter);
   }
}