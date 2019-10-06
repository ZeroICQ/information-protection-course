using System;
using System.Diagnostics;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace RSA {
public class Algorithms {
    public delegate bool TestPrimeDelegate(BigInteger n);
    private static RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider();
    private const char Delimeter = ':';
    

    public static void Encrypt(int bitsKeyLength, TestPrimeDelegate testPrime, string inputFilePath) {
        var bytesKeyLength = bitsKeyLength / 8;
        var p = GenerateLargePrimeNumber(bytesKeyLength, testPrime);
        var q = GenerateLargePrimeNumber(bytesKeyLength, testPrime);

        var n = p * q;
        var phiN = EulerFunction(p, q);
        var e = new BigInteger(65537);
        Debug.Assert(Gcd(e, phiN).IsOne);
        
//        // calculate d
        BigInteger d, tmp;
        var g = Gcd(e, phiN, out d, out tmp);
        d = (d % phiN + phiN) % phiN;
//        Console.WriteLine("Public key: " + n);
//        Console.WriteLine("Public exponent: " + e);
//        Console.WriteLine("Private key: " + d);
//        
//        
//        while (true) {
//            Console.WriteLine("Enter string to encrypt: ");
//            var sourceString = Console.ReadLine();
//            var encryptedString = Encrypt(sourceString, e, n);
//            Console.WriteLine("Encrypted string: " + encryptedString);
//
//            var decryptedString = Decrypt(encryptedString, d, n);
//            Console.WriteLine("Decrypted string: " + decryptedString);
//
//        }
    }
    
//    public static string Encrypt(string source, BigInteger e, BigInteger n) {
//        var bld = new StringBuilder();
//        
//        for (var i = 0; i < source.Length; i++) {
//            bld.Append((string) BinPowMod(source[i], e, n).ToString());
//            if (i != source.Length - 1)
//                bld.Append((char) Delimeter);
//        }
//
//        return bld.ToString();
//    }

//    public static string Decrypt(string source, BigInteger d, BigInteger n) {
//        var bld = new StringBuilder();
//        foreach (var encryptedChar in source.Split(Delimeter)) {
//            var decryptedInt = BinPowMod(BigInteger.Parse(encryptedChar), d, n);
//            var decryptedChar = Encoding.UTF8.GetString((byte[]) decryptedInt.ToByteArray());
//            bld.Append(decryptedChar);
//        }
//
//        return bld.ToString();
//    }

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
        throw new NotImplementedException();
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