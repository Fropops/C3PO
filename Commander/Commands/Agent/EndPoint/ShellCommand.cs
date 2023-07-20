using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Commander.Executor;
using Shared;
using System.Security.Cryptography;
using BinarySerializer;
using System.CommandLine;

namespace Commander.Commands.Agent.EndPoint
{
    public class ShellCommand : SimpleEndPointCommand
    {
        public override string Name => "shell";
        public override string Description => "Send a command to be executed by the agent";
        public override CommandId CommandId => CommandId.Shell;

        protected override ParameterDictionary SpecifyParameters(CommandContext context)
        {
            ParameterDictionary parameters = new ParameterDictionary();
            parameters.Add(ParameterId.Cmd, context.CommandParameters.BinarySerializeAsync().Result);
            return parameters;
        }
    }

}
