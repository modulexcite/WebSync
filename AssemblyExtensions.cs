using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace WebSync
{
    /// <summary>
    /// Contains extension methods for <see cref="Assembly"/> class.
    /// </summary>
    public static class AssemblyExtensions
    {
        /// <summary>
        /// Gets assembly description using <see cref="AssemblyDescriptionAttribute"/>.
        /// </summary>
        /// <param name="assembly">The assembly to get description of.</param>
        public static string GetDescription(this Assembly assembly)
        {
            AssemblyDescriptionAttribute attribute =
                assembly.GetCustomAttribute(typeof(AssemblyDescriptionAttribute))
                    as AssemblyDescriptionAttribute;

            return null == attribute ? string.Empty : attribute.Description;
        }

        /// <summary>
        /// Gets assembly title using <see cref="AssemblyTitleAttribute"/>.
        /// </summary>
        /// <param name="assembly">The assembly to get title of.</param>
        public static string GetTitle(this Assembly assembly)
        {
            AssemblyTitleAttribute attribute =
                assembly.GetCustomAttribute(typeof(AssemblyTitleAttribute)) as AssemblyTitleAttribute;

            return null == attribute ? string.Empty : attribute.Title;
        }

        /// <summary>
        /// Gets the user friendly assembly version string.
        /// </summary>
        /// <param name="assembly">The assembly to get version of.</param>
        public static string GetVersion(this Assembly assembly)
        {
            var informationalVersionAttributes =
                assembly.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), true);

            if (informationalVersionAttributes.Any())
            {
                var attr = (AssemblyInformationalVersionAttribute)informationalVersionAttributes.First();
                return attr.InformationalVersion;
            }

            return assembly.GetName().Version.ToString();
        }

        /// <summary>
        /// Gets all types that can be loaded from the specified assembly.
        /// </summary>
        /// <param name="assembly">The assembly to get types from.</param>
        public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
        {
            // Based on the StackOverflow answer here: http://stackoverflow.com/questions/7889228/how-to-
            // prevent-reflectiontypeloadexception-when-calling-assembly-gettypes.
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(t => t != null);
            }
        }
    }
}