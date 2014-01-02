using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ServiceStack.Text;

namespace WebSync
{
    /// <summary>
    /// Encapsulates communication with the browser through the debugging protocol.
    /// </summary>
    internal class BrowserController
    {
        private const int DefaultPort = 9222;

        private readonly CancellationToken _token;
        private readonly string _hostName;

        internal BrowserController(string hostName = @"http://localhost")
        {
            _hostName = hostName;
            _token = CancellationToken.None;
        }

        /// <summary>
        /// Refreshes browser page.
        /// </summary>
        /// <exception cref="System.ApplicationException">
        /// Indicates that browser has returned an error as a response to the reload request.
        /// </exception>
        internal void Refresh()
        {
            foreach (string debuggerUrl in GetDebuggerUrlsForMatchingTabs())
            {
                Trace.TraceInformation("Trying to establish connection with debug tools on {0}", debuggerUrl);

                try
                {
                    RefreshInternal(debuggerUrl);
                }
                catch (AggregateException ex)
                {
                    Trace.TraceError("Unexpected error occurred while trying to send reload request: {0}",
                                     ex.InnerException);
                }
            }
        }

        private void RefreshInternal(string debuggerUrl)
        {
            if (string.IsNullOrEmpty(debuggerUrl))
                return;

            ClientWebSocket connection = new ClientWebSocket();
            connection.ConnectAsync(new Uri(debuggerUrl), _token).Wait();

            PageReloadRequest request = new PageReloadRequest();
            string rawRequest = request.ToJson();

            var data = new ArraySegment<byte>(Encoding.UTF8.GetBytes(rawRequest));
            connection.SendAsync(data, WebSocketMessageType.Text, true, _token).Wait();
            var buffer = new ArraySegment<byte>(new byte[1024]);
            Task<WebSocketReceiveResult> receiveAsync = connection.ReceiveAsync(buffer, _token);
            receiveAsync.Wait();

            string rawResponse = Encoding.UTF8.GetString(buffer.Array, 0, receiveAsync.Result.Count);
            DebugProtocolResponse response = rawResponse.FromJson<DebugProtocolResponse>();

            if (null != response.Error)
            {
                Trace.TraceWarning("Browser returned an error as a response to reload request: {0}",
                                   response.Error.Message);
            }

            connection.Dispose();
        }

        /// <summary>
        /// Returns list of web socket debugger urls for all browser tabs that have page from the specified
        /// host opened.
        /// </summary>
        private IEnumerable<string> GetDebuggerUrlsForMatchingTabs()
        {
            WebClient client = new WebClient();
            string statusUrl = string.Format(@"http://localhost:{0}/json", DefaultPort);

            string dbgData = client.DownloadString(statusUrl);

            BrowserTabState[] tabState = dbgData.FromJson<BrowserTabState[]>();
            IEnumerable<BrowserTabState> targetTabs = tabState.Where(
                bs => bs != null &&
                      !string.IsNullOrEmpty(bs.Url) &&
                      bs.Url.StartsWith(_hostName));

            foreach (var browserTabState in targetTabs)
            {
                Trace.TraceInformation("Found tab to reload: {0} ({1})",
                                       browserTabState.Title,
                                       browserTabState.Url);
                yield return browserTabState.WebSocketDebuggerUrl;
            }
        }
    }
}