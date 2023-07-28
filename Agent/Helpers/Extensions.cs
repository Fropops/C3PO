using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Net.Sockets;

namespace Agent
{
    public static class Extensions
    {
        public static byte[] Serialize<T>(this T obj)
        {
            var serializer = new DataContractJsonSerializer(typeof(T));
            using (var ms = new MemoryStream())
            {
                serializer.WriteObject(ms, obj);
                return ms.ToArray();
            }
        }

        public static T Deserialize<T>(this byte[] json)
        {
            var serializer = new DataContractJsonSerializer(typeof(T));
            using (var ms = new MemoryStream(json))
            {
                return (T)serializer.ReadObject(ms);
            }
        }

        public static string[] GetArgs(this string src)
        {
            if(src == null)
                return new string[0];

            var res = new List<string>();
            src = src.Trim();

            bool inQuotes = false;
            bool inDoubleQuotes = false;

            string currentValue = string.Empty;
            foreach (var c in src)
            {
                if (c == '\"')
                {
                    if (!inQuotes)
                    {
                        if (inDoubleQuotes)
                        {
                            //end of params => 
                            inDoubleQuotes = false;
                            res.Add(currentValue);
                            currentValue = string.Empty;
                        }
                        else
                            inDoubleQuotes = true;
                    }
                    else
                    {
                        currentValue += c;
                    }
                    continue;
                }

                if (c== '\'')
                {
                    if (!inDoubleQuotes)
                    {
                        if (inQuotes)
                        {
                            //end of params => 
                            inQuotes = false;
                            res.Add(currentValue);
                            currentValue = string.Empty;
                        }
                        else
                            inQuotes = true;
                    }
                    else
                    {
                        currentValue += c;
                    }
                    continue;
                }

                if (c == ' ')
                {
                    if (!inQuotes && !inDoubleQuotes)
                    {
                        if (!string.IsNullOrEmpty(currentValue))
                        {
                            res.Add(currentValue);
                            currentValue = string.Empty;
                        }
                    }
                    else
                        currentValue += c;
                    continue;
                }

                currentValue += c;
            }


            if (!string.IsNullOrEmpty(currentValue))
            {
                res.Add(currentValue);
                currentValue = string.Empty;
            }

            return res.ToArray();
        }

        public static void Clear(this MemoryStream stream)
        {
            var buffer = stream.GetBuffer();
            Array.Clear(buffer, 0, buffer.Length);
            stream.Position = 0;
            stream.SetLength(0);
        }

        public static async Task<byte[]> ReadStream(this Stream stream)
        {
            // read length
            var lengthBuf = new byte[4];
            var read = await stream.ReadAsync(lengthBuf, 0, 4);

            if (read != 4)
                throw new Exception("Failed to read length");

            var length = BitConverter.ToInt32(lengthBuf, 0);

            // read rest of data
            using (var ms = new MemoryStream())
            {
                var totalRead = 0;

                do
                {
                    var buf = length - totalRead >= 1024 ? new byte[1024] : new byte[length - totalRead];
                    read = await stream.ReadAsync(buf, 0, buf.Length);

                    await ms.WriteAsync(buf, 0, read);
                    totalRead += read;
                }
                while (totalRead < length);

                return ms.ToArray();
            }
        }
    }

}
