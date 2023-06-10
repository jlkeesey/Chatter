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

using Chatter.UnitTests.Support;
using NUnit.Framework;

namespace Chatter.UnitTests;

[TestFixture]
public class FileHelperTests
{
    public class EnsureDirectoryExistsTests
    {
        [Test]
        public void EnsureDirectoryExists_DirectoryExists()
        {
            const string path = "C:\\A\\B\\C";
            var fileSystem = new FileSystemFake();
            fileSystem.Directories.Add(path);
            var fileHelper = new FileHelper(fileSystem);

            var result = fileHelper.EnsureDirectoryExists(path);

            Assert.AreEqual(FileHelper.EnsureCode.Success, result);
        }

        [Test]
        public void EnsureDirectoryExists_FileExists()
        {
            const string path = "C:\\A\\B\\C";
            var fileSystem = new FileSystemFake();
            fileSystem.Files.Add(path);
            var fileHelper = new FileHelper(fileSystem);

            var result = fileHelper.EnsureDirectoryExists(path);

            Assert.AreEqual(FileHelper.EnsureCode.FileExists, result);
        }

        [Test]
        public void EnsureDirectoryExists_DirectoryDoesNotExist()
        {
            const string parent = "C:\\A\\B";
            const string path = "C:\\A\\B\\C";
            var fileSystem = new FileSystemFake();
            fileSystem.Directories.Add(parent);
            var fileHelper = new FileHelper(fileSystem);

            var result = fileHelper.EnsureDirectoryExists(path);

            Assert.AreEqual(FileHelper.EnsureCode.Success, result);
            Assert.True(fileSystem.DirectoryExists(path), "Directory now exists");
        }

        [Test]
        public void EnsureDirectoryExists_ParentDoesNotExist()
        {
            const string path = "C:\\A\\B\\C";
            var fileSystem = new FileSystemFake();
            var fileHelper = new FileHelper(fileSystem);

            var result = fileHelper.EnsureDirectoryExists(path);

            Assert.AreEqual(FileHelper.EnsureCode.ParentDoesNotExist, result);
        }
    }

    public class InitialLogDirectoryTests
    {
        [Test]
        public void InitialLogDirectory()
        {
            const string parent = "C:\\A\\B";
            var fileSystem = new FileSystemFake {DocumentsPath = parent,};
            var fileHelper = new FileHelper(fileSystem);

            var path = fileHelper.InitialLogDirectory();

            Assert.AreEqual(parent + "\\" + FileHelper.DefaultDirectory, path);
        }
    }

    public class FullFileNameTests
    {
        [Test]
        public void FullFileName()
        {
            const string path = "C:\\A\\B\\C";
            const string expected = path + "\\" + "filename.log";
            var fileSystem = new FileSystemFake();
            var fileHelper = new FileHelper(fileSystem);

            var result = fileHelper.FullFileName(path, "filename", ".log");

            Assert.AreEqual(expected, result);
        }

        [Test]
        public void FullFileName_Exists()
        {
            const string path = "C:\\A\\B\\C";
            const string first = path + "\\" + "filename.log";
            const string expected = path + "\\" + "filename-1.log";
            var fileSystem = new FileSystemFake();
            fileSystem.Files.Add(first);
            var fileHelper = new FileHelper(fileSystem);

            var result = fileHelper.FullFileName(path, "filename", ".log");

            Assert.AreEqual(expected, result);
        }
    }
}
