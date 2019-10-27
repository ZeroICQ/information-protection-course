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
                var secretKeyBytes = GetKeyBytes(commonSecretKey); 
                EncryptAndSendFile(handler, filePath, secretKeyBytes);
                CalculateAndSendSignature(handler);
                
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
            var encryptedFileLength = new FileInfo(EncryptedFilePath).Length;
            SendLong(handler, encryptedFileLength);
            handler.SendFile(EncryptedFilePath);
//            File.Delete(EncryptedFilePath);
        }
        
        // http://crypto-r.narod.ru/glava6/glava6_3.html
        private static void CalculateAndSendSignature(Socket handler) {
            var hash = CalculateHash();
            
            //generate G and P
            // 1< m<p-1 
            var p = GenerateLargePrimeNumber(BigInteger.Pow(2, 160));
            var g = GenerateRandomNumber(BigInteger.One, p-1);
            
            // (1, (P-1)). x - secret key 
            var x = GenerateRandomNumber(new BigInteger(2), p - 2);
            //y
            var openKey = BigInteger.ModPow(g, x, p);
            
            Console.WriteLine("Generating coprime K");
            BigInteger k;
            while (true) {
                var kCandidate = GenerateRandomNumber(new BigInteger(2), p - 2);
                if (Gcd(kCandidate, p - 1) != 1) continue;
                k = kCandidate;
                break;
            }

            var a = BigInteger.ModPow(g, k, p);
            
            // calc k_inverse
            BigInteger kInverse, tmp;
            Gcd(k, p - 1, out kInverse, out tmp);
            var b = (((hash - x * a) * kInverse % (p-1)) + (p-1)) % (p-1);
            Console.WriteLine("b is: " + b);
            
            //S=(a, b) - signature
            SendBigInteger(handler, g);
            SendBigInteger(handler, openKey);
            SendBigInteger(handler, p);
            SendBigInteger(handler, a);
            SendBigInteger(handler, b);
            

        }
    }
}