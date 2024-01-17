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
        public string SourceFolder { get; set; }
        public string OutputFolder { get; set; }
        public string WorkingFolder { get; set; }
        public string NimPath { get; set; }
        public string IncRustPath { get; set; }
        public string DonutPath { get; set; }
        public string ReanimatorPath { get; set; }

        public void FromSection(IConfigurationSection section)
        {
            this.SourceFolder = section.GetValue<string>("SourceFolder");
            this.OutputFolder = section.GetValue<string>("OutputFolder", "/tmp");
            this.WorkingFolder = section.GetValue<string>("WorkingFolder", "/tmp");
            this.NimPath = section.GetValue<string>("NimPath", "/usr/bin/nim");
            this.DonutPath = section.GetValue<string>("DonutPath", "/opt/donut/donut");
            this.ReanimatorPath = section.GetValue<string>("ReanimatorPath", "/mnt/Share/Projects/reaNimator-modif/reaNimator");
            this.IncRustPath = section.GetValue<string>("IncRustPath", "/mnt/Share/Projects/Rust/incrust/");
        }
    }
}
