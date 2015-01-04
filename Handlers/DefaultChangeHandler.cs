using System.Diagnostics;
using System.IO;

namespace WebSync.Handlers
{
    internal class DefaultChangeHandler : ProjectChangeHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultChangeHandler"/> class.
        /// </summary>
        public DefaultChangeHandler() : base(string.Empty)
        {
        }

        protected override bool HandleChangeInternal(string objectName, WatcherChangeTypes changeType)
        {
            Trace.TraceInformation("Refreshing browser due to the change notification.");

            RefreshBrowser();

            return true;
        }
    }
}