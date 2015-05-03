using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace YoloDev.Dnx.Utils
{
    [DebuggerStepThrough]
    public static class Check
    {
        public static void NotNull<T>(T value, string parameterName)
            where T : class
        {
            if (ReferenceEquals(value, null))
            {
                NotEmpty(parameterName, nameof(parameterName));

                throw new ArgumentNullException(parameterName);
            }
        }
        
        public static void NotNull<T>(T value, string propertyName, string parameterName)
            where T : class
        {
            if (ReferenceEquals(value, null))
            {
                NotEmpty(propertyName, nameof(propertyName));
                NotEmpty(parameterName, nameof(parameterName));
                
                throw new ArgumentNullException(parameterName, $"Property {propertyName} does not accept null values.");
            }
        }
        
        public static void NotEmpty<T>(IEnumerable<T> enumerable, string parameterName)
        {
            NotNull(enumerable, parameterName);
            
            if (!enumerable.Any())
            {
                NotEmpty(parameterName, nameof(parameterName));
                
                throw new ArgumentException($"{parameterName} should not be empty.", parameterName);
            }
        }
        
        public static void NotEmpty<T>(IEnumerable<T> enumerable, string propertyName, string parameterName)
        {
            NotNull(enumerable, propertyName, parameterName);
            
            if (!enumerable.Any())
            {
                NotEmpty(propertyName, nameof(propertyName));
                NotEmpty(parameterName, nameof(parameterName));
                
                throw new ArgumentException($"Property {propertyName} does not accept empty values.", parameterName);
            }
        }

        public static void NotEmpty(string value, string parameterName)
        {
            NotNull(value, parameterName);

            if(string.IsNullOrWhiteSpace(value))
            {
                NotEmpty(parameterName, nameof(parameterName));

                throw new ArgumentException($"{parameterName} should not be null or empty.", parameterName);
            }
        }
        
        public static void NotEmpty(string value, string propertyName, string parameterName)
        {
            NotNull(value, propertyName, parameterName);

            if(string.IsNullOrWhiteSpace(value))
            {
                NotEmpty(propertyName, nameof(propertyName));
                NotEmpty(parameterName, nameof(parameterName));

                throw new ArgumentException($"Property {propertyName} does not accept empty values.", parameterName);
            }
        }
    }
}
