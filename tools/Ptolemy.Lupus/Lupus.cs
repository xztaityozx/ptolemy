using System;
using System.Collections.Generic;
using System.IO;
using Ptolemy.Draco;
using Ptolemy.Libra.Request;

namespace Ptolemy.Lupus {
    public class Lupus : Cli.SingleCli<LupusOptions, LupusRequest, Tuple<string, long>[]> {
        private readonly string tempDir;
        
        public Lupus(IEnumerable<string> args, Logger.Logger log) : base(args, log) {
            tempDir = Path.Combine(Path.GetTempPath(), "Ptolemy.Lupus");
            Directory.CreateDirectory(tempDir);
        }

        protected override Tuple<string, long>[] Process() {
            try {
                foreach (var item in request.DracoRequests) {
                    using var draco = new Draco.Draco(token, item);
                    draco.Run();
                }

                
                return new Libra.Libra(token).Run(request.LibraRequest);
            }
            catch (DracoException de) {
                throw new LupusException($"Ptolemy.Draco内でエラーが起きました\n\t-->{de}");
            }
            catch (LibraException le) {
                throw new LibraException($"Ptolemy.Libra内でエラーが起きました\n\t-->{le}");
            }
            catch (Exception e) {
                throw new LupusException($"Unknown error has occured\n\t-->{e}");
            }
        }

        public new void Dispose() {
            base.Dispose();
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
        }
    }
}
