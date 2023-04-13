﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Common.Config;
using ApiModels;

namespace Commander
{

    public class ApiConfig
    {
        public string Address { get; set; }
        public int Port { get; set; }
        public string User { get; set; }
        public string ApiKey { get; set; }

        public int Delay { get; set; } = 500;
        public string EndPoint => this.Address + ":" + this.Port;

        public void FromSection(IConfigurationSection section)
        {
            this.Address = section.GetValue<string>("Address");
            this.Port = section.GetValue<int>("Port");
            this.User = section.GetValue<string>("User");
            this.ApiKey = section.GetValue<string>("ApiKey");
        }
    }

    
    public class CommanderConfig
    {
        public ApiConfig ApiConfig { get; private set; }
        public PayloadConfig PayloadConfig { get; private set; }

        public ServerConfig ServerConfig { get; set; }
        public string Session { get; private set; }

        public CommanderConfig()
        {
            this.ApiConfig = new ApiConfig();
            this.PayloadConfig = new PayloadConfig();
            this.Session = Guid.NewGuid().ToString();
        }

        public CommanderConfig(IConfiguration config) : this()
        {
            this.ApiConfig.FromSection(config.GetSection("Api"));
            this.PayloadConfig.FromSection(config.GetSection("Payload"));
        }


    }
}
