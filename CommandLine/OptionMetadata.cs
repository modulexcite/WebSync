using System;

namespace WebSync.CommandLine
{
    /// <summary>
    /// Contains command line option metadata.
    /// </summary>
    internal struct OptionMetadata
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OptionMetadata"/> struct.
        /// </summary>
        /// <param name="name">Option name as exected on the command line.</param>
        /// <param name="required">Indicates whether this option is required.</param>
        /// <param name="targetType">Type of the option value.</param>
        /// <param name="description">Option help message.</param>
        /// <param name="valueHint">Hint message for the option value.</param>
        internal OptionMetadata(string name,
                                bool required,
                                Type targetType,
                                string description,
                                string valueHint) : this()
        {
            Name = name;
            Required = required;
            TargetType = targetType;
            Description = description;
            ValueHint = valueHint;
        }

        /// <summary>
        /// Gets the name of the option.
        /// </summary>
        internal string Name { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this option is required.
        /// </summary>
        internal bool Required { get; private set; }

        /// <summary>
        /// Gets the type of the option value.
        /// </summary>
        internal Type TargetType { get; private set; }

        /// <summary>
        /// Gets the option help message.
        /// </summary>
        internal string Description { get; private set; }

        /// <summary>
        /// Gets the option value help message.
        /// </summary>
        internal string ValueHint { get; private set; }
    }
}