using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using taa.Config;
using taa.Model;

namespace taa.Repository {


    public abstract class Repository<TEntity>:IDisposable where TEntity:class {
        protected Context<TEntity> Context;
        public abstract void Use(string db, Action<ModelBuilder> builderAction);
        public abstract void BulkUpsert(IList<TEntity> list);
        public abstract Tuple<string, long>[] Count(Func<TEntity, bool> whereFunc,Filter filter);

        public void Dispose() {
            Context?.Dispose();
        }
    }

    public class Context<TEntity> : DbContext where TEntity :class {
        private readonly Action<ModelBuilder> builderAction;
        private readonly Action<DbContextOptionsBuilder> opBuilderAction;

        public Context(Action<ModelBuilder> builder, Action<DbContextOptionsBuilder> op) {
            builderAction = builder;
            opBuilderAction = op;
        }

        public DbSet<TEntity> Records { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            base.OnConfiguring(optionsBuilder);
            opBuilderAction(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            base.OnModelCreating(modelBuilder);
            builderAction(modelBuilder);
        }
    }
}