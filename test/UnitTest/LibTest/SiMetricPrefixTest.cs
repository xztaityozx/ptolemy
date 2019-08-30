using System;
using Ptolemy.SiMetricPrefix;
using Xunit;

namespace UnitTest.LibTest {
    public class SiMetricPrefixTest {
        [Fact]
        public void ParseDecimalTest() {
            var data = new[] {
                new {src = "1Y", ept = 1e24M},
                new {src = "1Z", ept = 1e21M},
                new {src = "1E", ept = 1e18M},
                new {src = "1P", ept = 1e15M},
                new {src = "1T", ept = 1e12M},
                new {src = "1G", ept = 1e9M},
                new {src = "1M", ept = 1e6M},
                new {src = "1K", ept = 1e3M},
                new {src = "1h", ept = 1e2M},
                new {src = "1da", ept = 1e1M},
                new {src = "1d", ept = 1e-1M},
                new {src = "1c", ept = 1e-2M},
                new {src = "1m", ept = 1e-3M},
                new {src = "1u", ept = 1e-6M},
                new {src = "1n", ept = 1e-9M},
                new {src = "1p", ept = 1e-12M},
                new {src = "1f", ept = 1e-15M},
                new {src = "1a", ept = 1e-18M},
                new {src = "1z", ept = 1e-21M},
                new {src = "1y", ept = 1e-24M},
            };

            foreach (var d in data) {
                Assert.Equal(d.ept, d.src.ParseDecimalWithSiPrefix());
            }

            Assert.Throws<FormatException>(() => "1e".ParseDecimalWithSiPrefix());
        }

        [Fact]
        public void ParseDoubleTest() {
            var data = new[] {
                new {src = "1Y", ept = 1e24},
                new {src = "1Z", ept = 1e21},
                new {src = "1E", ept = 1e18},
                new {src = "1P", ept = 1e15},
                new {src = "1T", ept = 1e12},
                new {src = "1G", ept = 1e9},
                new {src = "1M", ept = 1e6},
                new {src = "1K", ept = 1e3},
                new {src = "1h", ept = 1e2},
                new {src = "1da", ept = 1e1},
                new {src = "1d", ept = 1e-1},
                new {src = "1c", ept = 1e-2},
                new {src = "1m", ept = 1e-3},
                new {src = "1u", ept = 1e-6},
                new {src = "1n", ept = 1e-9},
                new {src = "1p", ept = 1e-12},
                new {src = "1f", ept = 1e-15},
                new {src = "1a", ept = 1e-18},
                new {src = "1z", ept = 1e-21},
                new {src = "1y", ept = 1e-24},
            };

            foreach (var d in data) {
                Assert.Equal(d.ept, d.src.ParseDoubleWithSiPrefix());
            }

            Assert.Throws<FormatException>(() => "1e".ParseDoubleWithSiPrefix());
        }

        [Fact]
        public void ParseIntTest() {
            var data = new[] {
                new {src = "1G", ept = (int) 1e9},
                new {src = "1M", ept = (int) 1e6},
                new {src = "1K", ept = (int) 1e3},
                new {src = "1h", ept = (int) 1e2},
                new {src = "1da", ept = (int) 1e1},
                new {src = "1d", ept = 0},
                new {src = "1c", ept = 0},
                new {src = "1m", ept = 0},
                new {src = "1u", ept = 0},
                new {src = "1n", ept = 0},
                new {src = "1p", ept = 0},
                new {src = "1f", ept = 0},
                new {src = "1a", ept = 0},
                new {src = "1z", ept = 0},
                new {src = "1y", ept = 0},
            };
            var fail = new[] {
                new {src = "1Y"},
                new {src = "1Z"},
                new {src = "1E"},
                new {src = "1P"},
                new {src = "1T"},
            };

            foreach (var d in data) {
                Assert.Equal(d.ept, d.src.ParseIntWithSiPrefix());
            }

            foreach (var f in fail) {
                Assert.Throws<OverflowException>(() => f.src.ParseIntWithSiPrefix());
            }

            Assert.Throws<FormatException>(() => "1e".ParseIntWithSiPrefix());
        }

        [Fact]
        public void ParseLongTest() {
            var data = new[] {
                new {src = "1E", ept = (long) 1e18},
                new {src = "1P", ept = (long) 1e15},
                new {src = "1T", ept = (long) 1e12},
                new {src = "1G", ept = (long) 1e9},
                new {src = "1M", ept = (long) 1e6},
                new {src = "1K", ept = (long) 1e3},
                new {src = "1h", ept = (long) 1e2},
                new {src = "1da", ept = (long) 1e1},
                new {src = "1d", ept = 0L},
                new {src = "1c", ept = 0L},
                new {src = "1m", ept = 0L},
                new {src = "1u", ept = 0L},
                new {src = "1n", ept = 0L},
                new {src = "1p", ept = 0L},
                new {src = "1f", ept = 0L},
                new {src = "1a", ept = 0L},
                new {src = "1z", ept = 0L},
                new {src = "1y", ept = 0L},
            };
            var fail = new[] {
                new {src = "1Y"},
                new {src = "1Z"}
            };

            foreach (var d in data) {
                Assert.Equal(d.ept, d.src.ParseLongWithSiPrefix());
            }

            foreach (var f in fail) {
                Assert.Throws<OverflowException>(() => f.src.ParseLongWithSiPrefix());
            }

            Assert.Throws<FormatException>(() => "1e".ParseLongWithSiPrefix());
        }
        
        [Fact]
        public void TryTest() {
            Assert.True("1Y".TryParseDecimalWithSiPrefix(out var v));
            Assert.Equal(1E24M, v);

            Assert.False("1e".TryParseDecimalWithSiPrefix(out var x));
            Assert.Equal(0M, x);

            Assert.True("1Y".TryParseDoubleWithSiPrefix(out var y));
            Assert.Equal(1E24, y);

            Assert.False("1e".TryParseDoubleWithSiPrefix(out var z));
            Assert.Equal(0, z);
        }
    }
}