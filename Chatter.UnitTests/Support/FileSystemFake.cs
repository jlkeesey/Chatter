using System.Collections.Generic;
using System.IO;
using Chatter.System;
using NotImplementedException = System.NotImplementedException;

namespace Chatter.UnitTests.Support;

internal class FileSystemFake : IFileSystem
{
    public List<string> Directories { get; } = new();
    public List<string> Files { get; } = new();
    public string DocumentsPath { get; init; } = "C:\\Users\\Bob\\Documents";

    public FileSystemFake()
    {
        Directories.Add(DocumentsPath);
    }

    public bool DirectoryExists(string path)
    {
        return Directories.Contains(path);
    }

    public bool FileExists(string path)
    {
        return Files.Contains(path);
    }

    public bool Exists(string path)
    {
        return DirectoryExists(path) || FileExists(path);
    }

    public bool CreateDirectory(string path)
    {
        if (DirectoryExists(path)) return false;
        Directories.Add(path);
        return true;
    }

    public TextWriter OpenFile(string path, bool append)
    {
        var writer = new ChatLogWriter();
        Writers.Add(path, writer);
        return writer;
    }

    public Dictionary<string, ChatLogWriter> Writers { get; } = new();

    public string GetDirectoryName(string path)
    {
        var index = path.LastIndexOf('\\');
        return index == -1 ? string.Empty : path[..index];
    }

    public string Join(string part1, string part2)
    {
        if (part1[^1] == '\\')
            return part1 + part2;
        return part1 + '\\' + part2;
    }

    public string Combine(string path, string filename, string extension)
    {
        var result = Join(path, filename);
        if (!result.Contains('.')) result += extension;
        return result;
    }
}