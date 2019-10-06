using System;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using static RSA.Algorithms;

[assembly: InternalsVisibleTo("UnitTests")]
namespace RSA {
class Program {
    const int DefaultKeyLength = 256;

    static void Main(string[] args) {
        while (true) {
            Console.WriteLine("===MAIN MENU===");
            Console.WriteLine("1) Encrypt.");
            Console.WriteLine("2) Decrypt.");
            Console.WriteLine("0) Exit.");

            int input;
            if (!int.TryParse(Console.ReadLine(), out input))
                continue;

            switch (input) {
                case 0:
                    return;
                case 1:
                    EncryptMenu();
                    break;
                case 2:
//                    DecryptMenu();
                    break;
            }
        }
        
//        
//        var p = GenerateLargePrimeNumber(bytesKeyLength);
//        var q = GenerateLargePrimeNumber(bytesKeyLength);
//
//        var n = p * q;
//        var phiN = EulerFunction(p, q);
//        var e = new BigInteger(65537);
//        Debug.Assert(Gcd(e, phiN).IsOne);
//        
//        // count d
//        BigInteger d, tmp;
//        var g = Gcd(e, phiN, out d, out tmp);
//        d = (d % phiN + phiN) % phiN;
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
        
        
//--------------------------------        
//        var c = 'a';
//        var h = BinPowMod(c, e, n);
//        Console.WriteLine(h);;
//
//        Console.WriteLine((char)BinPowMod(h, d, n));
////        Debug.Assert(c == (char)BinPowMod(h, d, n));
//        rngCsp.Dispose();
    }

    private static void EncryptMenu() {
        var bitsKeyLength = ChooseKeyLength();
        var testPrime = ChoosePrimeTest();
        var inputFilePath = ChooseInputFilePath();
        Encrypt(bitsKeyLength, testPrime, inputFilePath);
    }

    private static string ChooseInputFilePath() {
        const string defaultFileName = "source.txt";
        
        while (true) {
            Console.Write("Enter path to file [" + defaultFileName + "]: ");
            var input = Console.ReadLine();
            if (input.Length == 0)
                input = defaultFileName;
            
            var path = Path.GetFullPath(input);
            
            if (!File.Exists(path)) {
                Console.WriteLine("File " + path  + " doesn't exist.");
                continue;
            }

            return path;
        }
    }

    private static TestPrimeDelegate ChoosePrimeTest() {
        while (true) {
            Console.WriteLine("Choose method to test prime number: ");
            Console.WriteLine("1 - Trial division");
            Console.WriteLine("2 - Miller-Rabin");
            Console.WriteLine("3 - Solovay–Strassen");
            Console.Write("Choose [1]: ");
            
            var input = Console.ReadLine().Trim();
            if (input.Length == 0) {
                return TestTrialDivision;
            }

            int intInput;
            if (!int.TryParse(input, out intInput)) {
                Console.WriteLine(input + " is not a number.");
                continue;
            }
            
            switch (intInput) {
                case 1:
                    return TestTrialDivision;
                case 2:
                    return TestMillerRabin;
                case 3:
                    return TestSolovayStrassen;
                default:
                    continue;
            }
        }
    }

    // returns key length in bits.
    private static int ChooseKeyLength() {
        while (true) {
            Console.Write("Please choose key length in bits [" + DefaultKeyLength + "]: ");
            var input = Console.ReadLine().Trim();
            if (input.Length == 0) {
                return DefaultKeyLength;
            }

            int intInput;
            if (!int.TryParse(input, out intInput)) {
                Console.WriteLine(input + " is not a number.");
                continue;
            }

            if (intInput % 8 != 0) {
                Console.WriteLine("Key length must be divisible by 8.");
                continue;
            }

            return intInput;
        }
    }

    
}
}