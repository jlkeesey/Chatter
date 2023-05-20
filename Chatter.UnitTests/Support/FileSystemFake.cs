// Copyright 2023 James Keesey
// 
// Redistribution and use in source and binary forms, with or without modification,
// are permitted provided that the following conditions are met:
// 
// 1. Redistributions of source code must retain the above copyright notice,
//    this list of conditions and the following disclaimer.
// 
// 2. Redistributions in binary form must reproduce the above copyright notice,
//    this list of conditions and the following disclaimer in the documentation
//    and/or other materials provided with the distribution.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS “AS IS”
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Collections.Generic;
using System.IO;
using Chatter.System;

namespace Chatter.UnitTests.Support;

internal class FileSystemFake : IFileSystem
{
    public FileSystemFake()
    {
        Directories.Add(DocumentsPath);
    }

    public List<string> Directories { get; } = new();
    public List<string> Files { get; } = new();

    public Dictionary<string, ChatLogWriter> Writers { get; } = new();
    public string DocumentsPath { get; init; } = "C:\\Users\\Bob\\Documents";

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
        if (Writers.TryGetValue(path, out var file))
        {
            if (append) return file;
            file.Close();
            Writers.Remove(path);
        }

        var writer = new ChatLogWriter();
        Writers.Add(path, writer);
        return writer;
    }

    public string GetDirectoryName(string path)
    {
        var index = path.LastIndexOf('\\');
        return index == -1 ? string.Empty : path[..index];
    }

    public string Join(string part1, string part2)
    {
        if (part1[^1] == '\\') return part1 + part2;
        return part1 + '\\' + part2;
    }

    public string Combine(string path, string filename, string extension)
    {
        var result = Join(path, filename);
        if (!result.Contains('.')) result += extension;
        return result;
    }
}
