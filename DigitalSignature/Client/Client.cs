using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using Common;
using static Common.CommonNetwork;
using static Common.Algorithms;

// client gets document and signature from master, checks and decrypts.
namespace Client {
    class Client {
        static void Main(string[] args) {
            var remoteEndpoint = GetIpEndpoint();
            byte[] bytes = new byte[1024];  
  
            // Connect to a remote device.  
            try {  
                // Establish the remote endpoint for the socket.  
                Socket handler = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);  
  
                // Connect the socket to the remote endpoint. Catch any errors.  
                try {  
                    handler.Connect(remoteEndpoint);  
  
                    Console.WriteLine("Socket connected to {0}",  
                        handler.RemoteEndPoint.ToString());

                    var p = ReceiveBigInteger(handler);
                    var g = ReceiveBigInteger(handler);
                    
                    Console.WriteLine("p is");
                    Console.WriteLine(p.ToString());
                    
                    Console.WriteLine("g is");
                    Console.WriteLine(g.ToString());
                    
                    var mySecretKey = GenerateRandomNumber();
                    var myOpenKey = BigInteger.ModPow(g, mySecretKey, p);
                    var otherOpenKey = ReceiveBigInteger(handler);
                    SendBigInteger(handler, myOpenKey);
                    var commonSecretKey = BigInteger.ModPow(otherOpenKey, mySecretKey, p);

                    Console.WriteLine("My open key:");
                    Console.WriteLine(myOpenKey.ToString());
                    Console.WriteLine("My secret key:");
                    Console.WriteLine(mySecretKey.ToString());
                    Console.WriteLine("Common secret key:");
                    Console.WriteLine(commonSecretKey.ToString());
                    
                    ReceiveAndDecryptFile(handler);
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                    return;
                    
                    // Encode the data string into a byte array.  
                    byte[] msg = Encoding.ASCII.GetBytes("This is a test<EOF>");  
  
                    // Send the data through the socket.  
                    int bytesSent = handler.Send(msg);  
  
                    // Receive the response from the remote device.  
                    int bytesRec = handler.Receive(bytes);  
                    Console.WriteLine("Echoed test = {0}",  
                        Encoding.ASCII.GetString(bytes,0,bytesRec));  
  
                    // Release the socket.  
                    handler.Shutdown(SocketShutdown.Both);  
                    handler.Close();  
  
                } catch (ArgumentNullException ane) {  
                    Console.WriteLine("ArgumentNullException : {0}",ane.ToString());  
                } catch (SocketException se) {  
                    Console.WriteLine("SocketException : {0}",se.ToString());  
                } catch (Exception e) {  
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());  
                }  
  
            } catch (Exception e) {  
                Console.WriteLine( e.ToString());  
            }  
        }

        private static void ReceiveAndDecryptFile(Socket handler) {
            var fileLength = ReceiveLong(handler);
            var receivedBytes = 0;
            using (var writer = File.OpenWrite(EncryptedFilePath)) {
                var tmp = new byte[1];
                while (receivedBytes != fileLength) {
                    handler.Receive(tmp);
                    writer.Write(tmp);
                    receivedBytes++;
                }
                
            }
            
            
        }
    }
}