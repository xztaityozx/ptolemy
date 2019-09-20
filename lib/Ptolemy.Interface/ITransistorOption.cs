using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandLine;
using Ptolemy.Parameters;
using Ptolemy.SiMetricPrefix;

namespace Ptolemy.Interface {
    public static partial class OptionDefault {
        private const decimal VtnThreshold = 0.6M;
        private const decimal Sigma = 0.046M;
        private const decimal Deviation = 1M;
        private const decimal VtpThreshold = -0.6M;


        private static T Bind<T>(T def, params T?[] items) where T :struct {
            if (items.All(x => x is null)) return def;
            return items.First(x => x != null) ?? def;
        }

        public static TransistorPair Bind(this ITransistorOption @this, TransistorPair config) {
            var vtnOpt = @this.VtnStrings
                .Select(s => string.IsNullOrEmpty(s) ? (decimal?) null : s.ParseDecimalWithSiPrefix()).ToArray();
            var vtpOpt = @this.VtpStrings
                .Select(s => string.IsNullOrEmpty(s) ? (decimal?) null : s.ParseDecimalWithSiPrefix()).ToArray();

            return new TransistorPair {
                Vtn = new Transistor(
                    Bind(VtnThreshold, vtnOpt[0], config?.Vtn?.Threshold),
                    Bind(Sigma, vtnOpt[1], (decimal?)@this.Sigma, config?.Vtn?.Sigma),
                    Bind(Deviation, vtnOpt[2], config?.Vtn?.Deviation)
                ),
                
                Vtp = new Transistor(
                    Bind(VtpThreshold, vtpOpt[0], config?.Vtp?.Threshold),
                    Bind(Sigma, vtpOpt[1], (decimal?)@this.Sigma, config?.Vtp?.Sigma),
                    Bind(Deviation, vtpOpt[2], config?.Vtp?.Deviation)
                ),
            };
        }
    }

    public interface ITransistorOption {
        [Option('N', "vtn", Default = new[]{"0.6","0.046","1.0"}, HelpText = "Vtnの値を[閾値],[シグマ],[偏差]で指定します", Separator = ',')]
        IEnumerable<string> VtnStrings { get; set; }

        [Option('P', "vtp", Default = new[]{"-0.6","0.046","1.0"}, HelpText = "Vtpの値を[閾値],[シグマ],[偏差]で指定します", Separator = ',')]
        IEnumerable<string> VtpStrings { get; set; }

        [Option('S', "sigma", Default = null, HelpText = "Vtn,Vtp両方のSigmaを指定します。個別設定が優先されます")]
        double? Sigma { get; set; }
    }
}
