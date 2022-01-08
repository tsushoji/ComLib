using System;
using System.Threading;
using System.Threading.Tasks;

namespace ComTCP
{
    public class TCPComBase
    {
        /// <summary>
        /// 接続スレッド待機用
        /// </summary>
        protected readonly ManualResetEvent connectMre = new ManualResetEvent(false);

        /// <summary>
        /// ポールソケットタスク
        /// </summary>
        protected Task TaskPollSocket { get; set; }

        /// <summary>
        /// 受信タイムアウトミリ秒
        /// </summary>
        protected int ReceiveTimeoutMillSec { get; set; }

        /// <summary>
        /// ソケットロックオブジェクト
        /// </summary>
        protected object SocketLockObj { get; set; } = new object();

        /// <summary>
        /// 接続イベントハンドラー
        /// </summary>
        /// <param name="sender">イベント発生オブジェクト</param>
        /// <param name="e">接続イベントデータ</param>
        public delegate void ConnectedEventHandler(object sender, ConnectedEventArgs e);

        /// <summary>
        /// 接続イベントデータ
        /// </summary>
        public class ConnectedEventArgs : EventArgs
        {
            /// <summary>
            /// 接続先IP
            /// </summary>
            public string IP { get; }

            /// <summary>
            /// 接続先ポート
            /// </summary>
            public int Port { get; }

            /// <summary>
            /// 引数付きコンストラクタ
            /// </summary>
            /// <param name="ip">接続先IP</param>
            /// <param name="port">接続先ポート</param>
            internal ConnectedEventArgs(string ip, int port)
            {
                IP = ip;
                Port = port;
            }
        }

        /// <summary>
        /// 送信イベントハンドラー
        /// </summary>
        /// <param name="sender">イベント発生オブジェクト</param>
        /// <param name="e">送信イベントデータ</param>
        public delegate void SendDataEventHandler(object sender, SendEventArgs e);

        /// <summary>
        /// 送信イベントデータ
        /// </summary>
        public class SendEventArgs : EventArgs
        {
            /// <summary>
            /// 送信先IP
            /// </summary>
            public string IP { get; }

            /// <summary>
            /// 送信先ポート
            /// </summary>
            public int Port { get; }

            /// <summary>
            /// 送信バイト数
            /// </summary>
            public int SendByteNum { get; }

            /// <summary>
            /// 引数付きコンストラクタ
            /// </summary>
            /// <param name="ip">IP</param>
            /// <param name="port">ポート</param>
            /// <param name="sendByteNum">送信バイト数</param>
            internal SendEventArgs(string ip, int port, int sendByteNum)
            {
                IP = ip;
                Port = port;
                SendByteNum = sendByteNum;
            }
        }

        /// <summary>
        /// 切断イベントハンドラー
        /// </summary>
        /// <param name="sender">イベント発生オブジェクト</param>
        /// <param name="e">切断イベントデータ</param>
        public delegate void DisconnectedEventHandler(object sender, DisconnectedEventArgs e);

        /// <summary>
        /// 切断イベントデータ
        /// </summary>
        public class DisconnectedEventArgs : EventArgs
        {
            /// <summary>
            /// 切断先IP
            /// </summary>
            public string IP { get; }

            /// <summary>
            /// 切断先ポート
            /// </summary>
            public int Port { get; }

            /// <summary>
            /// 引数付きコンストラクタ
            /// </summary>
            /// <param name="ip">接続先IP</param>
            /// <param name="port">接続先ポート</param>
            internal DisconnectedEventArgs(string ip, int port)
            {
                IP = ip;
                Port = port;
            }
        }

        /// <summary>
        /// 受信イベントデータ
        /// </summary>
        public class ReceivedEventArgs : EventArgs
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
            internal ReceivedEventArgs(string ip, int port, byte[] receivedData)
            {
                IP = ip;
                Port = port;
                ReceivedData = receivedData;
            }
        }

        ~TCPComBase()
        {
            connectMre.Dispose();
            SocketLockObj = null;
        }
    }
}
