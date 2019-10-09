using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Common;
using static Common.CommonNetwork;

namespace DigitalSignature {
    class Master {
        static void Main(string[] args) {
            var localEndpoint = GetIPEndpoint();
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            byte[] bytes = new Byte[1024];
            string data = null;
            try {
                listener.Bind(localEndpoint);
                listener.Listen(1);

                while (true) {
                    Console.WriteLine("Waiting for a connection...");
                    // Program is suspended while waiting for an incoming connection.  
                    Socket handler = listener.Accept();
                    data = null;

                    // An incoming connection needs to be processed.  
                    while (true) {
                        int bytesRec = handler.Receive(bytes);
                        data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                        if (data.IndexOf("<EOF>") > -1) {
                            break;
                        }
                    }

                    // Show the data on the console.  
                    Console.WriteLine("Text received : {0}", data);

                    // Echo the data back to the client.  
                    byte[] msg = Encoding.ASCII.GetBytes(data);

                    handler.Send(msg);
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }
            }
            catch (Exception e) {
                Console.WriteLine(e);
            }
        }
    }
}