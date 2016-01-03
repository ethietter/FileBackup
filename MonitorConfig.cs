using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderWatcher {
    class MonitorConfig {

        public List<string> include = new List<string>();
        public HashSet<string> exclude = new HashSet<string>();

        public override string ToString() {
            return "Include=[" + String.Join(",", include) + "], Exclude=[" + String.Join(",", exclude) + "]";
        }
    }
}
