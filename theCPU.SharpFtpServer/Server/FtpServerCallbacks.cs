﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using theCPU.SharpFtpServer.POCO;

namespace theCPU.SharpFtpServer.Server
{
    public record DirectoryListResponse(bool IsDirectoryExist, IEnumerable<FtpEntryInfo>? Entries);
    public record DownloadFileResponse(Stream? Stream, bool AutoDispose = true);

    public delegate bool CheckUsernameDelegte(string username);
    public delegate bool CheckPasswordDelegte(string username, string password);

    public delegate bool DirectoryExistDelegate(string username, string path);
    public delegate bool FileExistDelegate(string username, string path);

    public delegate string GetDefaultWorkingDirectoryDelegate(string username);
    public delegate DirectoryListResponse GetDirectoryEntriesDelegate(string username, string path);

    public delegate DownloadFileResponse DownloadFileDelegate(string username, string path);
    public delegate bool DeleteFileDelegate(string username, string path);
    public delegate bool FileRenameDelegate(string username, string oldPath, string newPath);
    public delegate bool CanCreateFile(string username, string path);
    public delegate bool CreateFile(string username, string path, byte[] bytes);
    public delegate bool CreateDirectory(string username, string path);

    public class FtpServerCallbacks
    {
        public event CheckUsernameDelegte OnCheckUsername = null!;
        public event CheckPasswordDelegte OnCheckPassword = null!;

        public event DirectoryExistDelegate OnDirectoryExist = null!;
        public event FileExistDelegate OnFileExist = null!;

        public event GetDefaultWorkingDirectoryDelegate OnGetDefaultWorkingDirectory = null!;
        public event GetDirectoryEntriesDelegate OnGetDirectoryEntries = null!;

        public event DownloadFileDelegate OnDownloadFile = null!;
        public event DeleteFileDelegate OnDeleteFile = null!;
        public event FileRenameDelegate OnFileRename = null!;
        public event CanCreateFile OnCanCreateFile = null!;
        public event CreateFile OnCreateFile = null!;
        public event CreateDirectory OnCreateDirectory = null!;

        public bool CheckUsername(string username) => OnCheckUsername.Invoke(username);
        public bool CheckPassword(string username, string password) => OnCheckPassword.Invoke(username, password);

        public bool DirectoryExist(string username, string path) => OnDirectoryExist.Invoke(username, path);
        public bool FileExist(string username, string path) => OnFileExist.Invoke(username, path);

        public string GetDefaultWorkingDirectory(string username) => OnGetDefaultWorkingDirectory.Invoke(username);
        public DirectoryListResponse GetDirectoryEntries(string username, string path) => OnGetDirectoryEntries.Invoke(username, path);

        public DownloadFileResponse GetDownloadStream(string username, string path) => OnDownloadFile.Invoke(username, path);
        public bool DeleteFile(string username, string path) => OnDeleteFile.Invoke(username, path);
        public bool RenameFile(string username, string oldPath, string newPath) => OnFileRename(username, oldPath, newPath);
        public bool CanCreateFile(string username, string path) => OnCanCreateFile(username, path);
        public bool CreateFile(string username, string path, byte[] bytes) => OnCreateFile(username, path, bytes);
        public bool CreateDirectory(string username, string path) => OnCreateDirectory(username, path);

        public IEnumerable<string> GetUnlinkedCallbacks() => this.GetType()
            .GetEvents((BindingFlags)60)
            .Where(x => this.GetType().GetField(x.Name, (BindingFlags)60)!.GetValue(this) == null)
            .Select(x => x.Name);
    }
}