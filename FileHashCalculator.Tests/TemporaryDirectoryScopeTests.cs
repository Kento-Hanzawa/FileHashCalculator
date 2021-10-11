using System.IO;
using Xunit;

namespace FileHashCalculator.Tests
{
    public class TemporaryDirectoryScopeTests
    {
        [Fact]
        public void ScopeCreate()
        {
            using var scope = new TemporaryDirectoryScope();
            Assert.True(scope.Directory.Exists);
        }

        [Fact]
        public void ScopeDispose()
        {
            string directoryName;
            using (var scope = new TemporaryDirectoryScope())
            {
                directoryName = scope.DirectoryName;
            }
            Assert.False(Directory.Exists(directoryName));
        }

        [Fact]
        public void ScopeDispose_NoDelete()
        {
            string directoryName;
            using (var scope = new TemporaryDirectoryScope(false))
            {
                directoryName = scope.DirectoryName;
            }
            Assert.True(Directory.Exists(directoryName));

            try
            {
                Directory.Delete(directoryName, true);
            }
            catch { }
        }

        [Fact]
        public void DirectoryInfoMatch()
        {
            using var scope = new TemporaryDirectoryScope();
            Assert.Equal(scope.DirectoryName, scope.Directory.FullName);
        }

        [Fact]
        public void ScopeCreate_SpecifyBaseDirectory()
        {
            var baseDirectory = Path.GetTempPath();
            using var scope = new TemporaryDirectoryScope(baseDirectory);
            Assert.True(scope.Directory.Exists);
        }

        [Fact]
        public void DirectoryInfoMatch_SpecifyBaseDirectory()
        {
            var baseDirectory = Path.GetTempPath();
            using var scope = new TemporaryDirectoryScope(baseDirectory);
            Assert.Equal(baseDirectory, scope.Directory.Parent?.FullName + '\\');
        }
    }
}
