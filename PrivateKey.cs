using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Newtonsoft.Json;

namespace Messenger
{
    /// <summary>
    /// This class contains a private key
    /// </summary>
    public class PrivateKey
    {
        public string key { get; set; }
        public List<string> email { get; set; }
        
        /// <summary>
        /// Constructor for private key
        /// </summary>
        /// <param name="key"> base64 encoded key</param>
        public PrivateKey(string key)
        {
            this.key = key;
        }
        
        /// <summary>
        /// writes private key as a json file to current working directory
        /// </summary>
        public void WriteKey() {
            string filePath = @"./private.key";
            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            using (StreamWriter writer = File.CreateText(filePath)) {
                writer.Write(json);
            }
        }
    }
}