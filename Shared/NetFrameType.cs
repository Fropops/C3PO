﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public enum NetFrameType : byte
    {
        CheckIn = 0x00,
        Task = 0x01,
        TaskResult = 0x02,
    }
}