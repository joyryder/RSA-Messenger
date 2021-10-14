/***
 * @author Shrif Rai
 * CSCI251 Section 02
 * 03/18/2021
 */
using System;
using System.Diagnostics;
using System.Numerics;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Messenger
{
    /// <summary>
    /// a program used to generate prime numbers
    /// </summary>
    static class PrimeGen
    {
        /// <summary>
        /// the main program
        /// </summary>
        /// <param name="args"> command line arguments used to drive the program</param>
        public static BigInteger getPrimeN(int size)
        {
            BigInteger num = 0;
                do
                {
                    var rn = new RandomNumber(size/8);
                    num = rn.getRandomNum();
                } while (!IsProbablyPrime(num) || num % 2 ==0);

                return num;
        }
        
        /// <summary>
        /// checks if input BigInteger is a prime
        /// </summary>
        /// <param name="value"> value to check</param>
        /// <param name="witnesses"> a number that guarantees either the primality or compositeness of the value </param>
        /// <returns>true if value is prime, false if value is not prime</returns>
        static Boolean IsProbablyPrime(this BigInteger value, int witnesses = 10) {
            if (value <= 1) return false;
            if (witnesses <= 0) witnesses = 10;
            BigInteger d = value - 1;
            int s = 0;
            while (d % 2 == 0) {
                d /= 2;
                s += 1;
            }
            

            Byte[] bytes = new Byte[value.ToByteArray().LongLength];
            BigInteger a;
            for (int i = 0; i < witnesses; i++) {
                do {
                    var Gen = new Random();
                    Gen.NextBytes(bytes);
                    a = new BigInteger(bytes);
                } while (a < 2 || a >= value - 2);
                BigInteger x = BigInteger.ModPow(a, d, value);
                if (x == 1 || x == value - 1) continue;
                for (int r = 1; r < s; r++) {
                    x = BigInteger.ModPow(x, 2, value);
                    if (x == 1) return false;
                    if (x == value - 1) break;
                }
                if (x != value - 1) return false;
            }
            return true;
        }

    }

    /// <summary>
    /// class that generates a random number using RNGCryptoServiceProvider in parallel
    /// </summary>
    class RandomNumber
    {
        private static RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
        private int bytes;
        private BigInteger randomNumber;

        /// <summary>
        /// constructor for random number 
        /// </summary>
        /// <param name="bytes"> size of random number to generate in bytes</param>
        public RandomNumber(int bytes)
        {
            this.bytes = bytes;
            randomNumber = generateRandomNumber();
        }

        /// <summary>
        /// method used to get the generated random number
        /// </summary>
        /// <returns> a random number </returns>
        public BigInteger getRandomNum()
        {
            return randomNumber;
        }

        private BigInteger generateRandomNumber()
        {
            byte[] digits = new byte[bytes];
            Parallel.For(0, bytes, bytes =>
            {
                byte[] newByte = new byte[1];
                rng.GetBytes(newByte);
                digits[bytes] = newByte[0];
            });
            return new BigInteger(digits);
        }
    }
}