using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Agent.Helpers
{
    public static class PipeExtensions
    {
        [DllImport("kernel32.dll", EntryPoint = "PeekNamedPipe", SetLastError = true)]
        static extern bool PeekNamedPipe(IntPtr hNamedPipe, IntPtr lpBuffer,
           IntPtr nBufferSize, IntPtr lpBytesRead, ref uint lpTotalBytesAvail,
           IntPtr lpBytesLeftThisMessage);

        public static bool DataAvailable(this PipeStream pipe)
        {
            var hPipe = pipe.SafePipeHandle.DangerousGetHandle();
            uint nb = 0;
            return PeekNamedPipe(hPipe, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, ref nb, IntPtr.Zero);
        }

        public static async Task WriteStream(this Stream stream, byte[] data)
        {
            // format data as [length][value]
            var lengthBuf = BitConverter.GetBytes(data.Length);
            var lv = new byte[lengthBuf.Length + data.Length];

            Buffer.BlockCopy(lengthBuf, 0, lv, 0, lengthBuf.Length);
            Buffer.BlockCopy(data, 0, lv, lengthBuf.Length, data.Length);

            using (var ms = new MemoryStream(lv))
            {

                // write in chunks
                var bytesRemaining = lv.Length;
                do
                {
                    var lengthToSend = bytesRemaining < 1024 ? bytesRemaining : 1024;
                    var buf = new byte[lengthToSend];

                    var read = await ms.ReadAsync(buf, 0, lengthToSend);

                    if (read != lengthToSend)
                        throw new Exception("Could not read data from stream");

                    await stream.WriteAsync(buf, 0, buf.Length);

                    bytesRemaining -= lengthToSend;
                }
                while (bytesRemaining > 0);
            }
        }

    }
}
