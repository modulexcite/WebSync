using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace WebSync
{
    /// <summary>
    /// Composite pattern implementation for FileSystemWatcher class.
    /// </summary>
    internal class CompositeFileSystemWatcher : IDisposable
    {
        private static readonly object _syncObj = new object();

        private readonly Action<string, WatcherChangeTypes> _callback;

        private List<FileSystemWatcher> _watchers;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeFileSystemWatcher"/> class.
        /// </summary>
        /// <param name="config">
        /// Configuration options for wrapped file system watchers where key is full path to the watch
        /// directory and value is files filter.
        /// </param>
        /// <param name="callback">
        /// The callback to invoke when change occurs.
        /// </param>
        internal CompositeFileSystemWatcher(IDictionary<string, string> config,
                                            Action<string, WatcherChangeTypes> callback)
        {
            _callback = callback;
            _watchers = new List<FileSystemWatcher>(config.Count);

            foreach (KeyValuePair<string, string> pair in config)
            {
                FileSystemWatcher watcher = new FileSystemWatcher(pair.Value, pair.Key);
                watcher.IncludeSubdirectories = true;
                watcher.NotifyFilter = NotifyFilters.LastWrite |
                                       NotifyFilters.FileName |
                                       NotifyFilters.DirectoryName;
                watcher.Created += OnChanged;
                watcher.Changed += OnChanged;
                watcher.Renamed += OnChanged;
                _watchers.Add(watcher);
            }
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        public void Dispose()
        {
            if (null == _watchers)
                return;

            foreach (var watcher in _watchers)
                watcher.Dispose();

            _watchers = null;
        }

        /// <summary>
        /// Begins monitoring file system for changes.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        /// Indicates that object has already been disposed and cannot be used.
        /// </exception>
        internal void BeginMonitoring()
        {
            if (null == _watchers)
                throw new ObjectDisposedException("FileSystemWatcher");

            foreach (var watcher in _watchers)
                watcher.EnableRaisingEvents = true;
        }

        private void OnChanged(object sender, FileSystemEventArgs args)
        {
            if (!Monitor.TryEnter(_syncObj))
                return;

            try
            {
                _callback(args.Name, args.ChangeType);
            }
            finally
            {
                Monitor.Exit(_syncObj);
            }
        }
    }
}