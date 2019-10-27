using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Security.Cryptography;
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
                    
                    var secretKeyBytes = GetKeyBytes(commonSecretKey);
                    ReceiveAndDecryptFile(handler, secretKeyBytes);
                    var isVerified = GetAndVerifySignature(handler);
                    Console.WriteLine("File is verified: " + isVerified);
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

        private static bool GetAndVerifySignature(Socket handler) {
            var hash = CalculateHash();
            var g = ReceiveBigInteger(handler);
            var openKey = ReceiveBigInteger(handler);
            var p = ReceiveBigInteger(handler);
            var a = ReceiveBigInteger(handler);
            var b = ReceiveBigInteger(handler);

            var A = (BigInteger.ModPow(openKey, a, p) * BigInteger.ModPow(a, b, p)) % p;
            return A == BigInteger.ModPow(g, hash, p);
        }

        private static void ReceiveAndDecryptFile(Socket handler, byte[] secretKey) {
            var IV = ReceiveBytes(handler);
            
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
            
            Decrypt(secretKey, IV);
            
        }

        private static void Decrypt(byte[] secretKey, byte[] IV) {
            using (var aesAlg = Aes.Create()) {
                aesAlg.Key = secretKey;
                aesAlg.IV = IV;

                // Create a decryptor to perform the stream transform.
                var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using (var writer = File.OpenWrite("decrypted")) {
                    using (var msDecrypt = new FileStream(EncryptedFilePath, FileMode.Open)) {
                        using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read)) {
                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            int b;
                            
                            while ((b = csDecrypt.ReadByte()) != -1) {
                                var realB = new byte[1];
                                realB[0] = BitConverter.GetBytes(b)[0];
                                writer.Write(realB);
                            }
                        }
                    }
                }
            }
        }
    }
}