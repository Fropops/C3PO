using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeamServer.Models;

namespace TeamServer.Services
{
    public interface IBinMakerService
    {
        //string GenerateStagersFor(Listener listener);

        string DonutFolder { get; }
        string ReaNimatorFolder { get; }

        void GenerateB64(Listener listener);
        string GenerateBin(Listener listener);


        string GeneratedAgentBinFileName { get; }
        string GeneratedAgentExeFileName { get; }

        string GeneratedAgentDllFileName { get; }

        string SourceAgentExePath { get; }
    }
}
