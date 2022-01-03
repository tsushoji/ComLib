# ComLib  
## Release Ver1.0.0  
### 概要  
TCP通信クライアント、サーバークラスライブラリーを作成。  
※TCPサーバーは1(サーバー)対多(クライアント)に対応。  
※バイナリーデータをクライアントとサーバーで通信する。  
### 更新事項
* 以下、インターフェースを追加。  

<table>
  <tr>
    <th width="150">ファイル名</th>
    <th width="100">クラス名</th>
    <th width="100">メソッド名</th>
    <th width="250">引数</th>
    <th width="350">戻り値</th>
    <th width="400">説明</th></tr>
  <tr>
    <td>ComLib.dll</td>
    <td>TCPClient</td>
    <td>Connect</td>
    <td>string <br> 接続IP, <br> int <br> 接続ポート, <br> int <br> 接続タイムアウトミリ秒, <br> int <br> 受信タイムアウトミリ秒, <br> int <br> 接続トライ回数</td>
    <td>接続可否</td>
    <td>引数で指定した条件でサーバーへ接続</td>
  </tr>
  <tr>
    <td>ComLib.dll</td>
    <td>TCPClient</td>
    <td>DisConnect</td>
    <td>なし</td>
    <td>void</td>
    <td>サーバーとの接続を切断する</td>
  </tr>
  <tr>
    <td>ComLib.dll</td>
    <td>TCPClient</td>
    <td>Send</td>
    <td>byte[] <br> 送信データ</td>
    <td>送信可否</td>
    <td>指定したバイナリーデータを接続したサーバーへ送信</td>
  </tr>
  <tr>
    <td>ComLib.dll</td>
    <td>TCPClient</td>
    <td>IsConnected</td>
    <td>なし</td>
    <td>クライアントとサーバーが接続しているとき、true <br> そうでないとき、false</td>
    <td>クライアントとサーバーの接続状態を取得</td>
  </tr>
  <tr>
    <td>ComLib.dll</td>
    <td>TCPClient</td>
    <td>StartService</td>
    <td>int <br> acceptされていないクライアントからの接続要求を保持しておくキューの最大長, <br> int <br> 受信タイムアウトミリ秒</td>
    <td>void</td>
    <td>サーバー処理を開始</td>
  </tr>
  <tr>
    <td>ComLib.dll</td>
    <td>TCPServer</td>
    <td>EndService</td>
    <td>なし</td>
    <td>void</td>
    <td>サーバー処理終了</td>
  </tr>
  <tr>
    <td>ComLib.dll</td>
    <td>TCPServer</td>
    <td>IsServiceRunning</td>
    <td>なし</td>
    <td>サーバー処理が起動中のとき、true <br> そうでないとき、false</td>
    <td>サーバー処理起動状態を取得</td>
  </tr>
</table>

* 以下、完了イベントを追加。  

<table>
  <tr>
    <th width="120">ファイル名</th>
    <th width="80">クラス名</th>
    <th width="60">メソッド名</th>
    <th width="180">引数</th>
    <th width="80">戻り値</th>
    <th width="150">トリガー</th>
    <th width="150">説明</th>
  </tr>
  <tr>
    <td>ComLib.dll</td>
    <td>TCPClient</td>
    <td>OnClientConnected</td>
    <td>object <br> イベント発生オブジェクト, <br> ConnectedEventArgs <br> 接続イベントデータ</td>
    <td>void</td>
    <td>サーバーへ接続した</td>
    <td>クライアント接続完了イベント</td>
  </tr>
  <tr>
    <td>ComLib.dll</td>
    <td>TCPClient</td>
    <td>OnClientSendData</td>
    <td>object <br> イベント発生オブジェクト, <br> SendEventArgs <br> 送信イベントデータ</td>
    <td>void</td>
    <td>サーバーへデータを送信した</td>
    <td>クライアント送信完了イベント</td>
  </tr>
  <tr>
    <td>ComLib.dll</td>
    <td>TCPClient</td>
    <td>OnClientDisconnected</td>
    <td>object <br> イベント発生オブジェクト, <br> DisconnectedEventArgs <br> 切断イベントデータ</td>
    <td>void</td>
    <td>サーバーとの接続が切断された</td>
    <td>クライアント切断完了イベント</td>
  </tr>
  <tr>
    <td>ComLib.dll</td>
    <td>TCPClient</td>
    <td>OnClientReceivedData</td>
    <td>object <br> イベント発生オブジェクト, <br> ReceivedEventArgs <br> 受信イベントデータ</td>
    <td>void</td>
    <td>サーバーからのデータを受信した</td>
    <td>クライアント受信完了イベント</td>
  </tr>
  <tr>
    <td>ComLib.dll</td>
    <td>TCPServer</td>
    <td>OnServerReceivedData</td>
    <td>object <br> イベント発生オブジェクト, <br> ReceivedEventArgs <br> 受信イベントデータ, <br> ref byte[] <br> レスポンスバイナリーデータ, <br> ref bool <br> 接続済みのすべてのクライアントへ送信するか</td>
    <td>void</td>
    <td>クライアントからのデータを受信した</td>
    <td>サーバー受信完了イベント</td>
  </tr>
  <tr>
    <td>ComLib.dll</td>
    <td>TCPServer</td>
    <td>OnServerDisconnected</td>
    <td>object <br> イベント発生オブジェクト, <br> DisconnectedEventArgs <br> 切断イベントデータ</td>
    <td>void</td>
    <td>クライアントとの接続が切断された</td>
    <td>サーバー切断完了イベント</td>
  </tr>
  <tr>
    <td>ComLib.dll</td>
    <td>TCPServer</td>
    <td>OnServerConnected</td>
    <td>object <br> イベント発生オブジェクト, <br> ConnectedEventArgs <br> 接続イベントデータ</td>
    <td>void</td>
    <td>クライアントへ接続した</td>
    <td>サーバー接続完了イベント</td>
  </tr>
  <tr>
    <td>ComLib.dll</td>
    <td>TCPServer</td>
    <td>OnServerSendData</td>
    <td>object <br> イベント発生オブジェクト, <br> SendEventArgs <br> 送信イベントデータ</td>
    <td>void</td>
    <td>クライアントへデータを送信した</td>
    <td>サーバー送信完了イベント</td>
  </tr>
</table>

※イベント登録はライブラリー側では行わない。  
※イベント登録方法については以下プロジェクト参照。  
<https://github.com/tsushoji/ComLib/tree/main/C%23/ComLibDemo>  
