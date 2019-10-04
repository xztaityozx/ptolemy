using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Ptolemy.Draco;
using Ptolemy.Draco.Request;
using Ptolemy.Libra.Request;
using Ptolemy.Map;
using Ptolemy.Repository;

namespace Ptolemy.Lupus {
    public class Lupus : IDisposable {

        private readonly string temp;

        public Lupus() {
            temp = Path.Combine(Path.GetTempPath(), "Ptolemy.Lupus");
            Directory.CreateDirectory(temp);
        }

        public Tuple<string,long>[] Run(CancellationToken token, LupusRequest request) {
            try {
                foreach (var item in request.DracoRequests) {
                    using var draco = new Draco.Draco(token, item);
                    draco.Run();
                }

                
                return new Libra.Libra(token, request.LibraRequest).Run();
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

        public void Dispose() {
            Directory.Delete(temp, true);
        }
    }

    public class LupusResult {
        public string Name { get; set; }
        public Tuple<string,long>[] Results { get; set; }
    }
}
