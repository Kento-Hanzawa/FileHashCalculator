# FileHashCalculator
ファイルのハッシュ値を計算するコンソールアプリ。

## コマンド
```
Usage: FileHashCalculator <ハッシュ値を計算するファイル、または計算対象のファイルが含まれるディレクトリのパスを指定します。> [options...]

Arguments:
  [0] <String>    ハッシュ値を計算するファイル、または計算対象のファイルが含まれるディレクトリのパスを指定します。

Options:
  -o, --Output <String>       計算されたハッシュ値をリストにしたテキストの出力先ファイルパスを指定します。指定しない場合は、'Input' の場所に '{FileName}.{Algorithm}.txt' の形式で出力します。 (Default: null)
  -a, --Algorithm <String>    ハッシュアルゴリズムの名前を指定します。大文字、小文字は区別しません。(CRC32, CRC64, MD5, SHA1, SHA256, SHA384, SHA512) (Default: SHA256)
  -rx, --Regex <String>       'Input' にディレクトリパスを指定した場合に有効になります。計算対象のファイルを選択するための正規表現を指定します。指定しない場合、全てのファイルが対象となります。 (Default: null)
  -rc, --Recursive            'Input' にディレクトリパスを指定した場合に有効になります。計算対象にサブフォルダ内のファイルを含めるかどうかを指定します。含める場合は true、含めない場合は false。 (Optional)

Commands:
  help          Display help.
  version       Display version.
```
