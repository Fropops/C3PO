﻿using System;
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

        public static string Services { get; set; } = "Agent Services";
        public static string Module { get; set; } = "Modules";
        public static string Commander { get; set; } = "Commander";

        public static string Network { get; set; } = "Network";
        public static string Media { get; set; } = "Media";


        public static string Listeners { get; set; } = "Listeners";

        
        public static List<string> All { get; }  = new List<string> { Commander, Network, Core, Services, Media, Listeners, Module, Others };
    }
}
