using WebSync.Diagnostics;

namespace WebSync.CommandLine
{
    /// <summary>
    /// Contains information about assembly scanning problem.
    /// </summary>
    public class EmptyAssemblyExceptionArgs : ExceptionArgs
    {
        private readonly string _assemblyName;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmptyAssemblyExceptionArgs"/> class.
        /// </summary>
        /// <param name="assemblyName">Name of the assembly that was scanned for commands.</param>
        public EmptyAssemblyExceptionArgs(string assemblyName)
        {
            _assemblyName = assemblyName;
        }

        /// <summary>
        /// Gets the error message specific to the exception.
        /// </summary>
        public override string Message
        {
            get { return string.Format("Assembly {0} does not contain any commands.", _assemblyName); }
        }
    }
}