using Orchard.Workflows.Models;

namespace Orchard.Workflows.Helpers {
    public static class ActivityRecordExtensions {
        public static string ClientId(this ActivityRecord record) {
            return record.Name + "_" + record.Id;
        }
    }
}