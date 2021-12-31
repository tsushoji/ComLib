using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace ComTCP
{
    public class TCPServer : TCPComBase
    {
        /// <summary>
        /// リッスンタスク
        /// </summary>
        private Task TaskListen { get; set; }

        /// <summary>
        /// サーバーのエンドポイント
        /// </summary>
        private IPEndPoint IPEndPoint { get; set; }

        /// <summary>
        /// 接続待機ループ実行中
        /// </summary>
        private bool RunningListen { get; set; } = false;

        /// <summary>
        /// サーバにacceptされていないクライアントからの接続要求を保持しておくキューの最大長
        /// </summary>
        private int Backlog { get; set; }

        /// <summary>
        /// 接続中のクライアント
        /// スレッドセーフコレクション
        /// 「foreach」はサポートなし
        /// </summary>
        private SynchronizedCollection<Socket> ClientSockets { get; set; } = new SynchronizedCollection<Socket>();

        /// <summary>
        /// データ受信イベントハンドラー
        /// </summary>
        /// <param name="sender">イベント発生オブジェクト</param>
        /// <param name="receivedEventArgs">受信イベントデータ</param>
        public delegate void ServerReceivedDataEventHandler(object sender, ServerReceivedEventArgs e);

        /// <summary>
        /// データ受信イベント
        /// </summary>
        public event ServerReceivedDataEventHandler OnServerReceivedData;

        /// <summary>
        /// 受信イベントデータ
        /// </summary>
        public class ServerReceivedEventArgs : EventArgs
        {
            /// <summary>
            /// 受信先IP
            /// </summary>
            public string IP { get; }

            /// <summary>
            /// 受信先ポート
            /// </summary>
            public int Port { get; }

            /// <summary>
            /// 受信データ
            /// </summary>
            public byte[] ReceivedData { get; }

            /// <summary>
            /// 全接続済みクライアントへ送信するか
            /// </summary>
            public bool IsSendAll { get; set; } = false;

            /// <summary>
            /// 送信データ
            /// </summary>
            public byte[] SendData { get; set; } = new byte[1024];

            /// <summary>
            /// 引数付きコンストラクタ
            /// </summary>
            /// <param name="ip">受信先IP</param>
            /// <param name="port">受信先ポート</param>
            /// <param name="receivedData">受信データ</param>
            internal ServerReceivedEventArgs(string ip, int port, byte[] receivedData)
            {
                IP = ip;
                Port = port;
                ReceivedData = receivedData;
            }
        }

        /// <summary>
        /// 切断イベント
        /// </summary>
        public event DisconnectedEventHandler OnServerDisconnected;

        /// <summary>
        /// 接続イベント
        /// </summary>
        public event ConnectedEventHandler OnServerConnected;

        /// <summary>
        /// 送信イベント
        /// </summary>
        public event SendDataEventHandler OnServerSendData;

        /// <summary>
        /// 引数付きコンストラクタ
        /// </summary>
        /// <param name="port">リッスンするポート</param>
        public TCPServer(int port)
        {
            // IPエンドポイント作成
            IPEndPoint = new IPEndPoint(IPAddress.Any, port);
        }

        /// <summary>
        /// デストラクタ
        /// </summary>
        ~TCPServer()
        {
            ClientSockets = null;
        }

        /// <summary>
        /// サーバー処理開始
        /// </summary>
        /// <param name="listenBackLog">acceptされていないクライアントからの接続要求を保持しておくキューの最大長</param>
        /// <param name="receiveTimeoutMillSec">受信タイムアウトミリ秒</param>
        public void StartService(int listenBackLog, int receiveTimeoutMillSec)
        {
            Backlog = listenBackLog;
            ReceiveTimeoutMillSec = receiveTimeoutMillSec;

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
            try
            {
                using (Socket listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    // 切断後、再接続を可能にする
                    listenerSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                    // ソケットをアドレスにバインド
                    listenerSocket.Bind(IPEndPoint);

                    // 接続待機開始
                    listenerSocket.Listen(Backlog);

                    // 接続待機のループ開始
                    RunningListen = true;

                    while (RunningListen)
                    {
                        connectMre.Reset();
                        // 非同期ソケットを開始して、接続をリッスンする
                        listenerSocket.BeginAccept(new AsyncCallback(AcceptCallback), listenerSocket);
                        // 接続があるまでスレッドを待機
                        connectMre.WaitOne();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(MethodBase.GetCurrentMethod().Name);
                System.Diagnostics.Debug.WriteLine(ex.Message);

                EndService();
            }
        }

        /// <summary>
        /// 接続受付時の非同期コールバック処理
        /// </summary>
        /// <param name="asyncResult">接続受付結果</param>
        private void AcceptCallback(IAsyncResult asyncResult)
        {
            // サービス終了時は処理しない
            if (!RunningListen)
            {
                return;
            }

            Socket clientSocket = null;
            StateObject state = null;

            try
            {
                // 待機スレッドが進行するようにシグナルをセット
                connectMre.Set();

                // ソケットを取得
                var listenerSocket = asyncResult.AsyncState as Socket;
                clientSocket = listenerSocket.EndAccept(asyncResult);

                // 接続中のクライアントを追加
                ClientSockets.Add(clientSocket);

                var clientIPEndPoint = (IPEndPoint)clientSocket.RemoteEndPoint;

                // 接続イベント発生
                OnServerConnected?.Invoke(this, new ConnectedEventArgs(clientIPEndPoint.Address.ToString(), clientIPEndPoint.Port));

                state = new StateObject
                {
                    ClientSocket = clientSocket,
                    IsReceiveTimeoutLoop = true,
                    IsReceived = false
                };

                // 非同期ソケットを開始して、受信する
                clientSocket.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);

                var start = DateTime.Now;

                // 受信タイムアウト処理
                while (state.IsReceiveTimeoutLoop)
                {
                    if (DateTime.Now - start > TimeSpan.FromMilliseconds(ReceiveTimeoutMillSec))
                    {
                        // 受信タイムアウト
                        RemoveClientSocket(clientSocket);

                        break;
                    }

                    if (state.IsReceived)
                    {
                        // 受信成功
                        state.IsReceived = false;

                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(MethodBase.GetCurrentMethod().Name);
                System.Diagnostics.Debug.WriteLine(ex.Message);

                RemoveClientSocket(clientSocket);
            }
            finally
            {
                state.IsReceiveTimeoutLoop = false;
            }
        }

        /// <summary>
        /// データ受信時の非同期コールバック処理
        /// </summary>
        /// <param name="asyncResult">受信結果</param>
        private void ReceiveCallback(IAsyncResult asyncResult)
        {
            // サービス終了時は処理しない
            if (!RunningListen)
            {
                return;
            }

            // データ受信格納情報、クライアントソケットを取得
            var state = asyncResult.AsyncState as StateObject;
            var clientSocket = state.ClientSocket;

            try
            {
                // クライアントソケットから受信データを取得終了
                var bytes = clientSocket.EndReceive(asyncResult);

                if (bytes > 0)
                {
                    // 受信成功
                    state.IsReceived = true;
                    // 呼び出し元スレッドが完了するまで待機
                    while (state.IsReceiveTimeoutLoop)
                    {
                        Thread.Sleep(100);
                    }

                    var serverIPEndPoint = clientSocket.RemoteEndPoint as IPEndPoint;
                    var serverReceivedEventArgs = new ServerReceivedEventArgs(serverIPEndPoint.Address.ToString(), serverIPEndPoint.Port, state.Buffer);

                    // 受信イベント発生
                    OnServerReceivedData?.Invoke(this, serverReceivedEventArgs);

                    if (serverReceivedEventArgs.IsSendAll)
                    {
                        // 全接続済みクライアントへ送信
                        SendAllClient(serverReceivedEventArgs.SendData);
                    }
                    else
                    {
                        // リクエストクライアントへ送信
                        Send(clientSocket, serverReceivedEventArgs.SendData);
                    }

                    // 非同期ソケットを開始して、受信する
                    clientSocket.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);

                    var start = DateTime.Now;

                    state.IsReceiveTimeoutLoop = true;

                    // 受信タイムアウト処理
                    while (state.IsReceiveTimeoutLoop)
                    {
                        if (DateTime.Now - start > TimeSpan.FromMilliseconds(ReceiveTimeoutMillSec))
                        {
                            // 受信タイムアウト
                            RemoveClientSocket(clientSocket);

                            break;
                        }

                        if (state.IsReceived)
                        {
                            // 受信成功
                            state.IsReceived = false;

                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(MethodBase.GetCurrentMethod().Name);
                System.Diagnostics.Debug.WriteLine(ex.Message);

                RemoveClientSocket(clientSocket);
            }
            finally
            {
                state.IsReceiveTimeoutLoop = false;
            }
        }

        /// <summary>
        /// 接続されたクライアント情報を格納するクラス
        /// </summary>
        private class StateObject
        {
            /// <summary>
            /// クライアントソケット
            /// </summary>
            public Socket ClientSocket { get; set; }

            /// <summary>
            /// 受信タイムアウトループするか
            /// </summary>
            public bool IsReceiveTimeoutLoop { get; set; }

            /// <summary>
            /// 受信したか
            /// </summary>
            public bool IsReceived { get; set; }

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
                // 非同期ソケットを開始して、送信する
                clientSocket.BeginSend(data, 0, data.Length, 0, new AsyncCallback(SendCallback), clientSocket);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(MethodBase.GetCurrentMethod().Name);
                System.Diagnostics.Debug.WriteLine(ex.Message);

                RemoveClientSocket(clientSocket);
            }
        }

        /// <summary>
        /// 全接続済みクライアントへのメッセージ送信処理
        /// </summary>
        /// <param name="data">データ</param>
        private void SendAllClient(byte[] data)
        {
            int index = ClientSockets.Count - 1;
            while (index > -1)
            {
                Send(ClientSockets[index], data);
                index--;
            }
        }

        /// <summary>
        /// 送信時の非同期コールバック処理
        /// </summary>
        /// <param name="asyncResult">送信結果</param>
        private void SendCallback(IAsyncResult asyncResult)
        {
            // サービス終了時は処理しない
            if (!RunningListen)
            {
                return;
            }

            Socket clientSocket = null;

            try
            {
                // クライアントソケットへのデータ送信処理を完了する
                clientSocket = asyncResult.AsyncState as Socket;
                var byteSize = clientSocket.EndSend(asyncResult);
                var clientIPEndPoint = (IPEndPoint)clientSocket.RemoteEndPoint;
                // 送信イベント発生
                OnServerSendData?.Invoke(this, new SendEventArgs(clientIPEndPoint.Address.ToString(), clientIPEndPoint.Port, byteSize));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(MethodBase.GetCurrentMethod().Name);
                System.Diagnostics.Debug.WriteLine(ex.Message);

                RemoveClientSocket(clientSocket);
            }
        }

        /// <summary>
        /// サーバー処理終了
        /// </summary>
        public void EndService()
        {
            RunningListen = false;

            // 待機スレッドが進行するようにシグナルをセット
            connectMre.Set();

            // リッスンタスク終了
            TaskListen?.Wait();
            TaskListen?.Dispose();
            TaskListen = null;

            // 保持しているすべての接続済みクライアント情報を削除
            int index = ClientSockets.Count - 1;
            while (index > -1)
            {
                var serverIPEndPoint = ClientSockets[index].RemoteEndPoint as IPEndPoint;
                OnServerDisconnected?.Invoke(this, new DisconnectedEventArgs(serverIPEndPoint.Address.ToString(), serverIPEndPoint.Port));

                ClientSockets.RemoveAt(index);
                index--;
            }
        }

        /// <summary>
        /// 保持している指定クライアント情報を削除
        /// </summary>
        /// <param name="clientSocket">クライアントソケット</param>
        /// <returns>削除したとき、true それ以外のとき、false</returns>
        private bool RemoveClientSocket(Socket clientSocket)
        {
            if (ClientSockets.Contains(clientSocket))
            {
                var serverIPEndPoint = clientSocket.RemoteEndPoint as IPEndPoint;
                OnServerDisconnected?.Invoke(this, new DisconnectedEventArgs(serverIPEndPoint.Address.ToString(), serverIPEndPoint.Port));

                clientSocket.Close();
                ClientSockets.Remove(clientSocket);

                return true;
            }

            return false;
        }

        /// <summary>
        /// 接続待機中か
        /// </summary>
        /// <returns>接続待機中のとき、true それ以外のとき、false</returns>
        public bool IsServiceRunning()
        {
            return RunningListen;
        }
    }
}
