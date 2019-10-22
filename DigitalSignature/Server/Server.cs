using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Common;
using static Common.CommonNetwork;

// server sends encrypted document and signature.
namespace DigitalSignature {
    class Server {
        static void Main(string[] args) {
            var filePath = GetFilePath(args);
            if (filePath is null)
                return;
            Console.WriteLine(filePath);
            return;
            //----------------------------------
            var localEndpoint = GetIPEndpoint();
            var listener = GetSocket();
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
        
        // get file path from args
        private static string GetFilePath(string[] args) {
            if (args.Length != 0 && args[0].Length != 0) return args[0];
            Console.WriteLine("USAGE: ");
            Console.WriteLine("master [filepath]");
            return null;

        }
        
    }
}