using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Ptolemy.Map;

namespace Ptolemy.Repository {
    public class DbContainer : IDisposable {
        private readonly Map<string, Subject<ResultEntity>> subjectMap;
        private readonly Map<string, SqliteRepository> repositories;
        private readonly Map<string, bool> isClosed;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="token">キャンセル用のToken</param>
        /// <param name="containerRoot">Dbファイルの保存されるルートディレクトリ</param>
        /// <param name="dbs">Db名のリスト</param>
        /// <param name="bufferSize">1度のBulkUpsertで書き込まれるEntity数の最大値</param>
        /// <param name="logger"></param>
        public DbContainer(
            CancellationToken token, 
            string containerRoot, 
            IEnumerable<string> dbs, 
            int bufferSize,
            IObserver<string> logger) {
            
            if(!Directory.Exists(containerRoot)) throw new DirectoryNotFoundException($"{containerRoot} nou found");

            subjectMap=new Map<string, Subject<ResultEntity>>();
            repositories = new Map<string, SqliteRepository>();
            isClosed=new Map<string, bool>();

            foreach (var db in dbs.Distinct()) {
                var path = Path.Combine(containerRoot, $"{db}.sqlite");

                logger.OnNext(File.Exists(path) ? $"\u2611 {path} found" : $"\u26A0 {path} new");

                subjectMap[db] = new Subject<ResultEntity>();
                repositories[db] = new SqliteRepository(path);
                subjectMap[db].Buffer(bufferSize).Subscribe(s => {
                        repositories[db].BulkUpsert(s);
                    },
                    () => logger.OnNext($"finished: {db}.sqlite"), token);
                isClosed[db] = false;
            }
        }


        /// <summary>
        /// DBを閉じ書き込みを終了する
        /// </summary>
        /// <param name="db">閉じるDBの名前</param>
        public void Close(string db) {
            if (isClosed[db] || subjectMap[db].IsDisposed) return;
         
            subjectMap[db]?.OnCompleted();
            isClosed[db] = true;
        }

        /// <summary>
        /// すべてのDBを閉じる
        /// </summary>
        public void CloseAll() {
            foreach (var key in subjectMap.Keys) {
                Close(key);
            }
        }

        public int Count => subjectMap.Count;
        
        /// <summary>
        /// DBへ書き込む
        /// </summary>
        /// <param name="db">書き込むDBの名前</param>
        /// <param name="item"></param>
        public void Add(string db, ResultEntity item) {
            subjectMap[db].OnNext(item);
        }

        public Subject<ResultEntity> this[string index] => subjectMap[index];

        public void Dispose() {
            foreach (var (key,_) in isClosed.Where(k => !k.Value)) {
                Close(key);
            }

            foreach (var sqliteRepository in repositories) {
               sqliteRepository.Value.Dispose(); 
            }

            foreach (var subject in subjectMap) {
                subject.Value.Dispose();
            }
        }
    }
}