using System;
using System.Threading.Tasks;
using ConsoleAppFramework;
using ZLogger;

namespace FileHashCalculator.ConsoleAppCore.Filters
{
    /// <summary>
    /// 文字列 "-----------------------------" を出力します。
    /// </summary>
    /// <remarks>このフィルターは、フィルター内で出力されたログと実際の処理の中で呼び出されたログを区別しやすくするために使用しています。</remarks>
    internal sealed class LogFiltersBorderFilter : ConsoleAppFilter
    {
        public override async ValueTask Invoke(ConsoleAppContext context, Func<ConsoleAppContext, ValueTask> next)
        {
            context.Logger.ZLogDebug("-----------------------------");
            try
            {
                await next(context);
            }
            finally
            {
                context.Logger.ZLogDebug("-----------------------------");
            }
        }
    }
}
