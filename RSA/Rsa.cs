using System;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace RSA {
class Program {
    private static RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider();
    
    static void Main(string[] args) {
        var test = GenerateRandomNumber(2);
        BigInteger a = new BigInteger();
        test.buf[0] = 45;
        test.buf[1] = 191;
        Console.WriteLine(TestMillerRabin(test, 2));
        rngCsp.Dispose();
    }

    static LargeUInt GenerateRandomNumber(int bytesLength) {
        var number = new LargeUInt(bytesLength);
        // make odd
        number.buf[number.buf.Length - 1] |= 1;
        rngCsp.GetBytes(number.buf);
        return number;
    }

//    static LargeUInt GenerateLargePrimeNumber(uint bytesLength) {
//        while (true) {
//            var candidate = GenerateRandomNumber(bytesLength);
//            if (test)
//                return candidate
//        }
//    }

    static bool TestMillerRabin(LargeUInt n, int rounds) {
        LargeUInt nMinusOne = n - 1;
        
        // n-1=2^s *t
        var s = 0;
        var t = new LargeUInt(nMinusOne);
        while (t.isEven) {
            t = LargeUInt.DivideBy2(t);
            s++;
        }

        for (int i = 0; i < rounds; i++) {
            //Выбрать случайное целое число a в отрезке [2, n − 2]
            var a = GenRndLUint(LargeUInt.FromInt(2), n - 2);
            var x = a pow t mod n;
        }

//        return true;
    }
    
    // [min, max]
    static LargeUInt GenRndLUint(LargeUInt min, LargeUInt max) {
        var res = new LargeUInt(max.buf.Length);
        
        if (min.buf.Length < max.buf.Length)
            min.IncreaseLengthTo(max.buf.Length);
        
        var isLessMax = false;
        var isMoreMin = false;
        var rnd = new Random();
        
        for (var i = 0; i < max.buf.Length; i++) {
            var upperBound = Byte.MaxValue + 1;
            if (!isLessMax)
                upperBound = max.buf[i] + 1;

            var lowerBound = Byte.MinValue;
            if (!isMoreMin)
                lowerBound = min.buf[i];
            
            var bt = (byte)rnd.Next(lowerBound, upperBound);
            
            if (bt < max.buf[i])
                isLessMax = true;

            if (bt > min.buf[i])
                isMoreMin = true;

            res.buf[i] = bt;
        }

        return res;
    }
}
}