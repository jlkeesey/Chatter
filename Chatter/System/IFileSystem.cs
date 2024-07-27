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

using System.IO;

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
    string DocumentsPath { get; }

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
    ///     Creates the given directory. This will attempt to create the all the components of the path.
    /// </summary>
    /// <param name="path">The directory path to create.</param>
    /// <returns><c>true</c> if the directory was created, <c>false</c> if anything went wrong.</returns>
    bool CreateDirectory(string path);

    /// <summary>
    ///     Opens the give file name as a <see cref="TextWriter" />.
    /// </summary>
    /// <param name="path">The file name to open.</param>
    /// <param name="append"><c>true</c> if the stream should be opened for append if the file exists.</param>
    /// <returns>The <see cref="TextWriter" /> for the given path.</returns>
    TextWriter OpenFile(string path, bool append);

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
