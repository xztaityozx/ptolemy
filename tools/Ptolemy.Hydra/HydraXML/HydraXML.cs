using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Serialization;

namespace Ptolemy.Hydra.HydraXML {
    public class HydraXML {
        private const string XMLmonteCarlo = "MONTE_CARLO";
        private const string XMLanalysisName = "analysisName";
        private const string XMLfilename = "filename";
        private const string XMLformat = "format";
        private const string XMLgiString = "giString";
        private const string XMLname = "name";
        private const string XMLparams = "params";
        private const string XMLpsf = "psf";
        private const string XMLresultFiles = "resultFiles";
        private const string XMLresultType = "resultType";
        private const string XMLsaPair = "saPair";
        private const string XMLsaResultFile = "saResultFile";
        private const string XMLsaResults = "saResults";
        private const string XMLsaSweepFile = "saSweepFile";
        private const string XMLstatistical = "statistical";
        private const string XMLsweepFiles = "sweepFiles";
        private const string XMLtran = "tran";
        private const string XMLvalue = "value";


        [XmlRoot("attribute")]
        public class Attribute {
            [XmlAttribute("type")] public string Type { get; set; }
            [XmlAttribute("value")] public string Value { get; set; }
            [XmlAttribute("name")] public string Name { get; set; }
        }

        [XmlRoot("collection")]
        public class Collection {
            [XmlAttribute("name")] public string Name { get; set; }
            [XmlElement("object")] public Object[] Objects { get; set; }
        }

        [XmlRoot("file-format")]
        public class HydraXmlRoot {
            [XmlAttribute("name")] public string Name { get; set; }
            [XmlAttribute("version")] public string Version { get; set; }
            [XmlElement("object")] public Object[] Objects { get; set; }
        }

        [XmlRoot("object")]
        public class Object {
            [XmlAttribute("version")] public string Version { get; set; }
            [XmlAttribute("type")] public string Type { get; set; }
            [XmlAttribute("name")] public string Name { get; set; }
            [XmlElement("attribute")] public Attribute[] Attributes { get; set; }
            [XmlElement("collection")] public Collection[] Collections { get; set; }
        }

