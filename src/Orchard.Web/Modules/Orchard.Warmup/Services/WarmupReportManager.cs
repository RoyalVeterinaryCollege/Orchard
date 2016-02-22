using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Orchard.Environment.Configuration;
using Orchard.FileSystems.AppData;
using Orchard.Warmup.Models;

namespace Orchard.Warmup.Services {
    public class WarmupReportManager : IWarmupReportManager {
        private readonly IAppDataFolder _appDataFolder;
        private const string WarmupReportFilename = "warmup.xml";
        private readonly string _warmupReportPath;
        private XDocument _warmupReport;

        private XDocument WarmupReport {
            get {
                if (_warmupReport == null && _appDataFolder.FileExists(_warmupReportPath)) {
                    var warmupReportContent = _appDataFolder.ReadFile(_warmupReportPath);
                    _warmupReport = XDocument.Parse(warmupReportContent);
                }
                return _warmupReport;
            }
            set { _warmupReport = value; }
        }

        public WarmupReportManager(
            ShellSettings shellSettings,
            IAppDataFolder appDataFolder) {
            _appDataFolder = appDataFolder;

            _warmupReportPath = _appDataFolder.Combine("Sites", _appDataFolder.Combine(shellSettings.Name, WarmupReportFilename));
        }

        public IEnumerable<ReportEntry> Read(int skip = 0, int count = int.MaxValue) {
            if (WarmupReport == null) {
                yield break;
            }
            foreach (var entryNode in WarmupReport.Root.Descendants("ReportEntry").Skip(skip).Take(count)) {
                yield return new ReportEntry {
                    CreatedUtc = XmlConvert.ToDateTime(entryNode.Attribute("CreatedUtc").Value, XmlDateTimeSerializationMode.Utc),
                    Filename = entryNode.Attribute("Filename").Value,
                    RelativeUrl = entryNode.Attribute("RelativeUrl").Value,
                    StatusCode = Int32.Parse(entryNode.Attribute("StatusCode").Value)
                };
            }            
        }

        public int GetReportCount() {
            return WarmupReport == null ? 0 : WarmupReport.Root.Descendants("ReportEntry").Count();
        }

        public void Create(IEnumerable<ReportEntry> reportEntries) {
            var report = new XDocument(new XElement("WarmupReport"));

            foreach (var reportEntry in reportEntries) {
                report.Root.Add(
                    new XElement("ReportEntry",
                        new XAttribute("RelativeUrl", reportEntry.RelativeUrl),
                        new XAttribute("Filename", reportEntry.Filename),
                        new XAttribute("StatusCode", reportEntry.StatusCode),
                        new XAttribute("CreatedUtc", XmlConvert.ToString(reportEntry.CreatedUtc, XmlDateTimeSerializationMode.Utc))
                    )
                );
            }

            _appDataFolder.CreateFile(_warmupReportPath, report.ToString());
            WarmupReport = report;
        }
    }
}