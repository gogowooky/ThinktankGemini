using System;
using System.IO;

namespace ThinktankApp
{
    public abstract class TTApplicationResource : TTApplicationBase
    {
        public string BaseDir { get; private set; }

        private string _memoDir;
        public string MemoDir
        {
            get { return _memoDir; }
            set
            {
                _memoDir = value;
                if (!string.IsNullOrEmpty(_memoDir))
                {
                    if (!Directory.Exists(_memoDir)) Directory.CreateDirectory(_memoDir);
                    string cacheDir = Path.Combine(_memoDir, "gcache");
                    if (!Directory.Exists(cacheDir)) Directory.CreateDirectory(cacheDir);
                    string backupDir = Path.Combine(_memoDir, "gbackup");
                    if (!Directory.Exists(backupDir)) Directory.CreateDirectory(backupDir);
                }
            }
        }

        private string _linkDir;
        public string LinkDir
        {
            get { return _linkDir; }
            set
            {
                _linkDir = value;
                if (!string.IsNullOrEmpty(_linkDir) && !Directory.Exists(_linkDir))
                {
                    Directory.CreateDirectory(_linkDir);
                }
            }
        }

        private string _chatDir;
        public string ChatDir
        {
            get { return _chatDir; }
            set
            {
                _chatDir = value;
                if (!string.IsNullOrEmpty(_chatDir) && !Directory.Exists(_chatDir))
                {
                    Directory.CreateDirectory(_chatDir);
                }
            }
        }

        public string PCName { get; private set; }
        public string UserName { get; private set; }

        public TTApplicationResource(string xamlPath, string stylePath, string scriptDir)
            : base(xamlPath, stylePath)
        {
            BaseDir = Path.GetDirectoryName(scriptDir);
            MemoDir = Path.GetFullPath(Path.Combine(BaseDir, "..", "Memo"));
            LinkDir = Path.GetFullPath(Path.Combine(BaseDir, "..", "Link"));
            ChatDir = Path.GetFullPath(Path.Combine(BaseDir, "..", "Chat"));

            PCName = Environment.MachineName;
            UserName = Environment.UserName;
        }
    }
}
