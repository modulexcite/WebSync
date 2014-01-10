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
        private const int IdleIntervalMilliseconds = 1000;

        private static CompositeFileSystemWatcher _watcher;

        private static string _workingDirectory;

        private static DateTime _lastReloadTime = DateTime.MinValue;

        private static BrowserController _browserController;

        /// <summary>
        /// Application entry point.
        /// </summary>
        /// <param name="args">Command line parameters.</param>
        public static void Main(string[] args)
        {
            Console.WriteLine("WebSync Utility by Sergey Rybalkin");
            Console.WriteLine("Reloads local website in Google Chrome every time changes are made to it.");

            _workingDirectory = Environment.CurrentDirectory;

            _browserController = new BrowserController();

            SetupDirectoryWatcher();

            Console.WriteLine("Type [r] to refresh manually, [q] to quit...");
            int cmd;
            while ((cmd = Console.Read()) != 'q')
            {
                if ('r' == cmd)
                    OnChanged();
            }

            CleanupResources();
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
            config.Add(Path.Combine(_workingDirectory, @"css"), @"*.css");
            config.Add(Path.Combine(_workingDirectory, @"js"), @"*.js");
            config.Add(Path.Combine(_workingDirectory, @"Views"), @"*.cshtml");

            _watcher = new CompositeFileSystemWatcher(config, OnChanged);
            _watcher.BeginMonitoring();

            Trace.TraceInformation("WebSync started watching for changes in {0}", _workingDirectory);
        }

        private static void OnChanged()
        {
            DateTime now = DateTime.Now;

            if ((now - _lastReloadTime) > TimeSpan.FromMilliseconds(IdleIntervalMilliseconds))
            {
                _lastReloadTime = now;

                Trace.TraceInformation("Refreshing browser due to the change notification.");

                try
                {
                    _browserController.Refresh();

                    Trace.TraceInformation("Browser refresh completed successfully.");
                }
                catch (WebException ex)
                {
                    Trace.TraceError("Could not connect to the browser: {0}", ex);
                }
            }
            else
            {
                Trace.TraceInformation("Skipped browser refresh due to frequency rules.");
            }
        }
    }
}