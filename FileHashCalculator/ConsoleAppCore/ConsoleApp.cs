using System;
using System.Threading.Tasks;
using ConsoleAppFramework;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ZLogger;

namespace FileHashCalculator.ConsoleAppCore
{
    /// <summary>
    /// <see cref="ConsoleAppBase"/> から派生する全てのクラスの基底となるクラスです。
    /// </summary>
    internal class ConsoleApp : ConsoleAppBase, IAsyncDisposable
    {
        /// <summary>
        /// 現在実行中の <see cref="ConsoleApp"/> インスタンスを取得します。
        /// </summary>
        internal static ConsoleApp? Current { get; private set; }

        /// <summary>
        /// appsettings.json ファイルから読み込まれたアプリケーション設定を取得します。
        /// </summary>
        internal IOptions<AppSettings> Settings { get; private set; }

        /// <summary>
        /// 現在実行中の <see cref="ConsoleApp"/> インスタンスに紐付けられた <see cref="ILogger{TCategoryName}"/> インスタンスを取得します。
        /// </summary>
        internal ILogger<ConsoleApp> Logger { get; private set; }

        /// <summary>
        /// <see cref="ConsoleApp"/> クラスの新しいインスタンスを作成します。
        /// </summary>
        /// <param name="settings"><see cref="IOptions{TOptions}"/> のインスタンス。この変数は GenericHost による DI 機能によって値が渡されます。</param>
        /// <param name="logger"><see cref="ILogger{TCategoryName}"/> のインスタンス。この変数は GenericHost による DI 機能によって値が渡されます。</param>
        internal ConsoleApp(IOptions<AppSettings> settings, ILogger<ConsoleApp> logger)
        {
            logger.ZLogDebug("{0} Calling .ctor", GetType().FullName);
            Settings = settings;
            Logger = logger;
            Current = this;
        }

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            Logger.ZLogDebug("{0} Cleanup Start", GetType().FullName);
            try
            {
                await Cleanup();
            }
            catch
            {
                Logger.ZLogError("{0} Cleanup Failed", GetType().FullName);
                throw;
            }
            Logger.ZLogDebug("{0} Cleanup Complete", GetType().FullName);
        }

        /// <summary>
        /// クリーンアップ処理。
        /// </summary>
        /// <remarks>このメソッドは、メインの処理が実行された後に <see cref="ConsoleAppBase"/> 側から呼び出されます。</remarks>
        /// <returns></returns>
        protected virtual ValueTask Cleanup() => default;
    }
}
