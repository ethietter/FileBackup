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
                    addToTable(item, 0, d_info.LastWriteTimeUtc);
                }
                else if (File.Exists(item)) {
                    FileInfo f_info = new FileInfo(item);
                    addToTable(item, f_info.Length, f_info.LastWriteTimeUtc);
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
                        addToTable(dir, 0, d_info.LastWriteTimeUtc);
                    }
                }

                List<string> files = new List<string>(Directory.EnumerateFiles(parent_dir));
                foreach(string file in files) {
                    //If the file is on the exclude list, ignore it
                    if (monitor_config.exclude.Contains(file)) {
                        continue;
                    }
                    FileInfo f_info = new FileInfo(file);
                    addToTable(file, f_info.Length, f_info.LastWriteTimeUtc);
                }
            }
        }

        private void addToTable(string path, Int64 size, DateTime modified) {
            int modified_int = (Int32)(modified.Subtract(new DateTime(1970, 1, 1)).TotalSeconds);
            string query = "INSERT INTO `files` (`path`, `size`, `modified`) VALUES(@path, @size, @modified)";
            SQLiteCommand cmd = new SQLiteCommand(query, DBConnection.GetConn());
            cmd.Parameters.Add(new SQLiteParameter("@path", path));
            cmd.Parameters.Add(new SQLiteParameter("@size", size));
            cmd.Parameters.Add(new SQLiteParameter("@modified", modified_int));
            cmd.ExecuteNonQuery();
            //Console.WriteLine(path);// + ": Size=" + size + ", modified=" + modified);
        }
    }
}
