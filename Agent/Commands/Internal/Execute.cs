﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Commands.Internal
{
    public static class ExecuteHelper
    {
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

            return output;
        }
    }
}
