using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace FileHashCalculator.Tests
{
    public class CalculatorTests
    {
        [Theory]
        [InlineData("MD5")]
        [InlineData("SHA1")]
        [InlineData("SHA256")]
        [InlineData("SHA384")]
        [InlineData("SHA512")]
        public void FileHash_Compute(string algorithmName)
        {
            byte[] data = Encoding.ASCII.GetBytes("123456789");
            byte[] expected, actual;

            using (var alg = HashAlgorithm.Create(algorithmName))
            {
                expected = alg.ComputeHash(data);
            }

            using (var scope = new TemporaryDirectoryScope())
            {
                var file = new FileInfo(Path.Combine(scope.DirectoryName, Path.GetRandomFileName()));
                using (var writer = file.Create())
                {
                    writer.Write(data);
                }
                actual = Calculator.Compute(file, () => HashAlgorithm.Create(algorithmName));
            }

            actual.Should().Equal(expected);
        }

        [Fact]
        public void FileHash_Compute_ArgumentNull()
        {
            Assert.Throws<ArgumentNullException>(() => Calculator.Compute((FileInfo)null, () => HashAlgorithm.Create("SHA256")));
            Assert.Throws<ArgumentNullException>(() => Calculator.Compute(new FileInfo(Path.GetRandomFileName()), null));
        }

        [Fact]
        public void FileHash_Compute_FileNotFound()
        {
            Assert.Throws<FileNotFoundException>(() => Calculator.Compute(new FileInfo(Path.GetRandomFileName()), () => HashAlgorithm.Create("SHA256")));
        }

        [Theory]
        [InlineData("MD5")]
        [InlineData("SHA1")]
        [InlineData("SHA256")]
        [InlineData("SHA384")]
        [InlineData("SHA512")]
        public void FileHash_ComputeMultiple(string algorithmName)
        {
            var dataList = new List<byte[]>
            {
                Encoding.ASCII.GetBytes("123456789"),
                Encoding.ASCII.GetBytes("ABCDEFGHI"),
                Encoding.ASCII.GetBytes("JKLMNOPQR"),
                Encoding.ASCII.GetBytes("STUVWXYZ_"),
                Encoding.ASCII.GetBytes("+*><?=!~]"),
            };
            List<byte[]> expected = new(), actual = new();

            foreach (var data in dataList)
            {
                using (var alg = HashAlgorithm.Create(algorithmName))
                {
                    expected.Add(alg.ComputeHash(data));
                }
            }

            using (var scope = new TemporaryDirectoryScope())
            {
                var fileList = new List<FileInfo>();
                foreach (var data in dataList)
                {
                    var file = new FileInfo(Path.Combine(scope.DirectoryName, Path.GetRandomFileName()));
                    using (var writer = file.Create())
                    {
                        writer.Write(data);
                    }
                    fileList.Add(file);
                }

                foreach ((FileInfo File, byte[] Hash) in Calculator.Compute(fileList.OrderBy(f => f.CreationTime), () => HashAlgorithm.Create(algorithmName)))
                {
                    actual.Add(Hash);
                }
            }

            for (var i = 0; i < dataList.Count; i++)
            {
                actual[i].Should().Equal(expected[i]);
            }
        }

        [Fact]
        public void FileHash_ComputeMultiple_ArgumentNull()
        {
            Assert.Throws<ArgumentNullException>(() => Calculator.Compute((FileInfo[])null, () => HashAlgorithm.Create("SHA256")).ToArray());
            Assert.Throws<ArgumentNullException>(() => Calculator.Compute(Array.Empty<FileInfo>(), null).ToArray());
        }

        [Fact]
        public void FileHash_ComputeMultiple_FileNotFound()
        {
            Assert.Throws<FileNotFoundException>(() => Calculator.Compute(new[] { new FileInfo(Path.GetRandomFileName()) }, () => HashAlgorithm.Create("SHA256")).ToArray());
        }

        [Theory]
        [InlineData("MD5")]
        [InlineData("SHA1")]
        [InlineData("SHA256")]
        [InlineData("SHA384")]
        [InlineData("SHA512")]
        public async Task FileHash_ComputeAsync(string algorithmName)
        {
            byte[] data = Encoding.ASCII.GetBytes("123456789");
            byte[] expected, actual;

            using (var alg = HashAlgorithm.Create(algorithmName))
            {
                expected = alg.ComputeHash(data);
            }

            using (var scope = new TemporaryDirectoryScope())
            {
                var file = new FileInfo(Path.Combine(scope.DirectoryName, Path.GetRandomFileName()));
                using (var writer = file.Create())
                {
                    writer.Write(data);
                }
                actual = await Calculator.ComputeAsync(file, () => HashAlgorithm.Create(algorithmName));
            }

            actual.Should().Equal(expected);
        }

        [Fact]
        public async Task FileHash_ComputeAsync_ArgumentNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => Calculator.ComputeAsync((FileInfo)null, () => HashAlgorithm.Create("SHA256")));
            await Assert.ThrowsAsync<ArgumentNullException>(() => Calculator.ComputeAsync(new FileInfo(Path.GetRandomFileName()), null));
        }

        [Fact]
        public async Task FileHash_ComputeAsync_FileNotFound()
        {
            await Assert.ThrowsAsync<FileNotFoundException>(() => Calculator.ComputeAsync(new FileInfo(Path.GetRandomFileName()), () => HashAlgorithm.Create("SHA256")));
        }

        [Theory]
        [InlineData("MD5")]
        [InlineData("SHA1")]
        [InlineData("SHA256")]
        [InlineData("SHA384")]
        [InlineData("SHA512")]
        public async Task FileHash_ComputeAsyncMultiple(string algorithmName)
        {
            var dataList = new List<byte[]>
            {
                Encoding.ASCII.GetBytes("123456789"),
                Encoding.ASCII.GetBytes("ABCDEFGHI"),
                Encoding.ASCII.GetBytes("JKLMNOPQR"),
                Encoding.ASCII.GetBytes("STUVWXYZ_"),
                Encoding.ASCII.GetBytes("+*><?=!~]"),
            };
            List<byte[]> expected = new(), actual = new();

            foreach (var data in dataList)
            {
                using (var alg = HashAlgorithm.Create(algorithmName))
                {
                    expected.Add(alg.ComputeHash(data));
                }
            }

            using (var scope = new TemporaryDirectoryScope())
            {
                var fileList = new List<FileInfo>();
                foreach (var data in dataList)
                {
                    var file = new FileInfo(Path.Combine(scope.DirectoryName, Path.GetRandomFileName()));
                    using (var writer = file.Create())
                    {
                        writer.Write(data);
                    }
                    fileList.Add(file);
                }
                await foreach ((FileInfo File, byte[] Hash) in Calculator.ComputeAsync(fileList.OrderBy(f => f.CreationTime), () => HashAlgorithm.Create(algorithmName)))
                {
                    actual.Add(Hash);
                }
            }

            for (var i = 0; i < dataList.Count; i++)
            {
                actual[i].Should().Equal(expected[i]);
            }
        }

        [Fact]
        public async Task FileHash_ComputeAsyncMultiple_ArgumentNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(async () => { await foreach (var _ in Calculator.ComputeAsync((FileInfo[])null, () => HashAlgorithm.Create("SHA256"))) { } });
            await Assert.ThrowsAsync<ArgumentNullException>(async () => { await foreach (var _ in Calculator.ComputeAsync(Array.Empty<FileInfo>(), null)) { } });
        }

        [Fact]
        public async Task FileHash_ComputeAsyncMultiple_FileNotFound()
        {
            await Assert.ThrowsAsync<FileNotFoundException>(async () => { await foreach (var _ in Calculator.ComputeAsync(new[] { new FileInfo(Path.GetRandomFileName()) }, () => HashAlgorithm.Create("SHA256"))) { } });
        }
    }
}
