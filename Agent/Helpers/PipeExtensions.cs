using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Agent.Helpers
{
    public static class PipeExtensions
    {

        const int PipeChunckSize = 20000;

        public static void SendMessage(this PipeStream self, byte[] bytes)
        {
            InnerSendByChuncks(Convert.ToBase64String(bytes), self);
        }

        public static byte[] ReceivedMessage(this PipeStream self, bool wait = true)
        {
            return Convert.FromBase64String(InnerReceiveByChuncks(self));
        }
        private static void InnerSendByChuncks(string b64, PipeStream stream)
        {
            var writer = new StreamWriter(stream);
            for (int i = 0; i < b64.Length; i += PipeChunckSize)
            {
                var tosend = b64.Substring(i, Math.Min(PipeChunckSize, b64.Length - i));
                //Debug.WriteLine(tosend);
                writer.WriteLine(tosend);
                writer.Flush();
            }
            writer.WriteLine(); //Empty line to close the sending
            writer.Flush();
        }

        private static string InnerReceiveByChuncks(PipeStream stream)
        {
            string mess = string.Empty;
            var reader = new StreamReader(stream);
            string line = string.Empty;
            while (true)
            {
                line = reader.ReadLine();
                //if (line == "<<EOF>>")
                //    break;
                if (!string.IsNullOrEmpty(line))
                    mess += line;
                else
                    break;
            }

            return mess;
        }

    }
}
