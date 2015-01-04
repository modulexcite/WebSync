using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using WebSync.Diagnostics;

namespace WebSync.CommandLine
{
    /// <summary>
    /// Creates and instance of the command class based on its metadata and command line arguments.
    /// </summary>
    internal class CommandBuilder
    {
        private const BindingFlags Flags = BindingFlags.CreateInstance |
                                           BindingFlags.Public |
                                           BindingFlags.Instance |
                                           BindingFlags.OptionalParamBinding;

        private readonly CommandMetadata _metadata;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandBuilder" /> class.
        /// </summary>
        /// <param name="metadata">Target command metadata.</param>
        internal CommandBuilder(CommandMetadata metadata)
        {
            _metadata = metadata;
        }

        /// <summary>
        /// Gets the instance of the created command object.
        /// </summary>
        /// <param name="tokens">Command line arguments to initialize command with.</param>
        /// <exception cref="Exception{OptionsValidationExceptionArgs}">
        /// Indicates that at least one required option was not specified.
        /// </exception>
        internal object CreateInstance(IDictionary<string, OptionValue> tokens)
        {
            HashSet<string> availableOptions =
                new HashSet<string>(tokens.Select(t => t.Key.ToLowerInvariant()));

            OptionMetadata[] targetMetadata = null;

            // Find options set that matches our tokens collection.
            foreach (var usage in _metadata.Options)
            {
                HashSet<string> requiredOptions =
                    new HashSet<string>(usage.Where(o => o.Required).Select(o => o.Name.ToLowerInvariant()));
                HashSet<string> allOptions =
                    new HashSet<string>(usage.Select(o => o.Name.ToLowerInvariant()));

                if (requiredOptions.IsSubsetOf(availableOptions) &&
                    allOptions.IsSupersetOf(availableOptions))
                {
                    targetMetadata = usage;
                    break;
                }
            }

            if (null == targetMetadata)
            {
                var args = new OptionsValidationExceptionArgs("Invalid command arguments provided.");
                throw new Exception<OptionsValidationExceptionArgs>(args);
            }

            try
            {
                return CreateInstanceInternal(targetMetadata, tokens);
            }
            catch (TargetInvocationException ex)
            {
                var argumentError = ex.InnerException as ArgumentException;
                if (null == argumentError)
                    throw;

                string msg = string.Format("Invalid value for option '{0}'. {1}",
                                           argumentError.ParamName,
                                           argumentError.Message);
                var args = new OptionsValidationExceptionArgs(msg);
                throw new Exception<OptionsValidationExceptionArgs>(args);
            }
            catch (MissingMethodException)
            {
                var args = new OptionsValidationExceptionArgs("Invalid command arguments provided.");
                throw new Exception<OptionsValidationExceptionArgs>(args);
            }
        }

        private object CreateInstanceInternal(OptionMetadata[] parameters,
                                              IDictionary<string, OptionValue> tokens)
        {
            object[] constructorParameters = new object[parameters.Length];

            for (int index = 0; index < constructorParameters.Length; index++)
            {
                OptionMetadata parameter = parameters[index];
                OptionValue token;
                object value;

                if (!tokens.TryGetValue(parameter.Name, out token))
                {
                    // don't use null as it will overwrite default values
                    constructorParameters[index] = Type.Missing;
                    continue;
                }

                if (!tokens[parameter.Name].TryChangeType(parameter.TargetType, out value))
                {
                    string msg = string.Format("Value '{0}' for option '{1}' is invalid. {2} expected",
                                               token,
                                               parameter.Name,
                                               parameter.TargetType.Name);
                    var args = new OptionsValidationExceptionArgs(msg);
                    throw new Exception<OptionsValidationExceptionArgs>(args);
                }

                constructorParameters[index] = value;
            }

            return Activator.CreateInstance(_metadata.CommandType,
                                            Flags,
                                            null,
                                            constructorParameters,
                                            CultureInfo.CurrentCulture);
        }
    }
}