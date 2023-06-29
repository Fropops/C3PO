using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public enum CommandId : byte
    {
        CheckIn = 0x00,
        Whoami = 0x01,
    }
}
