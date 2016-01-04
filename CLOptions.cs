using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;

namespace FolderWatcher {
    class CLOptions {

        [Option('c', "config", DefaultValue = false, HelpText = "Sets the program in config mode")]
        public bool IsConfig { get; set; }

        [Option('d', "daemon", DefaultValue = false, HelpText = "Runs the program as a daemon. Must already have been configured")]
        public bool IsDaemon { get; set; }

        [HelpOption]
        public string GetUsage() {
            return HelpText.AutoBuild(this, (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }
}
