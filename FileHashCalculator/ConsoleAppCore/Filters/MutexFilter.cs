using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ConsoleAppFramework;
using Cysharp.Text;
using ZLogger;

namespace FileHashCalculator.ConsoleAppCore.Filters
{
    /// <summary>
    /// 複数のプロセスによる重複実行を避けるための機能を提供します。
    /// </summary>
    /// <remarks>参考: <see href="https://github.com/Cysharp/ConsoleAppFramework#filter"/></remarks>
    internal sealed class MutexFilter : ConsoleAppFilter
    {
        public async override ValueTask Invoke(ConsoleAppContext context, Func<ConsoleAppContext, ValueTask> next)
        {
            var name = $"{Assembly.GetExecutingAssembly().GetName().Name}.{context.MethodInfo.DeclaringType?.ToString() ?? "(UnknownDeclaringType)"}.{context.MethodInfo.Name}";
            context.Logger.ZLogDebug("# Mutex Identifier: {0}", name);

            using (var mutex = new Mutex(true, name, out var createdNew))
            {
                if (!createdNew)
                {
                    context.Logger.ZLogError("プロセス名 {0} は既に実行されています。", name);
                    throw new MultipleExecutionException(name);
                }

                await next(context);
            }
        }
    }

    internal sealed class MultipleExecutionException : Exception
    {
        private const string MessageFormat = "プロセス名 {0} は既に実行されています。プロセスの完了を待つか、終了処理を行ってください。";

        public string? MutexName { get; }

        public MultipleExecutionException()
            : this(null)
        {
        }

        public MultipleExecutionException(string? mutexName)
            : base(ZString.Format(MessageFormat, mutexName))
        {
            MutexName = mutexName;
        }

        public MultipleExecutionException(string? mutexName, Exception? innerException)
            : base(ZString.Format(MessageFormat, mutexName), innerException)
        {
            MutexName = mutexName;
        }

        public MultipleExecutionException(string? mutexName, string? message)
            : base(message)
        {
            MutexName = mutexName;
        }

        public MultipleExecutionException(string? mutexName, string? message, Exception? innerException)
            : base(message, innerException)
        {
            MutexName = mutexName;
        }
    }
}
