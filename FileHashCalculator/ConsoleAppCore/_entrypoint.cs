//
// Console Application Template-net5.0-2.0.2
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using ConsoleAppFramework;
using Cysharp.Text;
using FileHashCalculator;
using FileHashCalculator.ConsoleAppCore;
using FileHashCalculator.ConsoleAppCore.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ZLogger;
using AppSettings = FileHashCalculator.AppSettings;


#if DEBUG
// Swagger デバッグが実行された際の URL。
// ビルド構成が Debug の場合、Swagger によるデバッグが実行されます。
// F5 からデバッグを開始したのち、この URL にアクセスすることで Swagger ページを開くことができます。
// 詳しくは ConsoleAppFramework: Web Interface with Swagger を参照してください。
// https://github.com/Cysharp/ConsoleAppFramework#web-interface-with-swagger
const string SwaggerUrl = "http://localhost:12345";
#endif

// 出力するログの最小レベル。
#if DEBUG
const LogLevel MinimumLoggingLevel = LogLevel.Trace;
#else
const LogLevel MinimumLoggingLevel = LogLevel.Debug;
#endif


// 既定では .NET Core (.NET 5 以降も含む) で Shift-JIS などのコードページはサポートされていません。
// 事前に CodePagesEncodingProvider.Instance を Encoding へ登録しておくことで使用可能となります。
// 参考: https://docs.microsoft.com/ja-jp/dotnet/api/system.text.codepagesencodingprovider
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

await Host.CreateDefaultBuilder()
    .ConfigureServices((HostBuilderContext _, IServiceCollection services) =>
    {
        // appsettings.json の Section キーとして、現在の実行アセンブリ名を使用します。
        // 実行アセンブリ名が取得できなかった場合は設定を読み取ることが出来ないため、例外をスローします。
        var sectionName = Assembly.GetExecutingAssembly().GetName().Name
            ?? throw new InvalidOperationException("実行アセンブリの名前を取得できませんでした。");

        AddSettingsFile(services, sectionName, new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json")));
        static void AddSettingsFile(in IServiceCollection services, in string sectionName, in FileInfo file)
        {
            // 設定ファイルが存在しない場合は、初期値を構築してファイルを作成します。
            if (!file.Exists)
            {
                CreateDefault(sectionName, file.FullName);
                static void CreateDefault(string sectionName, string path)
                {
                    var settings = new Dictionary<string, object>
                    {
                        [sectionName] = AppSettings.Default
                    };
                    var options = new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
                    };
                    File.WriteAllText(path, JsonSerializer.Serialize(settings, options));
                }
            }

            var section = new ConfigurationBuilder()
                .SetBasePath(file.DirectoryName)
                .AddJsonFile(file.Name)
                .Build()
                .GetSection(sectionName);
            services.Configure<AppSettings>(section);
        }
    })
    .ConfigureLogging((HostBuilderContext _, ILoggingBuilder builder) =>
    {
        builder.ClearProviders();
        builder.SetMinimumLevel(MinimumLoggingLevel);

        AddLoggerConsole(builder);
        static void AddLoggerConsole(in ILoggingBuilder builder)
        {
            builder.AddZLoggerConsole(options =>
            {
                // Console への出力には、視認性向上の目的も兼ねて色付け処理をしておきます。
                // 色付けの設定についてはこちらのページを参照してください。 ( https://en.wikipedia.org/wiki/ANSI_escape_code#Colors )
                options.PrefixFormatter = (writer, info) =>
                {
                    switch (info.LogLevel)
                    {
                        case LogLevel.Trace:
                        case LogLevel.Debug:
                            // Foreground: Dark gray
                            ZString.Utf8Format(writer, "\u001b[38;5;08m[{0}] ", info.LogLevel.ToString()[0]);
                            break;

                        case LogLevel.Information:
                            // Foreground: White
                            ZString.Utf8Format(writer, "\u001b[38;5;15m[{0}] ", info.LogLevel.ToString()[0]);
                            break;

                        case LogLevel.Warning:
                            // Foreground: Light yellow
                            ZString.Utf8Format(writer, "\u001b[38;5;11m[{0}] ", info.LogLevel.ToString()[0]);
                            break;

                        case LogLevel.Error:
                            // Foreground: Light red
                            ZString.Utf8Format(writer, "\u001b[38;5;09m[{0}] ", info.LogLevel.ToString()[0]);
                            break;

                        case LogLevel.Critical:
                            // Foreground: White, Background: Red
                            ZString.Utf8Format(writer, "\u001b[38;5;15;41m[{0}] ", info.LogLevel.ToString()[0]);
                            break;

                        default:
                            // Default
                            ZString.Utf8Format(writer, "\u001b[0m[{0}] ", info.LogLevel.ToString()[0]);
                            break;
                    }
                };
                options.SuffixFormatter = (writer, info) =>
                {
                    // Reset
                    ZString.Utf8Format(writer, "\u001b[0m", "");
                };
                options.ExceptionFormatter = (writer, exception) =>
                {
                    // Foreground: White, Background: Red + Reset
                    ZString.Utf8Format(writer, "\n\u001b[38;5;15;41m{0}\u001b[0m", exception);
                };
            }, configureEnableAnsiEscapeCode: true);
        }

        AddLoggerFile(builder);
        static void AddLoggerFile(in ILoggingBuilder builder)
        {
            var file = LogFileInfo.LogFile;
            if (file.Exists)
            {
                // 1/10000000 秒単位まで一致したログファイルが既に存在した場合、例外をスローします。
                // 複数プロセスから同一ファイルへログの書き込みを行った場合、書き込みタイミングによってログの欠損が発生する場合があります。
                throw new InvalidOperationException("ログファイルの作成に失敗しました。");
            }

            builder.AddZLoggerFile(
                file.FullName,
                options =>
                {
                    // ログのプレフィックスを設定。("yyyy-MM-dd HH:mm:ss.fff ±zzz  (LogLevel)  ")
                    var prefixFormat = ZString.PrepareUtf8<int, int, int, int, int, int, int, char, int, int, LogLevel>("{0:D4}-{1:D2}-{2:D2} {3:D2}:{4:D2}:{5:D2}.{6:D3} {7}{8:D2}:{9:D2}  {10,-11}  ");
                    options.PrefixFormatter = (writer, info) =>
                    {
                        var local = info.Timestamp.ToLocalTime();
                        prefixFormat.FormatTo(
                            ref writer,
                            local.Year,        // 0
                            local.Month,       // 1
                            local.Day,         // 2
                            local.Hour,        // 3
                            local.Minute,      // 4
                            local.Second,      // 5
                            local.Millisecond, // 6
                            local.Offset.Ticks < 0 ? '-' : '+', // 7
                            Math.Abs(local.Offset.Hours),       // 8
                            Math.Abs(local.Offset.Minutes),     // 9
                            info.LogLevel      // 10
                        );
                    };
                });
        }
    })
#if DEBUG
    .RunConsoleAppFrameworkWebHostingAsync(SwaggerUrl);
#else
    .RunConsoleAppFrameworkAsync<AppProgram>(args, new ConsoleAppOptions()
    {
        StrictOption = true,
        GlobalFilters = new ConsoleAppFilter[]
        {
            new LogEnvironmentInfoFilter() { Order = int.MinValue + 1 },
            new LogRotateFilter() { Order = int.MinValue + 1 },
            new LogRunningTimeFilter() { Order = int.MinValue + 1 },
            new LogFiltersBorderFilter() { Order = int.MaxValue },
        }
    });
#endif
