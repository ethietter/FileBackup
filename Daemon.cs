using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace FolderWatcher {
    class Daemon {

        public MonitorConfig monitor_config;
        private HashSet<string> event_set;

        public Daemon() {
            event_set = new HashSet<string>();

            monitor_config = Config.GetMonitorConfig();
            foreach(var dir in monitor_config.include) {
                Monitor m = new Monitor(dir, RaiseDirEvent);
            }
            Console.WriteLine(monitor_config);
        }

        private void RaiseDirEvent(string dir) {
            Console.WriteLine(dir);
            event_set.Add(dir);
        }
    }
}