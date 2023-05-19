using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commander.Commands
{
    public static class CommandCategory
    {
        public static string Others { get; set; } = "Others";
        public static string Core { get; set; } = "Agent Core";
        public static string Module { get; set; } = "Modules";
        public static string Commander { get; set; } = "Commander";

        public static string Network { get; set; } = "Network";
        public static string Media { get; set; } = "Media";


        public static string Launcher { get; set; } = "Launcher";

        public static List<string> All { get; }  = new List<string> { Commander, Network, Core, Media, Launcher, Module, Others };
    }
}
