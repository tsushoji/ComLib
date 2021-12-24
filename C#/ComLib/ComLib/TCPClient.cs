using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ComTCP
{
    public class TCPClient
    {
        /// <summary>
        /// サーバーエンドポイント
        /// </summary>
        public IPEndPoint ServerIPEndPoint { get; private set; }

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
        public byte[] Buffer { get; } = new byte[BufferSize];

        /// <summary>
        /// 接続タイムアウトループするか
        /// </summary>
        private bool IsConnectTimeoutLoop { get; set; }

        /// <summary>
        /// 受信タイムアウトループするか
        /// </summary>
        private bool IsReceiveTimeoutLoop { get; set; }

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
        public delegate void ConnectEventHandler(object sender, EventArgs e, EndPoint connectedEndPoint);

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
            do
            {
                try
                {
                    // 受信タイムアウトセット
                    ReceiveTimeoutMillSec = receiveTimeoutMillSec;

                    // サーバーエンドポイント作成
                    var ipAddress = IPAddress.Parse(ip);
                    ServerIPEndPoint = new IPEndPoint(ipAddress, port);

                    // リモートホスト接続
                    Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    Socket.Connect(ServerIPEndPoint);

                    IsConnectTimeoutLoop = true;
                    var start = DateTime.Now;

                    // リモートホスト接続を開始
                    IAsyncResult asyncResult = Socket.BeginConnect(ServerIPEndPoint, new AsyncCallback(ConnectCallback), Socket);

                    // 接続タイムアウト
                    while (IsConnectTimeoutLoop)
                    {
                        if (DateTime.Now - start > TimeSpan.FromMilliseconds(connectTimeoutMillSec))
                        {
                            // 接続終了
                            Socket.EndConnect(asyncResult);

                            // 切断
                            DisConnect();

                            return false;
                        }
                        Thread.Sleep(100);
                    }

                    // 接続成功
                    if (!IsConnectTimeoutLoop)
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);

                    // 切断
                    DisConnect();

                    return false;
                }

                reTryNum--;
            }
            while (reTryNum > -1);

            return false;
        }

        /// <summary>
        /// 接続時の非同期コールバック処理
        /// </summary>
        /// <param name="asyncResult">接続結果</param>
        private void ConnectCallback(IAsyncResult asyncResult)
        {
            IsConnectTimeoutLoop = false;
            Thread.Sleep(100);

            // ソケットを取得
            var clientSocket = asyncResult.AsyncState as Socket;
            clientSocket.EndConnect(asyncResult);

            // 接続イベント発生
            OnClientConnected?.Invoke(this, new EventArgs(), clientSocket.RemoteEndPoint);

            IsReceiveTimeoutLoop = true;
            var start = DateTime.Now;

            // データ受信開始
            Socket.BeginReceive(Buffer, 0, BufferSize, 0, new AsyncCallback(ReceiveCallback), Socket);

            // 受信タイムアウト
            while (IsReceiveTimeoutLoop)
            {
                if (DateTime.Now - start > TimeSpan.FromMilliseconds(ReceiveTimeoutMillSec))
                {
                    // 受信終了
                    Socket.EndReceive(asyncResult);

                    // 切断
                    DisConnect();

                    break;
                }
                Thread.Sleep(100);
            }
        }

        /// <summary>
        /// データ受信時の非同期コールバック処理
        /// </summary>
        /// <param name="asyncResult">受信結果</param>
        private void ReceiveCallback(IAsyncResult asyncResult)
        {
            IsReceiveTimeoutLoop = false;
            Thread.Sleep(100);

            // ソケットを取得
            var socket = asyncResult.AsyncState as Socket;

            var byteSize = -1;
            try
            {
                // 受信を待機
                byteSize = socket.EndReceive(asyncResult);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return;
            }

            if (byteSize > 0)
            {
                // 受信したバイナリーデータを取得
                var receivedData = new byte[byteSize];
                Array.Copy(Buffer, receivedData, byteSize);

                // データ受信イベント発生
                OnClientReceiveData?.Invoke(this, receivedData);

                IsReceiveTimeoutLoop = true;
                var start = DateTime.Now;

                // 再度受信を開始
                socket.BeginReceive(this.Buffer, 0, this.Buffer.Length, 0, ReceiveCallback, socket);

                // 受信タイムアウト
                while (IsReceiveTimeoutLoop)
                {
                    if (DateTime.Now - start > TimeSpan.FromMilliseconds(ReceiveTimeoutMillSec))
                    {
                        // 受信終了
                        socket.EndReceive(asyncResult);

                        // 切断
                        DisConnect();

                        break;
                    }
                    Thread.Sleep(100);
                }
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
            }
            catch (Exception ex)
            {
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
                System.Diagnostics.Debug.WriteLine(ex.Message);

                return false;
            }

            return true;
        }
    }
}
