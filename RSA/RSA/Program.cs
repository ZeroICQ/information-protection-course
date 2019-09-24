using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

[assembly: InternalsVisibleTo("UnitTests")]
namespace RSA {
class Program {
    private static RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider();
    
    static void Main(string[] args) {
        const int bytesKeyLength = 8/8;
        var p = GenerateLargePrimeNumber(bytesKeyLength);
        var q = GenerateLargePrimeNumber(bytesKeyLength);

        var n = p * q;
        var phiN = EulerFunction(p, q);
        var e = new BigInteger(65537);
        Debug.Assert(Gcd(e, phiN).IsOne);
        
        // count d
        BigInteger d, tmp;
        var g = Gcd(e, phiN, out d, out tmp);
        d = (d % phiN + phiN) % phiN;
        Console.WriteLine(d);
        Console.WriteLine(phiN);
        
        // encrypt
        var c = 'a';
        var h = BinPowMod(c, e, n);
        Console.WriteLine(h);;

        Console.WriteLine((char)BinPowMod(h, d, n));
        Debug.Assert(c == (char)BinPowMod(h, d, n));
        rngCsp.Dispose();
    }

    public static BigInteger GenerateRandomNumber(int bytesLength) {
        var buff = new byte[bytesLength];
        rngCsp.GetBytes(buff);
        return new BigInteger(buff, true);
    }

    public static BigInteger EulerFunction(BigInteger p, BigInteger q) {
        return (p - 1) * (q - 1);
    }

    static BigInteger Gcd(BigInteger a, BigInteger b) {
        while (!b.IsZero) {
            a %= b;
            var tmp = a;
            a = b;
            b = tmp;
        }

        return a;
    }

    static BigInteger Gcd(BigInteger a, BigInteger b, out BigInteger x, out BigInteger y) {
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

    static BigInteger BinPowMod(BigInteger a, BigInteger power, BigInteger modulo) {
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