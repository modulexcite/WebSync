using System;
using System.IO;

namespace WebSync
{
    internal abstract class ProjectChangeHandler
    {
        private static readonly BrowserController _browserController = new BrowserController();

        private ProjectChangeHandler _next;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectChangeHandler"/> class.
        /// </summary>
        /// <param name="targetExtension">Extension of the files that this handler expects.</param>
        protected ProjectChangeHandler(string targetExtension)
        {
            TargetExtension = targetExtension;
        }

        public string TargetExtension { get; protected set; }

        internal void SetNext(ProjectChangeHandler next)
        {
            _next = next;
        }

        internal void HandleChange(string objectName, WatcherChangeTypes changeType)
        {
            if (!string.IsNullOrEmpty(TargetExtension) &&
                objectName.EndsWith(objectName, StringComparison.InvariantCultureIgnoreCase))
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