using System.Collections.Generic;
using WebSync.Diagnostics;

namespace WebSync.CommandLine
{
    /// <summary>
    /// Creates name/value pairs from command line arguments array.
    /// </summary>
    internal static class ArgumentsTokenizer
    {
        internal const char OptionMark = '-';

        internal static IDictionary<string, OptionValue> GetTokens(string[] arguments, int offset = 0)
        {
            var retVal = new Dictionary<string, OptionValue>(arguments.Length);

            string currentOption = null;
            for (int index = offset; index < arguments.Length; index++)
            {
                string currentValue = arguments[index];

                // currentValue is an option if it starts with option mark
                if (currentValue[0] == OptionMark)
                {
                    if (null != currentOption)
                        retVal.Add(currentOption, new OptionValue(null));

                    // Normalize option names to speed up lookup in the future
                    currentOption = currentValue.Substring(1).ToLowerInvariant();

                    if (retVal.ContainsKey(currentOption))
                    {
                        string msg = string.Format("Duplicate option '{0}' found", currentOption);
                        var args = new OptionsValidationExceptionArgs(msg);
                        throw new Exception<OptionsValidationExceptionArgs>(args);
                    }
                }
                else
                {
                    if (null == currentOption)
                    {
                        string msg = string.Format("No option associated with value '{0}'", currentValue);
                        var args = new OptionsValidationExceptionArgs(msg);
                        throw new Exception<OptionsValidationExceptionArgs>(args);
                    }

                    retVal.Add(currentOption, new OptionValue(currentValue));
                    currentOption = null;
                }
            }

            if (null != currentOption)
                retVal.Add(currentOption, new OptionValue(null));

            return retVal;
        }
    }
}