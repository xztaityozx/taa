using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using taa.Config;
using taa.Model;

namespace taa.Repository {
    public class MssqlRepository : Repository<RecordModel> {

        public MssqlRepository(string db, Action<ModelBuilder> builderAction) : base(builderAction, ob => {
            ob.UseSqlServer(Config.Config.GetInstance().ConnectionsString + $";Database={db}");
            ob.UseLazyLoadingProxies();
        }) {
        }

        public override void Use(string db, Action<ModelBuilder> builderAction) {
            throw new NotImplementedException();
        }

        public override void BulkUpsert(IList<RecordModel> list) {
            throw new NotImplementedException();

            //using (var tr = context.Database.BeginTransaction()) {
            //    context.BulkInsertOrUpdate(list);
            //    tr.Commit();
            //}
        }

        public override Tuple<string, long>[] Count(Func<RecordModel, bool> whereFunc, Filter filter) {
            throw new NotImplementedException();
            //return filter.Aggregate(
            //    context
            //        .Records
            //        .Where(whereFunc)
            //        .GroupBy(r => new {r.Sweep, r.Seed})
            //        .Select(g => g.ToMap(r => r.Key, r => r.Value))
            //);
        }
    }
}
