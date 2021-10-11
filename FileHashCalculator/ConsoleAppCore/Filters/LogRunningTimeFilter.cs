using System;
using System.Threading.Tasks;
using ConsoleAppFramework;
using ZLogger;

namespace FileHashCalculator.ConsoleAppCore.Filters
{
    /// <summary>
    /// アプリの実行に要した時間を出力します。
    /// </summary>
    internal sealed class LogRunningTimeFilter : ConsoleAppFilter
    {
        public override async ValueTask Invoke(ConsoleAppContext context, Func<ConsoleAppContext, ValueTask> next)
        {
            context.Logger.ZLogDebug("処理開始: {0}", context.Timestamp.ToLocalTime().ToString("yyyy/MM/dd HH:mm:ss"));
            try
            {
                await next(context);
                context.Logger.ZLogDebug("処理は正常に終了しました。経過時間: {0}", DateTimeOffset.UtcNow - context.Timestamp);
            }
            catch
            {
                context.Logger.ZLogError("処理は失敗で終了しました。経過時間: {0}", DateTimeOffset.UtcNow - context.Timestamp);
                throw;
            }
        }
    }
}
