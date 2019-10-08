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
                    DecryptMenu();
                    break;
            }
        }
    }

    private static void DecryptMenu() {
        var encryptedFilePath = ChooseFilePath("encrypted", DefaultEncryptedFileName, true);
        var privateKeyFilePath = ChooseFilePath("private key", DefaultPrivateKeyFilename, true);
        var publicKeyFilePath = ChooseFilePath("public key", DefaultPublicKeyFilename, true);
        Decrypt(encryptedFilePath, privateKeyFilePath, publicKeyFilePath);
    }

    private static void EncryptMenu() {
        var bitsKeyLength = ChooseKeyLength();
        var testPrime = ChoosePrimeTest();
        var sourceFilePath = ChooseFilePath("source", DefaultSourceFileName, true);
        var publicKeyFilePath = ChooseFilePath("public key", DefaultPublicKeyFilename, false);
        var privateKeyFilePath = ChooseFilePath("private key", DefaultPrivateKeyFilename, false);
        Encrypt(bitsKeyLength, testPrime, sourceFilePath, sourceFilePath, publicKeyFilePath, privateKeyFilePath);
    }

    private static string ChooseFilePath(string description, string defaultFileName, bool isShouldExist) {
        while (true) {
            Console.Write("Enter path to " + description + " file [" + defaultFileName + "]: ");
            var input = Console.ReadLine();
            if (input.Length == 0)
                input = defaultFileName;
            
            var path = Path.GetFullPath(input);
            
            if (isShouldExist && !File.Exists(path)) {
                Console.WriteLine("File " + path  + " doesn't exist.");
                continue;
            }

            return path;
        }
    }

    private static TestPrimeDelegate ChoosePrimeTest() {
        while (true) {
            Console.WriteLine("Choose method to test prime number: ");
            Console.WriteLine("1 - Miller-Rabin");
            Console.WriteLine("2 - Trial division");
            Console.WriteLine("3 - Solovay–Strassen");
            Console.Write("Choose [1]: ");
            
            var input = Console.ReadLine().Trim();
            if (input.Length == 0) {
                return TestMillerRabin;
            }

            int intInput;
            if (!int.TryParse(input, out intInput)) {
                Console.WriteLine(input + " is not a number.");
                continue;
            }
            
            switch (intInput) {
                case 1:
                    return TestMillerRabin;
                case 2:
                    return TestTrialDivision;
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
            if (intInput < 1) {
                Console.WriteLine("Key must be a positive integer");
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