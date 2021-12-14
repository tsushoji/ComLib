﻿using System;
using System.Net;
using System.Net.Sockets;

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
        /// 接続
        /// </summary>
        /// <param name="ip">IP</param>
        /// <param name="port">ポート</param>
        public void Connect(string ip, int port)
        {
            try
            {
                // サーバーエンドポイント作成
                var ipAddress = IPAddress.Parse(ip);
                ServerIPEndPoint = new IPEndPoint(ipAddress, port);

                // サーバーと接続
                Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                Socket.Connect(ServerIPEndPoint);

                // データ受信開始
                Socket.BeginReceive(Buffer, 0, BufferSize, SocketFlags.None, new AsyncCallback(ReceiveCallback), Socket);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// 切断
        /// </summary>
        public void DisConnect()
        {
            Socket?.Disconnect(false);
            Socket?.Dispose();
        }

        /// <summary>
        /// 送信
        /// </summary>
        /// <param name="sendData">データ</param>
        public void Send(byte[] sendData)
        {
            Socket.Send(sendData);
        }

        /// <summary>
        /// データ受信時の非同期コールバック処理
        /// </summary>
        /// <param name="asyncResult">受信結果</param>
        private void ReceiveCallback(IAsyncResult asyncResult)
        {
            var socket = asyncResult.AsyncState as Socket;

            var byteSize = -1;
            try
            {
                // 受信を待機
                byteSize = socket.EndReceive(asyncResult);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }

            if (byteSize > 0)
            {
                // 受信したバイナリーデータを取得
                var receivedData = new byte[byteSize];
                Array.Copy(Buffer, receivedData, byteSize);

                // データ受信イベント発生
                OnServerReceiveData?.Invoke(this, receivedData);

                // 再度受信を開始
                socket.BeginReceive(this.Buffer, 0, this.Buffer.Length, SocketFlags.None, ReceiveCallback, socket);
            }
        }
    }
}
