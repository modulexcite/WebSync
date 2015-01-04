using System;
using System.Diagnostics;
using System.Reflection;
using WebSync.CommandLine;
using WebSync.Commands;
using WebSync.Diagnostics;

namespace WebSync
{
    /// <summary>
    /// Contains application entry point.
    /// </summary>
    /// <remarks>
    /// In order for this application to work correctly you need to have Google Chrome launched with
    /// --remote-debugging-port=9222 command line option and start this application from the root of your
    /// website folder or pass it as the first argument.
    /// </remarks>
    public class Program
    {
        /// <summary>
        /// Application entry point.
        /// </summary>
        /// <param name="args">Command line parameters.</param>
        public static void Main(string[] args)
        {
            Assembly thisAssembly = Assembly.GetExecutingAssembly();
            WriteWelcomeMessage(thisAssembly);

            CommandLineParser.HandleDebugArguments(ref args);
            var parser = CommandLineParser.FromAssembly<WatchCommand>(thisAssembly);

            if (args.Length == 0 ||
                (args.Length == 1 && args[0].EndsWith("help", StringComparison.OrdinalIgnoreCase)))
            {
                Console.WriteLine(parser.GetHelp());
                return;
            }

            if (args[0].EndsWith("help", StringComparison.OrdinalIgnoreCase))
            {
                GetCommandHelp(parser, args[1]);
                return;
            }

            WatchCommand cmd = null;
            try
            {
                cmd = parser.Parse<WatchCommand>(args);
                cmd.StartWatching();

                Console.WriteLine("Type [r] to refresh manually, [q] to quit...");
                int command;
                while ((command = Console.Read()) != 'q')
                {
                    if ('r' == command)
                        cmd.ForceRefresh();
                }
            }
            catch (TargetInvocationException ex)
            {
                Trace.TraceError("Unexpected error occured: {0}", ex.InnerException.Message);
            }
            catch (Exception ex)
            {
                Trace.TraceError("Unexpected error occured: {0}", ex.Message);
            }
            finally
            {
                if (null != cmd)
                    cmd.StopWatching();
            }
        }

        private static void GetCommandHelp(CommandLineParser parser, string command)
        {
            try
            {
                Console.WriteLine(parser.GetHelp(command));
            }
            catch (Exception<CommandNotFoundExceptionArgs>)
            {
                Trace.TraceError("Command {0} is not supported.", command);
                Console.WriteLine(parser.GetHelp());
            }
        }

        private static void WriteWelcomeMessage(Assembly asm)
        {
            Console.WriteLine();
            Console.WriteLine("{0}. v{1}", asm.GetTitle(), asm.GetVersion());
            Console.WriteLine(asm.GetDescription());
            Console.WriteLine();
        }
    }
}