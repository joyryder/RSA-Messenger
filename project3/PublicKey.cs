using System.IO;
using Newtonsoft.Json;

namespace Messenger
{
    /// <summary>
    /// This class contains a public key and an associated email
    /// </summary>
    public class PublicKey
    {
        public string key { get; set; }
        public string email { get; set; }

        /// <summary>
        /// Constructor for public key
        /// </summary>
        /// <param name="key"> Base64 encoded key</param>
        /// <param name="email"> email associated with the key</param>
        public PublicKey(string key, string email)
        {
            this.key = key;
            this.email = email;
        }
        
        /// <summary>
        /// writes the key to current working directory as a json file
        /// </summary>
        public void WriteKey() {
            string filePath = @"./public.key";
            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            using (StreamWriter writer = File.CreateText(filePath)) {
                writer.Write(json);
            }
        }
    }
}