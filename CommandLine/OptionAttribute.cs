using System;

namespace WebSync.CommandLine
{
    /// <summary>
    /// Specifies details of the command line option parameter.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public sealed class OptionAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OptionAttribute"/> class.
        /// </summary>
        /// <param name="description">Help message for this option.</param>
        /// <param name="valueHint">Hint for the option value (e.g. unit).</param>
        public OptionAttribute(string description, string valueHint = null)
        {
            Description = description;
            ValueHint = valueHint;
        }

        /// <summary>
        /// Gets the option description that will be used as the command line help message.
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Gets an option value hint (e.g. unit).
        /// </summary>
        public string ValueHint { get; private set; }
    }
}