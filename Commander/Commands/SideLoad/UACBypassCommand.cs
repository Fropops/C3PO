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
    public class UACBypassCommand : SiteLoadCommand
    {
        public override string Description => "Site load a dll bypassing UAC and running parm as high integrity level";
        public override string Name => "uac-bypass";
        public override ExecutorMode AvaliableIn => ExecutorMode.AgentInteraction;

        public override string ExeName => "UACByPass.exe";

    }
}
