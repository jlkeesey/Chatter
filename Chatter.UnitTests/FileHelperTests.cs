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

            Assert.True(result);
        }

        [Test]
        public void EnsureDirectoryExists_FileExists()
        {
            const string path = "C:\\A\\B\\C";
            var fileSystem = new FileSystemFake();
            fileSystem.Files.Add(path);
            var fileHelper = new FileHelper(fileSystem);

            var result = fileHelper.EnsureDirectoryExists(path);

            Assert.False(result);
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

            Assert.True(result, "Directory was created");
            Assert.True(fileSystem.DirectoryExists(path), "Directory now exists");
        }

        [Test]
        public void EnsureDirectoryExists_ParentDoesNotExist()
        {
            const string path = "C:\\A\\B\\C";
            var fileSystem = new FileSystemFake();
            var fileHelper = new FileHelper(fileSystem);

            var result = fileHelper.EnsureDirectoryExists(path);

            Assert.False(result);
        }
    }

    public class InitialLogDirectoryTests
    {
        [Test]
        public void InitialLogDirectory()
        {
            const string parent = "C:\\A\\B";
            var fileSystem = new FileSystemFake
            {
                DocumentsPath = parent,
            };
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