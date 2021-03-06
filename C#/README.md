# ComLib  
## Release Ver2.1.1  
### 概要  
TCPClientリファクタリング
### 更新事項  
・「表2-1-0-1」の「No.3」メソッド内部処理変更。  


## Release Ver2.1.0  
### 概要  
TCPClient、Serverメソッド名変更
### 更新事項  
・以下、「表2-1-0-1」のようにメソッド名変更。  

<table>
  <tr>
    <th width="50">No</th>
    <th width="300">変更前メソッド名</th>
    <th width="300">変更後メソッド名</th>
  </tr>
  <tr>
    <td>1</td>
    <td>Connect</td>
    <td>ConnectToServerAsync</td>
  </tr>
  <tr>
    <td>2</td>
    <td>DisConnect</td>
    <td>DisConnectServer</td>
  </tr>
  <tr>
    <td>3</td>
    <td>Send</td>
    <td>SendToServer</td>
  </tr>
  <tr>
    <td>4</td>
    <td>IsConnected</td>
    <td>IsClientConnected</td>
  </tr>
  <tr>
    <td>5</td>
    <td>StartService</td>
    <td>StartServerAsync</td>
  </tr>
  <tr>
    <td>6</td>
    <td>EndService</td>
    <td>EndServer</td>
  </tr>
  <tr>
    <td>7</td>
    <td>IsServiceRunning</td>
    <td>IsServerRunning</td>
  </tr>
</table>

※上記変更前メソッド名については「表1-0-0-1」を参照


## Release Ver1.1.0  
### 概要  
TCPClient、Serverメソッド引数制約追加
### 更新事項  
・以下、「表1-1-0-1」メソッド制約を追加。

<table>
  <tr>
    <th width="50">No</th>
    <th width="150">ファイル名</th>
    <th width="100">クラス名</th>
    <th width="100">メソッド名</th>
    <th width="250">引数</th>
    <th width="350">制約</th>
  </tr>
  <tr>
    <td>1</td>
    <td>ComLib.dll</td>
    <td>TCPClient</td>
    <td>Connect</td>
    <td>int 接続タイムアウトミリ秒</td>
    <td>1以上の値を引数に渡すとき、値ミリ秒タイムアウトを行う。<br>1より小さい値を引数に渡すまたは引数を渡さないとき、タイムアウトを行わない。</td>
  </tr>
  <tr>
    <td>2</td>
    <td>ComLib.dll</td>
    <td>TCPClient</td>
    <td>Connect</td>
    <td>int 受信タイムアウトミリ秒</td>
    <td>1以上の値を引数に渡すとき、値ミリ秒タイムアウトを行う。<br>1より小さい値を引数に渡すまたは引数を渡さないとき、タイムアウトを行わない。</td>
  </tr>
  <tr>
    <td>3</td>
    <td>ComLib.dll</td>
    <td>TCPClient</td>
    <td>Connect</td>
    <td>int 接続トライ回数</td>
    <td>1以上の値を引数に渡すとき、値回数接続リトライを行う。<br>1より小さい値を引数に渡すまたは引数を渡さないとき、接続リトライを行わない。</td>
  </tr>
  <tr>
    <td>4</td>
    <td>ComLib.dll</td>
    <td>TCPServer</td>
    <td>StartService</td>
    <td>int 受信タイムアウトミリ秒</td>
    <td>1以上の値を引数に渡すとき、値ミリ秒タイムアウトを行う。<br>1より小さい値を引数に渡すまたは引数を渡さないとき、タイムアウトを行わない。</td>
  </tr>
</table>

  
## Release Ver1.0.0  
### 概要  
TCP通信クライアント、サーバークラスライブラリーを作成。  
※TCPサーバーは1(サーバー)対多(クライアント)に対応。  
※バイナリーデータをクライアントとサーバーで通信する。  
### 更新事項
* 以下、「表1-0-0-1」インターフェースを追加。  

