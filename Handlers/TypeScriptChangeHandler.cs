using System;
using System.Diagnostics;
using System.IO;

namespace WebSync.Handlers
{
    internal class TypeScriptChangeHandler : ProjectChangeHandler
    {
         private readonly string _projectFilePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeScriptChangeHandler"/> class.
        /// </summary>
        /// <param name="projectFilePath">
        /// Full path to the visual studio project file that contains type script files.
        /// </param>
        internal TypeScriptChangeHandler(string projectFilePath) : base(".ts")
        {
            _projectFilePath = projectFilePath;
        }

        protected override bool HandleChangeInternal(string objectName, WatcherChangeTypes changeType)
        {
            Trace.TraceInformation("Running MSBuild to compile type script files");

            ProcessStartInfo startInfo = new ProcessStartInfo("msbuild.exe");
            startInfo.Arguments =
                string.Format("\"{0}\" /p:Configuration=Debug,BuildingProject=true /t:CompileTypeScript",
                              _projectFilePath);
            startInfo.CreateNoWindow = true;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            Process proc = Process.Start(startInfo);
            if (null != proc)
            {
                proc.WaitForExit();
                Trace.TraceInformation("MSBuild exited with code {0}", proc.ExitCode);
                if (proc.ExitCode != 0)
                    Console.Beep();
            }
            else
                Trace.TraceWarning("MSBuild executable not found");

            RefreshBrowser();

            return true;
        }
    }
}