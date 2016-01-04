using System;
using System.IO;
using System.Data.SQLite;

public class Monitor
{
    private string path;
    private FileSystemWatcher fs_watcher;
    private Action<string> RaiseDirEvent;

	public Monitor(string path, Action<string> RaiseDirEvent)
	{
        this.path = path;
        this.RaiseDirEvent = RaiseDirEvent;
        WatchDir();
	}

    private void WatchDir() {
        try {
            fs_watcher = new FileSystemWatcher(path);
            fs_watcher.Filter = "*.*"; //All files in dir
            fs_watcher.EnableRaisingEvents = true;
            fs_watcher.IncludeSubdirectories = true;
            fs_watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;

            fs_watcher.Changed += new FileSystemEventHandler(OnChanged);
            fs_watcher.Created += new FileSystemEventHandler(OnChanged);
            fs_watcher.Deleted += new FileSystemEventHandler(OnChanged);
            fs_watcher.Renamed += new RenamedEventHandler(OnRenamed);
        }
        catch (ArgumentException e) {
            Console.WriteLine("Path does not exist: " + path);
        }
        catch (PathTooLongException e) {
            Console.WriteLine("Path is too long: " + path);
        }
    }

    private void OnChanged(object source, FileSystemEventArgs args) {
        string dir = GetAffectedDirectory(args.FullPath);
        if(dir != "") {
            RaiseDirEvent(dir);
        }
    }

    private void OnRenamed(object send, RenamedEventArgs args) {
        string dir = GetAffectedDirectory(args.FullPath);
        if (dir != "") {
            RaiseDirEvent(dir);
        }
    }

    //Returns "" if the path no longer refers to a file
    private string GetAffectedDirectory(string full_path) {
        if (Directory.Exists(full_path)) {
            return full_path;
        }
        else {
            //It's a file, so find its parent directory
            try {
                string dir = new FileInfo(full_path).DirectoryName;
                return dir;
            }
            catch (Exception e) {
                //File doesn't exist anymore, probably due to a race condition somewhere, so just ignore.
            }
        }
        return "";
    }
}
