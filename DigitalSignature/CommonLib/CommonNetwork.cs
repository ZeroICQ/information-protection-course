using System;
using System.Net;
using System.Net.Sockets;

namespace Common {
    public class CommonNetwork {
        public static readonly string HostAddress = "127.0.0.1";
        public static readonly int HostPort = 1337;
        
        static public IPEndPoint GetIPEndpoint() {
            var host = Dns.GetHostEntry(CommonNetwork.HostAddress);
            foreach (var ip in host.AddressList) {
                if (ip.AddressFamily == AddressFamily.InterNetwork) {
                    return new IPEndPoint(ip, CommonNetwork.HostPort);
                }
            }

            throw new CannotFindLocalIPException();
        }
    }
}