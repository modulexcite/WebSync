﻿namespace WebSync
{
    /// <summary>
    /// Stores debug information about single browser tab.
    /// </summary>
    internal class BrowserTabState
    {
        public string DevToolsFrontendUrl { get; set; }

        public string FaviconUrl { get; set; }

        public string Id { get; set; }

        public string ThumbnailUrl { get; set; }

        public string Title { get; set; }

        public string Type { get; set; }

        public string Url { get; set; }

        public string WebSocketDebuggerUrl { get; set; }
    }
}