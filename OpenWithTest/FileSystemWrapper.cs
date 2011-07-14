using System;
using System.IO;

namespace MattManela.OpenWithTest
{
    public interface IFileSystemWrapper
    {
        FileStream Open(string path, FileMode mode, FileAccess access);
        bool Exists(string path);
        DirectoryInfo CreateDirectory(string path);
        void DeleteFile(string path);
    }

    internal class FileSystemWrapper : IFileSystemWrapper
    {
        public FileStream Open(string path, FileMode mode, FileAccess access)
        {
            return File.Open(path, mode, access);
        }

        public bool Exists(string path)
        {
            return File.Exists(path);
        }

        public DirectoryInfo CreateDirectory(string path)
        {
            return Directory.CreateDirectory(path);
        }

        public void DeleteFile(string path)
        {
            File.Delete(path);
        }
    }
}