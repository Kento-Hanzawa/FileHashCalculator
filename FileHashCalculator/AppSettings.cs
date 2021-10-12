using FileHashCalculator.ConsoleAppCore;

namespace FileHashCalculator
{
    /// <summary>
    /// アプリケーションの設定を表すレコード。
    /// </summary>
    /// <remarks>このレコードは appsettings.json を元に作成され、<see cref="ConsoleApp(Microsoft.Extensions.Options.IOptions{AppSettings}, Microsoft.Extensions.Logging.ILogger{ConsoleApp})"/> コンストラクタを通じて値が渡されます。</remarks>
    internal sealed record AppSettings : AppSettingsCore
    {
        public static AppSettings Default { get; } = new AppSettings();
    }
}
