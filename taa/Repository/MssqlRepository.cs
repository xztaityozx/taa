using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using taa.Config;
using taa.Extension;
using taa.Model;

namespace taa.Repository {
    public class MssqlRepository : Repository<RecordModel> {
        private string name;
        public override void Use(string db) {
            name = db;
        }

        public override void BulkUpsert(IList<RecordModel> list) {
            using (var context = new Context(name)) {
                context.Database.EnsureCreated();
                using (var tr = context.Database.BeginTransaction()) {
                    context.BulkInsertOrUpdate(list);
                    tr.Commit();
                }
            }
        }

        public override Tuple<string, long>[] Count(Func<RecordModel, bool> whereFunc, Filter filter) {
            using (var context = new Context(name))
                return filter.Aggregate(
                    context
                        .Records
                        .Where(whereFunc)
                        .GroupBy(r => new {r.Sweep, r.Seed})
                        .Select(g => g.ToMap(r => r.Key, r => r.Value)).ToList()
                );
        }

        internal class Context : DbContext {
            private readonly string name;
            public Context(string name) => this.name = name;

            public DbSet<RecordModel> Records;

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
                base.OnConfiguring(optionsBuilder);
                optionsBuilder.UseSqlServer(Config.Config.GetInstance().ConnectionsString+$";Database={name}");
                optionsBuilder.UseLazyLoadingProxies();
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder) {
                base.OnModelCreating(modelBuilder);
                modelBuilder.Entity<RecordModel>().HasKey(e => new {e.Sweep, e.Key, e.Seed});
            }
        }
    }
}
