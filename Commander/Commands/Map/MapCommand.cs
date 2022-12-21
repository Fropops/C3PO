using Commander.Communication;
using Commander.Executor;
using Commander.Terminal;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commander.Commands.Agent
{
    public class MapCommandOptions
    {
    }

    public class MapCommand : EnhancedCommand<MapCommandOptions>
    {
        public override string Category => CommandCategory.Commander;
        public override string Description => "Show current agents graph";
        public override string Name => "map";

        public override ExecutorMode AvaliableIn => ExecutorMode.All;

        public override RootCommand Command => new RootCommand(this.Description);

        protected async override Task<bool> HandleCommand(CommandContext<MapCommandOptions> context)
        {

            var tree = this.GenerateTree(context);
            this.RenderTree(tree, context);


            return true;
        }

        private void RenderTree(MapTreeNode root, CommandContext<MapCommandOptions> context)
        {
            this.RenderNode(root, context);

        }

        private void RenderNode(MapTreeNode node, CommandContext<MapCommandOptions> context, int indent = 0)
        {
            int indentOffset = (indent == 0 ? 0 : node.LinkToParent.Length + 5);

            context.Terminal.Write(" ", indent);
            if (indent != 0)
            {
                context.Terminal.Write("|-");
                context.Terminal.Write(node.LinkToParent);
                context.Terminal.Write("-> ");
            }
            if (node.IsAlive)
                context.Terminal.SetForeGroundColor(ConsoleColor.Green);
            else
                context.Terminal.SetForeGroundColor(ConsoleColor.Red);
            context.Terminal.WriteLine(node.Name);
            if (!string.IsNullOrEmpty(node.ShortId))
            {
                context.Terminal.Write(" ", indent + indentOffset + (node.Name.Length - node.ShortId.Length) / 2);
                context.Terminal.WriteLine(node.ShortId);
            }


            var maxLength = Math.Max(node.Name.Length, node.ShortId.Length);

            indent += indentOffset + (maxLength / 2);

            context.Terminal.SetForeGroundColor(context.Terminal.DefaultColor);

            foreach (var child in node.Children)
                RenderNode(child, context, indent);
        }

        private MapTreeNode GenerateTree(CommandContext<MapCommandOptions> context)
        {
            var allAgents = context.CommModule.GetAgents();
            var allListeners = context.CommModule.GetListeners();

            MapTreeNode root = new MapTreeNode() { Name = "TeamServer", IsAlive = context.CommModule.ConnectionStatus == ConnectionStatus.Connected, ShortId = string.Empty };
            Dictionary<string, MapTreeNode> allNodes = new Dictionary<string, MapTreeNode>();
            foreach (var agent in allAgents)
            {
                var node = new MapTreeNode();
                node.Name = agent.Metadata.Desc;
                node.Id = agent.Metadata.Id;
                node.ShortId = agent.Metadata.Id;
                node.IsAlive = agent.LastSeen.AddSeconds(30) > DateTime.UtcNow;
                allNodes.Add(agent.Metadata.Id, node);
            }

            foreach (var agent in allAgents)
            {
                var node = allNodes[agent.Metadata.Id];
                if (agent.Path.Count == 1)
                {
                    root.Children.Add(node);
                    if (!string.IsNullOrEmpty(agent.ListenerId))
                    {
                        var listener = allListeners.FirstOrDefault(l => l.Id == agent.ListenerId);
                        if (listener != null)
                            node.LinkToParent = listener.Name;
                    }
                }
                else
                {
                    var parId = agent.Path[agent.Path.Count - 2];
                    node.LinkToParent = "Pipe";
                    allNodes[parId].Children.Add(node);
                }
            }

            return root;
        }
    }


    //public enum MapLinkType
    //{
    //    Unknown,
    //    Http,
    //    Pipe
    //}

    public class MapTreeNode
    {
        public string Id { get; set; }

        public string ShortId { get; set; }
        public string Name { get; set; }
        public List<MapTreeNode> Children { get; set; } = new List<MapTreeNode>();

        public string LinkToParent { get; set; } = "Unknown";

        public bool IsAlive { get; set; }
    }


}
