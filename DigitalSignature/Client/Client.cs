﻿using System;
using System.Net;
using System.Net.Sockets;
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
                Socket sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);  
  
                // Connect the socket to the remote endpoint. Catch any errors.  
                try {  
                    sender.Connect(remoteEndpoint);  
  
                    Console.WriteLine("Socket connected to {0}",  
                        sender.RemoteEndPoint.ToString());

                    var p = ReceiveBigInteger(sender);
                    var g = ReceiveBigInteger(sender);
                    
                    Console.WriteLine("p is");
                    Console.WriteLine(p.ToString());
                    
                    Console.WriteLine("g is");
                    Console.WriteLine(g.ToString());
                    return;
  
                    // Encode the data string into a byte array.  
                    byte[] msg = Encoding.ASCII.GetBytes("This is a test<EOF>");  
  
                    // Send the data through the socket.  
                    int bytesSent = sender.Send(msg);  
  
                    // Receive the response from the remote device.  
                    int bytesRec = sender.Receive(bytes);  
                    Console.WriteLine("Echoed test = {0}",  
                        Encoding.ASCII.GetString(bytes,0,bytesRec));  
  
                    // Release the socket.  
                    sender.Shutdown(SocketShutdown.Both);  
                    sender.Close();  
  
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
    }
}