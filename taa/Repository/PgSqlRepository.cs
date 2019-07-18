using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using taa.Model;

namespace taa.Repository {
    
    /// <summary>
    /// PostgreSQL
    /// </summary>
    public class PgSqlRepository : IRepository {
        public void BulkWrite(IEnumerable<RecordModel> list) {
            throw new NotImplementedException();
        }

        public RecordModel[] Get(Func<RecordModel, bool> filter) {
            throw new NotImplementedException();
        }
    }
    
    public class PgSqlDbContext: DbContext {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="options">DbContextのオプション</param>
        public PgSqlDbContext(DbContextOptions options) : base(options) {
        }
        

        /// <summary>
        /// Recordのセット
        /// </summary>
        public DbSet<RecordModel> Records { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            // 主キーはParameterとSweep
            modelBuilder.Entity<RecordModel>().HasKey(e => new {e.Parameter, e.Sweep, e.Key});
        }

        protected override void OnConfiguring(DbContextOptionsBuilder ob) {
            // Dbの設定
            ob.UseNpgsql(Config.Config.GetInstance().ConnectionsString);

            // 遅延ロードプロキシーを有効化する
            ob.UseLazyLoadingProxies();
        }

    }
}