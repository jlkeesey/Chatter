using System;
using Chatter.System;

namespace Chatter;

/// <summary>
///     Basic file system helper functions.
/// </summary>
public sealed class FileHelper
{
    /// <summary>
    ///     The file extension for log files.
    /// </summary>
    public const string LogFileExtension = ".log";

    /// <summary>
    ///     The name of the default directory to write logs into.
    /// </summary>
    private const string DefaultDirectory = "FFXIV Chatter";

    private readonly IFileSystem _fileSystem;

    public FileHelper(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    /// <summary>
    ///     Creates the given directory if it doesn't exist.
    /// </summary>
    /// <remarks>
    ///     Unlike CreateDirectory, this will only create the last directory in the path if it does not exist.
    ///     If any other part of the path does not exist this fails and returns <see cref="string.Empty" />.
    /// </remarks>
    /// <param name="directory">The directory to ensure exists.</param>
    /// <returns>The name of the directory if it exists, <see cref="string.Empty" /> otherwise.</returns>
    public void EnsureDirectoryExists(string directory)
    {
        if (_fileSystem.DirectoryExists(directory))
            return;
        if (_fileSystem.FileExists(directory))
            // TODO Add handling for this case. Preferably not allow getting here.
            return;

        var parent = _fileSystem.GetDirectoryName(directory);
        if (_fileSystem.DirectoryExists(parent))
        {
            _fileSystem.CreateDirectory(directory);
        }

        // TODO Add handling for this case. Preferably not allow getting here.
    }

    /// <summary>
    ///     Returns the directory to use for logging before the user has changed it.
    /// </summary>
    /// <remarks>
    ///     When this plugin is first run, there needs to be a location to write log files to before
    ///     the user has a chance to set the location. We default to a subdirectory of the user's <c>Document</c>
    ///     directory.
    /// </remarks>
    /// <returns></returns>
    public string InitialLogDirectory()
    {
        return _fileSystem.Join(_fileSystem.DocumentsPath(), DefaultDirectory);
    }

    /// <summary>
    ///     Returns a non-existing, fully qualified file name with the current date and time appended.
    /// </summary>
    /// <remarks>
    ///     This methods combines the parts and then appends the current date and time. This path is returned ff the
    ///     resulting path does not exist. If it does exist, then a counter value is appended until a non-existing name is
    ///     created.
    /// </remarks>
    /// <param name="path">The directory for the file.</param>
    /// <param name="filename">The name component of the full file name.</param>
    /// <param name="extension">The extension for the file.</param>
    /// <returns>A non-existing file name.</returns>
    /// <exception cref="IndexOutOfRangeException">
    ///     If the counter overflows. The counter is an int so more than 2G file name must exist for this to trigger.
    /// </exception>
    public string FullFileName(string path, string filename, string extension)
    {
        var fullName = _fileSystem.Combine(path, filename, extension);
        if (!_fileSystem.Exists(fullName)) return fullName;
        for (var i = 0; i < int.MaxValue; i++)
        {
            var nameCounter = filename + "-" + i;
            fullName = _fileSystem.Combine(path, nameCounter, extension);
            if (!_fileSystem.Exists(fullName)) return fullName;
        }

        throw new IndexOutOfRangeException("More than 2G worth of file names for log files.");
    }
}