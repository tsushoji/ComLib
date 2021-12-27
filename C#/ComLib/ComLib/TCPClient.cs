using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace ComTCP
{
    public class TCPClient
    {
        /// <summary>
        /// 接続スレッド待機用
        /// </summary>
        private readonly ManualResetEvent connectMre = new ManualResetEvent(false);

        /// <summary>
        /// ソケット
        /// </summary>
        private Socket Socket { get; set; }

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
        /// 接続したか
        /// </summary>
        private bool IsConnected { get; set; }

        /// <summary>
        /// 受信したか
        /// </summary>
        private bool IsReceived { get; set; }

        /// <summary>
        /// 受信タイムアウトループするか
        /// </summary>
        private bool IsReceiveTimeoutLoop { get; set; }

        /// <summary>
        /// 接続タイムアウトミリ秒
        /// </summary>
        private int ConnectTimeoutMillSec { get; set; }

        /// <summary>
        /// 受信タイムアウトミリ秒
        /// </summary>
        private int ReceiveTimeoutMillSec { get; set; }

        /// <summary>
        /// 接続イベントハンドラー
        /// </summary>
        /// <param name="sender">イベント発生オブジェクト</param>
        /// <param name="receivedData">受信データ</param>
        /// <param name="connectedEndPoint">接続エンドポイント</param>
        public delegate void ConnectEventHandler(object sender, EventArgs e, IPEndPoint connectedEndPoint);

        /// <summary>
        /// 接続イベント
        /// </summary>
        public event ConnectEventHandler OnClientConnected;

        /// <summary>
        /// データ受信イベントハンドラー
        /// </summary>
        /// <param name="sender">イベント発生オブジェクト</param>
        /// <param name="receivedData">受信データ</param>
        public delegate void ReceiveEventHandler(object sender, byte[] receivedData);

        /// <summary>
        /// データ受信イベント
        /// </summary>
        public event ReceiveEventHandler OnClientReceiveData;

        /// <summary>
        /// デストラクタ
        /// </summary>
        ~TCPClient()
        {
            connectMre.Dispose();
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

            EndPoint ServerIPEndPoint;
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
                    IsConnectTimeoutLoop = true;

                    // ソケット作成
                    Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                    connectMre.Reset();
                    // 非同期ソケットを開始して、接続する
                    IAsyncResult asyncResult = Socket.BeginConnect(ServerIPEndPoint, new AsyncCallback(ConnectCallback), Socket);
                    // 接続コールバック処理が完了するまでスレッドを待機
                    connectMre.WaitOne();

                    // 接続成功
                    if (IsConnected)
                    {
                        IsConnected = false;

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
                var start = DateTime.Now;
                // ソケットを取得
                var clientSocket = asyncResult.AsyncState as Socket;

                // 接続タイムアウト
                while (IsConnectTimeoutLoop)
                {
                    if (DateTime.Now - start > TimeSpan.FromMilliseconds(ConnectTimeoutMillSec))
                    {
                        break;
                    }

                    if (clientSocket.Connected)
                    {
                        // 接続成功

                        IsConnectTimeoutLoop = false;
                        IsConnected = true;

                        // 待機スレッドが進行するようにシグナルをセット
                        connectMre.Set();

                        // 非同期ソケットを終了
                        // 保留中のスレッドを待機させたまま、呼び出すとエラーになるため、上記で待機スレッド進行
                        clientSocket.EndConnect(asyncResult);

                        // 接続イベント発生
                        OnClientConnected?.Invoke(this, new EventArgs(), (IPEndPoint)clientSocket.RemoteEndPoint);

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
                IsReceiveTimeoutLoop = true;

                // 非同期ソケットを開始して、受信する
                IAsyncResult asyncResult = Socket.BeginReceive(Buffer, 0, BufferSize, 0, new AsyncCallback(ReceiveCallback), Socket);

                var start = DateTime.Now;

                // 受信タイムアウト
                while (IsReceiveTimeoutLoop)
                {
                    if (DateTime.Now - start > TimeSpan.FromMilliseconds(ReceiveTimeoutMillSec))
                    {
                        IsReceiveTimeoutLoop = false;
                        // 切断
                        DisConnect();
                        break;
                    }

                    // 受信成功
                    if (IsReceived)
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(MethodBase.GetCurrentMethod().Name);
                System.Diagnostics.Debug.WriteLine(ex.Message);
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
                int byteSize = -1;
                // ソケットを取得
                var socket = asyncResult.AsyncState as Socket;
                // 非同期ソケットを終了
                byteSize = socket.EndReceive(asyncResult);

                if (byteSize > 0)
                {
                    // 受信
                    IsReceived = true;
                    Thread.Sleep(1000);

                    // データ受信イベント発生
                    OnClientReceiveData?.Invoke(this, Buffer);

                    IsReceiveTimeoutLoop = true;
                    IsReceived = false;

                    // 非同期ソケットを開始して、受信する
                    IAsyncResult asyncResultCallback = socket.BeginReceive(Buffer, 0, BufferSize, 0, new AsyncCallback(ReceiveCallback), Socket);

                    var start = DateTime.Now;

                    // 受信タイムアウト
                    while (IsReceiveTimeoutLoop)
                    {
                        if (DateTime.Now - start > TimeSpan.FromMilliseconds(ReceiveTimeoutMillSec))
                        {
                            IsReceiveTimeoutLoop = false;
                            // 切断
                            DisConnect();
                            break;
                        }

                        // 受信成功
                        if (IsReceived)
                        {
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

                // タイムアウトループ終了
                IsConnectTimeoutLoop = false;
                IsReceiveTimeoutLoop = false;
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
        /// <param name="sendData">データ</param>
        /// <returns>送信可否</returns>
        public bool Send(byte[] sendData)
        {
            try
            {
                Socket.Send(sendData);
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
        /// 接続状態を取得
        /// </summary>
        /// <returns>接続されているとき、true それ以外のとき、false</returns>
        public bool GetConnectedStatus()
        {
            if (Socket != null)
            {
                return Socket.Connected;
            }

            return false;
        }
    }
}
