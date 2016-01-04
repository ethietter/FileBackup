using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using Newtonsoft.Json;

namespace FolderWatcher {
    class Setup {

        List<RowInfo> rows_to_insert = new List<RowInfo>();

        public Setup() {
            InitDB();
            ScanFileSystem();
        }

        private void InitDB() {

            SQLiteConnection conn = DBConnection.GetConn();

            //Create table if it doesn't exist
            string query = "DROP TABLE IF EXISTS `files`";
            SQLiteCommand cmd = new SQLiteCommand(query, conn);
            cmd.ExecuteNonQuery();

            query = "CREATE TABLE IF NOT EXISTS 'files' ('id' INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL, 'path' TEXT, 'size' INTEGER, 'modified' INTEGER)";
            cmd = new SQLiteCommand(query, conn);
            cmd.ExecuteNonQuery();

        }

        private void ScanFileSystem() {
            MonitorConfig monitor_config = Config.GetMonitorConfig();

            Queue<String> dirs = new Queue<String>();
            foreach (var item in monitor_config.include) {
                //If the item is on the exclude list, ignore it and all subdirectories
                if (monitor_config.exclude.Contains(item)) {
                    continue;
                }
                if (Directory.Exists(item)) {
                    dirs.Enqueue(item);
                    DirectoryInfo d_info = new DirectoryInfo(item);
                    AddRow(item, 0, d_info.LastWriteTimeUtc);
                }
                else if (File.Exists(item)) {
                    FileInfo f_info = new FileInfo(item);
                    AddRow(item, f_info.Length, f_info.LastWriteTimeUtc);
                }
                else {
                    //File does not exist, so ignore it
                }
            }

            while (dirs.Count() > 0) {
                string parent_dir = dirs.Dequeue();
                List<string> subdirs = new List<string>(Directory.EnumerateDirectories(parent_dir));
                foreach(string dir in subdirs) {
                    //If the directory is on the exclude list, ignore it and all subdirectories
                    if (!monitor_config.exclude.Contains(dir)) {
                        dirs.Enqueue(dir);
                        DirectoryInfo d_info = new DirectoryInfo(dir);
                        AddRow(dir, 0, d_info.LastWriteTimeUtc);
                    }
                }

                List<string> files = new List<string>(Directory.EnumerateFiles(parent_dir));
                foreach(string file in files) {
                    //If the file is on the exclude list, ignore it
                    if (monitor_config.exclude.Contains(file)) {
                        continue;
                    }
                    FileInfo f_info = new FileInfo(file);
                    AddRow(file, f_info.Length, f_info.LastWriteTimeUtc);
                }
            }

            AddAllRows();
            
        }

        private void AddAllRows() {
            var conn = DBConnection.GetConn();
            using (var cmd = new SQLiteCommand(conn)) {
                double rows_per_transaction = 100000;
                for (int j = 0; j < Math.Ceiling(rows_to_insert.Count() / rows_per_transaction); j++) {
                    using (var transaction = conn.BeginTransaction()) {
                        for (int i = 0; i < rows_per_transaction; i++) {
                            int index = j * (int) rows_per_transaction + i;
                            if (index >= rows_to_insert.Count()) {
                                break;
                            }
                            else {
                                RowInfo curr_row = rows_to_insert[index];
                                cmd.CommandText = "INSERT INTO `files` (`path`, `size`, `modified`) VALUES(@path, @size, @modified)";
                                cmd.Parameters.Add(new SQLiteParameter("@path", curr_row.path));
                                cmd.Parameters.Add(new SQLiteParameter("@size", curr_row.size));
                                cmd.Parameters.Add(new SQLiteParameter("@modified", curr_row.modified));
                                cmd.ExecuteNonQuery();
                            }
                        }
                        transaction.Commit();
                    }
                }
            }
        }

        private void AddRow(string path, long size, DateTime modified) {
            int modified_int = (int)(modified.Subtract(new DateTime(1970, 1, 1)).TotalSeconds);
            rows_to_insert.Add(new RowInfo(path, size, modified_int));
            //Console.WriteLine(path);// + ": Size=" + size + ", modified=" + modified);
        }

        private class RowInfo {

            public string path;
            public long size;
            public int modified;
            public RowInfo(string path, long size, int modified) {
                this.path = path;
                this.size = size;
                this.modified = modified;
            }

        }
    }
}
