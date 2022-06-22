using Commander.Communication;
using Commander.Executor;
using Commander.Terminal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commander.Commands
{
    public class CommandContext
    {
        public IExecutor Executor { get; set; }

        public ITerminal Terminal { get; set; }

        public ICommModule CommModule { get; set; }
        public string CommandLabel { get; set; }

        public string CommandParameters { get; set; }



    }

    public class CommandContext<T> : CommandContext
    {
        public T Options { get; set; }
    }
}
