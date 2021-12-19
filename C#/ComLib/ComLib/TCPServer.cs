using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ComTCP
{
    public class TCPServer
    {
        /// <summary>
        /// スレッド待機用
        /// </summary>
        private readonly ManualResetEvent mre = new ManualResetEvent(false);

        /// <summary>
        /// リッスンタスク
        /// </summary>
        private Task TaskListen { get; set; }

        /// <summary>
        /// サーバーのエンドポイント
        /// </summary>
        public IPEndPoint IPEndPoint { get; private set; }

        /// <summary>
        /// 接続待機ループ実行中
        /// </summary>
        private bool RunningListen { get; set; } = false;

        /// <summary>
        /// サーバにacceptされていないクライアントからの接続要求を保持しておくキューの最大長
        /// </summary>
        private int Backlog { get; set; } = 10;

        /// <summary>
        /// 接続中のクライアント
        /// スレッドセーフコレクション
        /// </summary>
        public SynchronizedCollection<Socket> ClientSockets { get; } = new SynchronizedCollection<Socket>();

        /// <summary>
        /// データ受信イベントハンドラー
        /// </summary>
        /// <param name="sender">イベント発生オブジェクト</param>
        /// <param name="receivedData">受信データ</param>
        /// <param name="sendData">送信データ</param>
        /// <param name="isSendAll">全接続済みクライアントへ送信するか</param>
        public delegate void ReceiveEventHandler(object sender, byte[] receivedData, ref byte[] sendData, ref bool isSendAll);

        /// <summary>
        /// データ受信イベント
        /// </summary>
        public event ReceiveEventHandler OnServerReceiveData;

        /// <summary>
        /// 切断イベントハンドラー
        /// </summary>
        /// <param name="sender">イベント発生オブジェクト</param>
        /// <param name="e">イベントデータ</param>
        /// <param name="ex">例外オブジェクト</param>
        public delegate void DisconnectedEventHandler(object sender, EventArgs e, Exception ex);

        /// <summary>
        /// 切断イベント
        /// </summary>
        public event DisconnectedEventHandler OnServerDisconnected;

        /// <summary>
        /// 接続イベントハンドラー
        /// </summary>
        /// <param name="sender">イベント発生オブジェクト</param>
        /// <param name="connectedEndPoint">接続エンドポイント</param>
        public delegate void ConnectedEventHandler(EventArgs e, EndPoint connectedEndPoint);

        /// <summary>
        /// 接続イベント
        /// </summary>
        public event ConnectedEventHandler OnServerConnected;

        /// <summary>
        /// 送信イベントハンドラー
        /// </summary>
        /// <param name="byteSize">送信バイト数</param>
        /// <param name="sendEndPoint">送信エンドポイント</param>
        public delegate void SendEventHandler(int byteSize, EndPoint sendEndPoint);

        /// <summary>
        /// 送信イベント
        /// </summary>
        public event SendEventHandler OnServerSend;

        /// <summary>
        /// 引数付きコンストラクタ
        /// </summary>
        /// <param name="port">リッスンするポート</param>
        TCPServer(int port)
        {
            // IPエンドポイント作成
            IPEndPoint = new IPEndPoint(IPAddress.Any, port);
        }

        /// <summary>
        /// サーバー処理開始
        /// </summary>
        public void StartService()
        {
            TaskListen = Task.Factory.StartNew(() =>
            {
                Run();
            });
        }

        /// <summary>
        /// サーバー起動
        /// </summary>
        private void Run()
        {
            using (Socket listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                // 切断後、再接続を可能にする
                listenerSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                // ソケットをアドレスにバインド
                listenerSocket.Bind(IPEndPoint);

                RunningListen = true;

                // 接続待機開始
                listenerSocket.Listen(Backlog);

                // 接続待機のループ
                while (RunningListen)
                {
                    mre.Reset();
                    // 非同期ソケットを開始して、接続をリッスンする
                    listenerSocket.BeginAccept(new AsyncCallback(AcceptCallback), listenerSocket);
                    // 接続があるまでスレッドを待機
                    mre.WaitOne();
                }
            }
        }

        /// <summary>
        /// 接続受付時の非同期コールバック処理
        /// </summary>
        /// <param name="asyncResult">接続受付結果</param>
        private void AcceptCallback(IAsyncResult asyncResult)
        {
            // 待機スレッドが進行するようにシグナルをセット
            mre.Set();

            // ソケットを取得
            var listenerSocket = asyncResult.AsyncState as Socket;
            var clientSocket = listenerSocket.EndAccept(asyncResult);

            // 接続中のクライアントを追加
            ClientSockets.Add(clientSocket);

            // 接続イベント発生
            OnServerConnected?.Invoke(new EventArgs(), clientSocket.RemoteEndPoint);

            // StateObject作成
            var state = new StateObject();
            state.ClientSocket = clientSocket;

            // データ受信開始
            clientSocket.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
        }

        /// <summary>
        /// データ受信時の非同期コールバック処理
        /// </summary>
        /// <param name="asyncResult">受信結果</param>
        private void ReceiveCallback(IAsyncResult asyncResult)
        {
            // StateObject、クライアントソケットを取得
            var state = asyncResult.AsyncState as StateObject;
            var clientSocket = state.ClientSocket;

            try
            {
                // クライアントソケットから受信データを取得終了
                int bytes = clientSocket.EndReceive(asyncResult);

                if (bytes > 0)
                {
                    // 受信したバイナリーデータを取得
                    var receivedData = new byte[bytes];
                    Array.Copy(state.Buffer, receivedData, bytes);

                    var sendData = new byte[StateObject.BufferSize];

                    bool isSendAll = false;

                    // データ受信イベント発生
                    OnServerReceiveData?.Invoke(this, receivedData, ref sendData, ref isSendAll);

                    if (isSendAll)
                    {
                        // 全接続済みクライアントへ送信
                        SendAllClient(sendData);
                    }
                    else
                    {
                        // リクエストクライアントへ送信
                        Send(clientSocket, sendData);
                    }

                    // データ受信開始
                    clientSocket.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
                }
                else
                {
                    // 0バイトのデータを受信したときは切断
                    clientSocket.Close();
                    ClientSockets.Remove(clientSocket);

                    // 切断イベント発生
                    string msg = "0バイトデータの受信";
                    OnServerDisconnected?.Invoke(this, new EventArgs(), new Exception(msg));
                }
            }
            catch (SocketException e)
            {
                if (e.NativeErrorCode.Equals(10054))
                {
                    // 既存の接続が、リモートホストによって強制的に切断された
                    // 保持しているクライアント情報をクリア
                    clientSocket.Close();
                    ClientSockets.Remove(clientSocket);

                    // 切断イベント発生
                    OnServerDisconnected?.Invoke(this, new EventArgs(), e);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// 接続されたクライアント情報を格納するクラス
        /// </summary>
        public class StateObject
        {
            /// <summary>
            /// クライアントソケット
            /// </summary>
            public Socket ClientSocket { get; set; }

            /// <summary>
            /// バッファーサイズ
            /// </summary>
            public const int BufferSize = 1024;

            /// <summary>
            /// バッファー
            /// </summary>
            public byte[] Buffer { get; } = new byte[BufferSize];
        }

        /// <summary>
        /// クライアントへのメッセージ送信処理
        /// </summary>
        /// <param name="clientSocket">クライアントソケット</param>
        /// <param name="data">データ</param>
        private void Send(Socket clientSocket, byte[] data)
        {
            try
            {
                clientSocket.BeginSend(data, 0, data.Length, 0, new AsyncCallback(SendCallback), clientSocket);
            }
            catch (SocketException e)
            {
                if (e.NativeErrorCode.Equals(10054))
                {
                    // 既存の接続が、リモート ホストによって強制的に切断されました
                    //接続断イベント発生
                    OnServerDisconnected?.Invoke(this, new EventArgs(), e);
                }
                else
                {
                    string msg = string.Format("Disconnected!: error code {0} : {1}", e.NativeErrorCode, e.Message);
                    System.Diagnostics.Debug.WriteLine(msg);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// 全接続済みクライアントへのメッセージ送信処理
        /// </summary>
        /// <param name="data">データ</param>
        private void SendAllClient(byte[] data)
        {
            foreach (var clientSocket in ClientSockets)
            {
                Send(clientSocket, data);
            }
        }

        /// <summary>
        /// 送信時の非同期コールバック処理
        /// </summary>
        /// <param name="asyncResult">送信結果</param>
        private void SendCallback(IAsyncResult asyncResult)
        {
            try
            {
                // クライアントソケットへのデータ送信処理を完了する
                var clientSocket = asyncResult.AsyncState as Socket;
                var byteSize = clientSocket.EndSend(asyncResult);
                OnServerSend?.Invoke(byteSize, clientSocket.RemoteEndPoint);
            }
            catch (Exception e)
            {
                string msg = e.Message;
                Console.Write(msg);
            }
        }

        /// <summary>
        /// サーバー処理終了
        /// </summary>
        public void EndService()
        {
            RunningListen = false;

            // 待機スレッドが進行するようにシグナルをセット
            mre.Set();

            TaskListen?.Wait();
            TaskListen?.Dispose();

            mre.Close();

            foreach (Socket clientSocket in ClientSockets)
            {
                clientSocket?.Close();
            }
        }
    }
}
