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
        public event EventHandler<StartToken> OnToken;
        public static IPCadapter getInstance()
        {
            return instance;
        }
        public void RunLoop(string initialURI)
        {
            if(initialURI != null)
            {
                OnToken?.Invoke(this, StartToken.FromString(initialURI));
            }
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

                    string rawToken = ss.ReadString();
                    StartToken startToken = StartToken.FromString(rawToken);
                    Console.WriteLine($@"Decoded message as {JsonConvert.SerializeObject(startToken, Formatting.Indented)}");
                    OnToken?.Invoke(this, startToken);
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
    public class StartToken : EventArgs
    {
        public string Host { get; set; }
        public string ConnectCode { get; set; }

        public static StartToken FromString(string rawToken)
        {
            try
            {
                byte[] data = Convert.FromBase64String(rawToken);
                string Decoded = Encoding.UTF8.GetString(data);
                return JsonConvert.DeserializeObject<StartToken>(Decoded);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return new StartToken();
            }
        }
    }

}
