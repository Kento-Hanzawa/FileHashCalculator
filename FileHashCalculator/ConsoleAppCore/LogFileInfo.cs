using System;
using System.Globalization;
using System.IO;
using Microsoft.Extensions.Logging;
using ZLogger;

namespace FileHashCalculator.ConsoleAppCore
{
    /// <summary>
    /// ログファイルに関する情報を保持するクラス。
    /// </summary>
    internal class LogFileInfo
    {
        /// <summary>
        /// ログの親ディレクトリの名前。
        /// </summary>
        private const string ParentDirectoryName = "logs";

        /// <summary>
        /// ログの世代別ディレクトリ名に使用する日付フォーマット。
        /// </summary>
        private const string GenerationDateTimeFormat = "yyyy-MM-dd";

        /// <summary>
        /// ログのファイル名に使用する日付フォーマット。
        /// </summary>
        private const string FileNameDateTimeFormat = "yyyyMMddTHHmmss.fffffff";

        /// <summary>
        /// ログファイルの拡張子。
        /// </summary>
        private const string FileExtension = ".log";


        // Singleton
        private static LogFileInfo? _instance;
        private static LogFileInfo Instance
        {
            get
            {
                if (_instance is null)
                {
                    _instance = new LogFileInfo();
                }
                return _instance;
            }
        }


        // これらの変数は DirectoryInfo, FileInfo のインスタンス作成用に内部でのみ使用します。
        private readonly DateTimeOffset _timestamp;
        private readonly string _parentDirectoryFullName;
        private readonly string _generationDirectoryFullName;
        private readonly string _logFileFullName;


        /// <summary>
        /// ログの親ディレクトリを表す <see cref="DirectoryInfo"/> インスタンスを取得します。
        /// </summary>
        public static DirectoryInfo ParentDirectory => new(Instance._parentDirectoryFullName);

        /// <summary>
        /// ログの世代ディレクトリを表す <see cref="DirectoryInfo"/> インスタンスを取得します。
        /// </summary>
        public static DirectoryInfo GenerationDirectory => new(Instance._generationDirectoryFullName);

        /// <summary>
        /// ログファイルを表す <see cref="FileInfo"/> インスタンスを取得します。
        /// </summary>
        public static FileInfo LogFile => new(Instance._logFileFullName);


        private LogFileInfo()
        {
            _timestamp = DateTimeOffset.UtcNow.ToLocalTime();
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            _parentDirectoryFullName = Path.Combine(baseDirectory, ParentDirectoryName);
            _generationDirectoryFullName = Path.Combine(_parentDirectoryFullName, _timestamp.ToString(GenerationDateTimeFormat));
            _logFileFullName = Path.Combine(_generationDirectoryFullName, _timestamp.ToString(FileNameDateTimeFormat) + FileExtension);
        }


        /// <summary>
        /// <see cref="ParentDirectory"/> 内に存在する日別世代ディレクトリのうち、指定した保持日数を過ぎたディレクトリを全て削除します。
        /// </summary>
        /// <param name="keepDays">保持日数。</param>
        /// <param name="logger">ログを出力する場合は値を設定してください。既定値は <see langword="null"/>。</param>
        internal static void LogRotate(in int keepDays, in ILogger? logger = null)
        {
            DateTime now = DateTimeOffset.UtcNow.ToLocalTime().Date;
            TimeSpan keepTime = TimeSpan.FromDays(keepDays);
            logger?.ZLogDebug("LogRotate Start (KeepDays: {0})", keepTime.Days);
            try
            {
                DirectoryInfo parent = ParentDirectory;
                foreach (var generation in parent.GetDirectories("*", SearchOption.TopDirectoryOnly))
                {
                    if (DateTime.TryParseExact(generation.Name, GenerationDateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var result))
                    {
                        DateTime date = result.Date;
                        if (keepTime < (now - date))
                        {
                            generation.Delete(true);
                            logger?.ZLogDebug("- Delete Log: {0}", generation.Name);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger?.ZLogError("LogRotate 処理中にエラーが発生しました。\n{0}", ex);
            }
            finally
            {
                logger?.ZLogDebug("LogRotate Complete");
            }
        }
    }
}
