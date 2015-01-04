using System;
using System.IO;
using WebSync.Browser;

namespace WebSync.Handlers
{
    internal abstract class ProjectChangeHandler
    {
        private static BrowserController _browserController;

        private ProjectChangeHandler _next;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectChangeHandler"/> class.
        /// </summary>
        /// <param name="targetExtension">Extension of the files that this handler expects.</param>
        protected ProjectChangeHandler(string targetExtension)
        {
            TargetExtension = targetExtension;
        }

        internal string TargetExtension { get; set; }

        internal static void Initialize(string domain)
        {
            if (!domain.StartsWith("http"))
                domain = "http://" + domain;

            _browserController = new BrowserController(domain);
        }

        internal void SetNext(ProjectChangeHandler next)
        {
            _next = next;
        }

        internal void HandleChange(string objectName, WatcherChangeTypes changeType)
        {
            if (string.IsNullOrEmpty(TargetExtension) ||
                objectName.EndsWith(TargetExtension, StringComparison.InvariantCultureIgnoreCase))
            {
                bool handled = HandleChangeInternal(objectName, changeType);
                if (handled)
                    return;
            }

            if (null != _next)
                _next.HandleChange(objectName, changeType);
        }

        protected void RefreshBrowser()
        {
            _browserController.Refresh();
        }

        protected abstract bool HandleChangeInternal(string objectName, WatcherChangeTypes changeType);
    }
}