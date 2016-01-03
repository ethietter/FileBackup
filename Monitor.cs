using System;
using System.IO;
using System.Data.SQLite;

public class Monitor
{
    private string path;
    private FileSystemWatcher fs_watcher;

	public Monitor(string path)
	{
        this.path = path;
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

    }

    private void OnRenamed(object send, RenamedEventArgs args) {

    }
}
