using System;
using System.Net;
using System.Net.Sockets;
using System.Numerics;

namespace Common {
    public class CommonNetwork {
        private const string HostAddress = "127.0.0.1";
        private const int HostPort = 13370;

        public static IPEndPoint GetIpEndpoint() {
            var host = Dns.GetHostEntry(CommonNetwork.HostAddress);
            foreach (var ip in host.AddressList) {
                if (ip.AddressFamily == AddressFamily.InterNetwork) {
                    return new IPEndPoint(ip, CommonNetwork.HostPort);
                }
            }

            throw new CannotFindLocalIPException();
        }

        public static Socket GetSocket() {
            return new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public static void SendBigInteger(Socket handler, BigInteger n) {
            var bytes = n.ToByteArray(true, true);
            var lengthBytes = BitConverter.GetBytes(bytes.LongLength);

            handler.Send(lengthBytes);
            handler.Send(bytes);
        }

        public static BigInteger ReceiveBigInteger(Socket handler) {
            var lengthBytes = new byte[sizeof(long)];

            handler.Receive(lengthBytes);
            var length = BitConverter.ToInt64(lengthBytes);

            var bytes = new byte[length];
            handler.Receive(bytes);
            return new BigInteger(bytes, true, true);
        }

        public static void SendLong(Socket handler, long n) {
            var b = BitConverter.GetBytes(n);
            handler.Send(b);
        }
        
        public static long ReceiveLong(Socket handler) {
            var b = new byte[sizeof(long)];
            handler.Receive(b);
            return BitConverter.ToInt64(b);
        }

        public static void SendBytes(Socket handler, byte[] bytes) {
            SendLong(handler, bytes.LongLength);
            handler.Send(bytes);
        }
        
        public static byte[] ReceiveBytes(Socket handler) {
            var length = ReceiveLong(handler);
            var bytes = new byte[length];
            handler.Receive(bytes);
            return bytes;
        }
    }
}
