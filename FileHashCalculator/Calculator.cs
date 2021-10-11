using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace FileHashCalculator
{
    public static class Calculator
    {
        public static byte[] Compute(FileInfo file, Func<HashAlgorithm> algorithmSelector)
        {
            if (file is null)
                throw new ArgumentNullException(nameof(file));
            if (algorithmSelector is null)
                throw new ArgumentNullException(nameof(algorithmSelector));
            if (!file.Exists)
                throw new FileNotFoundException("ファイルが見つかりませんでした。", file.FullName);

            using (var algorithm = algorithmSelector())
            using (var stream = file.OpenRead())
            {
                return algorithm.ComputeHash(stream);
            }
        }

        public static async Task<byte[]> ComputeAsync(FileInfo file, Func<HashAlgorithm> algorithmSelector, CancellationToken cancellationToken = default)
        {
            if (file is null)
                throw new ArgumentNullException(nameof(file));
            if (algorithmSelector is null)
                throw new ArgumentNullException(nameof(algorithmSelector));
            if (!file.Exists)
                throw new FileNotFoundException("ファイルが見つかりませんでした。", file.FullName);

            using (var algorithm = algorithmSelector())
            using (var stream = file.OpenRead())
            {
                return await algorithm.ComputeHashAsync(stream, cancellationToken);
            }
        }

        public static IEnumerable<(FileInfo File, byte[] Hash)> Compute(IEnumerable<FileInfo> files, Func<HashAlgorithm> algorithmSelector)
        {
            if (files is null)
                throw new ArgumentNullException(nameof(files));
            if (algorithmSelector is null)
                throw new ArgumentNullException(nameof(algorithmSelector));

            foreach (var file in files)
            {
                yield return (file, Compute(file, algorithmSelector));
            }
        }

        public static async IAsyncEnumerable<(FileInfo File, byte[] Hash)> ComputeAsync(IEnumerable<FileInfo> files, Func<HashAlgorithm> algorithmSelector, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (files is null)
                throw new ArgumentNullException(nameof(files));
            if (algorithmSelector is null)
                throw new ArgumentNullException(nameof(algorithmSelector));

            foreach (var file in files)
            {
                yield return (file, await ComputeAsync(file, algorithmSelector, cancellationToken));
                cancellationToken.ThrowIfCancellationRequested();
            }
        }
    }
}
