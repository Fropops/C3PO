using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public enum ParameterId : byte
    {
        Cmd = 0x00,
        File = 0x01,
        Path = 0x02,
        Payload = 0x03,
        Verb = 0x04,
        Id = 0x05,
    }
}
