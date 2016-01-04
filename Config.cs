using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderWatcher {
    class Config {

        public static string data_dir = "C:\\Users\\edwardhietter\\Documents\\Dev\\FileBackup\\FolderWatcher\\FileBackup\\data";
        public static MonitorConfig monitor_config = null;

        public static MonitorConfig GetMonitorConfig() {
            if(monitor_config == null) {
                StreamReader sr = new StreamReader(data_dir + "\\monitor-config.json");
                string contents = sr.ReadToEnd();
                sr.Close();
                monitor_config = JsonConvert.DeserializeObject<MonitorConfig>(contents);
            }
            return monitor_config;
        }
    }
}
