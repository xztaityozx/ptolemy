using System;
using System.Text.Json;
using CommandLine;
using Ptolemy.Interface;

namespace Ptolemy.Aries {
    internal class Program {
        private static void Main(string[] args) {

            args = "--vtn 1,2,3 --vtp 4,5,6 -e 7,8 -w 9,100".Split(' ', StringSplitOptions.RemoveEmptyEntries);

            var res = Parser.Default.ParseArguments<AriesMake>(args)
                .MapResult(a => {
                    return JsonSerializer.Serialize(a);
                }, e => "error");

            Console.WriteLine(res);
        }
    }
}
