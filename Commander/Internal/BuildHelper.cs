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
    public class ExecuteResult
    {
        public int Result { get; set; }
        public string Out { get; set; }
    }
    public class BuildHelper
    {
        public static string SourceFolder { get; set; } = Path.Combine(Environment.CurrentDirectory, "Source");
        public static string TmpFolder { get; set; } = "/Share/tmp/C2/Commander/Tmp";

        public static string NimPath { get; set; } = "/usr/bin/nim";

        public static List<string> ComputeNimBuildParameters(string scriptName, string outFile, bool isDebug, bool isDll)
        {
            string path = SourceFolder;
            path =  Path.Combine(path, scriptName);
            if (!Path.GetExtension(path).Equals(".nim", StringComparison.OrdinalIgnoreCase))
                path += ".nim";

            var parms = new List<string>();

            parms.Add("c");

            if (isDll)
                parms.Add("--app:lib");
            else if (isDebug)
                parms.Add("--app:console");
            else
                parms.Add("--app:gui");


            if (!isDebug)
            {
                parms.Add("-d:release");
                parms.Add("-d:strip");
                parms.Add("--passL:-s");
            }

            parms.Add("-f");
            parms.Add("-d:mingw");
            parms.Add($"-o:{outFile}");


            parms.Add($"{path}");

            return parms;
        }

        public static ExecuteResult NimBuild(List<string> parms)
        {
            return BuildHelper.ExecuteCommand(NimPath, parms, SourceFolder);
        }




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
            catch (Exception ex)
            {
                result.Result = -1;
                result.Out = ex.ToString();
            }

            return result;
        }

        public static string[] SplitIntoChunks(string input, int chunkSize)
        {
            int stringLength = input.Length;

            // Calculate the number of chunks we will need.
            int chunkCount = stringLength / chunkSize;
            if (stringLength % chunkSize > 0)
            {
                chunkCount++;
            }

            // Initialize the array to hold the chunks.
            string[] chunks = new string[chunkCount];

            // Split the input string into the array.
            for (int i = 0; i < chunkCount; i++)
            {
                int startIndex = i * chunkSize;
                int length = Math.Min(chunkSize, stringLength - startIndex);
                chunks[i] = input.Substring(startIndex, length);
            }

            return chunks;
        }

        public static string GenerateB64(string sourceFile)
        {
            byte[] bytes = File.ReadAllBytes(sourceFile);
            string base64 = Convert.ToBase64String(bytes);
            return base64;

        }
    }


}
