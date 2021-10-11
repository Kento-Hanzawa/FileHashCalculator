using System;
using System.IO;

namespace FileHashCalculator
{
    public sealed class TemporaryDirectoryScope : IDisposable
    {
        private readonly bool autoDelete;

        public string DirectoryName { get; private set; }
        public DirectoryInfo Directory => new DirectoryInfo(DirectoryName);

        public TemporaryDirectoryScope(bool autoDelete = true)
            : this(Path.GetTempPath(), autoDelete)
        {
        }

        public TemporaryDirectoryScope(string baseDirectoryName, bool autoDelete = true)
        {
            this.autoDelete = autoDelete;
            do
            {
                DirectoryName = Path.Combine(baseDirectoryName, Path.GetRandomFileName());
            } while (System.IO.Directory.Exists(DirectoryName));
            System.IO.Directory.CreateDirectory(DirectoryName);
        }

        public void Dispose()
        {
            if (autoDelete && System.IO.Directory.Exists(DirectoryName))
            {
                System.IO.Directory.Delete(DirectoryName, true);
            }
        }
    }
}
