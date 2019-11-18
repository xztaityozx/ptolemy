using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;


// TODO: Test
namespace Ptolemy.Repository {
    public class HydraRepository {
        private readonly string dbRoot;
        private readonly CancellationToken token;
        private readonly int bufferSize;
        private readonly ILogger logger;

        public HydraRepository(CancellationToken token, string dbRoot, int bufferSize, ILogger logger) {
            if(string.IsNullOrEmpty(dbRoot)) throw new ArgumentNullException(nameof(dbRoot));
            if (Directory.Exists(dbRoot)) throw new DirectoryNotFoundException(dbRoot);

            (this.token, this.dbRoot) = (token, dbRoot);
            this.bufferSize = bufferSize;
            this.logger = logger;
            receiverMap = new Dictionary<string, Subject<ResultEntity>>();
        }

        private bool SearchDb(string name) {
            return File.Exists(Path.Combine(dbRoot, $"{name}.sqlite"));
        }

        private readonly Dictionary<string, Subject<ResultEntity>> receiverMap;
        private bool HasRegistered(string key) => receiverMap.ContainsKey(key);

        public void AddDb(ParameterEntity pe) {
            var key = pe.Hash();
            if(HasRegistered(key)) return;

            receiverMap.Add(key, new Subject<ResultEntity>());
            if (!SearchDb(key)) {
                logger.LogInformation($"{key}.sqlite not found. Ptolemy.Hydra will create");
            }
            var db = new WriteOnlySqliteRepository(key);
            db.UpdateParameter(pe);

            // add upsert observer
            receiverMap[key].Synchronize().Buffer(bufferSize).Subscribe(list => {
                if (token.IsCancellationRequested) {
                    logger.LogError("Cancel was requested. Ptolemy.Hydra give up write entity to {key}.sqlite");
                }

                db.BulkUpsert(list);
                logger.LogInformation($"{list.Count} entities upsert to {key}.sqlite");
            }, token);
        }

        public void CloseDb(ParameterEntity pe) {
            var key = pe.Hash();

            if (!HasRegistered(key)) return;

            receiverMap[key].OnCompleted();
            receiverMap[key].Dispose();
            receiverMap.Remove(key);
        }
    }

    internal class WriteOnlySqliteRepository {
        private readonly string path;
        public WriteOnlySqliteRepository(string path) => this.path = path;

        private Context Open() {
            var rt = new Context(path);
            rt.Database.EnsureCreated();
            return rt;
        }

        public void BulkUpsert(IList<ResultEntity> list) {
            using var context = Open();
            context.BulkInsertOrUpdate(list);
        }

        public void UpdateParameter(ParameterEntity pe) {
            pe.Id = 1;
            using var context = Open();
            if (!context.ParameterEntities.Any()) context.ParameterEntities.Add(pe);
            context.SaveChanges();
        }
    }
}
