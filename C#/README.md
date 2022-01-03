# ComLib  
## Release Ver1.0.0  
### 概要  
TCP通信クライアント、サーバークラスライブラリーを作成。  
※TCPサーバーは1(サーバー)対多(クライアント)に対応。  
### 更新事項
* 以下、インターフェースを追加。  

| ファイル名 | クラス名 | メソッド名 | 引数 | 戻り値 | 説明 |
| ---------- | -------- | ---------- | ---- | ------ | ---- |
| ComLib.dll | TCPClient | Connect | string 接続IP, int 接続ポート, int 接続タイムアウトミリ秒, int 受信タイムアウトミリ秒, int 接続トライ回数 | 接続可否 | 引数で指定した条件でサーバーへ接続 |
| ComLib.dll | TCPClient | DisConnect | なし | void | サーバーとの接続を切断する |
| ComLib.dll | TCPClient | Send | byte[] 送信データ | 送信可否 | 指定したバイナリーデータを接続したサーバーへ送信 |
| ComLib.dll | TCPClient | DisConnect | なし | void | クライアントとサーバー接続を切断する |
| ComLib.dll | TCPClient | IsConnected | なし | クライアントとサーバーが接続しているとき、true そうでないとき、false | クライアントとサーバーの接続状態を取得 |
| ComLib.dll | TCPServer | StartService | int acceptされていないクライアントからの接続要求を保持しておくキューの最大長, int 受信タイムアウトミリ秒 | void | サーバー処理を開始 |
| ComLib.dll | TCPServer | EndService | なし | void | サーバー処理終了 |
| ComLib.dll | TCPServer | IsServiceRunning | なし | サーバー処理が起動中のとき、true そうでないとき、false | サーバー処理起動状態を取得 |

