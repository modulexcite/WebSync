using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using WebSync.Diagnostics;

namespace WebSync.CommandLine
{
    /// <summary>
    /// Custom command line arguments parser.
    /// </summary>
    public class CommandLineParser
    {
        public const string DebugArgument = "debug";

        private readonly IEnumerable<CommandMetadata> _metadata;

        private CommandLineParser(IEnumerable<CommandMetadata> metadata)
        {
            _metadata = metadata;
        }

        /// <summary>
        /// Checks for predefined debugging arguments and performs requested actions.
        /// </summary>
        /// <param name="args">Command line arguments to parse.</param>
        public static void HandleDebugArguments(ref string[] args)
        {
            if (args.Length > 0 && string.Equals(args[0], DebugArgument, StringComparison.OrdinalIgnoreCase))
            {
                // Remove debug string from the arguments list as it is not expected by the rest of the code
                string[] updatedArgs = new string[args.Length - 1];
                Array.Copy(args, 1, updatedArgs, 0, updatedArgs.Length);
                args = updatedArgs;

                if (!Debugger.IsAttached)
                    Debugger.Launch();
            }
        }

        /// <summary>
        /// Gets an instance of the parser that is able to create commands from the specified assembly.
        /// </summary>
        /// <typeparam name="TBase">Base class or interface for command classes.</typeparam>
        /// <param name="commandsContainer">Assembly that contains command definitions.</param>
        /// <param name="namespaceFilter">Optional namespace to get commands from. </param>
        /// <exception cref="Exception{EmptyAssemblyExceptionArgs}">
        /// Indicates that the specified assembly does not contain any command definitions.
        /// </exception>
        public static CommandLineParser FromAssembly<TBase>(
            Assembly commandsContainer,
            string namespaceFilter = null)
        {
            IEnumerable<Type> allTypes = commandsContainer.GetTypes();
            if (!string.IsNullOrEmpty(namespaceFilter))
            {
                allTypes = allTypes.Where(t => t.Namespace != null &&
                                               t.Namespace.Equals(namespaceFilter, StringComparison.Ordinal));
            }

            var targetType = typeof(TBase);
            var commandTypes = allTypes.Where(t => targetType.IsAssignableFrom(t) && !t.IsAbstract);
            CommandMetadata[] metadata = CommandMetadata.CreateMultiple(commandTypes).ToArray();

            if (!metadata.Any())
            {
                var args = new EmptyAssemblyExceptionArgs(commandsContainer.FullName);
                throw new Exception<EmptyAssemblyExceptionArgs>(args);
            }

            return new CommandLineParser(metadata);
        }

        /// <summary>
        /// Parses the specified arguments and returns and instance of the command class that is able to
        /// handle them.
        /// </summary>
        /// <typeparam name="TCommand">Type of the command object to create.</typeparam>
        /// <param name="args">Command line arguments to parse.</param>
        /// <exception cref="Exception{CommandNotFoundExceptionArgs}">
        /// Indicates that requested command does not exist.
        /// </exception>
        /// <exception cref="Exception{OptionsValidationExceptionArgs}">
        /// Indicates that options for the specified command cannot be parsed.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Indicates that the specified array is empty.
        /// </exception>
        public TCommand Parse<TCommand>(string[] args)
        {
            if (null == args || args.Length < 1)
                throw new ArgumentOutOfRangeException("args", "Arguments array should not be empty");

            CommandMetadata metadata = LookupCommand(args[0]);
            IDictionary<string, OptionValue> tokens = ArgumentsTokenizer.GetTokens(args, 1);
            CommandBuilder builder = new CommandBuilder(metadata);

            return (TCommand)builder.CreateInstance(tokens);
        }

        /// <summary>
        /// Gets the help message to display for the user.
        /// </summary>
        /// <param name="commandName">Optional name of the command to get help for.</param>
        /// <exception cref="Exception{CommandNotFoundExceptionArgs}">
        /// Indicates that requested command does not exist.
        /// </exception>
        public string GetHelp(string commandName = null)
        {
            StringBuilder retVal = new StringBuilder(1024);
            Assembly assembly = Assembly.GetCallingAssembly();
            string executableName = assembly.GetName().Name;

            if (!string.IsNullOrEmpty(commandName))
            {
                CommandMetadata metadata = LookupCommand(commandName);
                GetCommandHelp(retVal, metadata, executableName);
            }
            else
                GetApplicationHelp(retVal, executableName);

            return retVal.ToString();
        }

        private void GetCommandHelp(StringBuilder buffer, CommandMetadata metadata, string executableName)
        {
            buffer.Append("Description: ")
                  .AppendLine(metadata.Description)
                  .AppendLine()
                  .Append("Usage: ");

            foreach (var usage in metadata.Options)
            {
                buffer.AppendFormat("{0} {1} ", executableName, metadata.Name);

                foreach (var option in usage)
                {
                    if (!string.IsNullOrEmpty(option.ValueHint))
                    {
                        buffer.AppendFormat("{0}-{1} <{2}>{3} ",
                                            !option.Required ? "[" : string.Empty,
                                            option.Name,
                                            option.ValueHint,
                                            !option.Required ? "]" : string.Empty);
                    }
                    else
                    {
                        buffer.AppendFormat("{0}-{1}{2} ",
                                            !option.Required ? "[" : string.Empty,
                                            option.Name,
                                            !option.Required ? "]" : string.Empty);
                    }
                }

                foreach (var option in usage)
                {
                    buffer.AppendLine()
                          .AppendFormat("\t{0, -16}{1}", option.Name, option.Description);
                }

                buffer.AppendLine();
                buffer.AppendLine();
            }
        }

        private void GetApplicationHelp(StringBuilder buffer, string executableName)
        {
            buffer.AppendFormat("Usage: {0} <command> [<command args>]", executableName)
                  .AppendLine()
                  .AppendLine()
                  .AppendLine("Supported commands:");

            foreach (var cmd in _metadata)
            {
                buffer.AppendFormat("\t{0, -16}{1}", cmd.Name, cmd.Description)
                      .AppendLine();
            }
        }

        private CommandMetadata LookupCommand(string name)
        {
            CommandMetadata metadata = _metadata.SingleOrDefault(
                m => m.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));

            if (null == metadata)
            {
                var ex = new CommandNotFoundExceptionArgs(name);
                throw new Exception<CommandNotFoundExceptionArgs>(ex);
            }

            return metadata;
        }
    }
}