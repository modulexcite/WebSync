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

        private static BrowserController _browserController;

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

            _browserController = new BrowserController();

            SetupDirectoryWatcher();

            Console.WriteLine("Type [r] to refresh manually, [c] to compile, [q] to quit...");
            int cmd;
            while ((cmd = Console.Read()) != 'q')
            {
                if ('r' == cmd)
                    OnChanged(string.Empty, WatcherChangeTypes.All);
                else if ('c' == cmd)
                    RunCompass();
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
            config.Add(Path.Combine(_workingDirectory, @"js"), @"*.js");
            config.Add(Path.Combine(_workingDirectory, @"Views"), @"*.cshtml");

            _watcher = new CompositeFileSystemWatcher(config, OnChanged);
            _watcher.BeginMonitoring();

            Trace.TraceInformation("WebSync started watching for changes in {0}", _workingDirectory);
        }

        private static void OnChanged(string objectName, WatcherChangeTypes watcherChangeTypes)
        {
            DateTime now = DateTime.Now;

            Trace.TraceInformation("Notification of type {0} received for {1}",
                                   watcherChangeTypes,
                                   objectName);

            if (objectName.EndsWith(".scss"))
                RunCompass();

            if ((now - _lastReloadTime) > TimeSpan.FromMilliseconds(IdleIntervalMilliseconds))
            {
                _lastReloadTime = now;

                Trace.TraceInformation("Refreshing browser due to the change notification.");

                try
                {
                    _browserController.Refresh();

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

        private static void RunCompass()
        {
            Trace.TraceInformation("Running compass to compile changes");

            ProcessStartInfo startInfo = new ProcessStartInfo("compass.bat");
            startInfo.Arguments = string.Format("compile \"{0}\" --sourcemap",
                                                _workingDirectory.Replace('\\', '/'));
            startInfo.CreateNoWindow = true;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            Process proc = Process.Start(startInfo);
            if (null != proc)
            {
                proc.WaitForExit();
                Trace.TraceInformation("Compass exited with code {0}", proc.ExitCode);
            }
            else
                Trace.TraceWarning("Compass executable not found");
        }
    }
}