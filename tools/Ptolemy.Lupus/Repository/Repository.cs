using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EFCore.BulkExtensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Ptolemy.Map;
using ShellProgressBar;

namespace Ptolemy.Lupus.Repository {
    public abstract class Repository<TEntity> where TEntity : class {
        public abstract void Use(string db);
        public abstract void BulkUpsert(IList<TEntity> list);
        public abstract Tuple<string, long>[] Count((long start, long end) sweep, (long start, long end) seed, Filter filter);
    }

    public class MssqlRepository : Repository<Record.Record> {
        private string name;

        public override void Use(string db) {
            name = db;
            using (var c = new Context(name)) {
                c.Database.EnsureCreated();
            }
        }

        public override void BulkUpsert(IList<Record.Record> list) {
            using (var context = new Context(name)) {
                context.Database.EnsureCreated();
                using (var tr = context.Database.BeginTransaction()) {
                    context.BulkInsertOrUpdate(list);
                    tr.Commit();
                }
            }
        }

        public void BulkUpsert(CancellationToken token, IList<Record.Record> list, IProgressBar bar) {
            Task.WaitAll(Task.Factory.StartNew(() => {
                using (var context = new Context(name)) {
                        context.BulkInsertOrUpdate(list,
                            config => config.TrackingEntities = false, d => {
                                for (var i = bar.CurrentTick; i < (int) (d * 100); i++) bar.Tick($"{d:F2}/1.0");
                            });
                    

                    bar.Message = "Write to database...";
                    bar.Tick();
                }
            }, token));
        }

        public void BulkInsert(CancellationToken token, IList<Record.Record> list, IProgressBar bar) {
            Task.WaitAll(Task.Factory.StartNew(() => {
                using (var context = new Context(name)) {
                    context.BulkInsertOrUpdate(list,
                        config => config.TrackingEntities = false, d => {
                            for (var i = bar.CurrentTick; i < (int) (d * 100); i++) bar.Tick($"{d:F2}/1.0");
                        });


                    bar.Message = "Write to database...";
                    bar.Tick();
                }
            }, token));
        }

        public void Distinct() {
            using (var context = new Context(name)) {
                context.BulkDelete(
                    context.Records
                        .GroupBy(r => new {r.Sweep, r.Key, r.Seed})
                        .Where(g => g.Count() != 1)
                        .SelectMany(g => g.Skip(1)).ToList());
            }
        }

        public IList<Record.Record> BulkRead(IList<Record.Record> targets) {
            using (var ctx = new Context(name)) {
                ctx.BulkRead(targets, config => config.UpdateByProperties = new List<string> {
                    nameof(Record.Record.Sweep), nameof(Record.Record.Seed)
                });
                return targets;
            }
        }

        public override Tuple<string, long>[] Count((long start, long end) sweep, (long start, long end) seed, Filter filter) {
            using (var context = new Context(name)) {
                var (sweepStart, sweepEnd) = sweep;
                var (seedStart, seedEnd) = seed;
                var whereQuery = (from r in context.Records
                    where sweepStart <= r.Sweep && r.Sweep <= sweepEnd && seedStart <= r.Seed && r.Seed <= seedEnd
                    group r by new{r.Sweep, r.Seed});


                return filter.Aggregate(
                    whereQuery.Select(g => g.ToMap(r => r.Key, r => r.Value)).ToList()
                );
            }
        }

        /// <summary>
        /// Context class for EntityFrameworkCore
        /// </summary>
        internal class Context : DbContext {
            private readonly string name;
            public Context(string name) => this.name = name;

            public DbSet<Record.Record> Records { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
                base.OnConfiguring(optionsBuilder);
                optionsBuilder
                    //.UseLoggerFactory(GetLoggerFactory())
                    .UseSqlServer(LupusConfig.Instance.ConnectionString + $";Database={name}");
                optionsBuilder.UseLazyLoadingProxies();
            }
            private ILoggerFactory GetLoggerFactory()
            {
                IServiceCollection serviceCollection = new ServiceCollection();
                serviceCollection.AddLogging(builder =>
                    builder.AddConsole()
                        .AddFilter(DbLoggerCategory.Database.Command.Name,
                            LogLevel.Information));
                return serviceCollection.BuildServiceProvider()
                    .GetService<ILoggerFactory>();
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder) {
                base.OnModelCreating(modelBuilder);
                modelBuilder.Entity<Record.Record>().HasKey(e => new { e.Sweep, e.Key, e.Seed });
                modelBuilder.Entity<Record.Record>().HasIndex(e => new { e.Sweep, e.Seed });
            }
        }
    }
}