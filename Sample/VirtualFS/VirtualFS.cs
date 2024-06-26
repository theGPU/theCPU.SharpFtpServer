﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using theCPU.SharpFtpServer.Utils;

namespace Sample.VirtualFS
{
    internal interface VirtualFsEntry
    {
        bool IsDirectory { get; }
        string Name { get; }
        long Length { get; }
        string Path { get; }
    }

    internal class VirtualFsDirectory : VirtualFsEntry
    {
        public bool IsDirectory => true;

        public VirtualFsDirectory? Parent { get; set; }
        public required string Name { get; set; }
        public long Length => Files.Sum(x => x.Length);
        public string Path => Parent == null ? "/" : Parent.Path == "/" ? $"/{Name}" : $"{Parent.Path}/{Name}";

        public List<VirtualFsDirectory> Directories { get; set; } = [];
        public List<VirtualFsFile> Files { get; set; } = [];

        public bool IsRoot => Parent != null;

        public IEnumerable<VirtualFsDirectory> GetDirectoriesRecursive() => Directories.Concat(Directories.SelectMany(x => x.Directories));
        public IEnumerable<VirtualFsDirectory> GetDirectoriesRecursiveWithRoot() => Enumerable.Repeat(this, 1).Concat(GetDirectoriesRecursive());
        public IEnumerable<VirtualFsFile> GetFilesRecursive() => Files.Concat(GetDirectoriesRecursiveWithRoot().SelectMany(x => x.Files));
        public IEnumerable<VirtualFsEntry> GetEntries() => Directories.AsEnumerable<VirtualFsEntry>().Concat(Files);
        public IEnumerable<VirtualFsEntry> GetEntriesRecursive() => GetDirectoriesRecursiveWithRoot().AsEnumerable<VirtualFsEntry>().Concat(GetFilesRecursive());

        public bool IsDirectoryExist(string path) => GetDirectoriesRecursiveWithRoot().Any(x => x.Path.Equals(path, StringComparison.InvariantCultureIgnoreCase));
        public bool IsFileExist(string path) => GetFilesRecursive().Any(x => x.Path.Equals(path, StringComparison.InvariantCultureIgnoreCase));

        public bool TryGetDirectory(string path, [MaybeNullWhen(false)] out VirtualFsDirectory dir)
        {
            dir = GetDirectoriesRecursiveWithRoot().FirstOrDefault(x => x.Path.Equals(path, StringComparison.InvariantCultureIgnoreCase));
            return dir != null;
        }

        public bool TryGetFile(string path, [MaybeNullWhen(false)] out VirtualFsFile file)
        {
            file = GetFilesRecursive().FirstOrDefault(x => x.Path.Equals(path, StringComparison.InvariantCultureIgnoreCase));
            return file != null;
        }

        public bool TryCreateFile(string path, byte[] data, bool createDirectories, [MaybeNullWhen(false)] out VirtualFsFile file)
        {
            using var ms = new MemoryStream(data);
            return TryCreateFile(path, ms, createDirectories, out file);
        }

        public bool TryCreateFile(string path, Stream data, bool createDirectories, [MaybeNullWhen(false)] out VirtualFsFile file)
        {
            file = null;
            VirtualFsDirectory? targetDir = null;

            if (!createDirectories && !TryGetDirectory(path, out targetDir))
                return false;

            if (createDirectories)
                targetDir = CreateDirectories($"/{string.Join('/', path.Split('/').Skip(1).SkipLast(1))}");

            if (targetDir == null)
                return false;

            DeleteFile(path);

            var ms = new MemoryStream();
            data.CopyTo(ms);
            ms.Seek(0, SeekOrigin.Begin);

            file = new() { Name = path.Split('/').Last(), Parent = targetDir!, Data = ms.ToArray() };

            //prevent stuck buffer in LOH
            var buffer = ms.GetBuffer();
            Array.Clear(buffer, 0, buffer.Length);
            ms.Position = 0;
            ms.SetLength(0);
            ms.Capacity = 0;
            ms.Dispose();

            targetDir!.Files.Add(file);
            return true;
        }

        public VirtualFsDirectory CreateDirectories(string path)
        {
            if (path == "/")
                return this;

            if (path.StartsWith("//"))
                path = path.Remove(0, 1);

            var pathParts = path.Split('/');
            var lastDir = this;
            for (var i = 0; i < pathParts.Length; i++)
            {
                var dirPath = $"/{string.Join('/', pathParts.Take(i + 1))}";
                if (dirPath.StartsWith("//"))
                    dirPath = dirPath.Remove(0, 1);
                if (!TryGetDirectory(dirPath, out var dir))
                {
                    dir = new() { Name = dirPath.Split('/').Last(), Parent = lastDir };
                    lastDir!.Directories.Add(dir);
                }

                lastDir = dir;
            }

            return lastDir;
        }

        public bool DeleteFile(string path)
        {
            if (!TryGetFile(path, out var file))
                return false;

            file!.Parent.Files.Remove(file);
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive, true, true);
            return true;
        }

        public bool DeleteDirectory(string path)
        {
            if (!TryGetDirectory(path, out var directory))
                return false;

            if (directory!.GetEntries().Any())
                return false;

            directory.Parent!.Directories.Remove(directory);
            return true;
        }
    }

    internal class VirtualFsFile : VirtualFsEntry
    {
        public bool IsDirectory => false;

        public required VirtualFsDirectory Parent { get; set; }
        public required string Name { get; set; }
        public long Length => Data.LongLength;
        public string Path => $"{Parent.Path}/{Name}".Replace("//", "/");

        public byte[] Data { get; set; } = [];
    }
}
