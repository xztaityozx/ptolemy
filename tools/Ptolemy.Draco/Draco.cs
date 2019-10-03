using System;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using Ptolemy.Draco.Request;
using Ptolemy.Repository;

namespace Ptolemy.Draco {
    public class Draco : IDisposable {
        private readonly CancellationToken token;
        private readonly DracoRequest request;
        private readonly Subject<ResultEntity> receiver;
        
        /// <summary>
        /// Log Receiver
        /// </summary>
        public readonly Subject<string> Log;
        /// <summary>
        /// ひとつの処理が済むたびにOnNextされるSubject
        /// </summary>
        public readonly Subject<Unit> WriteProgress, ParseProgress;
        
        public Draco(CancellationToken token, DracoRequest request) {
            this.token = token;
            this.request = request;
            Log = new Subject<string>();
            receiver = new Subject<ResultEntity>();
            WriteProgress=new Subject<Unit>();
            ParseProgress=new Subject<Unit>();
        }
       
        /// <summary>
        /// Start draco process
        /// </summary>
        public void Run() {
            Log.OnNext("Start Ptolemy.Draco");
            Log.OnNext($"InputFile: {request.InputFile}");
            Log.OnNext($"TargetDatabaseFile: {request.OutputFile}");

            string[] document;
            try {
                Log.OnNext("Reading InputFile");
                using var sr = new StreamReader(request.InputFile);
                document = sr.ReadToEnd()
                    .Split("\n", StringSplitOptions.RemoveEmptyEntries)
                    .Skip(1)
                    .ToArray();
            }
            catch (FileNotFoundException) {
                throw new DracoException($"file not found: {request.InputFile}");
            }

            try {
                // input file's format
                // time     voltage   voltage ...
                //         signalA   signalB
                //    0.    value     value   ...
                //  ...


                // Get signal list
                var keys = document[0].Split(' ', StringSplitOptions.RemoveEmptyEntries);

                
                var writer = receiver.Buffer(request.BufferSize).Subscribe(
                    r => {
                        using var repo = new SqliteRepository(request.OutputFile);
                        repo.BulkUpsert(r);
                        WriteProgress.OnNext(Unit.Default);
                        Log.OnNext($"Write {r.Count} records");
                    }, () => WriteProgress.OnCompleted());

                token.Register(writer.Dispose);


                foreach (var line in document.Skip(1).SelectMany(line => {
                    // Parse: time value value ....
                    var rt = ResultEntity.Parse(request.Seed, request.Sweep, line, keys);
                    ParseProgress.OnNext(Unit.Default);
                    Log.OnNext($"Parsed: {line}");
                    return rt;
                })) {
                    token.ThrowIfCancellationRequested();
                    receiver.OnNext(line);
                }

                Log.OnNext($"Finished Parse");

                receiver.OnCompleted();
                ParseProgress.OnCompleted();
            }
            catch (IndexOutOfRangeException) {
                throw new DracoException($"invalid file format: {request.InputFile}");
            }
            catch (FormatException) {
                throw new DracoException($"データの数値に不正な値があります {request.InputFile}");
            }
            catch (OperationCanceledException) {
                throw new DracoException("Canceled by user");
            }
            catch (Exception e) {
                throw new DracoException($"Unknown error has occured\n\t-->{e}");
            }
        }

        public void Dispose() {
            receiver?.Dispose();
            Log?.Dispose();
            WriteProgress?.Dispose();
            ParseProgress?.Dispose();
        }

    }
}