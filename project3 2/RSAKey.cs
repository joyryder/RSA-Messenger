using System;
using System.Collections.Generic;
using System.Numerics;

namespace Messenger
{
    /// <summary>
    /// Class that generates the private and public keys
    /// </summary>
    class RSAKey
    {
        private PublicKey publicK;
        private PrivateKey privateK;
        private BigInteger N, r, D;
        private BigInteger E = 65537;

        /// <summary>
        /// Constructor to generate keys
        /// </summary>
        /// <param name="bitsSize"> size of key in bits</param>
        public RSAKey(String bitsSize)
        {
            var keySize = Int32.Parse(bitsSize);
            keySize = keySize / 2;
            var p = PrimeGen.getPrimeN(keySize);
            var q = PrimeGen.getPrimeN(keySize);
            N = p * q;
            r = (p - 1) * (q - 1);
            D = modInverse(E, r);
            
            byte[] part1 = BitConverter.GetBytes(E.ToByteArray().Length);
            byte[] part2 = E.ToByteArray();
            Array.Reverse(part1);
            byte[] part3 = BitConverter.GetBytes(N.ToByteArray().Length);
            byte[] part4 = N.ToByteArray();
            Array.Reverse(part3);
            byte[] publicKey = new byte[part1.Length + part2.Length + part3.Length + part4.Length];
            Buffer.BlockCopy(part1, 0, publicKey, 0, part1.Length);
            Buffer.BlockCopy(part2, 0, publicKey, part1.Length, part2.Length);
            Buffer.BlockCopy(part3, 0, publicKey, part1.Length + part2.Length, part3.Length);
            Buffer.BlockCopy(part4, 0, publicKey, part1.Length + part2.Length+part3.Length, part4.Length);
            publicK = new PublicKey(Convert.ToBase64String(publicKey), "");
            publicK.WriteKey();
            
            byte[] part5 = BitConverter.GetBytes(D.ToByteArray().Length);
            Array.Reverse(part5);
            byte[] part6 = D.ToByteArray();
            byte[] privateKey = new byte[part3.Length + part4.Length + part5.Length + part6.Length];
            Buffer.BlockCopy(part5, 0, privateKey, 0, part5.Length);
            Buffer.BlockCopy(part6, 0, privateKey, part5.Length, part6.Length);
            Buffer.BlockCopy(part3, 0, privateKey, part5.Length + part6.Length, part3.Length);
            Buffer.BlockCopy(part4, 0, privateKey, part3.Length+ part5.Length + part6.Length, part4.Length);
            privateK = new PrivateKey(Convert.ToBase64String(privateKey));
            privateK.email = new List<string>();
            privateK.WriteKey();
        }
        
        
        
        /// <summary>
        /// mod inverses E and r to produce D
        /// </summary>
        /// <param name="a"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        static BigInteger modInverse(BigInteger a, BigInteger n){
            BigInteger i = n, v = 0, d = 1;
            while (a > 0)
            {
                BigInteger t = i/a, x = a;
                a = i % x;
                i = x;
                x = d;
                d = v - t*x;
                v = x;
            }
            v %= n;
            if (v<0) v = (v+n)%n;
            return v;
        }
    }
}