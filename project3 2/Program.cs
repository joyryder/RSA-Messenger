using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
namespace Messenger
{
    /// <summary>
    /// @author Shrif Rai sr1908
    /// COPADS Project 3
    /// April 30th, 2021
    /// This program sends and receives RSA-encrypted messages via kayrun server at RIT.
    /// Generates public and private keys using the RSA algorithm.
    /// </summary>
    class Program
    {
        /// <summary>
        /// used to represent Public and Private Key types
        /// </summary>
        public enum KeyType
        {
            PUBLIC, PRIVATE
        }
        
        /// <summary>
        /// handles getting key from the server
        /// </summary>
        /// <param name="email"> email of key being retrieved</param>
        /// <returns> none </returns>
        static async Task getKey(string email)
        {
            HttpClient cli = new HttpClient();
            var response = await cli.GetStringAsync("http://kayrun.cs.rit.edu:5000/Key/" + email);

            if (response == null || response.Length == 0)
            {
                Console.WriteLine("Key for that email does not exist.");
                Environment.Exit(1);
            }

            PublicKey publicKey = JsonConvert.DeserializeObject<PublicKey>(response);
            string json = JsonConvert.SerializeObject(publicKey, Formatting.Indented);
            using (StreamWriter writer = File.CreateText(@"./"+email+".key")) {
                writer.Write(json);
            }
        }
        
        /// <summary>
        /// this functions handles sending key
        /// </summary>
        /// <param name="email"> the email associated with the key being sent</param>
        /// <returns> none </returns>
        static async Task sendKey(string email)
        {
            var jsonObj = File.ReadAllText(@"./public.key");
            PublicKey publicK = JsonConvert.DeserializeObject<PublicKey>(jsonObj);
            publicK.email = email;
            publicK.WriteKey();
            jsonObj = File.ReadAllText(@"./public.key");
            HttpClient cli = new HttpClient();
            var toBeSent = new StringContent (jsonObj, Encoding.UTF8, "application/json");
            var response = await cli.PutAsync("http://kayrun.cs.rit.edu:5000/Key/"+email, toBeSent);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("Attempt to send key unsuccessful");
                Environment.Exit(1);
            }
            jsonObj = File.ReadAllText(@"./private.key");
            PrivateKey privateK = JsonConvert.DeserializeObject<PrivateKey>(jsonObj);
            privateK.email.Add(email);
            privateK.WriteKey();
        }

