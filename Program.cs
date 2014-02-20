using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;

namespace WebSync
{
    /// <summary>
    /// Contains application entry point.
    /// </summary>
    /// <remarks>
    /// In order for this application to work correctly you need to have Google Chrome launched with
    /// --remote-debugging-port=9222 command line option and start this application from the root of your
    /// website folder.
    /// </remarks>
    public class Program
    {
        // Amount of time to skip monitoring after event fired
        private const int IdleIntervalMilliseconds = 300;

        private static CompositeFileSystemWatcher _watcher;

        private static string _workingDirectory;

        private static DateTime _lastReloadTime = DateTime.MinValue;

        private static ProjectChangeHandler _chain;

        /// <summary>
        /// Application entry point.
        /// </summary>
        /// <param name="args">Command line parameters.</param>
        public static void Main(string[] args)
        {
            Console.WriteLine("WebSync Utility by Sergey Rybalkin");
            Console.WriteLine("Reloads local website in Google Chrome every time changes are made to it.");

            if (args.Length == 0)
                _workingDirectory = ScanFolders(Environment.CurrentDirectory) ?? Environment.CurrentDirectory;
            else
                _workingDirectory = args[0];

            SetupDirectoryWatcher();

            TypeScriptChangeHandler tsHandler = new TypeScriptChangeHandler(
                Path.Combine(_workingDirectory, "Web.csproj"));
            SassChangeHandler sassHandler = new SassChangeHandler(_workingDirectory);
            tsHandler.SetNext(sassHandler);
            DefaultChangeHandler defaultHandler = new DefaultChangeHandler();
            sassHandler.SetNext(defaultHandler);

            _chain = tsHandler;

            Console.WriteLine("Type [r] to refresh manually, [q] to quit...");
            int cmd;
            while ((cmd = Console.Read()) != 'q')
            {
                if ('r' == cmd)
                    OnChanged(string.Empty, WatcherChangeTypes.All);
            }

            CleanupResources();
        }

        private static string ScanFolders(string start)
        {
            if (File.Exists(Path.Combine(start, "web.config")))
                return start;

            if (File.Exists(Path.Combine(start, "Web", "web.config")))
                return Path.Combine(start, "Web");

            var parent = Directory.GetParent(start);
            if (null == parent)
                return null;

            return ScanFolders(parent.FullName);
        }

        private static void CleanupResources()
        {
            if (null != _watcher)
            {
                _watcher.Dispose();
                _watcher = null;
            }
        }

        private static void SetupDirectoryWatcher()
        {
            Dictionary<string, string> config = new Dictionary<string, string>();
            config.Add(Path.Combine(_workingDirectory, @"css"), @"*.scss");
            config.Add(Path.Combine(_workingDirectory, @"js"), @"*.js,*.ts");
            config.Add(Path.Combine(_workingDirectory, @"Views"), @"*.cshtml");

            _watcher = new CompositeFileSystemWatcher(config, OnChanged);
            _watcher.BeginMonitoring();

            Trace.TraceInformation("WebSync started watching for changes in {0}", _workingDirectory);
        }

        private static void OnChanged(string objectName, WatcherChangeTypes changeType)
        {
            DateTime now = DateTime.Now;

            Trace.TraceInformation("Notification of type {0} received for {1}",
                                   changeType,
                                   objectName);

            if ((now - _lastReloadTime) > TimeSpan.FromMilliseconds(IdleIntervalMilliseconds))
            {
                _lastReloadTime = now;

                Trace.TraceInformation("Refreshing browser due to the change notification.");

                try
                {
                    _chain.HandleChange(objectName, changeType);

                    Trace.TraceInformation("Browser refresh completed successfully.\n");
                }
                catch (WebException ex)
                {
                    Trace.TraceError("Could not connect to the browser: {0}\n", ex);
                }
            }
            else
                Trace.TraceInformation("Skipped browser refresh due to frequency rules.\n");
        }
    }
}