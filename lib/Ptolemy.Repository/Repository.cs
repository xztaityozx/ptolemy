using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Castle.Core.Logging;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using Ptolemy.Map;
using ILoggerFactory = Microsoft.Extensions.Logging.ILoggerFactory;

namespace Ptolemy.Repository {
    public class ReadOnlySqliteRepository {
        private readonly string path;
        public ReadOnlySqliteRepository(string path) {
            this.path = path;
        }

        private Context Connect() {
            var rt = new Context(path);
            rt.Database.EnsureCreated();

            return rt;
        }

        /// <summary>
        /// Sweepの区間についてクエリを分割して数え上げをします
        /// </summary>
        /// <param name="token"></param>
        /// <param name="signals"></param>
        /// <param name="delegates"></param>
        /// <param name="seed"></param>
        /// <param name="sweepSectionList"></param>
        /// <param name="keyGenerator"></param>
        /// <returns></returns>
        public long[] Aggregate(
            CancellationToken token,
            IReadOnlyList<string> signals,
            IReadOnlyList<Func<Map<string, decimal>, bool>> delegates,
            long seed,
            IReadOnlyList<(long start,long end)> sweepSectionList,
            Func<string, decimal, string> keyGenerator
        ) {
            var box = Enumerable.Range(0, delegates.Count).Select(_ => new ConcurrentBag<long>()).ToList();

            sweepSectionList.AsParallel()
                .WithCancellation(token)
                .ForAll(section => {
                    if(token.IsCancellationRequested) return;
                    var (start, end) = section;

                    using var context = Connect();
                    var target = context
                        .Entities
                            // sweepの範囲で絞る
                        .Where(e=> start <= e.Sweep && e.Sweep <= end)
                            // seedで絞る
                        .Where(e => e.Seed == seed)
                            // signalがリスト内にあるものだけ。 SQLなら IN とか
                        .Where(e=> signals.Contains(e.Signal))
                        .GroupBy(e => e.Sweep)
                            // GroupをMapにする
                        .Select(g => g.ToMap(k => keyGenerator(k.Signal, k.Time), v => v.Value)).ToList();

                    for (var i = 0; i < delegates.Count; i++) {
                        box[i].Add(target.Count(delegates[i]));
                    }
                });

            return box.Select(bag => bag.Sum()).ToArray();
        }

        /// <summary>
        /// seedについてクエリを分割して数え上げをします
        /// </summary>
        /// <param name="token"></param>
        /// <param name="signals"></param>
        /// <param name="delegates"></param>
        /// <param name="seeds"></param>
        /// <param name="sweepSize"></param>
        /// <param name="sweepStart"></param>
        /// <param name="keyGenerator"></param>
        /// <returns></returns>
        public long[] Aggregate(
            CancellationToken token,
            IReadOnlyList<string> signals,
            IReadOnlyList<Func<Map<string, decimal>, bool>> delegates,
            IReadOnlyList<long> seeds,
            long sweepSize,
            long sweepStart,
            Func<string, decimal, string> keyGenerator
            ) {

            var box = Enumerable.Range(0, delegates.Count).Select(_ => new ConcurrentBag<long>()).ToList();
            seeds.AsParallel()
                .WithCancellation(token)
                .ForAll(seed => {
                    if (token.IsCancellationRequested) return;

                    using var context = Connect();
                    var target = context.Entities
                        .Where(e => sweepStart <= e.Sweep && e.Sweep <= sweepStart + sweepSize - 1)
                        .Where(e => e.Seed == seed)
                        .Where(e => signals.Contains(e.Signal))
                        .GroupBy(e => e.Sweep)
                        .Select(g => g.ToMap(k => keyGenerator(k.Signal, k.Time), v => v.Value)).ToList();

                    for (var i = 0; i < delegates.Count; i++) {
                        box[i].Add(target.Count(delegates[i]));
                    }
                });


            return box.Select(bag => bag.Sum()).ToArray();
        }


