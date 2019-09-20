using System;
using Microsoft.EntityFrameworkCore;

namespace Ptolemy.Repository {
    public class Repository {
        
    }

    internal class Context : DbContext {
        public DbSet<ResultEntity> Entities { get; set; }
    }
}
