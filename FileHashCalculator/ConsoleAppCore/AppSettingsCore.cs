using System.Text.Json.Serialization;

namespace FileHashCalculator.ConsoleAppCore
{
    /// <summary>
    /// <see cref="AppSettings"/> の基底となるレコード。
    /// </summary>
    /// <remarks>このレコードには <see cref="AppSettings"/> の値の中でも、特にコア実装側に使用される必須パラメータが定義されています。</remarks>
    internal abstract record AppSettingsCore
    {
        [JsonPropertyName("EnableLogRotate")]
        public bool EnableLogRotate { get; init; } = true;
        [JsonPropertyName("LogRotateKeepDays")]
        public int LogRotateKeepDays { get; init; } = 14;
    }
}
