namespace WebSync
{
    /// <summary>
    /// Represents response object that is received from the debugged browser.
    /// </summary>
    internal class DebugProtocolResponse
    {
        public int Id { get; set; }

        public ErrorInfo Error { get; set; }

        internal class ErrorInfo
        {
            public int Code { get; set; }

            public string Message { get; set; }
        }
    }
}