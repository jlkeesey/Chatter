using System;
using System.IO;
using System.Security;

namespace Chatter;

/// <summary>
///     Basic file system helper functions.
/// </summary>
internal class FileHelper
{
    /// <summary>
    ///     The file extension for log files.
    /// </summary>
    public const string LogFileExtension = ".log";

    /// <summary>
    ///     The name of the default directory to write logs into.
    /// </summary>
    public const string DefaultDirectory = "FFXIV Chatter";

    private const string FileDateTimePattern = "-{0:yyyyMMdd-HHmmss}";

    /// <summary>
    ///     Returns the current user's <c>Document</c> directory path.
    /// </summary>
    /// <returns>The <c>Document</c> directory path.</returns>
    public static string DocumentsDirectory()
    {
        return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    }

    /// <summary>
    ///     Creates the given directory.
    /// </summary>
    /// <remarks>
    ///     Attempts to creates the given directory and returns the name of the directory if successful,
    ///     or <see cref="string.Empty" /> if not. Any exceptions thrown by the underlying system should be caught and
    ///     signaled as a failure by returning <see cref="string.Empty" />.
    /// </remarks>
    /// <param name="directory">The directory oath to create.</param>
    /// <returns>The name of the directory ot <see cref="string.Empty" />.</returns>
    public static string CreateDirectory(string directory)
    {
        try
        {
            Directory.CreateDirectory(directory);
        }
        catch (IOException)
        {
            return string.Empty;
        }
        catch (UnauthorizedAccessException)
        {
            return string.Empty;
        }
        catch (ArgumentException)
        {
            return string.Empty;
        }
        catch (NotSupportedException)
        {
            return string.Empty;
        }

        return directory;
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
    public static string EnsureDirectoryExists(string directory)
    {
        if (Directory.Exists(directory))
            return directory;
        if (File.Exists(directory))
            // TODO Add handling for this case. Preferably not allow getting here.
            return string.Empty;

        var parent = Path.GetDirectoryName(directory);
        if (Directory.Exists(parent))
            return CreateDirectory(directory);
        // TODO Add handling for this case. Preferably not allow getting here.
        return string.Empty;
    }

    /// <summary>
    ///     Returns a fully qualified path for the given path.
    /// </summary>
    /// <remarks>
    ///     If the path is not qualified then it is made relative to the current user's <c>Document</c> directory.
    /// </remarks>
    /// <param name="path">The path to clean.</param>
    /// <returns>The fully qualified path.</returns>
    public static string CleanupPath(string path)
    {
        try
        {
            return Path.GetFullPath(path, DocumentsDirectory());
        }
        catch (ArgumentException)
        {
            return string.Empty;
        }
        catch (SecurityException)
        {
            return string.Empty;
        }
        catch (NotSupportedException)
        {
            return string.Empty;
        }
        catch (PathTooLongException)
        {
            return string.Empty;
        }
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
    public static string InitialLogDirectory()
    {
        return Path.Join(DocumentsDirectory(), DefaultDirectory);
    }

    /// <summary>
    ///     Returns the given file name prefix with the given <see cref="DateTime" /> appended to form a complete name.
    /// </summary>
    /// <param name="prefix">The file name prefix.</param>
    /// <param name="when">The timestamp for the file name or null for the current time.</param>
    /// <returns>The complete file name.</returns>
    private static string FileNameWithDateTime(string prefix, DateTime? when = null)
    {
        return prefix + string.Format(FileDateTimePattern, when ?? DateTime.Now);
    }

    /// <summary>
    ///     Returns a non-existing, fully qualified file name with the current <see cref="DateTime" /> appended.
    /// </summary>
    /// <remarks>
    ///     This methods combines the parts and then appends the current <see cref="DateTime" />. This path is returned ff the
    ///     resulting path does not exist. If it does exist, then a counter value is appended until a non-existing name is
    ///     created.
    /// </remarks>
    /// <param name="directory">The directory for the file.</param>
    /// <param name="name">The name component of the full file name.</param>
    /// <param name="extension">The extension for the file.</param>
    /// <returns>A non-existing file name.</returns>
    /// <exception cref="IndexOutOfRangeException">
    ///     If the counter overflows. The counter is an int so more than 2G file name must exist for this to trigger.
    /// </exception>
    public static string FullFileName(string directory, string name, string extension)
    {
        var fullName = MakeName(directory, name, extension);
        if (!Path.Exists(fullName)) return fullName;
        for (var i = 0; i < int.MaxValue; i++)
        {
            var nameCounter = name + "-" + i;
            fullName = MakeName(directory, nameCounter, extension);
            if (!Path.Exists(fullName)) return fullName;
        }

        throw new IndexOutOfRangeException("More than 2G worth of file names for log files.");
    }

    /// <summary>
    ///     Returns a new file name with all parts combined.
    /// </summary>
    /// <param name="directory">The directory of the new file.</param>
    /// <param name="name">The name of the file.</param>
    /// <param name="extension">The extension of the file.</param>
    /// <returns>The combined file name.</returns>
    private static string MakeName(string directory, string name, string extension)
    {
        return Path.ChangeExtension(Path.Join(directory, name), extension);
    }
}