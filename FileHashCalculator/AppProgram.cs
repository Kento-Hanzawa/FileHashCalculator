using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ConsoleAppFramework;
using CyclicRedundancyChecks;
using FileHashCalculator.ConsoleAppCore;
using ZLogger;

namespace FileHashCalculator
{
    internal sealed class AppProgram : ConsoleApp
    {
        public AppProgram(Microsoft.Extensions.Options.IOptions<AppSettings> config, Microsoft.Extensions.Logging.ILogger<AppProgram> logger)
            : base(config, logger) { }


        [Command("file", "指定したファイルのハッシュ値を計算し、結果をファイルに出力します。")]
        public async ValueTask RunFile(
                  [Option("a", "ハッシュアルゴリズムの名前を指定します。大文字、小文字は区別しません。(CRC16, CRC32, CRC64, MD5, SHA1, SHA256, SHA384, SHA512)")] string algorithm,
                  [Option("i", "ハッシュ値を計算するファイルのパスを指定します。")] string input,
                  [Option("o", "計算されたハッシュ値の出力先ファイルパスを指定します。")] string output)
        {
            var inputFile = new FileInfo(input);
            var outputFile = new FileInfo(output);

            var hash = await Calculator.ComputeAsync(inputFile, () => GetAlgorithm(algorithm), Context.CancellationToken);
            Logger.ZLogInformation($"{inputFile.Name}");

            var builder = new StringBuilder();
            builder.Append($"{HexConverter.ToHexString(hash)}  {inputFile.Name}\n");

            await File.WriteAllTextAsync(outputFile.FullName, builder.ToString(), new UTF8Encoding(false), Context.CancellationToken);
            Logger.ZLogInformation($"OUTPUT: {outputFile.FullName}");
        }

        [Command("directory", "指定したディレクトリに含まれるファイルのハッシュ値を計算し、結果をファイルに出力します。")]
        public async ValueTask RunDirectory(
            [Option("a", "ハッシュアルゴリズムの名前を指定します。大文字、小文字は区別しません。(CRC16, CRC32, CRC64, MD5, SHA1, SHA256, SHA384, SHA512)")] string algorithm,
            [Option("i", "ハッシュ値を計算するディレクトリのパスを指定します。")] string input,
            [Option("o", "計算されたハッシュ値の出力先ファイルパスを指定します。")] string output,
            [Option("rx", "計算対象のファイル名に一致する正規表現を指定します。指定しない場合、全てのファイルが対象となります。")] string? regex = null,
            [Option("rc", "計算対象にサブフォルダ内のファイルを含めるかどうかを指定します。含める場合は true、含めない場合は false。")] bool recursive = false)
        {
            var inputDir = new DirectoryInfo(input);
            var outputFile = new FileInfo(output);

            Regex? rx = (regex is null) ? null : new Regex(regex, RegexOptions.Singleline);
            var files = inputDir.EnumerateFiles("*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                .AsParallel() //並列化
                .Where(file => rx?.IsMatch(file.Name) ?? true)
                .OrderBy(file => Path.GetRelativePath(inputDir.FullName, file.FullName));

            var builder = new StringBuilder();
            await foreach ((FileInfo File, byte[] Hash) in Calculator.ComputeAsync(files, () => GetAlgorithm(algorithm), Context.CancellationToken))
            {
                var relativePath = Path.GetRelativePath(inputDir.FullName, File.FullName).Replace(Path.DirectorySeparatorChar, Settings.Value.DirectorySeparatorChar);
                builder.Append($"{HexConverter.ToHexString(Hash)}  {relativePath}\n");
                Logger.ZLogInformation($"{relativePath}");
            }

            await File.WriteAllTextAsync(outputFile.FullName, builder.ToString(), new UTF8Encoding(false), Context.CancellationToken);
            Logger.ZLogInformation($"OUTPUT: {outputFile.FullName}");
        }


        private static HashAlgorithm GetAlgorithm(string algorithmName)
        {
            switch (algorithmName.ToUpperInvariant())
            {
                // Standard CRC
                case "CRC16":
                    return CRC.Create(Catalog.CRC16_ARC);
                case "CRC32":
                    return CRC.Create(Catalog.CRC32_ISO_HDLC);
                case "CRC64":
                    return CRC.Create(Catalog.CRC64_ECMA_182);

                case "MD5":
                    return MD5.Create();

                case "SHA1":
                    return SHA1.Create();
                case "SHA256":
                    return SHA256.Create();
                case "SHA384":
                    return SHA384.Create();
                case "SHA512":
                    return SHA512.Create();

                default:
                    throw new NotSupportedException($"指定したハッシュアルゴリズムはサポートされていません。AlgorithmName: {algorithmName}");
            }
        }
    }
}
