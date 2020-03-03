using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Ptolemy.Argo.Request;
using Ptolemy.Repository;
using Ptolemy.Simulator;
using ShellProgressBar;

namespace Ptolemy.Argo {
    public class Argo  {
        public const string EnvArgoHspice = "ARGO_HSPICE";
        public const string EnvArgoIncludes = "ARGO_INCLUDES";

        /// <summary>
        /// パラメータに従ってシミュレーションを実行する
        /// </summary>
        /// <param name="token">キャンセル用のToken</param>
        /// <param name="request">パラメータ情報</param>
        /// <returns></returns>
        public static List<ResultEntity> Run(CancellationToken token, ArgoRequest request) {
            var hspice = new Hspice();
            using var bar = new ProgressBar((int) request.Sweep, "Ptolemy.Argo", new ProgressBarOptions {
                BackgroundCharacter = '-', ProgressCharacter = '>',
                BackgroundColor = ConsoleColor.DarkGray, ForegroundColor = ConsoleColor.DarkGreen,
                CollapseWhenFinished = true, DisplayTimeInRealTime = true,
                ForegroundColorDone = ConsoleColor.Green
            });
            return hspice.Run(token, request, ()=> bar.Tick()).ToList();
        }
    }
}