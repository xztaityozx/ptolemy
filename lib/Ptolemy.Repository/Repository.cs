using System;
using System.Collections.Generic;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;

namespace Ptolemy.Repository {
    
    public class SqliteRepository :IDisposable {
        private readonly Context context;
        public SqliteRepository(string path) {
            context = new Context(path);
            context.Database.EnsureCreated();
        }

        public void BulkUpsert(IList<ResultEntity> list) {
            context.BulkInsertOrUpdate(list);
        }

        internal class Context : DbContext {
            private readonly string path;
            public Context(string path) => this.path = path;
            public DbSet<ResultEntity> Entities { get; set; }
            
            protected override void OnModelCreating(ModelBuilder modelBuilder) {
                base.OnModelCreating(modelBuilder);
                modelBuilder.Entity<ResultEntity>().HasKey(e => new {e.Seed, e.Sweep, e.Time, e.Signal});
                modelBuilder.Entity<ResultEntity>().HasIndex(e => new {e.Seed, e.Sweep, e.Time, e.Signal});
            }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
                base.OnConfiguring(optionsBuilder);
                optionsBuilder.UseSqlite($"Data Source={path};");
                optionsBuilder.UseLazyLoadingProxies();
            }
        }

        public void Dispose() {
            context?.Dispose();
        }
    }
}
