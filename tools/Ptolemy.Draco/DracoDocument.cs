using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Ptolemy.Repository;

namespace Ptolemy.Draco {
    public class DracoDocument {
        public List<string> Keys { get; }
        public List<ResultEntity> Entities { get; }
        public DracoDocument(string path, long sweep, long seed) {
            List<string> document;
            using (var sr = new StreamReader(path)) 
                document = sr.ReadToEnd()
                    .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).ToList();

            Keys = document[0].Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
            Entities = new List<ResultEntity>();
            foreach (var line in document.Skip(1)) {
                var item = ResultEntity.Parse(sweep, seed, line, Keys);
                Entities.AddRange(item);
            }
        }

    }
}