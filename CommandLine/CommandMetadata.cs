using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace WebSync.CommandLine
{
    /// <summary>
    /// Contains metadata for a command type.
    /// </summary>
    internal class CommandMetadata
    {
        private readonly CommandAttribute _attribute;

        private OptionMetadata[][] _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandMetadata" /> class.
        /// </summary>
        /// <param name="commandType">Type of the command metadata belongs to.</param>
        /// <param name="attribute">Command attribute instance assigned to the target type.</param>
        private CommandMetadata(Type commandType, CommandAttribute attribute)
        {
            CommandType = commandType;
            _attribute = attribute;
        }

        /// <summary>
        /// Gets the type of the command metadata belongs to.
        /// </summary>
        internal Type CommandType { get; private set; }

        /// <summary>
        /// Gets the name of the command.
        /// </summary>
        internal string Name
        {
            get { return _attribute.Name; }
        }

        /// <summary>
        /// Gets the command description.
        /// </summary>
        internal string Description
        {
            get { return _attribute.Description; }
        }

        /// <summary>
        /// Gets metadata for all supported combinations of options for the target command.
        /// </summary>
        internal OptionMetadata[][] Options
        {
            get
            {
                if (null != _options)
                    return _options;

                var constructors = CommandType.GetConstructors(BindingFlags.Instance | BindingFlags.Public);
                _options = new OptionMetadata[constructors.Length][];

                for (int index = 0; index < constructors.Length; index++)
                    _options[index] = MetadataFromParameters(constructors[index].GetParameters());

                return _options;
            }
        }

        /// <summary>
        /// Creates collection of command metadata containers for the specified list of command types.
        /// </summary>
        /// <param name="commandTypes">List of command types to create metadata containers for.</param>
        internal static IEnumerable<CommandMetadata> CreateMultiple(IEnumerable<Type> commandTypes)
        {
            foreach (Type t in commandTypes)
            {
                CommandAttribute attribute = t.GetCustomAttribute<CommandAttribute>();
                if (null == attribute)
                    continue;

                yield return new CommandMetadata(t, attribute);
            }
        }

        private static OptionMetadata[] MetadataFromParameters(ParameterInfo[] parameters)
        {
            List<OptionMetadata> container = new List<OptionMetadata>(parameters.Length);
            foreach (var parameter in parameters)
            {
                var attribute = parameter.GetCustomAttribute<OptionAttribute>();
                string description = null;
                string valueHint = null;
                if (null != attribute)
                {
                    description = attribute.Description;
                    valueHint = attribute.ValueHint;
                }

                container.Add(new OptionMetadata(parameter.Name,
                                                 !parameter.IsOptional,
                                                 parameter.ParameterType,
                                                 description,
                                                 valueHint));
            }

            return container.ToArray();
        }
    }
}