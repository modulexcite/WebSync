using System.Diagnostics;
using System.IO;

namespace WebSync
{
    internal class SassChangeHandler : ProjectChangeHandler
    {
        private readonly string _workingDirectory;

        /// <summary>
        /// Initializes a new instance of the <see cref="SassChangeHandler"/> class.
        /// </summary>
        /// <param name="workingDirectory">
        /// Full path to the directory where config.rb file is located.
        /// </param>
        internal SassChangeHandler(string workingDirectory) : base("*.scss")
        {
            _workingDirectory = workingDirectory;
        }

        protected override bool HandleChangeInternal(string objectName, WatcherChangeTypes changeType)
        {
            Trace.TraceInformation("Running compass to compile changes");

            ProcessStartInfo startInfo = new ProcessStartInfo("compass.bat");
            startInfo.Arguments = string.Format("compile \"{0}\" --sourcemap",
                                                _workingDirectory.Replace('\\', '/'));
            startInfo.CreateNoWindow = true;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            Process proc = Process.Start(startInfo);
            if (null != proc)
            {
                proc.WaitForExit();
                Trace.TraceInformation("Compass exited with code {0}", proc.ExitCode);
            }
            else
                Trace.TraceWarning("Compass executable not found");

            RefreshBrowser();

            return true;
        }
    }
}