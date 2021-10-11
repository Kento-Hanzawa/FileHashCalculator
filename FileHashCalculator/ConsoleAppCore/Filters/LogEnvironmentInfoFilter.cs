using System;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;
using ConsoleAppFramework;
using ZLogger;

namespace FileHashCalculator.ConsoleAppCore.Filters
{
    /// <summary>
    /// 現在のマシンの環境情報、実行された <see cref="AssemblyName"/>、コマンドライン引数、<see cref="MethodInfo"/> などの情報を出力します。
    /// </summary>
    internal sealed class LogEnvironmentInfoFilter : ConsoleAppFilter
    {
        public override async ValueTask Invoke(ConsoleAppContext context, Func<ConsoleAppContext, ValueTask> next)
        {
            var options = new JsonSerializerOptions() { Encoder = JavaScriptEncoder.Create(UnicodeRanges.All) };

            // 実行マシン
            context.Logger.ZLogDebug("# OSVersion     : {0}", Environment.OSVersion);
            context.Logger.ZLogDebug("# MachineName   : {0}", Environment.MachineName);
            context.Logger.ZLogDebug("# UserName      : {0}", Environment.UserName);
            context.Logger.ZLogDebug("# RuntimeVersion: {0}", Environment.Version);

            // 実行アセンブリ
            context.Logger.ZLogDebug("# AssemblyName  : {0}", Assembly.GetExecutingAssembly().FullName);

            // 引数
            context.Logger.ZLogDebug("# ExecutingFile : {0}", Environment.GetCommandLineArgs()[0]);
            context.Logger.ZLogInformation("# Arguments     : {0}", JsonSerializer.Serialize(context.Arguments, options));
            // 実行メソッド
            context.Logger.ZLogDebug("# DeclaringType : {0}", context.MethodInfo.DeclaringType);
            context.Logger.ZLogInformation("# MethodInfo    : {0}", context.MethodInfo);

            // ConsoleAppBehavior
            context.Logger.ZLogInformation("# AppSettings   : {0}", JsonSerializer.Serialize(ConsoleApp.Current?.Settings.Value, options));

            await next(context);
        }
    }
}
