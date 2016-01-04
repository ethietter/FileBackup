using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using CommandLine;

namespace FolderWatcher {
    class Program {

        private SQLiteConnection db_conn;

        static void Main(string[] args) {
            CLOptions opts = new CLOptions();
            if (Parser.Default.ParseArguments(args, opts)) {
                if ((opts.IsConfig && opts.IsDaemon) || (!opts.IsConfig && !opts.IsDaemon)){
                    Console.WriteLine(opts.GetUsage());
                    Environment.Exit(1);
                }
                else {
                    Program program = new Program(opts.IsDaemon);
                }
            }
        }

        public Program(bool IsDaemon) {
            if (IsDaemon) {
                Daemon daemon = new Daemon();
            }
            else {
                Setup setup = new Setup();
                Console.WriteLine("Finished");
            }
            while (true) ;
        }
        
    }
}
