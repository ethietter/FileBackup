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
            string query = "CREATE TABLE IF NOT EXISTS 'files' ('id' INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL, 'path' TEXT, 'size' INTEGER, 'modified' INTEGER)";
            SQLiteCommand cmd = new SQLiteCommand(query, conn);
            cmd.ExecuteNonQuery();

        }

        private void ScanFileSystem() {
            try {
                StreamReader sr = new StreamReader(Config.data_dir + "\\monitor-config.json");
                String contents = sr.ReadToEnd();
                sr.Close();
                MonitorConfig monitor_config = JsonConvert.DeserializeObject<MonitorConfig>(contents);

                Queue<String> dirs = new Queue<String>();
                foreach (var item in monitor_config.include) {
                    //If the item is on the exclude list, ignore it and all subdirectories
                    if (monitor_config.exclude.Contains(item)) {
                        continue;
                    }
                    if (Directory.Exists(item)) {
                        dirs.Enqueue(item);
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
                        if (monitor_config.exclude.Contains(dir)) {
                            continue;
                        }
                        dirs.Enqueue(dir);
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

            catch (Exception e) {
                Console.WriteLine("The config file could not be read");
                Console.WriteLine(e.Message);
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
