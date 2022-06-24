using Commander.Commands.Agent.Execute_Assembly;
using Commander.Commands.Inject;
using Commander.Communication;
using Commander.Executor;
using Commander.Terminal;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commander.Commands.Module
{
    public class WinPEASCommand : ExecuteAssemblyModuleCommand
    {
        public override string ExeName => "winPEASAny-Ofuscated.exe";

        public override string Name => "winpeas";

        public override string Description => "Execute Assembly winPEAS in memory with parameters";


    }
}
