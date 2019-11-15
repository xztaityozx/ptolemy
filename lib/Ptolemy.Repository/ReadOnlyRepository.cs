using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Ptolemy.Map;

namespace Ptolemy.Repository {
    public class ReadOnlyRepository {
        private readonly string path;
        public ReadOnlyRepository(string path) {
            this.path = path;
        }

        private Context Connect() {
            var rt = new Context(path);
            rt.Database.EnsureCreated();

            return rt;
        }

        public ParameterEntity GetParameter() {
            using var c = Connect();
            return c.ParameterEntities.First();
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
    }
}