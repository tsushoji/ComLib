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
        /// サーバーのエンドポイント
        /// </summary>
        public IPEndPoint IPEndPoint { get; private set; }

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
        public delegate void ReceiveEventHandler(object sender, byte[] receivedData);

        /// <summary>
        /// データ受信イベント
        /// </summary>
        public event ReceiveEventHandler OnServerReceiveData;

        /// <summary>
        /// データ送信イベントハンドラー
        /// </summary>
        /// <param name="sender">イベント発生オブジェクト</param>
        /// <param name="receivedData">データ</param>
        public delegate byte[] SendEventHandler(object sender, byte[] data);

        /// <summary>
        /// データ送信イベント
        /// </summary>
        public event SendEventHandler OnSeverSendData;

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
        /// 送信完了イベントハンドラー
        /// </summary>
        /// <param name="byteSize">送信バイト数</param>
        /// <param name="sendEndPoint">送信エンドポイント</param>
        public delegate void SendCompletlyEventHandler(int byteSize, EndPoint sendEndPoint);

        /// <summary>
        /// 送信完了イベント
        /// </summary>
        public event SendCompletlyEventHandler OnServerSendCompletly;

        /// <summary>
        /// サーバー処理開始
        /// </summary>
        /// <param name="port">リッスンするポート</param>
        public void StartListen(int port)
        {
            Task.Factory.StartNew(() =>
            {
                Run(port);
            });
        }

        /// <summary>
        /// サーバー起動
        /// </summary>
        /// <param name="port">リッスンするポート</param>
        private void Run(int port)
        {
            using (Socket listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                // IPエンドポイント作成
                IPEndPoint = new IPEndPoint(IPAddress.Any, port);

                // 切断後、再接続を可能にする
                listenerSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                // ソケットをアドレスにバインド
                listenerSocket.Bind(IPEndPoint);

                // 接続待機開始
                listenerSocket.Listen(Backlog);

                // 接続待機のループ
                while (true)
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

                    // データ受信イベント発生
                    OnServerReceiveData?.Invoke(this, receivedData);

                    // クライアントに送信
                    Send(clientSocket, receivedData);

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
                //データ送信イベント発生
                var sendData = OnSeverSendData?.Invoke(this, data);

                clientSocket.BeginSend(sendData, 0, sendData.Length, 0, new AsyncCallback(SendCallback), clientSocket);
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
                OnServerSendCompletly?.Invoke(byteSize, clientSocket.RemoteEndPoint);
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
            foreach (Socket clientSocket in ClientSockets)
            {
                clientSocket?.Close();
            }
        }
    }
}
