using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using Common;
using static Common.CommonNetwork;
using static Common.Algorithms;

// server sends encrypted document and signature.
namespace DigitalSignature {
    class Server {
        static void Main(string[] args) {
            var filePath = GetFilePath(args);
            if (filePath is null)
                return;
            
            var localEndpoint = GetIpEndpoint();
            var listener = GetSocket();
            try {
                listener.Bind(localEndpoint);
                listener.Listen(1);
                
                Console.WriteLine("Waiting for a connection...");
                var handler = listener.Accept();
                Console.WriteLine("Connection accepted.");
                Console.WriteLine("Generating keys....");
                // generate parameters p an g
                var p = GenerateLargePrimeNumber();
                var g = GenerateLargePrimeNumber();
                Debug.Assert(p.GetByteCount(true) <= 256/8);
                
                SendBigInteger(handler, p);
                SendBigInteger(handler, g);
                
                Console.WriteLine("p is");
                Console.WriteLine(p.ToString());
                    
                Console.WriteLine("g is");
                Console.WriteLine(g.ToString());

                var mySecretKey = GenerateRandomNumber();
                // A = g^a mod p
                var myOpenKey = BigInteger.ModPow(g, mySecretKey, p);
                
                // send my open key to client
                SendBigInteger(handler, myOpenKey);
                var otherOpenKey = ReceiveBigInteger(handler);
                var commonSecretKey = BigInteger.ModPow(otherOpenKey, mySecretKey, p);
                
                Console.WriteLine("My open key:");
                Console.WriteLine(myOpenKey.ToString());
                Console.WriteLine("My secret key:");
                Console.WriteLine(mySecretKey.ToString());
                Console.WriteLine("Common secret key:");
                Console.WriteLine(commonSecretKey.ToString());
                var secretKeyBytes = GetSecretKeyBytes(commonSecretKey); 
                EncryptAndSendFile(handler, filePath, secretKeyBytes);
                
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();

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

        private static void EncryptAndSendFile(Socket handler, string filePath, byte[] secretKey) {
            using (var reader = File.OpenRead(Path.GetFullPath(filePath))) {
                using (var aesAlg = Aes.Create()) {
                    Console.WriteLine("Key size is " + secretKey.LongLength);
                    
                    aesAlg.Key = secretKey;
                    
                    var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                    // Create the streams used for encryption.
                    using (var fsEncrypt = new FileStream(Algorithms.EncryptedFilePath, FileMode.Create)) {
                        using (var csEncrypt = new CryptoStream(fsEncrypt, encryptor, CryptoStreamMode.Write)) {
                            int b;

                            while ((b = reader.ReadByte()) != -1) {
                                var realB = new byte[1];
                                realB[0] = BitConverter.GetBytes(b)[0];
                                csEncrypt.Write(realB);
                            }
                        }
                    }
                    
                    SendBytes(handler, aesAlg.IV);
                }
            }
            var encryptedFilelength = new FileInfo(EncryptedFilePath).Length;
            SendLong(handler, encryptedFilelength);
            handler.SendFile(EncryptedFilePath);
//            File.Delete(EncryptedFilePath);
        }
        
    }
}