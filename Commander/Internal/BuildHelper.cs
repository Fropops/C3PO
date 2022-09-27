using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commander.Internal
{
    public class ExecuteResult
    {
        public int Result { get; set; }
        public string Out { get; set; }
    }
    public class BuildHelper
    {

        public static ExecuteResult ExecuteCommand(string fileName, List<string> args, string startIn)
        {
            ExecuteResult result = new ExecuteResult();
            try
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

                result.Result = process.ExitCode;
                result.Out = output;
            }
            catch(Exception ex)
            {
                result.Result = -1;
                result.Out = ex.ToString();
            }

            return result;
        }
    }
}
