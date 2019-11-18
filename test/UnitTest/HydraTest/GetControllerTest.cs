using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Ptolemy.Argo.Request;
using Ptolemy.Config;
using Ptolemy.Hydra.Controllers;
using Xunit;

namespace UnitTest.HydraTest {
    public class GetControllerTest {
        [Theory]
        [InlineData(1)]
        [InlineData(0)]
        [InlineData(10)]
        public void GetPendingTasksTest_Ok(int cnt) {
            var tmp = Path.Combine(Path.GetTempPath(), "Ptolemy.Hydra", "Test");
            var taskDir = Path.Combine(tmp, "aries", "task");
            Directory.CreateDirectory(taskDir);
            Config.Assign(new Config {WorkingRoot = tmp});
            try {
                for (var i = 0; i < cnt; i++) {
                    var path = Path.Combine(taskDir, $"{i}.json");
                    using var sw=new StreamWriter(path);
                    sw.WriteLine(new ArgoRequest().ToJson());
                }

                var c = new GetController();
                var res = c.GetPendingTasks();
                
                Assert.Equal(cnt, res.Count());
            }
            finally {
                Directory.Delete(tmp, true);
                Config.Assign(null);
            }
        }
    }
}
