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

using Chatter.System;
using System;
using System.IO;

namespace Chatter;

/// <summary>
///     Basic file system helper functions.
/// </summary>
public sealed class FileHelper(IFileSystem fileSystem)
{
    /// <summary>
    ///     Defines the possible results of the <see cref="FileHelper.EnsureDirectoryExists" /> method.
    /// </summary>
    public enum EnsureCode
    {
        /// <summary>
        ///     The ensure operation was successful and the directory exists.
        /// </summary>
        Success = 0,

        /// <summary>
        ///     The path name exists but is a file instead of a directory.
        /// </summary>
        FileExists = 1,

        /// <summary>
        ///     The parent path does not exist.
        /// </summary>
        ParentDoesNotExist = 2,

        /// <summary>
        ///     Something went wrong at the system level.
        /// </summary>
        SystemError = 3,
    }

    /// <summary>
    ///     The file extension for log files.
    /// </summary>
    public const string LogFileExtension = ".log";

    /// <summary>
    ///     The name of the default directory to write logs into.
    /// </summary>
    public const string DefaultDirectory = "FFXIV Chatter";

    /// <summary>
    ///     Returns a <see cref="TextWriter" /> into the given file.
    /// </summary>
    /// <param name="path">The path name of the file to open.</param>
    /// <param name="append"><c>true</c> if the file should be appended to, false to overwrite.</param>
    /// <returns>The <see cref="TextWriter" />. If the file cannot be written to, <see cref="TextWriter.Null" /> is returned.</returns>
    public TextWriter OpenFile(string path, bool append)
    {
        return fileSystem.OpenFile(path, append);
    }

    /// <summary>
    ///     Creates the given directory if it doesn't exist.
    /// </summary>
    /// <remarks>
    ///     Unlike CreateDirectory, this will only create the last directory in the path if it does not exist.
    ///     If any other part of the path does not exist this fails and returns <see cref="string.Empty" />.
    /// </remarks>
    /// <param name="directory">The directory to ensure exists.</param>
    /// <returns>An <see cref="EnsureCode" /> describing the result of the operation.</returns>
    public EnsureCode EnsureDirectoryExists(string directory)
    {
        if (fileSystem.DirectoryExists(directory)) return EnsureCode.Success;
        if (fileSystem.FileExists(directory)) return EnsureCode.FileExists;

        var parent = fileSystem.GetDirectoryName(directory);
        if (!fileSystem.DirectoryExists(parent)) return EnsureCode.ParentDoesNotExist;
        return fileSystem.CreateDirectory(directory) ? EnsureCode.Success : EnsureCode.SystemError;
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
        return fileSystem.Join(fileSystem.DocumentsPath, DefaultDirectory);
    }

    /// <summary>
    ///     Returns a non-existing, fully qualified file name with the current date and time appended.
    /// </summary>
    /// <remarks>
    ///     This method combines the parts and then appends the current date and time. This path is returned ff the
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
        var fullName = fileSystem.Combine(path, filename, extension);
        if (!fileSystem.Exists(fullName)) return fullName;
        for (var i = 1; i < int.MaxValue; i++)
        {
            var nameCounter = filename + "-" + i;
            fullName = fileSystem.Combine(path, nameCounter, extension);
            if (!fileSystem.Exists(fullName)) return fullName;
        }

        throw new IndexOutOfRangeException("More than 2G worth of file names for log files.");
    }
}
