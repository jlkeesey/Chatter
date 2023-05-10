using System;
using System.IO;
using System.Security;

namespace Chatter.System;

/// <summary>
///     An implementation of <see cref="IFileSystem" /> for the Windows operating system.
/// </summary>
public sealed class WindowsFileSystem : IFileSystem
{
    public string DocumentsPath => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

    public bool DirectoryExists(string path)
    {
        return Directory.Exists(path);
    }

    public bool FileExists(string path)
    {
        return File.Exists(path);
    }

    public bool Exists(string path)
    {
        return Path.Exists(path);
    }

    public bool CreateDirectory(string path)
    {
        try
        {
            Directory.CreateDirectory(path);
        }
        catch (IOException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
        catch (ArgumentException)
        {
            return false;
        }
        catch (NotSupportedException)
        {
            return false;
        }

        return true;
    }

    public TextWriter OpenFile(string path, bool append)
    {
        try
        {
            return new StreamWriter(path, append);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
        catch (ArgumentException)
        {
        }
        catch (NotSupportedException)
        {
        }
        catch (SecurityException)
        {
        }

        return TextWriter.Null;
    }

    public string GetDirectoryName(string path)
    {
        return Path.GetDirectoryName(path) ?? string.Empty;
    }

    public string Join(string part1, string part2)
    {
        return Path.Join(part1, part2);
    }

    public string Combine(string path, string filename, string extension)
    {
        return Path.ChangeExtension(Path.Join(path, filename), extension);
    }
}