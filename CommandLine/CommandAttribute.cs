using System;

namespace WebSync.CommandLine
{
    /// <summary>
    /// Specifies details of the command line command class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class CommandAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandAttribute" /> class.
        /// </summary>
        /// <param name="name">Name of the command.</param>
        /// <param name="description">Command description.</param>
        public CommandAttribute(string name, string description)
        {
            Name = name;
            Description = description;
        }

        /// <summary>
        /// Gets the name of the command. That is basically what user is typing in the command line.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the command description that will be used as the command line help message.
        /// </summary>
        public string Description { get; private set; }
    }
}