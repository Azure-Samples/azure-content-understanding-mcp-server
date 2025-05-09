namespace ContentUnderstanding.MCP.Server.Stdio.Utils
{
    /// <summary>
    /// Provides utility methods for validating parameters and enforcing preconditions.
    /// </summary>
    public class Guard
    {
        /// <summary>
        /// Validates that a string parameter is not null or empty, throwing an exception if it is.
        /// </summary>
        /// <param name="value">The string value to validate.</param>
        /// <param name="name">The name of the parameter being validated, used in the exception message.</param>
        /// <exception cref="ArgumentException">Thrown when the provided string is null or empty.</exception>
        public static void ThrowIfNullOrEmpty(string? value, string name)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException($"Value for '{name}' is missing or empty.");
            }
        }
    }
}
