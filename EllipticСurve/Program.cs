using System;
using System.Numerics;
using System.Text;
using GostECC;

// https://habr.com/ru/post/191240/
namespace EllipticСurve {
    class Program {
        static void Main(string[] args) {
            BigInteger p = new BigInteger("6277101735386680763835789423207666416083908700390324961279", 10);
            BigInteger a = new BigInteger("-3", 10);
            BigInteger b = new BigInteger("64210519e59c80e70fa7e9ab72243049feb8deecc146b9b1", 16);
            byte[] xG = FromHexStringToByte("03188da80eb03090f67cbf20eb43a18800f4ff0afd82ff1012");
            BigInteger n = new BigInteger("ffffffffffffffffffffffff99def836146bc9b1b4d22831", 16);           
            DSGost DS = new DSGost(p, a, b, n, xG);
            BigInteger d=DS.GenPrivateKey(192);
            ECPoint Q = DS.GenPublicKey(d);            
            GOST hash = new GOST(256);
            byte[] H = hash.GetHash(Encoding.Default.GetBytes("Message"));
            string sign  = DS.SignGen(H, d);
            bool result = DS.SignVer(H, sign, Q);
            Console.WriteLine(result);
        }

        static byte[] FromHexStringToByte(string input) {
            byte[] data = new byte[input.Length / 2];
            string HexByte = "";
            for (int i = 0; i < data.Length; i++) {
                HexByte = input.Substring(i * 2, 2);
                data[i] = Convert.ToByte(HexByte, 16);
            }

            return data;
        }
    }
}