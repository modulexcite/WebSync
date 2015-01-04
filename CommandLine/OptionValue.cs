using System;

namespace WebSync.CommandLine
{
    /// <summary>
    /// Contains command line option value.
    /// </summary>
    internal struct OptionValue
    {
        private readonly string _rawValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionValue" /> struct.
        /// </summary>
        /// <param name="rawValue">The raw option value as it was specified in the command line.</param>
        internal OptionValue(string rawValue)
        {
            _rawValue = rawValue;
        }

        /// <summary>
        /// Returns raw value of the option.
        /// </summary>
        public override string ToString()
        {
            return _rawValue;
        }

        /// <summary>
        /// Tries to convert value to the specified type.
        /// </summary>
        /// <param name="targetType">Type to convert to.</param>
        /// <param name="value">Converted value.</param>
        internal bool TryChangeType(Type targetType, out object value)
        {
            // The fact that boolean option name is specified on the command line means it is turned on
            // regardless of its value.
            if (targetType == typeof(bool) || targetType == typeof(bool?))
                value = true;
            else if (targetType == typeof(string))
                value = _rawValue;
            else if (string.IsNullOrEmpty(_rawValue))
                value = null;
            else
            {
                try
                {
                    value = Convert.ChangeType(_rawValue, targetType);
                }
                catch (FormatException)
                {
                    value = null;
                    return false;
                }
                catch (OverflowException)
                {
                    value = null;
                    return false;
                }
                catch (InvalidCastException)
                {
                    value = null;
                    return false;
                }
            }

            return true;
        }
    }
}