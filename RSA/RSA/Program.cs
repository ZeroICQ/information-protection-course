using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

[assembly: InternalsVisibleTo("UnitTests")]
namespace RSA {
class Program {
    private static RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider();
    
    static void Main(string[] args) {
        var p = GenerateLargePrimeNumber(1024/8);
        rngCsp.Dispose();
    }

    public static BigInteger GenerateRandomNumber(int bytesLength) {
        var buff = new byte[bytesLength];
        rngCsp.GetBytes(buff);
        return new BigInteger(buff, true);
    }

    static BigInteger GenerateLargePrimeNumber(int bytesLength) {
        while (true) {
            var candidate = GenerateRandomNumber(bytesLength);
            while (candidate <= 2)
                candidate = GenerateRandomNumber(bytesLength);
            // make odd
            candidate |= 1;

            if (TestMillerRabin(candidate, 28))
                return candidate;
        }
    }

    static bool TestMillerRabin(BigInteger n, int rounds) {
        var nMinusOne = n - 1;
        
        // n-1=2^s *t
        var s = 0;
        var t = nMinusOne;
        while (t.IsEven) {
            t /= 2;
            s++;
        }

        for (var i = 0; i < rounds; i++) {
            //Выбрать случайное целое число a в отрезке [2, n − 2]
            var a = GenerateRandomNumber(2, n - 2);
//            var x = a pow t mod n;
        }

        return true;
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
            var upperBound = byte.MaxValue + 1;
            if (!isLessMax)
                upperBound = maxByteArray[i] + 1;

            var lowerBound = byte.MinValue;
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