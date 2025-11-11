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
        public string IncRustFolder { get; set; }
        public string DonutFolder { get; set; }
        public string ReanimatorPath { get; set; }

        public void FromSection(IConfigurationSection section, bool verbose = false)
        {
            this.SourceFolder = PathHelper.GetAbsolutePath(section.GetValue<string>("SourceFolder"));
            this.OutputFolder = PathHelper.GetAbsolutePath(section.GetValue<string>("OutputFolder", "/tmp"));
            this.WorkingFolder = PathHelper.GetAbsolutePath(section.GetValue<string>("WorkingFolder", "/tmp"));
            this.NimPath = PathHelper.GetAbsolutePath(section.GetValue<string>("NimPath", "/usr/bin/nim"));
            this.DonutFolder = PathHelper.GetAbsolutePath(section.GetValue<string>("DonutFolder", "/opt/donut"));
            this.ReanimatorPath = PathHelper.GetAbsolutePath(section.GetValue<string>("ReanimatorPath", "/mnt/Share/Projects/reaNimator-modif/reaNimator"));
            this.IncRustFolder = PathHelper.GetAbsolutePath(section.GetValue<string>("IncRustFolder", "/mnt/Share/Projects/Rust/incrust"));

            if (verbose)
            {
                Console.WriteLine("[CONFIG][PAYLOAD][SourceFolder] : " + this.SourceFolder);
                Console.WriteLine("[CONFIG][PAYLOAD][OutputFolder] : " + this.OutputFolder);
                Console.WriteLine("[CONFIG][PAYLOAD][WorkingFolder] : " + this.WorkingFolder);
                Console.WriteLine("[CONFIG][PAYLOAD][DonutPath] : " + this.DonutFolder);
                Console.WriteLine("[CONFIG][PAYLOAD][IncRustPath] : " + this.IncRustFolder);
            }

            if(!Directory.Exists(this.OutputFolder)) { Directory.CreateDirectory(this.OutputFolder); }
            if (!Directory.Exists(this.WorkingFolder)) { Directory.CreateDirectory(this.WorkingFolder); }
        }

       
    }
}
