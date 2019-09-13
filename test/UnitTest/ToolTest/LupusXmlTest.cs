using System;
using System.Linq;
using Ptolemy.Lupus.Xml;
using Xunit;

namespace UnitTest.ToolTest
{
    public class LupusXmlTest
    {
        [Fact]
        public void GenerateResultsXmlTest() {
            var res = LupusXml.GenerateResultsXml("/tmp", Enumerable.Range(1, 100).Select(l => (long) l).ToList());

            // Root
            Assert.Equal("saResults", res.Name);
            Assert.Equal("1.0", res.Version);
            Assert.Single(res.Objects);

            // root Object
            var obj = res.Objects[0];
            Assert.Equal("saResults", obj.Type);
            Assert.Equal("saResults", obj.Name);
            Assert.Equal("1", obj.Version);
            Assert.Equal(6, obj.Attributes.Length);
            Assert.All(obj.Attributes, attribute => Assert.Equal("giString", attribute.Type));
            Microsoft.VisualStudio.TestTools.UnitTesting.CollectionAssert.AreEquivalent(
                new[] { "resultsMap.xml", "/tmp", ".", $"{DateTime.Now}", "HSPICE", "" },
                obj.Attributes.Select(x => x.Value).ToList()
            );

            Microsoft.VisualStudio.TestTools.UnitTesting.CollectionAssert.AreEquivalent(
                new[] { "name", "netlistDir", "resultsDir", "runTime", "simulator", "version" },
                obj.Attributes.Select(x => x.Name).ToList()
            );
            Assert.Single(obj.Collections);

            // top collection
            var col = obj.Collections[0];
            Assert.Equal("resultFiles", col.Name);
            Assert.Equal(4, col.Objects.Length);
            Assert.All(col.Objects, o =>
            {
                Assert.Equal("1", o.Version);
                Assert.Equal("saResultFile", o.Type);
                Assert.Equal("resultFiles", o.Name);
            });
            {
                var subAttributes = col.Objects.SelectMany(o => o.Attributes).ToList();
                Assert.All(subAttributes, o => Assert.Equal("giString", o.Type));
                Microsoft.VisualStudio.TestTools.UnitTesting.CollectionAssert.AreEquivalent(
                    new[] {
                        "tran", "", "psf", string.Join(" ", Enumerable.Range(1, 100).Select(d => $"{d}.0")), "tran",
                        "statistical", "hspice.mc0", "psf", "statistical", "", "designVariables.wdf", "wdf", "variables",
                        "scalarData", "scalar.dat", "table", "statistical"
                    },
                    subAttributes.Select(x => x.Value).ToList()
                );
                Microsoft.VisualStudio.TestTools.UnitTesting.CollectionAssert.AreEquivalent(
                    new[] {
                        "analysisName", "filename", "format", "iterations", "resultType", "analysisName", "filename",
                        "format", "resultType", "analysisName", "filename", "format", "resultType", "analysisName",
                        "filename", "format", "resultType"
                    },
                    subAttributes.Select(x => x.Name).ToList()
                );
            }
            Assert.Single(col.Objects[0].Collections);

            // itr collection
            var itr = col.Objects[0].Collections[0];
            Assert.Equal("sweepFiles", itr.Name);
            Assert.Equal(100, itr.Objects.Length);
            Assert.All(itr.Objects, o =>
            {
                Assert.Equal("1", o.Version);
                Assert.Equal("saSweepFile", o.Type);
                Assert.Equal("sweepFiles", o.Name);
                Assert.Equal("giString", o.Attributes[0].Type);
                Assert.Equal("filename", o.Attributes[0].Name);
                Assert.Equal("params", o.Collections[0].Name);
                Assert.Equal("1", o.Collections[0].Objects[0].Version);
                Assert.Equal("saPair", o.Collections[0].Objects[0].Type);
                Assert.Equal("params", o.Collections[0].Objects[0].Name);
                Assert.Equal("giString", o.Collections[0].Objects[0].Attributes[0].Type);
                Assert.Equal("MONTE_CARLO", o.Collections[0].Objects[0].Attributes[0].Value);
                Assert.Equal("name", o.Collections[0].Objects[0].Attributes[0].Name);
                Assert.Equal("giString", o.Collections[0].Objects[0].Attributes[1].Type);
                Assert.Equal("value", o.Collections[0].Objects[0].Attributes[1].Name);
            });

            Microsoft.VisualStudio.TestTools.UnitTesting.CollectionAssert.AreEquivalent(
                Enumerable.Range(1, 100).Select(d => $"hspice.tr0@{d}").ToList(),
                itr.Objects.Select(x => x.Attributes[0].Value).ToList()
            );

            Microsoft.VisualStudio.TestTools.UnitTesting.CollectionAssert.AreEquivalent(
                Enumerable.Range(1, 100).Select(d => $"{d}.0").ToList(),
                itr.Objects.Select(x => x.Collections[0].Objects[0].Attributes[1].Value).ToList()
            );
        }

        [Fact]
        public void GenerateResultsMapXmlTest()
        {
            var doc = LupusXml.GenerateResultsMapXml("/tmp");
            Assert.Equal("1", doc.Version);
            Assert.Equal("saResultsMap", doc.Name);
            Assert.Single(doc.Objects);

            var obj = doc.Objects[0];
            Assert.Equal("1", obj.Version);
            Assert.Equal("saResultsMap", obj.Name);
            Assert.Equal("saResultsMap", obj.Type);
            Assert.Equal(6, obj.Attributes.Length);
            Assert.All(obj.Attributes, a => Assert.Equal("giString", a.Type));
            var attr = obj.Attributes;
            Microsoft.VisualStudio.TestTools.UnitTesting.CollectionAssert.AreEquivalent(
                new[] { ".", ".", "MONTE_CARLO", "resultsMap.xml", "HSPICE", $"{DateTime.Now}" },
                attr.Select(x => x.Value).ToList()
            );
            Microsoft.VisualStudio.TestTools.UnitTesting.CollectionAssert.AreEquivalent(
                new[] { "masterResultsDir", "monteCarlo", "name", "resultsMapDir", "simulator", "timeStamp" },
                attr.Select(x => x.Name).ToList()
            );

            Assert.Single(obj.Collections);
            var col = obj.Collections[0];
            Assert.Equal("resultsInfo", col.Name);
            Assert.Single(col.Objects);

            var sub = col.Objects[0];
            Assert.Equal("1", sub.Version);
            Assert.Equal("saResultsInfo", sub.Type);
            Assert.Equal("resultsInfo", sub.Name);
            Assert.Equal(2, sub.Attributes.Length);
            Assert.All(sub.Attributes, a => Assert.Equal("giString", a.Type));
            Microsoft.VisualStudio.TestTools.UnitTesting.CollectionAssert.AreEquivalent(
                new[] { "/tmp", "." }, sub.Attributes.Select(x => x.Value).ToList());
            Microsoft.VisualStudio.TestTools.UnitTesting.CollectionAssert.AreEquivalent(
                new[] { "netlistDir", "resultsDir" }, sub.Attributes.Select(x => x.Name).ToList());
        }
    }
}