using System;
using System.Data.SQLite;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderWatcher {
    class DBConnection {

        private static SQLiteConnection conn;
        private static bool is_connected = false;
        public static void Connect() {
            string db_path = Config.data_dir + "\\data.sqlite";
            if (!File.Exists(db_path)) {
                SQLiteConnection.CreateFile(db_path);
            }
            conn = new SQLiteConnection("Data Source=" + db_path + "; Version=3");
            is_connected = true;
            conn.Open();
        }

        public static SQLiteConnection GetConn() {
            if (!is_connected) {
                Connect();
            }
            return conn;
        }

        public static void Close() {
            if (is_connected) {
                conn.Dispose();
            }
        }
    }
}
