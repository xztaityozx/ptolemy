using System;
using System.Diagnostics;
using System.Linq;
using Ptolemy.Argo;
using Ptolemy.Logger;
using Xunit;

namespace UnitTest.ArgoTest {
    public class ArgoTest {
        public enum Throws{
            FailedParse,
            ArgoException,
            None,
        }
        [Fact]
        public void OptionsParseTest() {
            var data = new[] {
                new {throws =false, args = ""},
                new {throws =false, args="--json file"},
                new{throws=true, args="--json file --target t"},
                new{throws=true, args = "--json file --vtn 1,2,3"}
            };

            foreach (var d in data) {
                
                var args = d.args.Split(' ');
                if (d.throws) Assert.Throws<ArgoParseFailedException>(() => Options.Parse(args));
                else Options.Parse(args);
            }
        }

        [Fact]
        public void ArgoConstructorTest() {
            var log = new Logger();
            var data = new[] {
                new {
                    throws = Throws.ArgoException, args = "--target=t --model=m",
                    env = "ARGO_HSPICE:hspice,ARGO_MODEL_FILE:model,ARGO_CIRCUIT_ROOT:root,ARGO_TARGET_CIRCUIT:target"
                },
                new {
                    throws = Throws.ArgoException, args = "--target=t --model=m",
                    env = "ARGO_HSPICE:hspice,ARGO_MODEL_FILE:model,ARGO_CIRCUIT_ROOT:root,"
                },
                new {
                    throws = Throws.ArgoException, args = "--target=t --model=m",
                    env = "ARGO_HSPICE:hspice,ARGO_MODEL_FILE:model,,ARGO_TARGET_CIRCUIT:target"
                },
                new {
                    throws = Throws.ArgoException, args = "--target=t --model=m",
                    env = "ARGO_HSPICE:hspice,ARGO_MODEL_FILE:model,,"
                },
                new {
                    throws = Throws.ArgoException, args = "--target=t --model=m",
                    env = "ARGO_HSPICE:hspice,,ARGO_CIRCUIT_ROOT:root,ARGO_TARGET_CIRCUIT:target"
                },
                new {
                    throws = Throws.ArgoException, args = "--target=t --model=m",
                    env = "ARGO_HSPICE:hspice,,ARGO_CIRCUIT_ROOT:root,"
                },
                new {
                    throws = Throws.ArgoException, args = "--target=t --model=m",
                    env = "ARGO_HSPICE:hspice,,,ARGO_TARGET_CIRCUIT:target"
                },
                new {throws = Throws.ArgoException, args = "--target=t --model=m", env = "ARGO_HSPICE:hspice,,,"},
                new {
                    throws = Throws.ArgoException, args = "--target=t --model=m",
                    env = ",ARGO_MODEL_FILE:model,ARGO_CIRCUIT_ROOT:root,ARGO_TARGET_CIRCUIT:target"
                },
                new {
                    throws = Throws.ArgoException, args = "--target=t --model=m",
                    env = ",ARGO_MODEL_FILE:model,ARGO_CIRCUIT_ROOT:root,"
                },
                new {
                    throws = Throws.ArgoException, args = "--target=t --model=m",
                    env = ",ARGO_MODEL_FILE:model,,ARGO_TARGET_CIRCUIT:target"
                },
                new {throws = Throws.ArgoException, args = "--target=t --model=m", env = ",ARGO_MODEL_FILE:model,,"},
                new {
                    throws = Throws.ArgoException, args = "--target=t --model=m",
                    env = ",,ARGO_CIRCUIT_ROOT:root,ARGO_TARGET_CIRCUIT:target"
                },
                new {throws = Throws.ArgoException, args = "--target=t --model=m", env = ",,ARGO_CIRCUIT_ROOT:root,"},
                new {
                    throws = Throws.ArgoException, args = "--target=t --model=m", env = ",,,ARGO_TARGET_CIRCUIT:target"
                },
                new {throws = Throws.ArgoException, args = "--target=t --model=m", env = ",,,"},
                new {
                    throws = Throws.ArgoException, args = "--target=t ",
                    env = "ARGO_HSPICE:hspice,ARGO_MODEL_FILE:model,ARGO_CIRCUIT_ROOT:root,ARGO_TARGET_CIRCUIT:target"
                },
                new {
                    throws = Throws.ArgoException, args = "--target=t ",
                    env = "ARGO_HSPICE:hspice,ARGO_MODEL_FILE:model,ARGO_CIRCUIT_ROOT:root,"
                },
                new {
                    throws = Throws.ArgoException, args = "--target=t ",
                    env = "ARGO_HSPICE:hspice,ARGO_MODEL_FILE:model,,ARGO_TARGET_CIRCUIT:target"
                },
                new {
                    throws = Throws.ArgoException, args = "--target=t ",
                    env = "ARGO_HSPICE:hspice,ARGO_MODEL_FILE:model,,"
                },
                new {
                    throws = Throws.ArgoException, args = "--target=t ",
                    env = "ARGO_HSPICE:hspice,,ARGO_CIRCUIT_ROOT:root,ARGO_TARGET_CIRCUIT:target"
                },
                new {
                    throws = Throws.ArgoException, args = "--target=t ",
                    env = "ARGO_HSPICE:hspice,,ARGO_CIRCUIT_ROOT:root,"
                },
                new {
                    throws = Throws.ArgoException, args = "--target=t ",
                    env = "ARGO_HSPICE:hspice,,,ARGO_TARGET_CIRCUIT:target"
                },
                new {throws = Throws.ArgoException, args = "--target=t ", env = "ARGO_HSPICE:hspice,,,"},
                new {
                    throws = Throws.ArgoException, args = "--target=t ",
                    env = ",ARGO_MODEL_FILE:model,ARGO_CIRCUIT_ROOT:root,ARGO_TARGET_CIRCUIT:target"
                },
                new {
                    throws = Throws.ArgoException, args = "--target=t ",
                    env = ",ARGO_MODEL_FILE:model,ARGO_CIRCUIT_ROOT:root,"
                },
                new {
                    throws = Throws.ArgoException, args = "--target=t ",
                    env = ",ARGO_MODEL_FILE:model,,ARGO_TARGET_CIRCUIT:target"
                },
                new {throws = Throws.ArgoException, args = "--target=t ", env = ",ARGO_MODEL_FILE:model,,"},
                new {
                    throws = Throws.ArgoException, args = "--target=t ",
                    env = ",,ARGO_CIRCUIT_ROOT:root,ARGO_TARGET_CIRCUIT:target"
                },
                new {throws = Throws.ArgoException, args = "--target=t ", env = ",,ARGO_CIRCUIT_ROOT:root,"},
                new {throws = Throws.ArgoException, args = "--target=t ", env = ",,,ARGO_TARGET_CIRCUIT:target"},
                new {throws = Throws.ArgoException, args = "--target=t ", env = ",,,"},
                new {
                    throws = Throws.ArgoException, args = " --model=m",
                    env = "ARGO_HSPICE:hspice,ARGO_MODEL_FILE:model,ARGO_CIRCUIT_ROOT:root,ARGO_TARGET_CIRCUIT:target"
                },
                new {
                    throws = Throws.ArgoException, args = " --model=m",
                    env = "ARGO_HSPICE:hspice,ARGO_MODEL_FILE:model,ARGO_CIRCUIT_ROOT:root,"
                },
                new {
                    throws = Throws.ArgoException, args = " --model=m",
                    env = "ARGO_HSPICE:hspice,ARGO_MODEL_FILE:model,,ARGO_TARGET_CIRCUIT:target"
                },
                new {
                    throws = Throws.ArgoException, args = " --model=m",
                    env = "ARGO_HSPICE:hspice,ARGO_MODEL_FILE:model,,"
                },
                new {
                    throws = Throws.ArgoException, args = " --model=m",
                    env = "ARGO_HSPICE:hspice,,ARGO_CIRCUIT_ROOT:root,ARGO_TARGET_CIRCUIT:target"
                },
                new {
                    throws = Throws.ArgoException, args = " --model=m",
                    env = "ARGO_HSPICE:hspice,,ARGO_CIRCUIT_ROOT:root,"
                },
                new {
                    throws = Throws.ArgoException, args = " --model=m",
                    env = "ARGO_HSPICE:hspice,,,ARGO_TARGET_CIRCUIT:target"
                },
                new {throws = Throws.ArgoException, args = " --model=m", env = "ARGO_HSPICE:hspice,,,"},
                new {
                    throws = Throws.ArgoException, args = " --model=m",
                    env = ",ARGO_MODEL_FILE:model,ARGO_CIRCUIT_ROOT:root,ARGO_TARGET_CIRCUIT:target"
                },
                new {
                    throws = Throws.ArgoException, args = " --model=m",
                    env = ",ARGO_MODEL_FILE:model,ARGO_CIRCUIT_ROOT:root,"
                },
                new {
                    throws = Throws.ArgoException, args = " --model=m",
                    env = ",ARGO_MODEL_FILE:model,,ARGO_TARGET_CIRCUIT:target"
                },
                new {throws = Throws.ArgoException, args = " --model=m", env = ",ARGO_MODEL_FILE:model,,"},
                new {
                    throws = Throws.ArgoException, args = " --model=m",
                    env = ",,ARGO_CIRCUIT_ROOT:root,ARGO_TARGET_CIRCUIT:target"
                },
                new {throws = Throws.ArgoException, args = " --model=m", env = ",,ARGO_CIRCUIT_ROOT:root,"},
                new {throws = Throws.ArgoException, args = " --model=m", env = ",,,ARGO_TARGET_CIRCUIT:target"},
                new {throws = Throws.ArgoException, args = " --model=m", env = ",,,"},
                new {
                    throws = Throws.ArgoException, args = " ",
                    env = "ARGO_HSPICE:hspice,ARGO_MODEL_FILE:model,ARGO_CIRCUIT_ROOT:root,ARGO_TARGET_CIRCUIT:target"
                },
                new {
                    throws = Throws.ArgoException, args = " ",
                    env = "ARGO_HSPICE:hspice,ARGO_MODEL_FILE:model,ARGO_CIRCUIT_ROOT:root,"
                },
                new {
                    throws = Throws.ArgoException, args = " ",
                    env = "ARGO_HSPICE:hspice,ARGO_MODEL_FILE:model,,ARGO_TARGET_CIRCUIT:target"
                },
                new {throws = Throws.ArgoException, args = " ", env = "ARGO_HSPICE:hspice,ARGO_MODEL_FILE:model,,"},
                new {
                    throws = Throws.ArgoException, args = " ",
                    env = "ARGO_HSPICE:hspice,,ARGO_CIRCUIT_ROOT:root,ARGO_TARGET_CIRCUIT:target"
                },
                new {throws = Throws.ArgoException, args = " ", env = "ARGO_HSPICE:hspice,,ARGO_CIRCUIT_ROOT:root,"},
                new {
                    throws = Throws.ArgoException, args = " ", env = "ARGO_HSPICE:hspice,,,ARGO_TARGET_CIRCUIT:target"
                },
                new {throws = Throws.ArgoException, args = " ", env = "ARGO_HSPICE:hspice,,,"},
                new {
                    throws = Throws.ArgoException, args = " ",
                    env = ",ARGO_MODEL_FILE:model,ARGO_CIRCUIT_ROOT:root,ARGO_TARGET_CIRCUIT:target"
                },
                new {throws = Throws.ArgoException, args = " ", env = ",ARGO_MODEL_FILE:model,ARGO_CIRCUIT_ROOT:root,"},
                new {
                    throws = Throws.ArgoException, args = " ",
                    env = ",ARGO_MODEL_FILE:model,,ARGO_TARGET_CIRCUIT:target"
                },
                new {throws = Throws.ArgoException, args = " ", env = ",ARGO_MODEL_FILE:model,,"},
                new {
                    throws = Throws.ArgoException, args = " ",
                    env = ",,ARGO_CIRCUIT_ROOT:root,ARGO_TARGET_CIRCUIT:target"
                },
                new {throws = Throws.ArgoException, args = " ", env = ",,ARGO_CIRCUIT_ROOT:root,"},
                new {throws = Throws.ArgoException, args = " ", env = ",,,ARGO_TARGET_CIRCUIT:target"},
                new {throws = Throws.ArgoException, args = " ", env = ",,,"},
                new {
                    throws = Throws.ArgoException, args = "--target=t --model=m",
                    env = "ARGO_HSPICE:hspice,ARGO_MODEL_FILE:model,ARGO_CIRCUIT_ROOT:root,ARGO_TARGET_CIRCUIT:target"
                },
                new {
                    throws = Throws.ArgoException, args = "--target=t --model=m",
                    env = "ARGO_HSPICE:hspice,ARGO_MODEL_FILE:model,ARGO_CIRCUIT_ROOT:root,"
                },
                new {
                    throws = Throws.ArgoException, args = "--target=t --model=m",
                    env = "ARGO_HSPICE:hspice,ARGO_MODEL_FILE:model,,ARGO_TARGET_CIRCUIT:target"
                },
                new {
                    throws = Throws.ArgoException, args = "--target=t --model=m",
                    env = "ARGO_HSPICE:hspice,ARGO_MODEL_FILE:model,,"
                },
                new {
                    throws = Throws.ArgoException, args = "--target=t --model=m",
                    env = "ARGO_HSPICE:hspice,,ARGO_CIRCUIT_ROOT:root,ARGO_TARGET_CIRCUIT:target"
                },
                new {
                    throws = Throws.ArgoException, args = "--target=t --model=m",
                    env = "ARGO_HSPICE:hspice,,ARGO_CIRCUIT_ROOT:root,"
                },
                new {
                    throws = Throws.ArgoException, args = "--target=t --model=m",
                    env = "ARGO_HSPICE:hspice,,,ARGO_TARGET_CIRCUIT:target"
                },
                new {throws = Throws.ArgoException, args = "--target=t --model=m", env = "ARGO_HSPICE:hspice,,,"},
                new {
                    throws = Throws.ArgoException, args = "--target=t --model=m",
                    env = ",ARGO_MODEL_FILE:model,ARGO_CIRCUIT_ROOT:root,ARGO_TARGET_CIRCUIT:target"
                },
                new {
                    throws = Throws.ArgoException, args = "--target=t --model=m",
                    env = ",ARGO_MODEL_FILE:model,ARGO_CIRCUIT_ROOT:root,"
                },
                new {
                    throws = Throws.ArgoException, args = "--target=t --model=m",
                    env = ",ARGO_MODEL_FILE:model,,ARGO_TARGET_CIRCUIT:target"
                },
                new {throws = Throws.ArgoException, args = "--target=t --model=m", env = ",ARGO_MODEL_FILE:model,,"},
                new {
                    throws = Throws.ArgoException, args = "--target=t --model=m",
                    env = ",,ARGO_CIRCUIT_ROOT:root,ARGO_TARGET_CIRCUIT:target"
                },
                new {throws = Throws.ArgoException, args = "--target=t --model=m", env = ",,ARGO_CIRCUIT_ROOT:root,"},
                new {
                    throws = Throws.ArgoException, args = "--target=t --model=m", env = ",,,ARGO_TARGET_CIRCUIT:target"
                },
                new {throws = Throws.ArgoException, args = "--target=t --model=m", env = ",,,"},
                new {
                    throws = Throws.ArgoException, args = "--target=t ",
                    env = "ARGO_HSPICE:hspice,ARGO_MODEL_FILE:model,ARGO_CIRCUIT_ROOT:root,ARGO_TARGET_CIRCUIT:target"
                },
                new {
                    throws = Throws.ArgoException, args = "--target=t ",
                    env = "ARGO_HSPICE:hspice,ARGO_MODEL_FILE:model,ARGO_CIRCUIT_ROOT:root,"
                },
                new {
                    throws = Throws.ArgoException, args = "--target=t ",
                    env = "ARGO_HSPICE:hspice,ARGO_MODEL_FILE:model,,ARGO_TARGET_CIRCUIT:target"
                },
                new {
                    throws = Throws.ArgoException, args = "--target=t ",
                    env = "ARGO_HSPICE:hspice,ARGO_MODEL_FILE:model,,"
                },
                new {
                    throws = Throws.ArgoException, args = "--target=t ",
                    env = "ARGO_HSPICE:hspice,,ARGO_CIRCUIT_ROOT:root,ARGO_TARGET_CIRCUIT:target"
                },
                new {
                    throws = Throws.ArgoException, args = "--target=t ",
                    env = "ARGO_HSPICE:hspice,,ARGO_CIRCUIT_ROOT:root,"
                },
                new {
                    throws = Throws.ArgoException, args = "--target=t ",
                    env = "ARGO_HSPICE:hspice,,,ARGO_TARGET_CIRCUIT:target"
                },
                new {throws = Throws.ArgoException, args = "--target=t ", env = "ARGO_HSPICE:hspice,,,"},
                new {
                    throws = Throws.ArgoException, args = "--target=t ",
                    env = ",ARGO_MODEL_FILE:model,ARGO_CIRCUIT_ROOT:root,ARGO_TARGET_CIRCUIT:target"
                },
                new {
                    throws = Throws.ArgoException, args = "--target=t ",
                    env = ",ARGO_MODEL_FILE:model,ARGO_CIRCUIT_ROOT:root,"
                },
                new {
                    throws = Throws.ArgoException, args = "--target=t ",
                    env = ",ARGO_MODEL_FILE:model,,ARGO_TARGET_CIRCUIT:target"
                },
                new {throws = Throws.ArgoException, args = "--target=t ", env = ",ARGO_MODEL_FILE:model,,"},
                new {
                    throws = Throws.ArgoException, args = "--target=t ",
                    env = ",,ARGO_CIRCUIT_ROOT:root,ARGO_TARGET_CIRCUIT:target"
                },
                new {throws = Throws.ArgoException, args = "--target=t ", env = ",,ARGO_CIRCUIT_ROOT:root,"},
                new {throws = Throws.ArgoException, args = "--target=t ", env = ",,,ARGO_TARGET_CIRCUIT:target"},
                new {throws = Throws.ArgoException, args = "--target=t ", env = ",,,"},
                new {
                    throws = Throws.ArgoException, args = " --model=m",
                    env = "ARGO_HSPICE:hspice,ARGO_MODEL_FILE:model,ARGO_CIRCUIT_ROOT:root,ARGO_TARGET_CIRCUIT:target"
                },
                new {
                    throws = Throws.ArgoException, args = " --model=m",
                    env = "ARGO_HSPICE:hspice,ARGO_MODEL_FILE:model,ARGO_CIRCUIT_ROOT:root,"
                },
                new {
                    throws = Throws.ArgoException, args = " --model=m",
                    env = "ARGO_HSPICE:hspice,ARGO_MODEL_FILE:model,,ARGO_TARGET_CIRCUIT:target"
                },
                new {
                    throws = Throws.ArgoException, args = " --model=m",
                    env = "ARGO_HSPICE:hspice,ARGO_MODEL_FILE:model,,"
                },
                new {
                    throws = Throws.ArgoException, args = " --model=m",
                    env = "ARGO_HSPICE:hspice,,ARGO_CIRCUIT_ROOT:root,ARGO_TARGET_CIRCUIT:target"
                },
                new {
                    throws = Throws.ArgoException, args = " --model=m",
                    env = "ARGO_HSPICE:hspice,,ARGO_CIRCUIT_ROOT:root,"
                },
                new {
                    throws = Throws.ArgoException, args = " --model=m",
                    env = "ARGO_HSPICE:hspice,,,ARGO_TARGET_CIRCUIT:target"
                },
                new {throws = Throws.ArgoException, args = " --model=m", env = "ARGO_HSPICE:hspice,,,"},
                new {
                    throws = Throws.ArgoException, args = " --model=m",
                    env = ",ARGO_MODEL_FILE:model,ARGO_CIRCUIT_ROOT:root,ARGO_TARGET_CIRCUIT:target"
                },
                new {
                    throws = Throws.ArgoException, args = " --model=m",
                    env = ",ARGO_MODEL_FILE:model,ARGO_CIRCUIT_ROOT:root,"
                },
                new {
                    throws = Throws.ArgoException, args = " --model=m",
                    env = ",ARGO_MODEL_FILE:model,,ARGO_TARGET_CIRCUIT:target"
                },
                new {throws = Throws.ArgoException, args = " --model=m", env = ",ARGO_MODEL_FILE:model,,"},
                new {
                    throws = Throws.ArgoException, args = " --model=m",
                    env = ",,ARGO_CIRCUIT_ROOT:root,ARGO_TARGET_CIRCUIT:target"
                },
                new {throws = Throws.ArgoException, args = " --model=m", env = ",,ARGO_CIRCUIT_ROOT:root,"},
                new {throws = Throws.ArgoException, args = " --model=m", env = ",,,ARGO_TARGET_CIRCUIT:target"},
                new {throws = Throws.ArgoException, args = " --model=m", env = ",,,"},
                new {
                    throws = Throws.ArgoException, args = " ",
                    env = "ARGO_HSPICE:hspice,ARGO_MODEL_FILE:model,ARGO_CIRCUIT_ROOT:root,ARGO_TARGET_CIRCUIT:target"
                },
                new {
                    throws = Throws.ArgoException, args = " ",
                    env = "ARGO_HSPICE:hspice,ARGO_MODEL_FILE:model,ARGO_CIRCUIT_ROOT:root,"
                },
                new {
                    throws = Throws.ArgoException, args = " ",
                    env = "ARGO_HSPICE:hspice,ARGO_MODEL_FILE:model,,ARGO_TARGET_CIRCUIT:target"
                },
                new {throws = Throws.ArgoException, args = " ", env = "ARGO_HSPICE:hspice,ARGO_MODEL_FILE:model,,"},
                new {
                    throws = Throws.ArgoException, args = " ",
                    env = "ARGO_HSPICE:hspice,,ARGO_CIRCUIT_ROOT:root,ARGO_TARGET_CIRCUIT:target"
                },
                new {throws = Throws.ArgoException, args = " ", env = "ARGO_HSPICE:hspice,,ARGO_CIRCUIT_ROOT:root,"},
                new {
                    throws = Throws.ArgoException, args = " ", env = "ARGO_HSPICE:hspice,,,ARGO_TARGET_CIRCUIT:target"
                },
                new {throws = Throws.ArgoException, args = " ", env = "ARGO_HSPICE:hspice,,,"},
                new {
                    throws = Throws.ArgoException, args = " ",
                    env = ",ARGO_MODEL_FILE:model,ARGO_CIRCUIT_ROOT:root,ARGO_TARGET_CIRCUIT:target"
                },
                new {throws = Throws.ArgoException, args = " ", env = ",ARGO_MODEL_FILE:model,ARGO_CIRCUIT_ROOT:root,"},
                new {
                    throws = Throws.ArgoException, args = " ",
                    env = ",ARGO_MODEL_FILE:model,,ARGO_TARGET_CIRCUIT:target"
                },
                new {throws = Throws.ArgoException, args = " ", env = ",ARGO_MODEL_FILE:model,,"},
                new {
                    throws = Throws.ArgoException, args = " ",
                    env = ",,ARGO_CIRCUIT_ROOT:root,ARGO_TARGET_CIRCUIT:target"
                },
                new {throws = Throws.ArgoException, args = " ", env = ",,ARGO_CIRCUIT_ROOT:root,"},
                new {throws = Throws.ArgoException, args = " ", env = ",,,ARGO_TARGET_CIRCUIT:target"},
                new {throws = Throws.ArgoException, args = " ", env = ",,,"},
                new{throws=Throws.FailedParse,args="--target --model",env="ARGO_HSPICE:hspice,ARGO_MODEL_FILE:model,ARGO_CIRCUIT_ROOT:root,ARGO_TARGET_CIRCUIT:target"},
new{throws=Throws.FailedParse,args="--target --model",env="ARGO_HSPICE:hspice,ARGO_MODEL_FILE:model,ARGO_CIRCUIT_ROOT:root,"},
new{throws=Throws.FailedParse,args="--target --model",env="ARGO_HSPICE:hspice,ARGO_MODEL_FILE:model,,ARGO_TARGET_CIRCUIT:target"},
new{throws=Throws.FailedParse,args="--target --model",env="ARGO_HSPICE:hspice,ARGO_MODEL_FILE:model,,"},
new{throws=Throws.FailedParse,args="--target --model",env="ARGO_HSPICE:hspice,,ARGO_CIRCUIT_ROOT:root,ARGO_TARGET_CIRCUIT:target"},
new{throws=Throws.FailedParse,args="--target --model",env="ARGO_HSPICE:hspice,,ARGO_CIRCUIT_ROOT:root,"},
new{throws=Throws.FailedParse,args="--target --model",env="ARGO_HSPICE:hspice,,,ARGO_TARGET_CIRCUIT:target"},
new{throws=Throws.FailedParse,args="--target --model",env="ARGO_HSPICE:hspice,,,"},
new{throws=Throws.FailedParse,args="--target --model",env=",ARGO_MODEL_FILE:model,ARGO_CIRCUIT_ROOT:root,ARGO_TARGET_CIRCUIT:target"},
new{throws=Throws.FailedParse,args="--target --model",env=",ARGO_MODEL_FILE:model,ARGO_CIRCUIT_ROOT:root,"},
new{throws=Throws.FailedParse,args="--target --model",env=",ARGO_MODEL_FILE:model,,ARGO_TARGET_CIRCUIT:target"},
new{throws=Throws.FailedParse,args="--target --model",env=",ARGO_MODEL_FILE:model,,"},
new{throws=Throws.FailedParse,args="--target --model",env=",,ARGO_CIRCUIT_ROOT:root,ARGO_TARGET_CIRCUIT:target"},
new{throws=Throws.FailedParse,args="--target --model",env=",,ARGO_CIRCUIT_ROOT:root,"},
new{throws=Throws.FailedParse,args="--target --model",env=",,,ARGO_TARGET_CIRCUIT:target"},
new{throws=Throws.FailedParse,args="--target --model",env=",,,"},
new{throws=Throws.FailedParse,args="--target -m",env="ARGO_HSPICE:hspice,ARGO_MODEL_FILE:model,ARGO_CIRCUIT_ROOT:root,ARGO_TARGET_CIRCUIT:target"},
new{throws=Throws.FailedParse,args="--target -m",env="ARGO_HSPICE:hspice,ARGO_MODEL_FILE:model,ARGO_CIRCUIT_ROOT:root,"},
new{throws=Throws.FailedParse,args="--target -m",env="ARGO_HSPICE:hspice,ARGO_MODEL_FILE:model,,ARGO_TARGET_CIRCUIT:target"},
new{throws=Throws.FailedParse,args="--target -m",env="ARGO_HSPICE:hspice,ARGO_MODEL_FILE:model,,"},
new{throws=Throws.FailedParse,args="--target -m",env="ARGO_HSPICE:hspice,,ARGO_CIRCUIT_ROOT:root,ARGO_TARGET_CIRCUIT:target"},
new{throws=Throws.FailedParse,args="--target -m",env="ARGO_HSPICE:hspice,,ARGO_CIRCUIT_ROOT:root,"},
new{throws=Throws.FailedParse,args="--target -m",env="ARGO_HSPICE:hspice,,,ARGO_TARGET_CIRCUIT:target"},
new{throws=Throws.FailedParse,args="--target -m",env="ARGO_HSPICE:hspice,,,"},
new{throws=Throws.FailedParse,args="--target -m",env=",ARGO_MODEL_FILE:model,ARGO_CIRCUIT_ROOT:root,ARGO_TARGET_CIRCUIT:target"},
new{throws=Throws.FailedParse,args="--target -m",env=",ARGO_MODEL_FILE:model,ARGO_CIRCUIT_ROOT:root,"},
new{throws=Throws.FailedParse,args="--target -m",env=",ARGO_MODEL_FILE:model,,ARGO_TARGET_CIRCUIT:target"},
new{throws=Throws.FailedParse,args="--target -m",env=",ARGO_MODEL_FILE:model,,"},
new{throws=Throws.FailedParse,args="--target -m",env=",,ARGO_CIRCUIT_ROOT:root,ARGO_TARGET_CIRCUIT:target"},
new{throws=Throws.FailedParse,args="--target -m",env=",,ARGO_CIRCUIT_ROOT:root,"},
new{throws=Throws.FailedParse,args="--target -m",env=",,,ARGO_TARGET_CIRCUIT:target"},
new{throws=Throws.FailedParse,args="--target -m",env=",,,"},
new{throws=Throws.FailedParse,args="--target ",env="ARGO_HSPICE:hspice,ARGO_MODEL_FILE:model,ARGO_CIRCUIT_ROOT:root,ARGO_TARGET_CIRCUIT:target"},
new{throws=Throws.FailedParse,args="--target ",env="ARGO_HSPICE:hspice,ARGO_MODEL_FILE:model,ARGO_CIRCUIT_ROOT:root,"},
new{throws=Throws.FailedParse,args="--target ",env="ARGO_HSPICE:hspice,ARGO_MODEL_FILE:model,,ARGO_TARGET_CIRCUIT:target"},
new{throws=Throws.FailedParse,args="--target ",env="ARGO_HSPICE:hspice,ARGO_MODEL_FILE:model,,"},
new{throws=Throws.FailedParse,args="--target ",env="ARGO_HSPICE:hspice,,ARGO_CIRCUIT_ROOT:root,ARGO_TARGET_CIRCUIT:target"},
new{throws=Throws.FailedParse,args="--target ",env="ARGO_HSPICE:hspice,,ARGO_CIRCUIT_ROOT:root,"},
new{throws=Throws.FailedParse,args="--target ",env="ARGO_HSPICE:hspice,,,ARGO_TARGET_CIRCUIT:target"},
new{throws=Throws.FailedParse,args="--target ",env="ARGO_HSPICE:hspice,,,"},
new{throws=Throws.FailedParse,args="--target ",env=",ARGO_MODEL_FILE:model,ARGO_CIRCUIT_ROOT:root,ARGO_TARGET_CIRCUIT:target"},
new{throws=Throws.FailedParse,args="--target ",env=",ARGO_MODEL_FILE:model,ARGO_CIRCUIT_ROOT:root,"},
new{throws=Throws.FailedParse,args="--target ",env=",ARGO_MODEL_FILE:model,,ARGO_TARGET_CIRCUIT:target"},
new{throws=Throws.FailedParse,args="--target ",env=",ARGO_MODEL_FILE:model,,"},
new{throws=Throws.FailedParse,args="--target ",env=",,ARGO_CIRCUIT_ROOT:root,ARGO_TARGET_CIRCUIT:target"},
new{throws=Throws.FailedParse,args="--target ",env=",,ARGO_CIRCUIT_ROOT:root,"},
new{throws=Throws.FailedParse,args="--target ",env=",,,ARGO_TARGET_CIRCUIT:target"},
new{throws=Throws.FailedParse,args="--target ",env=",,,"},
new{throws=Throws.FailedParse,args="-t --model",env="ARGO_HSPICE:hspice,ARGO_MODEL_FILE:model,ARGO_CIRCUIT_ROOT:root,ARGO_TARGET_CIRCUIT:target"},
new{throws=Throws.FailedParse,args="-t --model",env="ARGO_HSPICE:hspice,ARGO_MODEL_FILE:model,ARGO_CIRCUIT_ROOT:root,"},
new{throws=Throws.FailedParse,args="-t --model",env="ARGO_HSPICE:hspice,ARGO_MODEL_FILE:model,,ARGO_TARGET_CIRCUIT:target"},
new{throws=Throws.FailedParse,args="-t --model",env="ARGO_HSPICE:hspice,ARGO_MODEL_FILE:model,,"},
new{throws=Throws.FailedParse,args="-t --model",env="ARGO_HSPICE:hspice,,ARGO_CIRCUIT_ROOT:root,ARGO_TARGET_CIRCUIT:target"},
new{throws=Throws.FailedParse,args="-t --model",env="ARGO_HSPICE:hspice,,ARGO_CIRCUIT_ROOT:root,"},
new{throws=Throws.FailedParse,args="-t --model",env="ARGO_HSPICE:hspice,,,ARGO_TARGET_CIRCUIT:target"},
new{throws=Throws.FailedParse,args="-t --model",env="ARGO_HSPICE:hspice,,,"},
new{throws=Throws.FailedParse,args="-t --model",env=",ARGO_MODEL_FILE:model,ARGO_CIRCUIT_ROOT:root,ARGO_TARGET_CIRCUIT:target"},
new{throws=Throws.FailedParse,args="-t --model",env=",ARGO_MODEL_FILE:model,ARGO_CIRCUIT_ROOT:root,"},
new{throws=Throws.FailedParse,args="-t --model",env=",ARGO_MODEL_FILE:model,,ARGO_TARGET_CIRCUIT:target"},
new{throws=Throws.FailedParse,args="-t --model",env=",ARGO_MODEL_FILE:model,,"},
new{throws=Throws.FailedParse,args="-t --model",env=",,ARGO_CIRCUIT_ROOT:root,ARGO_TARGET_CIRCUIT:target"},
new{throws=Throws.FailedParse,args="-t --model",env=",,ARGO_CIRCUIT_ROOT:root,"},
new{throws=Throws.FailedParse,args="-t --model",env=",,,ARGO_TARGET_CIRCUIT:target"},
new{throws=Throws.FailedParse,args="-t --model",env=",,,"},
new{throws=Throws.FailedParse,args="-t -m",env="ARGO_HSPICE:hspice,ARGO_MODEL_FILE:model,ARGO_CIRCUIT_ROOT:root,ARGO_TARGET_CIRCUIT:target"},
new{throws=Throws.FailedParse,args="-t -m",env="ARGO_HSPICE:hspice,ARGO_MODEL_FILE:model,ARGO_CIRCUIT_ROOT:root,"},
new{throws=Throws.FailedParse,args="-t -m",env="ARGO_HSPICE:hspice,ARGO_MODEL_FILE:model,,ARGO_TARGET_CIRCUIT:target"},
new{throws=Throws.FailedParse,args="-t -m",env="ARGO_HSPICE:hspice,ARGO_MODEL_FILE:model,,"},
new{throws=Throws.FailedParse,args="-t -m",env="ARGO_HSPICE:hspice,,ARGO_CIRCUIT_ROOT:root,ARGO_TARGET_CIRCUIT:target"},
new{throws=Throws.FailedParse,args="-t -m",env="ARGO_HSPICE:hspice,,ARGO_CIRCUIT_ROOT:root,"},
new{throws=Throws.FailedParse,args="-t -m",env="ARGO_HSPICE:hspice,,,ARGO_TARGET_CIRCUIT:target"},
new{throws=Throws.FailedParse,args="-t -m",env="ARGO_HSPICE:hspice,,,"},
new{throws=Throws.FailedParse,args="-t -m",env=",ARGO_MODEL_FILE:model,ARGO_CIRCUIT_ROOT:root,ARGO_TARGET_CIRCUIT:target"},
new{throws=Throws.FailedParse,args="-t -m",env=",ARGO_MODEL_FILE:model,ARGO_CIRCUIT_ROOT:root,"},
new{throws=Throws.FailedParse,args="-t -m",env=",ARGO_MODEL_FILE:model,,ARGO_TARGET_CIRCUIT:target"},
new{throws=Throws.FailedParse,args="-t -m",env=",ARGO_MODEL_FILE:model,,"},
new{throws=Throws.FailedParse,args="-t -m",env=",,ARGO_CIRCUIT_ROOT:root,ARGO_TARGET_CIRCUIT:target"},
new{throws=Throws.FailedParse,args="-t -m",env=",,ARGO_CIRCUIT_ROOT:root,"},
new{throws=Throws.FailedParse,args="-t -m",env=",,,ARGO_TARGET_CIRCUIT:target"},
new{throws=Throws.FailedParse,args="-t -m",env=",,,"},
new{throws=Throws.FailedParse,args="-t ",env="ARGO_HSPICE:hspice,ARGO_MODEL_FILE:model,ARGO_CIRCUIT_ROOT:root,ARGO_TARGET_CIRCUIT:target"},
new{throws=Throws.FailedParse,args="-t ",env="ARGO_HSPICE:hspice,ARGO_MODEL_FILE:model,ARGO_CIRCUIT_ROOT:root,"},
new{throws=Throws.FailedParse,args="-t ",env="ARGO_HSPICE:hspice,ARGO_MODEL_FILE:model,,ARGO_TARGET_CIRCUIT:target"},
new{throws=Throws.FailedParse,args="-t ",env="ARGO_HSPICE:hspice,ARGO_MODEL_FILE:model,,"},
new{throws=Throws.FailedParse,args="-t ",env="ARGO_HSPICE:hspice,,ARGO_CIRCUIT_ROOT:root,ARGO_TARGET_CIRCUIT:target"},
new{throws=Throws.FailedParse,args="-t ",env="ARGO_HSPICE:hspice,,ARGO_CIRCUIT_ROOT:root,"},
new{throws=Throws.FailedParse,args="-t ",env="ARGO_HSPICE:hspice,,,ARGO_TARGET_CIRCUIT:target"},
new{throws=Throws.FailedParse,args="-t ",env="ARGO_HSPICE:hspice,,,"},
new{throws=Throws.FailedParse,args="-t ",env=",ARGO_MODEL_FILE:model,ARGO_CIRCUIT_ROOT:root,ARGO_TARGET_CIRCUIT:target"},
new{throws=Throws.FailedParse,args="-t ",env=",ARGO_MODEL_FILE:model,ARGO_CIRCUIT_ROOT:root,"},
new{throws=Throws.FailedParse,args="-t ",env=",ARGO_MODEL_FILE:model,,ARGO_TARGET_CIRCUIT:target"},
new{throws=Throws.FailedParse,args="-t ",env=",ARGO_MODEL_FILE:model,,"},
new{throws=Throws.FailedParse,args="-t ",env=",,ARGO_CIRCUIT_ROOT:root,ARGO_TARGET_CIRCUIT:target"},
new{throws=Throws.FailedParse,args="-t ",env=",,ARGO_CIRCUIT_ROOT:root,"},
new{throws=Throws.FailedParse,args="-t ",env=",,,ARGO_TARGET_CIRCUIT:target"},
new{throws=Throws.FailedParse,args="-t ",env=",,,"},
new{throws=Throws.FailedParse,args=" --model",env="ARGO_HSPICE:hspice,ARGO_MODEL_FILE:model,ARGO_CIRCUIT_ROOT:root,ARGO_TARGET_CIRCUIT:target"},
new{throws=Throws.FailedParse,args=" --model",env="ARGO_HSPICE:hspice,ARGO_MODEL_FILE:model,ARGO_CIRCUIT_ROOT:root,"},
new{throws=Throws.FailedParse,args=" --model",env="ARGO_HSPICE:hspice,ARGO_MODEL_FILE:model,,ARGO_TARGET_CIRCUIT:target"},
new{throws=Throws.FailedParse,args=" --model",env="ARGO_HSPICE:hspice,ARGO_MODEL_FILE:model,,"},
new{throws=Throws.FailedParse,args=" --model",env="ARGO_HSPICE:hspice,,ARGO_CIRCUIT_ROOT:root,ARGO_TARGET_CIRCUIT:target"},
new{throws=Throws.FailedParse,args=" --model",env="ARGO_HSPICE:hspice,,ARGO_CIRCUIT_ROOT:root,"},
new{throws=Throws.FailedParse,args=" --model",env="ARGO_HSPICE:hspice,,,ARGO_TARGET_CIRCUIT:target"},
new{throws=Throws.FailedParse,args=" --model",env="ARGO_HSPICE:hspice,,,"},
new{throws=Throws.FailedParse,args=" --model",env=",ARGO_MODEL_FILE:model,ARGO_CIRCUIT_ROOT:root,ARGO_TARGET_CIRCUIT:target"},
new{throws=Throws.FailedParse,args=" --model",env=",ARGO_MODEL_FILE:model,ARGO_CIRCUIT_ROOT:root,"},
new{throws=Throws.FailedParse,args=" --model",env=",ARGO_MODEL_FILE:model,,ARGO_TARGET_CIRCUIT:target"},
new{throws=Throws.FailedParse,args=" --model",env=",ARGO_MODEL_FILE:model,,"},
new{throws=Throws.FailedParse,args=" --model",env=",,ARGO_CIRCUIT_ROOT:root,ARGO_TARGET_CIRCUIT:target"},
new{throws=Throws.FailedParse,args=" --model",env=",,ARGO_CIRCUIT_ROOT:root,"},
new{throws=Throws.FailedParse,args=" --model",env=",,,ARGO_TARGET_CIRCUIT:target"},
new{throws=Throws.FailedParse,args=" --model",env=",,,"},
new{throws=Throws.FailedParse,args=" -m",env="ARGO_HSPICE:hspice,ARGO_MODEL_FILE:model,ARGO_CIRCUIT_ROOT:root,ARGO_TARGET_CIRCUIT:target"},
new{throws=Throws.FailedParse,args=" -m",env="ARGO_HSPICE:hspice,ARGO_MODEL_FILE:model,ARGO_CIRCUIT_ROOT:root,"},
new{throws=Throws.FailedParse,args=" -m",env="ARGO_HSPICE:hspice,ARGO_MODEL_FILE:model,,ARGO_TARGET_CIRCUIT:target"},
new{throws=Throws.FailedParse,args=" -m",env="ARGO_HSPICE:hspice,ARGO_MODEL_FILE:model,,"},
new{throws=Throws.FailedParse,args=" -m",env="ARGO_HSPICE:hspice,,ARGO_CIRCUIT_ROOT:root,ARGO_TARGET_CIRCUIT:target"},
new{throws=Throws.FailedParse,args=" -m",env="ARGO_HSPICE:hspice,,ARGO_CIRCUIT_ROOT:root,"},
new{throws=Throws.FailedParse,args=" -m",env="ARGO_HSPICE:hspice,,,ARGO_TARGET_CIRCUIT:target"},
new{throws=Throws.FailedParse,args=" -m",env="ARGO_HSPICE:hspice,,,"},
new{throws=Throws.FailedParse,args=" -m",env=",ARGO_MODEL_FILE:model,ARGO_CIRCUIT_ROOT:root,ARGO_TARGET_CIRCUIT:target"},
new{throws=Throws.FailedParse,args=" -m",env=",ARGO_MODEL_FILE:model,ARGO_CIRCUIT_ROOT:root,"},
new{throws=Throws.FailedParse,args=" -m",env=",ARGO_MODEL_FILE:model,,ARGO_TARGET_CIRCUIT:target"},
new{throws=Throws.FailedParse,args=" -m",env=",ARGO_MODEL_FILE:model,,"},
new{throws=Throws.FailedParse,args=" -m",env=",,ARGO_CIRCUIT_ROOT:root,ARGO_TARGET_CIRCUIT:target"},
new{throws=Throws.FailedParse,args=" -m",env=",,ARGO_CIRCUIT_ROOT:root,"},
new{throws=Throws.FailedParse,args=" -m",env=",,,ARGO_TARGET_CIRCUIT:target"},
new{throws=Throws.FailedParse,args=" -m",env=",,,"},
new{throws=Throws.FailedParse,args=" ",env="ARGO_HSPICE:hspice,ARGO_MODEL_FILE:model,ARGO_CIRCUIT_ROOT:root,ARGO_TARGET_CIRCUIT:target"},
new{throws=Throws.FailedParse,args=" ",env="ARGO_HSPICE:hspice,ARGO_MODEL_FILE:model,ARGO_CIRCUIT_ROOT:root,"},
new{throws=Throws.FailedParse,args=" ",env="ARGO_HSPICE:hspice,ARGO_MODEL_FILE:model,,ARGO_TARGET_CIRCUIT:target"},
new{throws=Throws.FailedParse,args=" ",env="ARGO_HSPICE:hspice,ARGO_MODEL_FILE:model,,"},
new{throws=Throws.FailedParse,args=" ",env="ARGO_HSPICE:hspice,,ARGO_CIRCUIT_ROOT:root,ARGO_TARGET_CIRCUIT:target"},
new{throws=Throws.FailedParse,args=" ",env="ARGO_HSPICE:hspice,,ARGO_CIRCUIT_ROOT:root,"},
new{throws=Throws.FailedParse,args=" ",env="ARGO_HSPICE:hspice,,,ARGO_TARGET_CIRCUIT:target"},
new{throws=Throws.FailedParse,args=" ",env="ARGO_HSPICE:hspice,,,"},
new{throws=Throws.FailedParse,args=" ",env=",ARGO_MODEL_FILE:model,ARGO_CIRCUIT_ROOT:root,ARGO_TARGET_CIRCUIT:target"},
new{throws=Throws.FailedParse,args=" ",env=",ARGO_MODEL_FILE:model,ARGO_CIRCUIT_ROOT:root,"},
new{throws=Throws.FailedParse,args=" ",env=",ARGO_MODEL_FILE:model,,ARGO_TARGET_CIRCUIT:target"},
new{throws=Throws.FailedParse,args=" ",env=",ARGO_MODEL_FILE:model,,"},
new{throws=Throws.FailedParse,args=" ",env=",,ARGO_CIRCUIT_ROOT:root,ARGO_TARGET_CIRCUIT:target"},
new{throws=Throws.FailedParse,args=" ",env=",,ARGO_CIRCUIT_ROOT:root,"},
new{throws=Throws.FailedParse,args=" ",env=",,,ARGO_TARGET_CIRCUIT:target"},
new{throws=Throws.FailedParse,args=" ",env=",,,"},

            };

            foreach (var d in data) {
                var envList = d.env.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Split(':')).Select(e => Tuple.Create(e[0], e[1])).ToList();

                foreach (var (key,value) in envList) {
                    Environment.SetEnvironmentVariable(key,value);
                }
                
                var args = d.args.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                switch (d.throws) {
                    case Throws.FailedParse:
                        Assert.Throws<ArgoParseFailedException>(() => new Argo(Options.Parse(args), log));
                        break;
                    case Throws.ArgoException:
                        Assert.Throws<ArgoException>(() => new Argo(Options.Parse(args), log));
                        break;
                    case Throws.None:
                        var argo= new Argo(Options.Parse(args), log);
                        Assert.NotNull(argo);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                foreach (var (key,_) in envList) {
                    Environment.SetEnvironmentVariable(key, null);
                }
            }
        }
    }
}