using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;

namespace FolderWatcher {
    class Program {

        private SQLiteConnection db_conn;

        static void Main(string[] args) {
            Program program = new Program();
        }

        public Program() {
            Setup setup = new Setup();
            while (true) ;
        }
        
    }
}
