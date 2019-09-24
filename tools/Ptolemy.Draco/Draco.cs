using System;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Ptolemy.Draco.Request;
using Ptolemy.Repository;

namespace Ptolemy.Draco {
    public class Draco : IDisposable{
        private readonly CancellationToken token;
        private readonly DracoRequest request;
        private readonly Subject<ResultEntity> receiver;
        
        public readonly Subject<string> Log;

        public Draco(CancellationToken token, DracoRequest request) {
            this.token = token;
            this.request = request;
            Log = new Subject<string>();
            receiver = new Subject<ResultEntity>();
        }
       
        // TODO: 例外処理しろ
        public bool Run() {
            Log.OnNext("Start Ptolemy.Draco");
            Log.OnNext($"InputFile: {request.InputFile}");
            Log.OnNext($"TargetDatabaseFile: {request.SqLiteFile}");

            string[] document;
            using (var sr = new StreamReader(request.InputFile))
                document = sr.ReadToEnd()
                    .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
                    .Skip(1)
                    .ToArray();

            
            var keys = document[0].Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var pusher = receiver.Buffer(request.BufferSize).Subscribe(
                r => {
                    using var repo = new SqliteRepository(request.SqLiteFile);
                    repo.BulkUpsert(r);
                });

            token.Register(pusher.Dispose);
            
            foreach (var line in document.Skip(1).SelectMany(line =>
                ResultEntity.Parse(request.Seed, request.Sweep, line, keys))) {
                receiver.OnNext(line);
            }
            receiver.OnCompleted();

            return true;
        }

        public void Dispose() {
            receiver?.Dispose();
            Log?.Dispose();
        }
    }
}