        /// <summary>
        /// handles sending messages to the server
        /// </summary>
        /// <param name="email"> who the message is being sent to</param>
        /// <param name="msg"> the message in plain text</param>
        /// <returns>none</returns>
        static async Task sendMsg(string email, string msg)
        {
            Message newMsg =null;
            try
            {
                var jsonObj = File.ReadAllText(@"./" + email + ".key");
                List<BigInteger> en = decodeKey(KeyType.PUBLIC, jsonObj);
                var E = en[0];
                var N = en[1];
                byte[] bytes = Encoding.UTF8.GetBytes(msg);
                BigInteger msgInt = new BigInteger(bytes);
                var c = BigInteger.ModPow(msgInt, E, N);
                var cipherArr = c.ToByteArray();
                var chiperEncoded = Convert.ToBase64String(cipherArr);
                newMsg = new Message(email, chiperEncoded);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            HttpClient cli = new HttpClient();
            string json = JsonConvert.SerializeObject(newMsg, Formatting.Indented);
            var toBeSent = new StringContent (json, Encoding.UTF8, "application/json");
            var response = await cli.PutAsync("http://kayrun.cs.rit.edu:5000/Message/"+email, toBeSent);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("Attempt to send message unsuccessful");
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// handles receiving messages from the server
        /// </summary>
        /// <param name="email"> email of the user receiving message</param>
        /// <returns> none </returns>
        static async Task getMsg(string email)
        {
            var jsonObj = File.ReadAllText(@"./private.key");
            PrivateKey privateK = JsonConvert.DeserializeObject<PrivateKey>(jsonObj);
            if (privateK == null || !privateK.email.Contains(email))
            {
                Console.WriteLine("No private key found for "+email+". Message can not be decoded");
                Environment.Exit(1);
            }
            
            HttpClient cli = new HttpClient();
            
            var response = await cli.GetStringAsync("http://kayrun.cs.rit.edu:5000/Message/" + email);

            if (response == null || response.Length == 0)
            {
                Console.WriteLine("No messages found");
                Environment.Exit(1);
            }
            
            Message msg = JsonConvert.DeserializeObject<Message>(response);
            var msgArr = Convert.FromBase64String(msg.content);
            BigInteger msgInt = new BigInteger(msgArr);
            var dn = decodeKey(KeyType.PRIVATE, jsonObj);
            var D = dn[0];
            var N = dn[1];
            var p = BigInteger.ModPow(msgInt, D, N);
            var plainTextArr = p.ToByteArray();
            Console.WriteLine(Encoding.UTF8.GetString(plainTextArr));
        }
        
        /// <summary>
        /// extracts E/D and N from public/private key for encryption/decryption
        /// </summary>
        /// <param name="type"> enum for public or private key type</param>
        /// <param name="key"> PublicKey or Private Key json string</param>
        /// <returns> list of BigIntegers: E/D and N</returns>
        public static List<BigInteger> decodeKey(KeyType type ,string key)
        {
            PublicKey publicK;
            PrivateKey privateK;
            byte[] keyArr = null;
            if (type == KeyType.PUBLIC)
            {
                publicK = JsonConvert.DeserializeObject<PublicKey>(key);
                keyArr = Convert.FromBase64String(publicK.key);
            }
            else if(type == KeyType.PRIVATE)
            {
                privateK = JsonConvert.DeserializeObject<PrivateKey>(key);
                keyArr = Convert.FromBase64String(privateK.key);
            }

            byte[] eSize = new byte[4];
            Buffer.BlockCopy(keyArr, 0, eSize, 0, 4);
            Array.Reverse(eSize);
            int e = BitConverter.ToInt32(eSize);
            byte[] eArr = new byte[e];
            Buffer.BlockCopy(keyArr, 4, eArr, 0, e);
            BigInteger X = new BigInteger(eArr);
            byte[] nSize = new byte[4];
            Buffer.BlockCopy(keyArr, 4+e, nSize, 0, 4);
            Array.Reverse(nSize);
            int n = BitConverter.ToInt32(nSize);
            byte[] nArr = new byte[n];
            Buffer.BlockCopy(keyArr, 4+e+4, nArr, 0, n);
            BigInteger N = new BigInteger(nArr);
            List<BigInteger> enList = new List<BigInteger>();
            enList.Add(X);
            enList.Add(N);
            return enList;
        }
        
        /// <summary>
        /// main function that processes command line args
        /// </summary>
        /// <param name="args"> command line arguments</param>
        /// <returns></returns>
        static async Task Main(string[] args)
        {
            try
            {
                if (args.Length == 1 && args[0].Equals("help"))
                    Console.WriteLine("Usage: dotnet run <option> <other arguments>\n" +
                                      "\t-keyGen keysize\n" +
                                      "\t-sendKey email\n" +
                                      "\t-getKey email\n" +
                                      "\t-sendMsg email plaintext\n" +
                                      "\t-getMsg email");
                else if (args.Length == 2 && args[0].Equals("keyGen"))
                {
                    var key = new RSAKey(args[1]);
                    Console.WriteLine("Keys generated");

                }
                else if (args.Length == 2 && args[0].Equals("sendKey"))
                {
                    if (!File.Exists(@"./private.key") || !File.Exists(@"./public.key"))
                    {
                        Console.WriteLine("Keys have not been generated.");
                        Environment.Exit(1);
                    }

                    await sendKey(args[1]);
                    Console.WriteLine("Key saved");
                }
                else if (args.Length == 2 && args[0].Equals("getKey"))
                {
                    await getKey(args[1]);
                    Console.WriteLine("Key retrieved");
                }
                else if (args.Length == 3 && args[0].Equals("sendMsg"))
                {
                    if (!File.Exists(@"./" + args[1] + ".key"))
                    {
                        Console.WriteLine("Key does not exist for " + args[1]);
                        Environment.Exit(1);
                    }
                    await sendMsg(args[1], args[2]);
                    Console.WriteLine("Message written");
                }
                else if (args.Length == 2 && args[0].Equals("getMsg"))
                {
                    await getMsg(args[1]);
                }
                else
                {
                    DisplayErrorUsage();
                }
            }
            catch (Exception e)
            {
                DisplayErrorUsage();
            }
        }

        /// <summary>
        /// Displays usage for errors
        /// </summary>
        private static void DisplayErrorUsage()
        {
            Console.WriteLine("Invalid arguments!" +
                              "\nUsage: dotnet run <option> <other arguments>\n" +
                              "\t-keyGen keysize\n" +
                              "\t-sendKey email\n" +
                              "\t-getKey email\n" +
                              "\t-sendMsg email plaintext\n" +
                              "\t-getMsg email");
        }
    }
}