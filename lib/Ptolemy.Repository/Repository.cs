using System;
using System.Collections.Generic;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;

namespace Ptolemy.Repository {
    
    /// <summary>
    /// Sqlite3を扱うやつ
    /// </summary>
    public class SqliteRepository :IDisposable {
        private readonly Context context;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path">/path/to/sqlite3.db</param>
        public SqliteRepository(string path) {
            context = new Context(path);
            context.Database.EnsureCreated();
        }

        /// <summary>
        /// upsert records to database
        /// </summary>
        /// <param name="list">list of records</param>
        public void BulkUpsert(IList<ResultEntity> list) {
            context.BulkInsertOrUpdate(list);
        }

        /// <summary>
        /// DbContext(EFCore)
        /// </summary>
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

                //遅延LoadをOn
                optionsBuilder.UseLazyLoadingProxies();
            }
        }

        public void Dispose() {
            context?.Dispose();
        }
    }
}
