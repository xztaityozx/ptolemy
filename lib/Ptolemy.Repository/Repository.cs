using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Newtonsoft.Json;

namespace Ptolemy.Repository {

    public class Repository : IDisposable {
        private readonly ResultContext resultContext;
        private readonly ParameterContext parameterContext;

        private static string Hash(string str) {
            using (var sha = SHA256.Create())
                return string.Join("",
                    sha.ComputeHash(Encoding.UTF8.GetBytes(str)).Select(x => $"{x:x}"));
        }

        public Repository(IPAddress host, int port,string name, string password, string vtn, string vtp, string netList) {
            var db = Hash($"{vtn}{vtp}{netList}");
            var connection = $"Server={host},{port};User Id={name};Password={password};";
            resultContext = new ResultContext(connection + $"Database={db}", true);
            parameterContext = new ParameterContext(connection + $"Database=Parameters");
        }

        public void Dispose() {
            resultContext?.Dispose();
            parameterContext?.Dispose();
        }
    }

    public class ResultContext : IDisposable {
        private readonly Context<ResultEntity> context;

        public ResultContext(string connection, bool useSqlServer = false) {
            context = new Context<ResultEntity>(
                o => {
                    if (useSqlServer) o.UseSqlServer(connection);
                    else o.UseSqlite(connection);
                },
                b => {
                    b.Entity<ResultEntity>()
                        .Property(x => x.Values)
                        .HasConversion(
                            v => JsonConvert.SerializeObject(v),
                            v => JsonConvert.DeserializeObject<Map.Map<string, decimal>>(v)
                        );
                    b.Entity<ResultEntity>().HasIndex(e => new {e.Sweep, e.Seed});
                }
            );
        }

        public void Dispose() {
            context?.Dispose();
        }
    }

    public class ParameterContext : IDisposable {
        private readonly Context<ParameterEntity> context;

        public ParameterContext(string connection,bool useSqlServer=false) {
            context = new Context<ParameterEntity>(
                o => {
                    if (useSqlServer) o.UseSqlServer(connection);
                    else o.UseSqlite(connection);

                    o.UseLazyLoadingProxies();
                },
                b => {
                    
                }
            );
        }

        public void Dispose() {
            context?.Dispose();
        }
    }

    internal class Context<TEntity> : DbContext {
        public DbSet<ParameterEntity> ParameterEntities { get; set; }

        private readonly Action<DbContextOptionsBuilder> optionBuilderAction;
        private readonly Action<ModelBuilder> modelBuilderAction;

        public Context( 
            Action<DbContextOptionsBuilder> optionBuilderAction,
            Action<ModelBuilder> modelBuilderAction) {

            this.optionBuilderAction = optionBuilderAction;
            this.modelBuilderAction = modelBuilderAction;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            base.OnConfiguring(optionsBuilder);

            optionBuilderAction(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            base.OnModelCreating(modelBuilder);

            modelBuilderAction(modelBuilder);
        }
    }
}
