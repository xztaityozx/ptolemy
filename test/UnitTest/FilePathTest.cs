using System;
using System.IO;
using Ptolemy.FilePath;
using Xunit;

namespace UnitTest {
    public class FilePathTest {
        [Fact]
        public void HomeTest() {
            Assert.Equal(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                FilePath.Home
            );
        }

        [Fact]
        public void ExpandTest() {
            var data = new[] {
                new {s = "~/", e = FilePath.Home},
                new {s = "~/abc", e = Path.Combine(FilePath.Home, "abc")},
                new {s = Path.Combine(FilePath.Home, "a", "b"), e = Path.Combine(FilePath.Home, "a", "b")}
            };
            foreach (var d in data) {
                Assert.Equal(d.e, FilePath.Expand(d.s));
            }
        }

        [Fact]
        public void DotConfigTest() {
            Assert.Equal(
                Path.Combine(FilePath.Home, ".config"),
                FilePath.DotConfig
            );
        }
    }
}