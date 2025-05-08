using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace ContentUnderstanding.MCP.Tools
{
    /// <summary>
    /// Manages access permissions to specific folders for the Content Understanding tools.
    /// This class restricts file operations to a pre-defined list of allowed directories
    /// to enforce security boundaries.
    /// </summary>
    public class FolderPermissions
    {
        /// <summary>
        /// Collection of allowed folder paths that the tool can access.
        /// </summary>
        private readonly IEnumerable<string> _allowedFolders;

        /// <summary>
        /// Initializes a new instance of the FolderPermissions class with a set of allowed folders.
        /// </summary>
        /// <param name="allowedFolders">Collection of folder paths that the tool is permitted to access.</param>
        public FolderPermissions(IEnumerable<string> allowedFolders)
        {
            _allowedFolders = allowedFolders;
        }

        /// <summary>
        /// Retrieves a list of normalized, fully qualified folder paths that this tool has permission to access.
        /// Paths are normalized to ensure consistent comparison regardless of input format.
        /// </summary>
        /// <returns>
        /// Collection of fully qualified directory paths that are accessible to the tool.
        /// </returns>
        /// <remarks>
        /// This method is exposed as a Semantic Kernel function that can be called by AI systems
        /// to understand which directories they have permission to access.
        /// </remarks>
        [KernelFunction, Description("Gets a list of fully qualified folder paths that this tool has access to.")]
        public IEnumerable<string> GetAllowedFolders()
            => _allowedFolders.Select(dir =>
                NormalizePath(Path.GetFullPath(ExpandHomeDirectory(dir)))
            ).AsEnumerable();

        /// <summary>
        /// Expands the tilde (~) character at the beginning of a path to the user's home directory.
        /// This allows for paths to be specified using the shorthand "~/" notation common in Unix-like systems.
        /// </summary>
        /// <param name="path">The path that may contain a tilde representing the home directory.</param>
        /// <returns>The path with the tilde expanded to the user's home directory, or the original path if no tilde is present.</returns>
        private string ExpandHomeDirectory(string path)
        {
            if (path == null) return null;

            if (path.StartsWith("~"))
            {
                string homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                return Path.Combine(homeDirectory, path.Substring(1).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            }

            return path;
        }

        /// <summary>
        /// Normalizes a file system path to its canonical form.
        /// This resolves relative paths, removes redundant separators, and ensures consistent
        /// format for path comparison regardless of how the path was originally specified.
        /// </summary>
        /// <param name="path">The path to normalize.</param>
        /// <returns>
        /// A normalized absolute path without a trailing directory separator,
        /// or null if the input path is null.
        /// </returns>
        private string NormalizePath(string path)
        {
            if (path == null) return null;

            return Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar);
        }
    }
}