        private static IEnumerable<long> Range(long start, long end) {
            for (var l = start; l <= end; l++) yield return l;
        }
    }

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
        /// update parameter information
        /// </summary>
        /// <param name="pe"></param>
        public void UpdateParameter(ParameterEntity pe) {
            pe.Id = 1;
            if(!context.ParameterEntities.Any()) context.ParameterEntities.Add(pe);
            context.SaveChanges();
        }

        public long[] Aggregate(
            IReadOnlyList<string> signals,
            long seed, long totalSweep, long sweepStart,
            IReadOnlyList<Func<Map<string, decimal>, bool>> delegates,
            Func<string, decimal, string> keyGenerator,
            CancellationToken token) {
            token.ThrowIfCancellationRequested();

            var rt = new long[delegates.Count];

            var targets = context.Entities
                .Where(e => e.Seed == seed && sweepStart <= e.Sweep && e.Sweep <= totalSweep + sweepStart - 1)
                .Where(e => signals.Contains(e.Signal))
                .GroupBy(e => new {e.Sweep, e.Seed})
                .Select(g => g.ToMap(k => keyGenerator(k.Signal, k.Time), v => v.Value)).ToList();

            foreach (var map in targets) {
                Console.WriteLine(string.Join(",", map.Select(s => $"{s.Key}: {s.Value}")));
            }

            //delegates
            //    .Select((d, i) => new {d, i})
            //    .AsParallel()
            //    .ForAll(item => rt[item.i] = targets.Count(item.d));

            return rt;
        }

        /// <summary>
        /// 絞り込み条件とデリゲートを渡して数え上げをする
        /// </summary>
        /// <param name="signals"></param>
        /// <param name="seed">Seedの範囲</param>
        /// <param name="sweep">Sweepの範囲</param>
        /// <param name="delegates"></param>
        /// <param name="keyGenerator">信号名と時間からMapのKeyを生成するメソッド</param>
        /// <param name="token"></param>
        /// <returns></returns>
        public long[] Aggregate(
            IReadOnlyList<string> signals,
            (long start, long end) seed, (long start, long end) sweep,
            IReadOnlyList<Func<Map<string, decimal>, bool>> delegates,
            Func<string, decimal, string> keyGenerator,
            CancellationToken token
        ) {
            token.ThrowIfCancellationRequested();
            var rt = new long[delegates.Count];

            var (eStart, eEnd) = seed;
            var (wStart, wEnd) = sweep;

            var box = new long[eEnd - eStart - 1, delegates.Count];

            for (var s = eStart; s <= eEnd; s++) {
                var target = context.Entities
                    .Where(e => e.Seed == s)
                    .Where(e => wStart <= e.Sweep && e.Sweep <= wEnd)
                    .Where(e => signals.Contains(e.Signal))
                    .GroupBy(e => e.Sweep)
                    .Select(g => g.ToMap(k => keyGenerator(k.Signal, k.Time), v => v.Value)).ToList();

                token.ThrowIfCancellationRequested();
                for (var i = 0; i < delegates.Count; i++) {
                    rt[i] += target.Count(delegates[i]);
                }
            }
            

            return rt;
        }

        public void Dispose() {
            context?.Dispose();
        }
    }

    /// <summary>
    /// DbContext for SqliteRepository
    /// </summary>
    internal class Context : DbContext {
        private readonly string path;
        public Context(string path) => this.path = path;
        public DbSet<ResultEntity> Entities { get; set; }
        public DbSet<ParameterEntity> ParameterEntities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<ResultEntity>().HasKey(e => new {e.Seed, e.Sweep, e.Time, e.Signal});
            modelBuilder.Entity<ResultEntity>().HasIndex(e => new {e.Seed, e.Sweep, e.Time, e.Signal});
            modelBuilder.Entity<ParameterEntity>().HasKey(e => e.Id);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseSqlite($"Data Source={path};");

            var lf = new ServiceCollection().AddLogging(builder =>
                    builder.AddConsole().AddFilter(DbLoggerCategory.Database.Command.Name, LogLevel.Information))
                .BuildServiceProvider().GetService<ILoggerFactory>();
            optionsBuilder.UseLoggerFactory(lf);

            //遅延LoadをOn
            optionsBuilder.UseLazyLoadingProxies();
        }
    }

}
