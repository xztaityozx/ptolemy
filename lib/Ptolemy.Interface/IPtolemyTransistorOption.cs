using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandLine;
using Ptolemy.Parameters;
using Ptolemy.SiMetricPrefix;

namespace Ptolemy.Interface {
    public static class OptionDefault {
        private static readonly decimal[] VtnDefault = {0.6M, 0.046M, 1.0M}, VtpDefault = {-0.6M, 0.046M, 1.0M};

        // TODO: ここテストしろ。絶対バグってる
        public static void Bind(this IPtolemyTransistorOption @this, TransistorPair config) {
            config = config ?? new TransistorPair();
            {
                // vtn
                var box = @this.VtnStrings.ToArray();
                config.Vtn.Threshold = string.IsNullOrEmpty(box[0])
                    ? config.Vtn.Threshold ?? VtnDefault[0]
                    : box[0].ParseDecimalWithSiPrefix();
                config.Vtn.Sigma = string.IsNullOrEmpty(box[1])
                    ? config.Vtn.Sigma ?? VtnDefault[1]
                    : box[1].ParseDecimalWithSiPrefix();
                config.Vtn.Deviation = string.IsNullOrEmpty(box[2])
                    ? config.Vtn.Deviation ?? VtnDefault[2]
                    : box[2].ParseDecimalWithSiPrefix();
            }
            {
                // vtp
                var box = @this.VtpStrings.ToArray();
                config.Vtp.Threshold = string.IsNullOrEmpty(box[0])
                    ? config.Vtp.Threshold ?? VtpDefault[0]
                    : box[0].ParseDecimalWithSiPrefix();
                config.Vtp.Sigma = string.IsNullOrEmpty(box[1])
                    ? config.Vtp.Sigma ?? VtpDefault[1]
                    : box[1].ParseDecimalWithSiPrefix();
                config.Vtp.Deviation = string.IsNullOrEmpty(box[2])
                    ? config.Vtp.Deviation ?? VtpDefault[2]
                    : box[2].ParseDecimalWithSiPrefix();
            }
        }
    }

    public interface IPtolemyTransistorOption {
        [Option('N', "vtn", Default = null, HelpText = "Vtnの値を[閾値],[シグマ],[偏差]で指定します", Separator = ',')]
        IEnumerable<string> VtnStrings { get; set; }

        [Option('P', "vtp", Default = null, HelpText = "Vtpの値を[閾値],[シグマ],[偏差]で指定します", Separator = ',')]
        IEnumerable<string> VtpStrings { get; set; }

        [Option('S', "sigma", Default = null, HelpText = "Vtn,Vtp両方のSigmaを指定します。個別設定が優先されます")]
        double? Sigma { get; set; }
    }
}
