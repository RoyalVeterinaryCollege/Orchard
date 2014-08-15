using System.Collections.Generic;
using Orchard.Warmup.Models;

namespace Orchard.Warmup.Services {
    public interface IWarmupReportManager : IDependency {
        IEnumerable<ReportEntry> Read(int skip = 0, int count = int.MaxValue);
        int GetReportCount();
        void Create(IEnumerable<ReportEntry> reportEntries);
    }
}