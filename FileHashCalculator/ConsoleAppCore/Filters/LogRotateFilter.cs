using System;
using System.IO;
using System.Threading.Tasks;
using ConsoleAppFramework;
using ZLogger;

namespace FileHashCalculator.ConsoleAppCore.Filters
{
    /// <summary>
    /// ログローテート (一定期間以上古いログを削除する処理) を実行します。
    /// </summary>
    /// <remarks>ログローテート処理のオプションとして <see cref="AppSettingsCore.EnableLogRotate"/> 及び、<see cref="AppSettingsCore.LogRotateKeepDays"/> パラメータを使用しています。</remarks>
    internal sealed class LogRotateFilter : ConsoleAppFilter
    {
        public override async ValueTask Invoke(ConsoleAppContext context, Func<ConsoleAppContext, ValueTask> next)
        {
            AppSettings? settings = ConsoleApp.Current?.Settings.Value;
            if (settings is not null && settings.EnableLogRotate)
            {
                string lockFileName = Path.Combine(LogFileInfo.ParentDirectory.FullName, "log-rotate.lockfile");

                bool ready;
                if (File.Exists(lockFileName))
                {
                    // lockfile が既に存在する場合は、lockfile の作成日時を見て、現在との差が 1 日以内であればログローテートをスキップします。
                    // それ以上の差がある場合は、前回の処理で lockfile の削除が正常に行われなかった可能性が高いので、
                    // lockfile を再生成し、ログローテートを開始します。
                    if (TimeSpan.FromDays(1) < (DateTimeOffset.UtcNow.ToLocalTime() - File.GetCreationTime(lockFileName)))
                    {
                        File.Delete(lockFileName);
                        ready = true;
                        context.Logger.ZLogDebug("既に存在している 'log-rotate.lockfile' の作成日から 1 日以上経過しているため、lockfile を再生成してログローテートを開始します。");
                    }
                    else
                    {
                        ready = false;
                        context.Logger.ZLogDebug("'log-rotate.lockfile' が既に存在しているため、ログローテートをスキップします。");
                    }
                }
                else
                {
                    ready = true;
                }

                if (ready)
                {
                    try
                    {
                        using (File.Create(lockFileName))
                        {
                            LogFileInfo.LogRotate(settings.LogRotateKeepDays, context.Logger);
                        }
                    }
                    finally
                    {
                        File.Delete(lockFileName);
                    }
                }
            }
            await next(context);
        }
    }
}
