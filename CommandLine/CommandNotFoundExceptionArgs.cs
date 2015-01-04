using WebSync.Diagnostics;

namespace WebSync.CommandLine
{
    /// <summary>
    /// Contains information about command lookup problem.
    /// </summary>
    public class CommandNotFoundExceptionArgs : ExceptionArgs
    {
        private readonly string _command;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandNotFoundExceptionArgs" /> class.
        /// </summary>
        /// <param name="command">The missing command name.</param>
        public CommandNotFoundExceptionArgs(string command)
        {
            _command = command;
        }

        /// <summary>
        /// Gets the error message specific to the exception.
        /// </summary>
        public override string Message
        {
            get { return string.Format("Specified assembly does not contain command {0}.", _command); }
        }
    }
}