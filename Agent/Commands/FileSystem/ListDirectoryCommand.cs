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
        public override void InnerExecute(AgentTask task, Models.Agent agent, AgentTaskResult result, MessageManager commm)
        {
            var results = new SharpSploitResultList<ListDirectoryResult>();

            var list = new List<ListDirectoryResult>();
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
                list.Add(new ListDirectoryResult()
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
                list.Add(new ListDirectoryResult()
                {
                    Name = Path.GetFileName(fileInfo.FullName),
                    Length = fileInfo.Length,
                    IsFile= true
                });
            }


            results.AddRange(list.OrderBy(f => f.IsFile).ThenBy(f => f.Name));
            result.Result = results.ToString();
        }

        public sealed class ListDirectoryResult : SharpSploitResult
        {
            public long Length { get; set; }
            public string Name { get; set; }
            public bool IsFile { get; set; }

            public string Type
            {
                get
                {
                    return IsFile ? "File" : "Folder";
                }
            }

            protected internal override IList<SharpSploitResultProperty> ResultProperties => new List<SharpSploitResultProperty>()
            {
                new SharpSploitResultProperty { Name = nameof(Type), Value = Type },
                new SharpSploitResultProperty { Name = nameof(Name), Value = Name },
                new SharpSploitResultProperty { Name = nameof(Length), Value = Length },
            };
        }
    }
}
