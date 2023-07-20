using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public enum ServiceVerb : byte
    {
        Start = 0x00,
        Stop = 0x01,
        Show = 0x03,
        Kill = 0x04,
    }
}
