using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commander.Internal
{
    public class BinMaker
    {
        public static string DonutFolder = "/opt/donut/";
        public static string GenerateBin(string dotnetExePath, string binPath, string parameters)
        {
            var cmd = Path.Combine(DonutFolder, "donut");
            var inputFile = dotnetExePath;
            var outFile = binPath;

            List<string> args = new List<string>();
            args.Add(inputFile);
            args.Add("-f 1");
            args.Add("-a 2");
            args.Add($"-o");
            args.Add(outFile);
            args.Add($"-p");
            args.Add($"{parameters}");

            var ret = ExecuteCommand(cmd, args, DonutFolder);
            return ret;
        }

        //public static string GenerateDll(string binPath, string dllPath)
        //{

        //}

        public static string ExecuteCommand(string fileName, List<string> args, string startIn)
        {
            Collection<string> collection = new Collection<string>(args);

            var psi =
            new ProcessStartInfo
            {
                FileName = fileName,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                WorkingDirectory = startIn,
            };

            foreach (var arg in args)
                psi.ArgumentList.Add(arg);

            var process = Process.Start(psi);




            string output = string.Empty;
            process.OutputDataReceived += (s, e) => { output += e.Data + Environment.NewLine; };
            process.ErrorDataReceived += (s, e) => { output += e.Data + Environment.NewLine; };

            process.Start();

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            process.WaitForExit();

            return output;
        }
    }
}
