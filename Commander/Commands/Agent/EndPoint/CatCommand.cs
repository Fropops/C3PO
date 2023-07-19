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
    public class CatCommandOptions
    {
        public string path { get; set; }
    }
    public class CatCommand : EndPointCommand<CatCommandOptions>
    {
        public override string Description => "Display the content of a file.";
        public override string Name => "cat";

        public override CommandId CommandId => CommandId.Cat;

        public override RootCommand Command => new RootCommand(this.Description)
            {
                new Argument<string>("path", "Path of the file to display"),
            };

        protected override ParameterDictionary SpecifyParameters(CommandContext<CatCommandOptions> context)
        {
           
            var parameters = new ParameterDictionary();
            parameters.Add(ParameterId.Path, context.Options.path.BinarySerializeAsync().Result);
            return parameters;
        }
    }
}