<table>
  <tr>
    <th width="50">No</th>
    <th width="150">ファイル名</th>
    <th width="100">クラス名</th>
    <th width="100">メソッド名</th>
    <th width="250">引数</th>
    <th width="350">戻り値</th>
    <th width="350">説明</th>
  </tr>
  <tr>
    <td>1</td>
    <td>ComLib.dll</td>
    <td>TCPClient</td>
    <td>Connect</td>
    <td>string <br> 接続IP, <br> int <br> 接続ポート, <br> int <br> 接続タイムアウトミリ秒, <br> int <br> 受信タイムアウトミリ秒, <br> int <br> 接続トライ回数</td>
    <td>接続可否</td>
    <td>引数で指定した条件でサーバーへ接続</td>
  </tr>
  <tr>
    <td>2</td>
    <td>ComLib.dll</td>
    <td>TCPClient</td>
    <td>DisConnect</td>
    <td>なし</td>
    <td>void</td>
    <td>サーバーとの接続を切断する</td>
  </tr>
  <tr>
    <td>3</td>
    <td>ComLib.dll</td>
    <td>TCPClient</td>
    <td>Send</td>
    <td>byte[] <br> 送信データ</td>
    <td>送信可否</td>
    <td>指定したバイナリーデータを接続したサーバーへ送信</td>
  </tr>
  <tr>
    <td>4</td>
    <td>ComLib.dll</td>
    <td>TCPClient</td>
    <td>IsConnected</td>
    <td>なし</td>
    <td>クライアントとサーバーが接続しているとき、true <br> そうでないとき、false</td>
    <td>クライアントとサーバーの接続状態を取得</td>
  </tr>
  <tr>
    <td>5</td>
    <td>ComLib.dll</td>
    <td>TCPClient</td>
    <td>StartService</td>
    <td>int <br> acceptされていないクライアントからの接続要求を保持しておくキューの最大長, <br> int <br> 受信タイムアウトミリ秒</td>
    <td>void</td>
    <td>サーバー処理を開始</td>
  </tr>
  <tr>
    <td>6</td>
    <td>ComLib.dll</td>
    <td>TCPServer</td>
    <td>EndService</td>
    <td>なし</td>
    <td>void</td>
    <td>サーバー処理終了</td>
  </tr>
  <tr>
    <td>7</td>
    <td>ComLib.dll</td>
    <td>TCPServer</td>
    <td>IsServiceRunning</td>
    <td>なし</td>
    <td>サーバー処理が起動中のとき、true <br> そうでないとき、false</td>
    <td>サーバー処理起動状態を取得</td>
  </tr>
</table>

* 以下、「表1-0-0-2」完了イベントを追加。  

<table>
  <tr>
    <th width="50">No</th>
    <th width="120">ファイル名</th>
    <th width="80">クラス名</th>
    <th width="60">メソッド名</th>
    <th width="180">引数</th>
    <th width="80">戻り値</th>
    <th width="150">トリガー</th>
    <th width="150">説明</th>
  </tr>
  <tr>
    <td>1</td>
    <td>ComLib.dll</td>
    <td>TCPClient</td>
    <td>OnClientConnected</td>
    <td>object <br> イベント発生オブジェクト, <br> ConnectedEventArgs <br> 接続イベントデータ</td>
    <td>void</td>
    <td>サーバーへ接続した</td>
    <td>クライアント接続完了イベント</td>
  </tr>
  <tr>
    <td>2</td>
    <td>ComLib.dll</td>
    <td>TCPClient</td>
    <td>OnClientSendData</td>
    <td>object <br> イベント発生オブジェクト, <br> SendEventArgs <br> 送信イベントデータ</td>
    <td>void</td>
    <td>サーバーへデータを送信した</td>
    <td>クライアント送信完了イベント</td>
  </tr>
  <tr>
    <td>3</td>
    <td>ComLib.dll</td>
    <td>TCPClient</td>
    <td>OnClientDisconnected</td>
    <td>object <br> イベント発生オブジェクト, <br> DisconnectedEventArgs <br> 切断イベントデータ</td>
    <td>void</td>
    <td>サーバーとの接続が切断された</td>
    <td>クライアント切断完了イベント</td>
  </tr>
  <tr>
    <td>4</td>
    <td>ComLib.dll</td>
    <td>TCPClient</td>
    <td>OnClientReceivedData</td>
    <td>object <br> イベント発生オブジェクト, <br> ReceivedEventArgs <br> 受信イベントデータ</td>
    <td>void</td>
    <td>サーバーからのデータを受信した</td>
    <td>クライアント受信完了イベント</td>
  </tr>
  <tr>
    <td>5</td>
    <td>ComLib.dll</td>
    <td>TCPServer</td>
    <td>OnServerReceivedData</td>
    <td>object <br> イベント発生オブジェクト, <br> ReceivedEventArgs <br> 受信イベントデータ, <br> ref byte[] <br> レスポンスバイナリーデータ, <br> ref bool <br> 接続済みのすべてのクライアントへ送信するか</td>
    <td>void</td>
    <td>クライアントからのデータを受信した</td>
    <td>サーバー受信完了イベント</td>
  </tr>
  <tr>
    <td>6</td>
    <td>ComLib.dll</td>
    <td>TCPServer</td>
    <td>OnServerDisconnected</td>
    <td>object <br> イベント発生オブジェクト, <br> DisconnectedEventArgs <br> 切断イベントデータ</td>
    <td>void</td>
    <td>クライアントとの接続が切断された</td>
    <td>サーバー切断完了イベント</td>
  </tr>
  <tr>
    <td>7</td>
    <td>ComLib.dll</td>
    <td>TCPServer</td>
    <td>OnServerConnected</td>
    <td>object <br> イベント発生オブジェクト, <br> ConnectedEventArgs <br> 接続イベントデータ</td>
    <td>void</td>
    <td>クライアントへ接続した</td>
    <td>サーバー接続完了イベント</td>
  </tr>
  <tr>
    <td>8</td>
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

* 以下、「UML1-0-0-1」完了イベントデータクラスとする。

![完了イベントデータクラス図](Doc/README/Ver1-0-0/img/001.png)
