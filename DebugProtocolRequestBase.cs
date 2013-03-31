namespace WebSync
{
    /// <summary>
    /// Base class for all browser debugging protocol requests.
    /// </summary>
    internal abstract class DebugProtocolRequestBase
    {
        internal DebugProtocolRequestBase()
        {
            id = 1;
        }

        public int id { get; set; }

        public string method { get; set; }
    }
}