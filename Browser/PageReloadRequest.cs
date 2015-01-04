namespace WebSync.Browser
{
    /// <summary>
    /// Contains information required to send a Page.reload request.
    /// </summary>
    internal class PageReloadRequest : DebugProtocolRequestBase
    {
        public PageReloadRequest()
        {
            method = "Page.reload";
            Params = new PageReloadParams();
        }

        public PageReloadParams Params { get; set; }

        internal class PageReloadParams
        {
            internal PageReloadParams()
            {
                IgnoreCache = true;
            }

            public bool IgnoreCache { get; set; }
        }
    }
}