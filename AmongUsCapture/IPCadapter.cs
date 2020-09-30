using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Text;
using Newtonsoft.Json;

namespace AmongUsCapture
{
    class IPCadapter
    {
        private static IPCadapter instance = new IPCadapter();
        public event EventHandler<TokenReceivedArgs> OnToken;
        public static IPCadapter getInstance()
        {
            return instance;
        }
        public void runloop()
        {
            while (true)
            {
                NamedPipeServerStream pipeServer = new NamedPipeServerStream("AmongUsCapturePipe", PipeDirection.InOut, 1);

                pipeServer.WaitForConnection();
                Console.WriteLine("Client connected");
                try
                { 
                    //Read the request from the client. Once the client has
                    // written to the pipe its security token will be available.

                    StreamString ss = new StreamString(pipeServer);

                    // Verify our identity to the connected client using a
                    // string that the client anticipates.

                    ss.WriteString("Ready for token.");
                    string Base64String = ss.ReadString();
                    Console.WriteLine("Got data: "+Base64String);
                    byte[] data = System.Convert.FromBase64String(Base64String);
                    string Decoded = System.Text.UTF8Encoding.UTF8.GetString(data);
                    TokenJson jsonToken = JsonConvert.DeserializeObject<TokenJson>(Decoded);
                    TokenReceivedArgs thing = new TokenReceivedArgs();
                    thing.SigBytes = jsonToken.SigBytes;
                    thing.token = jsonToken.token;
                    Console.WriteLine($@"Decoded message as {JsonConvert.SerializeObject(jsonToken, Formatting.Indented)}");
                    OnToken?.Invoke(this, thing);

                }
                // Catch the IOException that is raised if the pipe is broken
                // or disconnected.
                catch (IOException e)
                {
                    Console.WriteLine(@"ERROR: {0}", e.Message);
                }
                pipeServer.Close();
            }

        }
    }
    public class StreamString
    {
        private Stream ioStream;
        private UnicodeEncoding streamEncoding;

        public StreamString(Stream ioStream)
        {
            this.ioStream = ioStream;
            streamEncoding = new UnicodeEncoding();
        }

        public string ReadString()
        {
            int len = 0;

            len = ioStream.ReadByte() * 256;
            len += ioStream.ReadByte();
            byte[] inBuffer = new byte[len];
            ioStream.Read(inBuffer, 0, len);

            return streamEncoding.GetString(inBuffer);
        }

        public int WriteString(string outString)
        {
            byte[] outBuffer = streamEncoding.GetBytes(outString);
            int len = outBuffer.Length;
            if (len > UInt16.MaxValue)
            {
                len = (int)UInt16.MaxValue;
            }
            ioStream.WriteByte((byte)(len / 256));
            ioStream.WriteByte((byte)(len & 255));
            ioStream.Write(outBuffer, 0, len);
            ioStream.Flush();

            return outBuffer.Length + 2;
        }
    }
    public class TokenReceivedArgs : EventArgs
    {
        public string token { get; set; }
        public byte[] SigBytes { get; set; }
    }
    public class TokenJson
    {
        public string token { get; set; }
        public byte[] SigBytes { get; set; }
    }

}
