using System;
using System.Diagnostics;

namespace YoloDev.Dnx.Utils
{
    [DebuggerStepThrough]
    public static class Check
    {
        public static void NotNull<T>(T value, string parameterName)
        {
            if (ReferenceEquals(value, null))
            {
                StringNotEmpty(parameterName, nameof(parameterName));

                throw new ArgumentNullException(parameterName);
            }
        }

        public static void StringNotEmpty(string value, string parameterName)
        {
            if(string.IsNullOrEmpty(value))
            {
                StringNotEmpty(parameterName, nameof(parameterName));

                throw new ArgumentException($"{parameterName} should not be empty", parameterName);
            }
        }
    }
}
