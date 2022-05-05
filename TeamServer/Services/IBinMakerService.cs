using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeamServer.Models;

namespace TeamServer.Services
{
    public interface IBinMakerService
    {
        string GenerateStagersFor(Listener listener);
    }
}
