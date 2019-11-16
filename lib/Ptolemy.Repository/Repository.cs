using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Ptolemy.Map;

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

            //遅延LoadをOn
            optionsBuilder.UseLazyLoadingProxies();
        }
    }

}
