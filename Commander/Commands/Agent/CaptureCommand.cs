using Commander.Executor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commander.Commands.Agent
{
    public class CaptureCommandoptions
    {
        public string output { get; set; }
    }
    public class CaptureCommand : EndPointCommand<CaptureCommandoptions>
    {
        public override string Category => CommandCategory.Media;
        public override string Description => "Capture current screen(s)";
        public override string Name => "capture";
        public override ExecutorMode AvaliableIn => ExecutorMode.AgentInteraction;
    }
}
