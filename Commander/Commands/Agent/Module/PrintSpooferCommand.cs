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
    public class PrintSpooferCommand : SpawnInjectModuleCommand
    {
        public override string ExeName => "PrintSpoofer.exe";

        public override string Name => "print-spoofer";

        public override string Description => "Inject a Printspoofer executable with parameters (executablke to be launched as admin)";

        public override string ComputeParams(string innerParams)
        {
            return $"-i -c {innerParams}";
        }

    }
}
