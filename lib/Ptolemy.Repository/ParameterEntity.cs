using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Ptolemy.Repository {
    public class ParameterComparer : IEqualityComparer<ParameterEntity> {
        public bool Equals(ParameterEntity x, ParameterEntity y) => x?.Hash() == y?.Hash();

        public int GetHashCode(ParameterEntity obj) {
            return obj.GetHashCode();
        }
    }
    public class ParameterEntity {
        public long Id { get; set; }
        public string Vtn { get; set; }
        public string Vtp { get; set; }
        public string Includes { get; set; }
        public string Time { get; set; }
        public string IcCommand { get; set; }
        public decimal Gnd { get; set; }
        public decimal Vdd { get; set; }
        public string Signals { get; set; }
        public decimal Temperature { get; set; }
        public string HspiceOption { get; set; }
        public string NetList { get; set; }
        public string Hspice { get; set; }

        public ParameterEntity() { }

        public string Hash() {
            using var sha = SHA256.Create();
            return string.Join("", sha.ComputeHash(Encoding.UTF8.GetBytes(string.Join("",
                new[] {Vtn, Vtp, Includes, Time, IcCommand, $"{Gnd}{Vdd}{Temperature}", Signals, HspiceOption, NetList},
                Hspice))));
        }


        public int GetHashCode(ParameterEntity obj) {
            throw new NotImplementedException();
        }
    }
}