        public static HydraXmlRoot GenerateResultsXml(string netListDir, int sweeps) {
            return new HydraXmlRoot {
                Version = "1.0",
                Name = XMLsaResults,
                Objects = new[] {
                    new Object {
                        Version = "1",
                        Type = XMLsaResults,
                        Name = XMLsaResults,
                        Attributes = new[] {
                            new Attribute{Type=XMLgiString, Value = "resultsMap.xml", Name = "name"},
                            new Attribute{Type=XMLgiString, Value = netListDir, Name = "netlistDir"},
                            new Attribute{Type=XMLgiString, Value = ".", Name = "resultsDir"},
                            new Attribute{Type=XMLgiString, Value = $"{DateTime.Now}", Name = "runTime"},
                            new Attribute{Type=XMLgiString, Value = "HSPICE", Name = "simulator"},
                            new Attribute{Type=XMLgiString, Value = "", Name = "version"}
                        },
                        Collections = new[] {
                            new Collection {
                                Name=XMLresultFiles,
                                Objects = new[] {
                                    new Object {
                                        Version = "1",
                                        Type = XMLsaResultFile,
                                        Name = XMLresultFiles,
                                        Attributes = new[] {
                                            new Attribute{Type = XMLgiString, Value = XMLtran, Name = XMLanalysisName},
                                            new Attribute{Type = XMLgiString, Value = "", Name = XMLfilename},
                                            new Attribute{Type = XMLgiString, Value = XMLpsf, Name = XMLformat},
                                            new Attribute{Type = XMLgiString, Value = string.Join(" ",Enumerable.Range(1, sweeps).Select(d=>$"{d}.0")), Name = "iterations"},
                                            new Attribute{Type = XMLgiString, Value = XMLtran, Name = XMLresultType}
                                        },
                                        Collections = new[] {
                                            new Collection {
                                                Name = XMLsweepFiles,
                                                Objects = Enumerable.Range(1, sweeps)
                                                    .Select(d => new Object {
                                                        Version = "1",
                                                        Type = XMLsaSweepFile,
                                                        Name = XMLsweepFiles,
                                                        Attributes = new[]{new Attribute{Type = XMLgiString, Value = $"hspice.tr0@{d}", Name = XMLfilename}},
                                                        Collections = new[] {
                                                            new Collection {
                                                                Name = XMLparams,
                                                                Objects = new[] {
                                                                    new Object {
                                                                        Version = "1",
                                                                        Type = XMLsaPair,
                                                                        Name = XMLparams,
                                                                        Attributes = new[] {
                                                                            new Attribute{Type = XMLgiString, Value = XMLmonteCarlo, Name = XMLname},
                                                                            new Attribute{Type = XMLgiString, Value = $"{d}.0", Name = XMLvalue},
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }).ToArray()
                                            } 
                                        }
                                    },
                                    new Object {
                                        Version = "1",
                                        Type = XMLsaResultFile,
                                        Name = XMLresultFiles,
                                        Attributes = new[] {
                                            new Attribute{Type = XMLgiString, Value = XMLstatistical, Name = XMLanalysisName},
                                            new Attribute{Type = XMLgiString, Value = "hspice.mc0", Name = XMLfilename},
                                            new Attribute{Type = XMLgiString, Value = XMLpsf, Name = XMLformat},
                                            new Attribute{Type = XMLgiString, Value = XMLstatistical, Name = XMLresultType}
                                        }
                                    },
                                    new Object {
                                        Version = "1",
                                        Type = XMLsaResultFile,
                                        Name = XMLresultFiles,
                                        Attributes = new[] {
                                            new Attribute{Type = XMLgiString, Value = "scalarData", Name = XMLanalysisName},
                                            new Attribute{Type = XMLgiString, Value = "scalar.dat", Name = XMLfilename},
                                            new Attribute{Type = XMLgiString, Value = "table", Name = XMLformat},
                                            new Attribute{Type = XMLgiString, Value = XMLstatistical, Name = XMLresultType}
                                        }
                                    },
                                    new Object {
                                        Version = "1",
                                        Type = XMLsaResultFile,
                                        Name = XMLresultFiles,
                                        Attributes = new[] {
                                            new Attribute{Type = XMLgiString, Value = "", Name = XMLanalysisName},
                                            new Attribute{Type = XMLgiString, Value = "designVariables.wdf", Name = XMLfilename},
                                            new Attribute{Type = XMLgiString, Value = "wdf", Name = XMLformat},
                                            new Attribute{Type = XMLgiString, Value = "variables", Name = XMLresultType}
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        public static HydraXmlRoot GenerateResultsMapXml(string netListDir) {
            const string saResultsMap = "saResultsMap";
            return new HydraXmlRoot {
                Version = "1",
                Name = saResultsMap,
                Objects = new[] {
                    new Object {
                        Version = "1",
                        Type = saResultsMap,
                        Name = saResultsMap,
                        Attributes = new[] {
                            new Attribute {Type = XMLgiString, Value = ".", Name = "masterResultsDir"},
                            new Attribute {Type = XMLgiString, Value = XMLmonteCarlo, Name = "monteCarlo"},
                            new Attribute {Type = XMLgiString, Value = "resultsMap.xml", Name = XMLname},
                            new Attribute {Type = XMLgiString, Value = ".", Name = "resultsMapDir"},
                            new Attribute {Type = XMLgiString, Value = "HSPICE", Name = "simulator"},
                            new Attribute {Type = XMLgiString, Value = $"{DateTime.Now}", Name = "timeStamp"}
                        },
                        Collections = new[] {
                            new Collection {
                                Name = "resultsInfo",
                                Objects = new[] {
                                    new Object {
                                        Version = "1", Type = "saResultsInfo", Name = "resultsInfo",
                                        Attributes = new[] {
                                            new Attribute {Type = XMLgiString, Value = netListDir, Name = "netlistDir"},
                                            new Attribute {Type = XMLgiString, Value = ".", Name = "resultsDir"}
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }
    }
}