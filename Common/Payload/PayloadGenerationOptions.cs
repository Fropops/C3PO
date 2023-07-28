using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;

namespace Common.Payload
{
    public class PayloadGenerationOptions
    {
        public ConnexionUrl Endpoint { get; set; }
        public PayloadType Type { get; set; } = PayloadType.Executable;
        public PayloadArchitecture Architecture { get; set; } = PayloadArchitecture.x64;
        public string ServerKey { get; set; }
        public bool IsDebug { get; set; }
        public bool IsVerbose { get; set; }

        public bool IsInjected { get; set; }

        public int InjectionDelay { get; set; } = 60;
        public string InjectionProcess { get; set; }

        public override string ToString()
        {
            return $"Payload {Type.ToString()} {Architecture.ToString()} {Endpoint.ToString()}";
        }
    }
}
