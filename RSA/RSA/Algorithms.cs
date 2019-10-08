using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace RSA {
public class Algorithms {
    public delegate bool TestPrimeDelegate(BigInteger n);
    private static RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider();
    private const char Delimeter = ':';
    
    public static readonly string DefaultSourceFileName = "source.txt";
    public static readonly string DefaultEncryptedFileName = "encrypted.bin";
    public static readonly string DefaultDecryptedFileName = "decrypted.txt";
    public static readonly string DefaultPublicKeyFilename = "public.key";
    public static readonly string DefaultPrivateKeyFilename = "private.key";
    private static readonly BigInteger PublicExponent = new BigInteger(65537); 

    public static void Encrypt(int bitsKeyLength, TestPrimeDelegate testPrime, string inputFilePath, 
                               string sourceFilePath, string publicKeyFilePath, string privateKeyFilePath) 
    {
        var bytesKeyLength = bitsKeyLength / 8;
        var p = GenerateLargePrimeNumber(bytesKeyLength, testPrime);
        var q = GenerateLargePrimeNumber(bytesKeyLength, testPrime);

        var n = p * q;
        var phiN = EulerFunction(p, q);
        Debug.Assert(Gcd(PublicExponent, phiN).IsOne);
        // calculate d
        BigInteger d, tmp;
        var g = Gcd(PublicExponent, phiN, out d, out tmp);
        d = (d % phiN + phiN) % phiN;
        
        Console.WriteLine("Writing public key to " + publicKeyFilePath);
        WriteKeyToFile(n, publicKeyFilePath);
        Console.WriteLine("Writing private key to " + privateKeyFilePath);
        WriteKeyToFile(d, privateKeyFilePath);
        
        Console.WriteLine("Writing encrypted data to " + DefaultEncryptedFileName);
        using (var reader = File.OpenRead(Path.GetFullPath(DefaultSourceFileName))) {
            using (var writer = File.CreateText(Path.GetFullPath(DefaultEncryptedFileName))) {
                int b;
                var first = true;
                
                while ((b = reader.ReadByte()) != -1) {
                    if (!first) 
                        writer.Write(Delimeter);
                    else
                        first = false;
                    
                    var encryptedBytes = BinPowMod(b, PublicExponent, n);
                    writer.Write(encryptedBytes.ToString());
                }
            }
        }
    }

    private static void WriteKeyToFile(BigInteger key, string filePath) {
        using (var writer = File.Create(filePath)) {
            writer.Write(key.ToByteArray(true, true));
        }
    }

    public static void Decrypt(string encryptedFilePath, string privateKeyFilePath, string publicKeyFilePath) {
        BigInteger privateKey;
        BigInteger publicKey;
        privateKey = new BigInteger(File.ReadAllBytes(privateKeyFilePath), true, true);
        publicKey = new BigInteger(File.ReadAllBytes(publicKeyFilePath), true, true);

        var encryptedBytes = File.ReadAllText(DefaultEncryptedFileName).Split(Delimeter);
        using (var writer = File.Create(Path.GetFullPath(DefaultDecryptedFileName))) {
            foreach (var b in encryptedBytes) {
                var encryptedBigInt =  BigInteger.Parse(b);
                var decryptedByte = BinPowMod(encryptedBigInt, privateKey, publicKey);
                Debug.Assert(decryptedByte.GetByteCount() == 1);
                writer.WriteByte(decryptedByte.ToByteArray()[0]);
            }
        }
    }

    public static BigInteger GenerateRandomNumber(int bytesLength) {
        var buff = new byte[bytesLength];
        rngCsp.GetBytes(buff);
        return new BigInteger(buff, true);
    }
    
    public static BigInteger EulerFunction(BigInteger p, BigInteger q) {
        return (p - 1) * (q - 1);
    }

    private static BigInteger Gcd(BigInteger a, BigInteger b) {
        while (!b.IsZero) {
            a %= b;
            var tmp = a;
            a = b;
            b = tmp;
        }

        return a;
    }

    private static BigInteger Gcd(BigInteger a, BigInteger b, out BigInteger x, out BigInteger y) {
        if (a.IsZero) {
            x = 0;
            y = 1;
            return b;
        }

        BigInteger x1, y1;
        var d = Gcd(b % a, a, out x1, out y1);
        x = y1 - (b / a) * x1;
        y = x1;
        return d;
    }

    private static BigInteger GenerateLargePrimeNumber(int bytesLength, TestPrimeDelegate testPrime) {
        while (true) {
            var candidate = GenerateRandomNumber(bytesLength);
            while (candidate <= 2)
                candidate = GenerateRandomNumber(bytesLength);
            // make odd
            candidate |= 1;

            if (testPrime(candidate))
                return candidate;
        }
    }

    public static bool TestMillerRabin(BigInteger n) {
        const int rounds = 28;
        var nMinusOne = n - 1;
        // n-1=2^s *t
        var s = 0;
        var t = nMinusOne;
        while (t.IsEven) {
            t /= 2;
            s++;
        }

        for (var i = 0; i < rounds; i++) {
            // Choose rnd a в in [2, n − 2]
            outer:
            var a = GenerateRandomNumber(2, n - 2);
            //x = a pow t mod n;
            var x = BinPowMod(a, t, n);
            if (x == 1 || x == nMinusOne)
                continue;

            for (var j = 0; j < s; j++) {
                x = BinPowMod(x, 2, n);
                if (x.IsOne)
                    return false;
                if (x == nMinusOne)
                    goto outer;
            }
            return false;
        }

        return true;
    }

    public static bool TestTrialDivision(BigInteger n) {
        var closestSqrt = Sqrt(n);

        for (BigInteger i = 2; i <= closestSqrt; i++) {
            if (BigInteger.Remainder(n, i) == 0) {
                return false;
            }
        }

        return true;
    }

    private static BigInteger Sqrt(BigInteger n) {
        // [left, right]
        BigInteger left = 2;
        BigInteger right = n;
        
        var middle = (right + left) / 2;
        var answer = middle * middle;
        
        while (right - left != 1) {
            middle = (right + left) / 2;
            if (middle * middle >= n) {
                answer = middle;
                right = middle;
                continue;
            }

            left = middle;

        }

        return answer;
    }

    public static bool TestSolovayStrassen(BigInteger n) {
        throw new NotImplementedException();
    }

    private static BigInteger BinPowMod(BigInteger a, BigInteger power, BigInteger modulo) {
        BigInteger res = 1;
        while (!power.IsZero) {
            if (!power.IsEven) {
                res *= a;
                res %= modulo;
            }
            a *= a;
            a %= modulo;
            power /= 2;
        }

        return res;
    }
    
    // [min, max]
    public static BigInteger GenerateRandomNumber(BigInteger min, BigInteger max) {
        var maxByteArray = max.ToByteArray(true, true);
        var minByteArray = min.ToByteArray(true, true);
        Debug.Assert(maxByteArray.Length >= minByteArray.Length);
        
        var resByteArray = new byte[maxByteArray.Length];

        if (minByteArray.Length < maxByteArray.Length) {
            //increase length
            var newBuf = new byte[maxByteArray.Length];
            var lengthDiff = maxByteArray.Length - minByteArray.Length;
            Array.Copy(minByteArray, 0, newBuf, lengthDiff, minByteArray.Length);
            minByteArray = newBuf;
        }

        var isLessMax = false;
        var isMoreMin = false;
        var rnd = new Random();
        
        for (var i = 0; i < maxByteArray.Length; i++) {
            var upperBound = Byte.MaxValue + 1;
            if (!isLessMax)
                upperBound = maxByteArray[i] + 1;

            var lowerBound = Byte.MinValue;
            if (!isMoreMin)
                lowerBound = minByteArray[i];
            
            var rndByte = (byte)rnd.Next(lowerBound, upperBound);
            if (rndByte < maxByteArray[i])
                isLessMax = true;
            
            if (rndByte > minByteArray[i])
                isMoreMin = true;
            resByteArray[i] = rndByte;
        }

        return new BigInteger(resByteArray, true, true);
    }
}
}