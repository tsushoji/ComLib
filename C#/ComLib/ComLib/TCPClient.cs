using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace ComTCP
{
    public class TCPClient : TCPComBase
    {
        /// <summary>
        /// 送信スレッド待機用
        /// </summary>
        protected readonly ManualResetEvent clientSendMre = new ManualResetEvent(false);

        /// <summary>
        /// ソケット
        /// </summary>
        private Socket Socket { get; set; }

        /// <summary>
        /// サーバーのエンドポイント
        /// </summary>
        private IPEndPoint ServerIPEndPoint { get; set; }

        /// <summary>
        /// バッファーサイズ
        /// </summary>
        public const int BufferSize = 1024;

        /// <summary>
        /// バッファー
        /// </summary>
        private byte[] Buffer { get; set; } = new byte[BufferSize];

        /// <summary>
        /// 接続タイムアウトループするか
        /// </summary>
        private bool IsConnectTimeoutLoop { get; set; }

        /// <summary>
        /// 接続タイムアウトミリ秒
        /// </summary>
        private int ConnectTimeoutMillSec { get; set; }

        /// <summary>
        /// 受信タイムアウトループするか
        /// </summary>
        private bool IsReceiveTimeoutLoop { get; set; }

        /// <summary>
        /// 接続したか
        /// </summary>
        private bool IsConnectDone { get; set; }

        /// <summary>
        /// 受信したか
        /// </summary>
        private bool IsReceiveDone { get; set; }

        /// <summary>
        /// 送信したか
        /// </summary>
        private bool IsSendDone { get; set; }

        /// <summary>
        /// 接続イベント
        /// </summary>
        public event ConnectedEventHandler OnClientConnected;

        /// <summary>
        /// 送信イベント
        /// </summary>
        public event SendDataEventHandler OnClientSendData;

        /// <summary>
        /// 切断イベント
        /// </summary>
        public event DisconnectedEventHandler OnClientDisconnected;

        /// <summary>
        /// データ受信イベントハンドラー
        /// </summary>
        /// <param name="sender">イベント発生オブジェクト</param>
        /// <param name="receivedEventArgs">受信イベントデータ</param>
        public delegate void ClientReceivedDataEventHandler(object sender, ClientReceivedEventArgs e);

        /// <summary>
        /// データ受信イベント
        /// </summary>
        public event ClientReceivedDataEventHandler OnClientReceivedData;

        /// <summary>
        /// 受信イベントデータ
        /// </summary>
        public class ClientReceivedEventArgs : EventArgs
        {
            /// <summary>
            /// 受信元IP
            /// </summary>
            public string IP { get; }

            /// <summary>
            /// 受信元ポート
            /// </summary>
            public int Port { get; }

            /// <summary>
            /// 受信データ
            /// </summary>
            public byte[] ReceivedData { get; }

            /// <summary>
            /// 引数付きコンストラクタ
            /// </summary>
            /// <param name="ip">受信元IP</param>
            /// <param name="port">受信元ポート</param>
            /// <param name="receivedData">受信データ</param>
            internal ClientReceivedEventArgs(string ip, int port, byte[] receivedData)
            {
                IP = ip;
                Port = port;
                ReceivedData = receivedData;
            }
        }

        /// <summary>
        /// デストラクタ
        /// </summary>
        ~TCPClient()
        {
            clientSendMre.Dispose();
            Buffer = null;
        }

        /// <summary>
        /// 接続
        /// </summary>
        /// <param name="ip">IP</param>
        /// <param name="port">ポート</param>
        /// <param name="connectTimeoutMillSec">接続タイムアウトミリ秒</param>
        /// <param name="receiveTimeoutMillSec">受信タイムアウトミリ秒</param>
        /// <param name="reTryNum">リトライ回数</param>
        /// <returns>接続可否</returns>
        public bool Connect(string ip, int port, int connectTimeoutMillSec, int receiveTimeoutMillSec, int reTryNum)
        {
            ConnectTimeoutMillSec = connectTimeoutMillSec;
            ReceiveTimeoutMillSec = receiveTimeoutMillSec;

            try
            {
                // サーバーエンドポイント作成
                var ipAddress = IPAddress.Parse(ip);
                ServerIPEndPoint = new IPEndPoint(ipAddress, port);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(MethodBase.GetCurrentMethod().Name);
                System.Diagnostics.Debug.WriteLine(ex.Message);

                return false;
            }

            try
            {
                do
                {
                    // ソケット作成
                    Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                    connectMre.Reset();
                    // 非同期ソケットを開始して、接続する
                    IAsyncResult asyncResult = Socket.BeginConnect(ServerIPEndPoint, new AsyncCallback(ConnectCallback), Socket);
                    // 接続コールバック処理が完了するまでスレッドを待機
                    connectMre.WaitOne();

                    if (IsConnectDone)
                    {
                        // 接続成功
                        IsConnectDone = false;

                        // 受信開始
                        Task.Factory.StartNew(() =>
                        {
                            Receive();
                        });

                        return true;
                    }

                    // ソケット解放
                    // 同じソケットで複数回リモートホスト接続への非同期要求はできない
                    Socket.Dispose();
                    Socket = null;

                    reTryNum--;
                }
                while (reTryNum > -1);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(MethodBase.GetCurrentMethod().Name);
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }

            // 切断
            DisConnect();

            return false;
        }

        /// <summary>
        /// 接続時の非同期コールバック処理
        /// </summary>
        /// <param name="asyncResult">接続結果</param>
        private void ConnectCallback(IAsyncResult asyncResult)
        {
            try
            {
                // ソケットを取得
                var clientSocket = asyncResult.AsyncState as Socket;

                IsConnectTimeoutLoop = true;

                var start = DateTime.Now;

                // 接続タイムアウト処理
                while (IsConnectTimeoutLoop)
                {
                    if (DateTime.Now - start > TimeSpan.FromMilliseconds(ConnectTimeoutMillSec))
                    {
                        // 接続タイムアウト
                        break;
                    }

                    if (clientSocket.Connected)
                    {
                        // 接続成功
                        IsConnectTimeoutLoop = false;
                        IsConnectDone = true;

                        // 待機スレッドが進行するようにシグナルをセット
                        connectMre.Set();

                        // 非同期ソケットを終了
                        // 保留中のスレッドを待機させたまま、呼び出すとエラーになるため、上記で待機スレッド進行
                        clientSocket.EndConnect(asyncResult);

                        // 接続イベント発生
                        OnClientConnected?.Invoke(this, new ConnectedEventArgs(ServerIPEndPoint.Address.ToString(), ServerIPEndPoint.Port));

                        return;
                    }
                }

                // 接続失敗
                IsConnectTimeoutLoop = false;

                // 待機スレッドが進行するようにシグナルをセット
                connectMre.Set();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(MethodBase.GetCurrentMethod().Name);
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// 受信
        /// </summary>
        public void Receive()
        {
            try
            {
                // 非同期ソケットを開始して、受信する
                IAsyncResult asyncResult = Socket.BeginReceive(Buffer, 0, BufferSize, 0, new AsyncCallback(ReceiveCallback), Socket);

                IsReceiveTimeoutLoop = true;

                var start = DateTime.Now;

                // 受信タイムアウト処理
                while (IsReceiveTimeoutLoop)
                {
                    if (DateTime.Now - start > TimeSpan.FromMilliseconds(ReceiveTimeoutMillSec))
                    {
                        // 受信タイムアウト
                        if (Socket != null && Socket.Connected)
                        {
                            DisConnect();
                        }
                        break;
                    }

                    if (IsReceiveDone)
                    {
                        // 受信成功
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(MethodBase.GetCurrentMethod().Name);
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            finally
            {
                IsReceiveTimeoutLoop = false;
            }
        }

        /// <summary>
        /// データ受信時の非同期コールバック処理
        /// </summary>
        /// <param name="asyncResult">受信結果</param>
        private void ReceiveCallback(IAsyncResult asyncResult)
        {
            try
            {
                // ソケットを取得
                var socket = asyncResult.AsyncState as Socket;

                // 非同期ソケットを終了
                var byteSize = socket.EndReceive(asyncResult);

                if (byteSize > 0)
                {
                    // 受信成功
                    IsReceiveDone = true;

                    // 受信イベント発生
                    OnClientReceivedData?.Invoke(this, new ClientReceivedEventArgs(ServerIPEndPoint.Address.ToString(), ServerIPEndPoint.Port, Buffer));

                    // 呼び出し元スレッドが完了するまで待機
                    while (IsReceiveTimeoutLoop)
                    {
                        Thread.Sleep(100);
                    }

                    // 非同期ソケットを開始して、受信する
                    IAsyncResult asyncResultCallback = socket.BeginReceive(Buffer, 0, BufferSize, 0, new AsyncCallback(ReceiveCallback), Socket);

                    IsReceiveTimeoutLoop = true;
                    IsReceiveDone = false;

                    var start = DateTime.Now;

                    // 受信タイムアウト処理
                    while (IsReceiveTimeoutLoop)
                    {
                        if (DateTime.Now - start > TimeSpan.FromMilliseconds(ReceiveTimeoutMillSec))
                        {
                            // 受信タイムアウト
                            if (Socket != null && Socket.Connected)
                            {
                                DisConnect();
                            }
                            break;
                        }

                        if (IsReceiveDone)
                        {
                            // 受信成功
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(MethodBase.GetCurrentMethod().Name);
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            finally
            {
                IsReceiveTimeoutLoop = false;
            }
        }

        /// <summary>
        /// 切断
        /// </summary>
        public void DisConnect()
        {
            try
            {
                // ソケット終了
                Socket?.Shutdown(SocketShutdown.Both);
                Socket?.Disconnect(false);
                Socket?.Dispose();
                Socket = null;

                IsConnectTimeoutLoop = false;
                IsReceiveTimeoutLoop = false;

                IsConnectDone = false;
                IsReceiveDone = false;
                IsSendDone = false;

                OnClientDisconnected?.Invoke(this, new DisconnectedEventArgs(ServerIPEndPoint.Address.ToString(), ServerIPEndPoint.Port));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(MethodBase.GetCurrentMethod().Name);
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// 送信
        /// </summary>
        /// <param name="data">データ</param>
        /// <returns>送信可否</returns>
        public bool Send(byte[] data)
        {
            try
            {
                var byteSize = Socket.Send(data);

                var clientIPEndPoint = (IPEndPoint)Socket.RemoteEndPoint;
                // 送信イベント発生
                OnClientSendData?.Invoke(this, new SendEventArgs(ServerIPEndPoint.Address.ToString(), ServerIPEndPoint.Port, byteSize));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(MethodBase.GetCurrentMethod().Name);
                System.Diagnostics.Debug.WriteLine(ex.Message);

                return false;
            }

            return true;
        }

        /// <summary>
        /// 接続されているか
        /// </summary>
        /// <returns>接続されているとき、true それ以外のとき、false</returns>
        public bool IsConnected()
        {
            if (Socket != null)
            {
                return Socket.Connected;
            }

            return false;
        }
    }
}
