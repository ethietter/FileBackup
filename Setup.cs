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

        List<DirRowInfo> dirs_to_insert = new List<DirRowInfo>();
        List<FileRowInfo> files_to_insert = new List<FileRowInfo>();
        int next_dir_id;

        public Setup() {
            InitDB();
            ScanFileSystem();
        }

        private void InitDB() {

            SQLiteConnection conn = DBConnection.GetConn();

            string query = "DROP TABLE IF EXISTS `directories`";
            SQLiteCommand cmd = new SQLiteCommand(query, conn);
            cmd.ExecuteNonQuery();

            query = "DROP TABLE IF EXISTS `files`";
            cmd = new SQLiteCommand(query, conn);
            cmd.ExecuteNonQuery();

            query = "CREATE TABLE `files` (`id` INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL, `dir_id` INTEGER, `name` TEXT, `size` INTEGER, `modified` INTEGER)";
            cmd = new SQLiteCommand(query, conn);
            cmd.ExecuteNonQuery();

            query = "CREATE TABLE `directories` (`id` INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL, `path` TEXT, `modified` INTEGER)";
            cmd = new SQLiteCommand(query, conn);
            cmd.ExecuteNonQuery();

        }

        private void ScanFileSystem() {
            next_dir_id = 1;
            MonitorConfig monitor_config = Config.GetMonitorConfig();

            List<String> dirs = new List<String>();
            foreach (var item in monitor_config.include) {
                //If the item is on the exclude list, ignore it and all subdirectories
                if (monitor_config.exclude.Contains(item)) {
                    continue;
                }
                if (Directory.Exists(item)) {
                    dirs.Add(item);
                }
                else {
                    //Directory does not exist, so ignore it
                }
            }

            foreach(var dir in dirs) {
                AddDirRecursive(dir);
            }

            AddAllRows();
            
        }

        private void AddDirRecursive(string dir_path) {
            MonitorConfig monitor_config = Config.GetMonitorConfig();
            List<string> files = new List<string>(Directory.EnumerateFiles(dir_path));
            int dir_id = SaveDirRow(dir_path);
            foreach(string file in files) {
                //If the file is on the exclude list, ignore it
                if (monitor_config.exclude.Contains(file)) {
                    continue;
                }
                FileInfo f_info = new FileInfo(file);
                SaveFileRow(f_info.FullName, f_info.Name, f_info.Length, f_info.LastAccessTimeUtc, dir_id);
            }

            List<string> subdirs = new List<string>(Directory.EnumerateDirectories(dir_path));
            foreach(string dir in subdirs) {
                if (!monitor_config.exclude.Contains(dir)) {
                    AddDirRecursive(dir);
                }
            }
        }

        private void SaveFileRow(string fullpath, string filename, long length, DateTime modified, int parent_id) {
            FileRowInfo row_info = new FileRowInfo();
            FileInfo f_info = new FileInfo(fullpath);
            row_info.dir_id = parent_id;
            row_info.name = filename;
            row_info.size = length;
            row_info.modified = modified;

            files_to_insert.Add(row_info);
        }

        private int SaveDirRow(string dir_path) {
            DirRowInfo row_info = new DirRowInfo();
            DirectoryInfo d_info = new DirectoryInfo(dir_path);
            row_info.id = next_dir_id;
            row_info.path = dir_path;
            row_info.modified = d_info.LastAccessTimeUtc;

            dirs_to_insert.Add(row_info);

            next_dir_id++;
            return row_info.id;
        }

        private void AddAllRows() {
            var conn = DBConnection.GetConn();
            int rows_per_transaction = 100000;
            for(int j = 0; j < Math.Ceiling(dirs_to_insert.Count()/(double)rows_per_transaction); j++) {
                int start = j * rows_per_transaction;
                int end = start + rows_per_transaction;
                BatchAddDirs(conn, start, end);
            }

            for(int j = 0; j < Math.Ceiling(files_to_insert.Count()/(double)rows_per_transaction); j++) {
                int start = j * rows_per_transaction;
                int end = start + rows_per_transaction;
                BatchAddFiles(conn, start, end);
            }
        }

        /// <summary>
        ///     
        /// </summary>
        /// <param name="conn">SQLiteConnection used to execute these queries</param>
        /// <param name="dir_rows"></param>
        /// <param name="start">Start index, inclusive</param>
        /// <param name="end">End index, exclusive</param>
        private void BatchAddDirs(SQLiteConnection conn, int start, int end) {
            using (var cmd = new SQLiteCommand(conn)) {
                using(var transaction = conn.BeginTransaction()) {
                    for(int i = start; i < end; i++) {
                        if(i >= dirs_to_insert.Count()) {
                            break; //Index out of bounds
                        }
                        DirRowInfo curr_row = dirs_to_insert[i];
                        cmd.CommandText = "INSERT INTO `directories` (`id`, `path`, `modified`) VALUES(@id, @path, @modified)";
                        cmd.Parameters.Add(new SQLiteParameter("@id", curr_row.id));
                        cmd.Parameters.Add(new SQLiteParameter("@path", curr_row.path));
                        cmd.Parameters.Add(new SQLiteParameter("@modified", DateTimeToInt(curr_row.modified)));
                        cmd.ExecuteNonQuery();
                    }
                    transaction.Commit();
                }
            }
        }

        private void BatchAddFiles(SQLiteConnection conn, int start, int end) {
            using (var cmd = new SQLiteCommand(conn)) {
                using (var transaction = conn.BeginTransaction()) {
                    for(int i = start; i < end; i++) {
                        if(i >= files_to_insert.Count()) {
                            break; //Index out of bounds
                        }
                        FileRowInfo curr_row = files_to_insert[i];
                        cmd.CommandText = "INSERT INTO `files` (`dir_id`, `name`, `size`, `modified`) VALUES(@dir_id, @name, @size, @modified)";
                        cmd.Parameters.Add(new SQLiteParameter("@dir_id", curr_row.dir_id));
                        cmd.Parameters.Add(new SQLiteParameter("@name", curr_row.name));
                        cmd.Parameters.Add(new SQLiteParameter("@size", curr_row.size));
                        cmd.Parameters.Add(new SQLiteParameter("@modified", DateTimeToInt(curr_row.modified)));
                        cmd.ExecuteNonQuery();
                        
                    }
                    transaction.Commit();
                }
            }
        }

        private int DateTimeToInt(DateTime dt) {
            return (int)(dt.Subtract(new DateTime(1970, 1, 1)).TotalSeconds);
        }

        private class DirRowInfo {

            public string path;
            public int id;
            public DateTime modified;

        }

        private class FileRowInfo {

            public string name;
            public int dir_id;
            public long size;
            public DateTime modified;

        }

    }
}
