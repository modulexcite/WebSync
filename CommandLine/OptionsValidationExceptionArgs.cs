using WebSync.Diagnostics;

namespace WebSync.CommandLine
{
    /// <summary>
    /// Contains information about command line options parsing problem.
    /// </summary>
    public class OptionsValidationExceptionArgs : ExceptionArgs
    {
        private readonly string _errorMessage;

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionsValidationExceptionArgs"/> class.
        /// </summary>
        /// <param name="errorMessage">Error details.</param>
        public OptionsValidationExceptionArgs(string errorMessage)
        {
            _errorMessage = errorMessage;
        }

        /// <summary>
        /// Gets the error message specific to the exception.
        /// </summary>
        public override string Message
        {
            get { return _errorMessage; }
        }
    }
}