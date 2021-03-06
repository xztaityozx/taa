﻿using System;
using System.Collections.Generic;
using System.Linq;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using taa.Config;
using taa.Extension;
using taa.Model;

namespace taa.Repository {
    public class MssqlRepository : Repository<Record> {
        private string name;
        public override void Use(string db) {
            name = db;
        }

        public override void BulkUpsert(IList<Record> list) {
            using (var context = new Context(name)) {
                context.Database.EnsureCreated();
                using (var tr = context.Database.BeginTransaction()) {
                    context.BulkInsertOrUpdate(list);
                    tr.Commit();
                }
            }
        }

        public override Tuple<string, long>[] Count(Func<Record, bool> whereFunc, Filter filter) {
            using (var context = new Context(name))
                return filter.Aggregate(
                    context
                        .Records
                        .Where(whereFunc)
                        .GroupBy(r => new {r.Sweep, r.Seed})
                        .Select(g => g.ToMap(r => r.Key, r => r.Value)).ToList()
                );
        }

        /// <summary>
        /// Context class for EntityFrameworkCore
        /// </summary>
        internal class Context : DbContext {
            private readonly string name;
            public Context(string name) => this.name = name;

            public DbSet<Record> Records { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
                base.OnConfiguring(optionsBuilder);
                optionsBuilder.UseSqlServer(Config.Config.GetInstance().ConnectionsString+$";Database={name}");
                optionsBuilder.UseLazyLoadingProxies();
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder) {
                base.OnModelCreating(modelBuilder);
                modelBuilder.Entity<Record>().HasKey(e => new {e.Sweep, e.Key, e.Seed});
            }
        }
    }
}
