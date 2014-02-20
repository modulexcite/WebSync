using System.Diagnostics;
using System.IO;
using System.Net;

namespace WebSync
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

            try
            {
                RefreshBrowser();

                Trace.TraceInformation("Browser refresh completed successfully.\n");
            }
            catch (WebException ex)
            {
                Trace.TraceError("Could not connect to the browser: {0}\n", ex);
            }

            return true;
        }
    }
}