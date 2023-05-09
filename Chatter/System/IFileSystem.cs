namespace Chatter.System;

/// <summary>
///     Defines the basic file system operations needed for this plugin.
/// </summary>
public interface IFileSystem
{
    /// <summary>
    ///     Returns the path to the default location for user documents.
    /// </summary>
    /// <returns>The documents path.</returns>
    string DocumentsPath();

    /// <summary>
    ///     Returns <c>true</c> if the given path exists and is a directory.
    /// </summary>
    /// <param name="path">The path to check.</param>
    /// <returns><c>true</c> if the given path exists and is a directory, <c>false</c> otherwise.</returns>
    bool DirectoryExists(string path);

    /// <summary>
    ///     Returns <c>true</c> if the given path exists and is a file.
    /// </summary>
    /// <param name="path">The path to check.</param>
    /// <returns><c>true</c> if the given path exists and is a file, <c>false</c> otherwise.</returns>
    bool FileExists(string path);

    /// <summary>
    ///     Returns <c>true</c> if the given path exists.
    /// </summary>
    /// <param name="path">The path to check.</param>
    /// <returns><c>true</c> if the given path exists, <c>false</c> otherwise.</returns>
    bool Exists(string path);

    /// <summary>
    ///     Creates the given directory. This will only attempt to create the final component of the path.
    /// </summary>
    /// <param name="path">The directory path to create.</param>
    /// <returns><c>true</c> if the directory was created, <c>false</c> if anything went wrong.</returns>
    bool CreateDirectory(string path);

    /// <summary>
    ///     Returns the path with the final component removed.
    /// </summary>
    /// <param name="path">The path to process.</param>
    /// <returns>The directory path of the given path, <see cref="string.Empty" /> if there is no directory specified.</returns>
    string GetDirectoryName(string path);

    /// <summary>
    ///     Joins the two path parts with the appropriate connector.
    /// </summary>
    /// <param name="part1">The first path of the path.</param>
    /// <param name="part2">The second part of the path.</param>
    /// <returns>The joined path.</returns>
    string Join(string part1, string part2);

    /// <summary>
    ///     Combines the given file name path into a complete path.
    /// </summary>
    /// <param name="path">The directory path.</param>
    /// <param name="filename">The base filename</param>
    /// <param name="extension">The file extension.</param>
    /// <returns></returns>
    string Combine(string path, string filename, string extension);
}