using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using WebSync.CommandLine;
using WebSync.Diagnostics;
using WebSync.Handlers;

namespace WebSync.Commands
{
    /// <summary>
    /// Command that monitors for changes in the project directory and reloads browser tabs with the
    /// specified domain name.
    /// </summary>
    [Command("watch", "Watches for changes in the specified directory and reloads browser tab.")]
    public class WatchCommand
    {
        private readonly string _dir;

        private readonly string _domain;

        private readonly bool _sass;

        private readonly bool _typescript;

        private readonly uint _idle;

        private CompositeFileSystemWatcher _watcher;

        private DateTime _lastReloadTime = DateTime.MinValue;

        private ProjectChangeHandler _chain;

        /// <summary>
        /// Initializes a new instance of the <see cref="WatchCommand" /> class.
        /// </summary>
        /// <param name="dir">Root directory of the project to monitor.</param>
        /// <param name="domain">Browser tabs domain to refresh.</param>
        /// <param name="sass">Enable instant SASS compilation.</param>
        /// <param name="typescript">Enable instant TypeScript compilation.</param>
        /// <param name="idle">Amout of milliseconds between tab refreshes.</param>
        /// <exception cref="Exception{OptionsValidationExceptionArgs}">
        /// Indicates that specified arguments are invalid.
        /// </exception>
        public WatchCommand([Option("Root directory of the project to monitor")]string dir = null,
                            [Option("Browser tabs domain to refresh")]string domain = "localhost",
                            [Option("Enable instant SASS compilation")]bool sass = true,
                            [Option("Enable instant TypeScript compilation")]bool typescript = true,
                            [Option("Amount of milliseconds between tab refreshes")]uint idle = 300)
        {
            _dir = Path.GetFullPath(dir ?? ".");
            _domain = domain;
            _sass = sass;
            _typescript = typescript;
            _idle = idle;

            if (!Directory.Exists(_dir))
            {
                var args = new OptionsValidationExceptionArgs("Specified project directory does not exist.");
                throw new Exception<OptionsValidationExceptionArgs>(args);
            }
        }

        /// <summary>
        /// Starts watching project directory for changes.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Indicates that project directory contains no resource supported for watching.
        /// </exception>
        public void StartWatching()
        {
            ProjectChangeHandler.Initialize(_domain);

            Dictionary<string, string> config = new Dictionary<string, string>();
            string binaries = Path.Combine(_dir, @"bin");
            if (Directory.Exists(binaries))
                config.Add("*.dll", binaries);

            string styles = Path.Combine(_dir, @"css");
            if (Directory.Exists(styles))
                config.Add(@"*.scss", styles);

            string scripts = Path.Combine(_dir, @"js");
            if (Directory.Exists(scripts))
            {
                config.Add(@"*.js", scripts);
                config.Add(@"*.ts", scripts);
            }

            string views = Path.Combine(_dir, @"Views");
            if (Directory.Exists(views))
                config.Add(@"*.cshtml", views);

            if (config.Count == 0)
                throw new InvalidOperationException(
                    "No resource supported for watching found in project directory.");

            _watcher = new CompositeFileSystemWatcher(config, OnChanged);
            _watcher.BeginMonitoring();

            Trace.TraceInformation("WebSync started watching for changes in {0}", _dir);

            // Using chain of responsibility pattern to manage change notification handlers.
            ProjectChangeHandler lastHandler = null;

            if (_typescript)
            {
                _chain = new TypeScriptChangeHandler(Path.Combine(_dir, "Web.csproj"));
                lastHandler = _chain;
            }

            if (_sass)
            {
                var sassHandler = new SassChangeHandler(_dir);
                if (null == _chain)
                    _chain = sassHandler;
                else
                {
                    lastHandler.SetNext(sassHandler);
                    lastHandler = sassHandler;
                }
            }

            DefaultChangeHandler defaultHandler = new DefaultChangeHandler();
            if (null == _chain)
                _chain = defaultHandler;
            else
                lastHandler.SetNext(defaultHandler);
        }

        public void ForceRefresh()
        {
            OnChanged(string.Empty, WatcherChangeTypes.All);
        }

        public void StopWatching()
        {
            if (null != _watcher)
            {
                _watcher.Dispose();
                _watcher = null;
            }
        }

        private void OnChanged(string objectName, WatcherChangeTypes changeType)
        {
            DateTime now = DateTime.Now;

            Trace.TraceInformation("Notification of type {0} received for {1}",
                                   changeType,
                                   objectName);

            if ((now - _lastReloadTime) <= TimeSpan.FromMilliseconds(_idle))
                return;

            _lastReloadTime = now;

            try
            {
                _chain.HandleChange(objectName, changeType);
            }
            catch (WebException ex)
            {
                Trace.TraceError("Browser refresh failed: {0}\n", ex.Message);
            }
        }
    }
}