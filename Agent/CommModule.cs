﻿using Agent.Models;
using Agent.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent
{
    public abstract class CommModule
    {
        public bool IsInitialized { get; protected set; } = false;

        public bool IsRunning { get; protected set; } = false;
        public int Interval { get; set; } = 2000;
        public double Jitter { get; set; } = 0.5;

        private Random random = new Random();

        public MessageService MessageService { get; protected set; }
        public FileService FileService { get; protected set; }

        public CommModule(MessageService messageManager, FileService fileService)
        {
            this.MessageService = messageManager;
            this.FileService = fileService;
        }

        protected int GetDelay()
        {
            var delta = (int)(Interval * Jitter);
            delta = random.Next(0, delta);
            if (random.Next(100) > 50)
                delta = -delta;
            return this.Interval + delta;
        }

        public abstract void Start();

        public abstract void Stop();
    }
}
