using System;
using System.Collections.Generic;
using System.Threading;
using Ptolemy.Argo.Request;
using Ptolemy.Repository;

namespace Ptolemy.Simulator {
    public interface ISimulator {
        IReadOnlyList<ResultEntity> Run(CancellationToken token, ArgoRequest request, Action intervalAction);
    }

    public static class SimulatorExtension {
        public static ParameterEntity ConvertToParameterEntity(ArgoRequest ar) {
            var rt = new ParameterEntity {
                Vtn = ar.Transistors.Vtn.ToString(),
                Vtp = ar.Transistors.Vtp.ToString(),
                NetList = ar.NetList,
                Time = ar.Time.ToString(),
                Signals = string.Join(":", ar.Signals),
                Includes = string.Join(":", ar.Includes),
                Hspice = ar.HspicePath,
                HspiceOption = string.Join(":", ar.HspiceOptions),
                Gnd = ar.Gnd,
                Vdd = ar.Vdd,
                IcCommand = string.Join(":", ar.IcCommands),
                Temperature = ar.Temperature
            };

            ar.ResultFile = rt.Hash();

            return rt;
        }
    }
}
