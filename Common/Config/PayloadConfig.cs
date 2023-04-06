using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Common.Config
{
    public class PayloadConfig
    {
        public string Source { get; set; }
        public string DefaultOutpath { get; set; }
        public void FromSection(IConfigurationSection section)
        {
            this.Source = section.GetValue<string>("Source");
            this.DefaultOutpath = section.GetValue<string>("DefaultOutpath");
        }
    }
}
