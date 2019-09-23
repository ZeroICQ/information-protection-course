using System;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace RSA {
class Program {
    private static RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider();
    
    static void Main(string[] args) {
        Console.WriteLine("Hello World!");
        var test = GenerateRandomNumber(1024/8);
        Console.WriteLine(test.ToString());
        Console.WriteLine((test - 1).ToString());
        rngCsp.Dispose();
    }

    static LargeUInt GenerateRandomNumber(uint bytesLength) {
        var number = new LargeUInt(bytesLength);
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

//    static bool TestMillerRabin(LargeUInt n, int rounds) {
//        LargeUInt nMinusOne = n - 1;
//    }

}
}