# taa

[![Build Status](https://dev.azure.com/taityonohibi/taa/_apis/build/status/xztaityozx.taa?branchName=master)](https://dev.azure.com/taityonohibi/taa/_build/latest?definitionId=2&branchName=master)

`taa` は `avv` から出力されたCSVをパースして、DBに書き込んだりDBからデータを取り出し、数え上げを行うCLIツールです

## Requirements
- Shell
  - Windows PowerShell
  - Bash
  - Zsh
- git 2.20.1 以上
- SQLServer

### Optional
- dotnet 2.1.701 以上

## Install
### .NETCoreをインストールした場合
GitHubからソースコードをクローンして.NETCoreでビルドします

```sh
# clone
$ git clone https://github.com/xztaityozx/taa
$ cd ./taa

# build. for linux
$ dotnet publish -c Release -r linux-x64 --self-contained true

# build. for win10
$ dotnet publish -c Release -r win10-x64 --self-contained true
```

ビルドすると `./bin/Release/netcoreapp2.1/linux-x64/publish`,  もしくは `./bin/Release/netcoreapp2.1/win10-x64/publish` に `taa` という実行ファイルが生成されます。コレに対してAliasを貼るといいと思います

```sh
# powershell
$ Set-Alias taa "/path/to/taa.exe"

# bash/zsh
$ alias taa="/path/to/taa"
```

### GitHubのReleaseからバイナリをダウンロードしてくる
[Release](https://github.com/xztaityozx/taa/releases) ページ

プラットフォームにあったバイナリを選択してダウンロードしてください

## Usage
`taa` は複数のサブコマンドで機能を切り替えて使います

```sh
Usage: taa [command]

  push       DBにデータをPushします

  get        数え上げます

  help       Display more information on a specific command.

  version    Display version information.
```

- [taa push](#taa-push)
- [taa get](#taa-get)
- [taa help](#taa-help)
- [taa version](#taa-version)


### taa push
CSVファイルを解析してDBにデータを書き込みます

-  `-d, --directory` -->   指定したディレクトリの下にあるファイルをすべてPushします

-  `-p, --Parallel` -->    (Default: 1) 並列数です

-  `-b, --bufferSize` -->    (Default: 50000) DBへのBulkUpsert一回当たりのEntityの個数です

-  `--config` -->   コンフィグファイルへのパスです

-  `-N, --VtnVoltage` -->   (Default: 0.6) Vtnの閾値電圧です

-  `--vtnSigma` -->   (Default: 0.046) Vtnのシグマです

-  `--vtnDeviation` -->   (Default: 1) Vtnの偏差です

-  `-P, --VtpVoltage` -->   (Default: -0.6) Vtpの閾値電圧です

-  `--vtpSigma` -->   (Default: 0.046) Vtpのシグマです

-  `--vtpDeviation` -->   (Default: 1) Vtpの偏差です

-  `-s, --sigma` -->   (Default: -1) Vtn,Vtpのシグマです.個別設定が優先されます

-  `--sweeps` -->   (Default: 5000) number of sweeps

-  `--help` -->   Display this help screen.

-  `--version` -->   Display version information.

-  `input (pos. 0)`  -->   入力ファイルです

### taa push Transistor オプション
pushするデータのVtnとVtpの値を指定します

- `-N, --vtnVoltage` --> Vtnのしきい値電圧です
- `--vtnSigma` --> Vtnのシグマです
- `--vtnDeviation` --> Vtnの偏差です
- `-N, --vtpVoltage` --> Vtpのしきい値電圧です
- `--vtpSigma` --> Vtpのシグマです
- `--vtpDeviation` --> Vtpの偏差です
- `-s,--sigma` --> VtnとVtpのシグマを同時に指定できます
  - 個別の設定がある(`--VtnSigma,--VtpSigma` が指定されている)場合はそちらが優先されます

### taa push bufferSize オプション
`taa` は1つのCSVから複数のデータを取り出し、それらをある程度の個数にまとめてDBに書き込みます。 `bufferSize` オプションではその個数を指定できます

- `-b, --bufferSize` --> DBへのBulkUpsert一回当たりのEntityの個数です

```sh
# ex) bufferSizeを200000にする
$ taa push -b 200000
```

### taa push Parallel オプション
CSV解析の並列数を指定できます

- `-p, --Parallel` -->  (Default: 1) 並列数です

```sh
# ex) 並列数を15にする
$ taa push -p 15
```

### taa push sweeps オプション
解析するファイルのSweep数を指定します

- `--sweeps` -->   (Default: 5000) number of sweeps

指定は、1つのCSVファイルが保存しているSweep数を指定します。例えば `avv` で5000回のモンテカルロ・シミュレーションを行った場合はこのオプションに 5000 を指定します

```sh
# ex) sweep数を10000に設定する
$ taa push --sweeps 10000
```

### taa push directory オプション
複数のファイルを一度にpushする場合は、ディレクトリ自体を指定することで、pushすることができます。このオプションを有効にする場合、引数に渡したファイルは無視されます

- `-d, --directory` -->  指定したディレクトリの下にあるファイルをすべてPushします

```sh
# ex) ./result 以下にある avv の出力結果をまとめてDBにpushする
$ taa push -d ./result
```

### taa push 引数
引数にCSVファイルを渡すとそれを解析して、pushします。`--directory` オプションを指定していると、この引数は無視されます

```sh
# ex) ./00001 を解析してpushする
$ taa push ./00001
```

## taa get
`taa get` はDBからデータを取り出して数え上げをします

-  `-w, --sweepRange`    (Default: 1,5000) Sweepの範囲を[開始],[終了値]で指定できます
-  `-e, --seedRange `    (Default: 1,2000) Seedの範囲を[開始],[終了値]で指定できます
-  `-i, --sigmaRange`    (Default: ) シグマの範囲を[開始],[刻み幅],[終了値]で指定できます
-  `--out` -->   (Default: ./out.csv) 結果を出力するCSVファイルへのパスです
-  `--config` -->   コンフィグファイルへのパスです
-  `-N, --VtnVoltage` -->   (Default: 0.6) Vtnの閾値電圧です
-  `--vtnSigma` -->   (Default: 0.046) Vtnのシグマです
-  `--vtnDeviation  ` -->   (Default: 1) Vtnの偏差です
-  `-P, --VtpVoltage` -->   (Default: -0.6) Vtpの閾値電圧です
-  `--vtpSigma` -->   (Default: 0.046) Vtpのシグマです
-  `--vtpDeviation  ` -->   (Default: 1) Vtpの偏差です
-  `-s, --sigma` -->   (Default: -1) Vtn,Vtpのシグマです.個別設定が優先されます
-  `--sweeps` -->   (Default: 5000) number of sweeps
-  `--help` -->   Display this help screen.
-  `--version` -->   Display version information.

### taa get Transistor オプション
[taa push Transistor オプション](#taa-push-Transistor-オプション)


### taa get sweeps オプション
[taa push sweeps オプション](#taa-push-sweeps-オプション)

### taa get out オプション
`taa` は数え上げの結果をCSVにしてファイルに出力します。その時のファイル名を指定できます

```sh
# ex) 出力先をA.csvにする
$ taa get --out A.csv
```

### taa get Range オプション
`taa` はSweep,Seed,Sigmaそれぞれについて、指定した範囲のデータを同時に取り出すことができます。数値をカンマで区切って記述します

-  `-w, --sweepRange`    (Default: 1,5000) Sweepの範囲を[開始],[終了値]で指定できます
-  `-e, --seedRange `    (Default: 1,2000) Seedの範囲を[開始],[終了値]で指定できます
-  `-i, --sigmaRange`    (Default: ) シグマの範囲を[開始],[刻み幅],[終了値]で指定できます
  - この値を指定する場合、`vtnSigma` , `vtpSigma` , `sigma` の値は無視されます

## taa help
簡単なヘルプを出力して終了します

`taa get` , `taa push` にも `--help` というオプションが存在し、これをつけて実行すると、オプションのヘルプが出力されます

## taa version
`taa` のバージョン情報を出力して終了します

## Config
`taa` のコンフィグには、数え上げの条件、DBサーバーの情報などを記述します

### conditions
条件式に名前をつけ列挙します。この名前を[expressions](#expressions)の設定で利用します

BNFは以下の通りです

```
<condition> := <name>: <cond>
<cond> := <value><operator><value>
<value> := <signal>, <number>
<signal> := <signalName>[<time>]
<time> := <number>
<number> := [0-9](.[0-9]+)*, [0-9](.[0-9]+)<siUnit>
<siUnit> := G, M, K, m, u, n, p
<operator> := <, <=, >, >=, !=, ==
<signalName> := <string>
<name> := <string>
<string> := ([a-zA-Z0-9])+
```

例えば、 `AX` という信号線の時間 `2.5ns` の値が `0.5以下` であるかどうかという条件式に、`Z` という名前をつける場合

```yaml
conditions:
  Z: "AX[2.5n]<=0.5"
```

と記述できます。また `AX` という信号線の時間 `2.5ns` の値が `BX` という信号線の時間 `17.5ns` より大きいかどうかという条件式に、 `C` という名前をつける場合

```yaml
conditions:
  C: "AX[2.5n]>BX[17.5n]"
```

と記述します

### expressions
`taa` が実際に数え上げに使う条件式を記述します

[conditions](#conditions)に列挙した名前を変数として使い条件式を組み立てます

扱える二項演算子、単行演算子は以下です

- && --> AND
- || --> OR
- !  --> NOT

また優先度を明確にするために、条件式を `()` でくくることが可能です


以下の例では、4つの数え上げ条件を指定しています

```yaml
conditions:
  A: "n1[2.5n]>=0.71"
  B: "n1[10n]>=0.4"
  C: "n1[17.5n]>=0.4"
expressions:
  - "A&&B&&C"  <-- A AND B AND C
  - "A"        <-- Aのみ
  - "(!B)&&C"  <-- (NOT B) AND C
  - "!(A||C)   <-- NOT(A OR C)
```

### logDir
`taa` が出力するlogの保存先のディレクトリです


### connection
DBの接続情報です。 `taa` では SQLServer を利用します

```yaml
connection: "Server=IPAddress,Port;User Id=username;Password=password"
```

__注意__ `taa` は回路の情報を持ちません。したがって別の回路の結果をpushするとき、同じDBサーバーを指定しないでください。一度DBを止めダンプを取ったあと新たにDBを起動するか、別のDBサーバーを利用してください。[DBサーバー](#DBサーバー)も参考にしてください


### machine
実行中のマシンに任意の名前をつけることができます。内部で利用されます


## DBサーバー
内部で利用しているライブラリの依存により、`taa` ではSQLServerを利用します。起動にはDocker-Composeが便利です

```yaml
version: '3'

services:
  mssql:
    image: mcr.microsoft.com/mssql/server:latest
    environment:
      - TZ=Asia/Tokyo
      - ACCEPT_EULA=Y
      - MSSQL_SA_PASSWORD=VjfKGTC8Kt
    ports:
      - "28001:1433"
    volumes:
      - ./setup.sh:/setup.sh
      - ./init:/init
    command: "sh -c '/setup.sh & /opt/mssql/bin/sqlservr'"
```

```sh
#!/bin/bash

echo "start setup"

status="1"
wait=30
seq $wait | 
  while read L; do
    /opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P $MSSQL_SA_PASSWORD -Q "select 1" > /dev/null && {

      echo "===== start configuration ====="

      /opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P $MSSQL_SA_PASSWORD -i /init/create_user.sql && echo "info: created user: for taa_push"

      echo "===== finished configuration ====="
    } && exit 1
  done && echo "error: failed connect to SQL Server" || echo "success"
```

```sql
create login taa_push with password = '28ZENw6c8i'
GO
create user taa for login taa_push;
GO
alter server role sysadmin add member [taa_push];
GO
```

以上のようなファイルを用意し、`docker-compose up -d` を実行すると `localhost:28001` でSQLServerが起動します。停止は `docker-compose down` です。 __Important: DBの永続化はしていないため、停止するとデータが消えます__

SAのパスワードは `VjfKGTC8Kt` で、 `taa push` 用のUser( `taa_push` )のパスワードは `28ZENw6c8i` です。この場合、設定ファイルの `connection` には `"Server=IPAddress,28001;User Id=taa_push;Password=28ZENw6c8i"` を指定します

SQLServerに設定できるパスワードはある程度の条件があります。MSのリファレンスをご覧ください



