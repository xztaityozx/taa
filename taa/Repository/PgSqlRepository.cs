using System;
using System.Collections.Generic;
using System.Linq;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using taa.Extension;
using taa.Model;

namespace taa.Repository {
    
    /// <summary>
    /// PostgreSQL
    /// </summary>
    public class PgSqlRepository : IRepository {
        public void BulkWrite(Parameter.Parameter parameter, IList<RecordModel> list) {
            parameter.DatabaseName().WL();
            
            using (var ctx = new PgSqlDbContext(parameter.DatabaseName())) {
                ctx.Database.EnsureCreated();


                using (var transaction = ctx.Database.BeginTransaction()) {
                    ctx.BulkInsertOrUpdate(list);
                    transaction.Commit();
                }
            }
        }

        public RecordModel[] Get(Parameter.Parameter parameter, Func<RecordModel, bool> filter) {
            using (var ctx = new PgSqlDbContext(parameter.DatabaseName())) {
                return ctx.Records.AsNoTracking().Where(item => filter(item)).ToArray();
            }
        }
    }
    
    public class PgSqlDbContext: DbContext {
        private readonly string dbName;

        public PgSqlDbContext(string name) :base() => dbName = name;

        /// <summary>
        /// Recordのセット
        /// </summary>
        public DbSet<RecordModel> Records { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder ob) {
            // Dbの設定
            ob.UseNpgsql(Config.Config.GetInstance().ConnectionsString+$";Database={dbName}");

            // 遅延ロードプロキシーを有効化する
            ob.UseLazyLoadingProxies();
        }

    }
}