using System.IO;
using Newtonsoft.Json;

namespace Messenger
{
    /// <summary>
    /// object that contains message along with email
    /// </summary>
    public class Message
    {
        public string email { get; set; }
        public string content { get; set; }

        /// <summary>
        /// Constructor for message object
        /// </summary>
        /// <param name="email"> email the message is being sent to</param>
        /// <param name="content"> message in base64 encoded form</param>
        public Message(string email, string content)
        {
            this.email = email;
            this.content = content;
        }
    }
}