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

        public async ValueTask Run(
            [Option(0, "ハッシュ値を計算するファイル、または計算対象のファイルが含まれるディレクトリのパスを指定します。")] string Input,
            [Option("o", "計算されたハッシュ値をリストにしたテキストの出力先ファイルパスを指定します。指定しない場合は、'Input' の場所に '{FileName}.{Algorithm}.txt' の形式で出力します。")] string? Output = null,
            [Option("a", "ハッシュアルゴリズムの名前を指定します。大文字、小文字は区別しません。(CRC32, CRC64, MD5, SHA1, SHA256, SHA384, SHA512)")] string Algorithm = "SHA256",
            [Option("rx", "'Input' にディレクトリパスを指定した場合に有効になります。計算対象のファイルを選択するための正規表現を指定します。指定しない場合、全てのファイルが対象となります。")] string? Regex = null,
            [Option("rc", "'Input' にディレクトリパスを指定した場合に有効になります。計算対象にサブフォルダ内のファイルを含めるかどうかを指定します。含める場合は true、含めない場合は false。")] bool Recursive = false)
        {
            var attr = File.GetAttributes(Input);
            if (attr.HasFlag(FileAttributes.Directory))
            {
                var input = new DirectoryInfo(Input);
                var output = (Output is null)
                    ? new FileInfo(Path.Combine(input.Parent?.FullName!, $"{input.Name}.{Algorithm}.txt"))
                    : new FileInfo(Output);
                var regex = (Regex is null)
                    ? null
                    : new Regex(Regex, RegexOptions.Singleline);

                await RunCore(input, output, Algorithm, regex, Recursive, Context);

                static async ValueTask RunCore(DirectoryInfo input, FileInfo output, string algorithm, Regex? regex, bool recursive, ConsoleAppContext context)
                {
                    var files = input.EnumerateFiles("*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                        .AsParallel()
                        .Where(file => regex?.IsMatch(file.Name) ?? true)
                        .OrderBy(file => Path.GetRelativePath(input.FullName, file.FullName));

                    var builder = new StringBuilder();
                    await foreach ((FileInfo File, byte[] Hash) in Calculator.ComputeAsync(files, () => GetAlgorithm(algorithm), context.CancellationToken))
                    {
                        var relativePath = Path.GetRelativePath(input.FullName, File.FullName)
                            .Replace(Path.DirectorySeparatorChar, '/');
                        builder.Append($"{HexConverter.ToHexString(Hash)}  {relativePath}\n");
                        context.Logger.ZLogInformation($"{relativePath}");
                    }

                    await File.WriteAllTextAsync(output.FullName, builder.ToString(), new UTF8Encoding(false), context.CancellationToken);
                    context.Logger.ZLogInformation($"OUTPUT: {output.FullName}");
                }
            }
            else
            {
                var input = new FileInfo(Input);
                var output = Output is null
                    ? new FileInfo(Path.Combine(input.Directory?.FullName!, $"{input.Name}.{Algorithm}.txt"))
                    : new FileInfo(Output);

                await RunCore(input, output, Algorithm, Context);

                static async ValueTask RunCore(FileInfo input, FileInfo output, string algorithm, ConsoleAppContext context)
                {
                    var hash = await Calculator.ComputeAsync(input, () => GetAlgorithm(algorithm), context.CancellationToken);
                    context.Logger.ZLogInformation($"{input.Name}");

                    var builder = new StringBuilder();
                    builder.Append($"{HexConverter.ToHexString(hash)}  {input.Name}\n");
                    await File.WriteAllTextAsync(output.FullName, builder.ToString(), new UTF8Encoding(false), context.CancellationToken);
                    context.Logger.ZLogInformation($"OUTPUT: {output.FullName}");
                }
            }
        }

        private static HashAlgorithm GetAlgorithm(string algorithmName)
        {
            switch (algorithmName.ToUpperInvariant())
            {
                // 7z と同じ結果が出る実装
                case "CRC32":
                    return CRC.Create(Catalog.CRC32_ISO_HDLC, isBigEndian: true);
                case "CRC64":
                    return CRC.Create(Catalog.CRC64_XZ, isBigEndian: true);

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
