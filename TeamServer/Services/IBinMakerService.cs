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
        //string ReaNimatorFolder { get; }

        void GenerateB64s(Listener listener);
        string GenerateBins(Listener listener);


        //string GeneratedAgentBinFileName { get; }
        //string GeneratedAgentExeFileName { get; }

        //string GeneratedAgentDllFileName { get; }

        string SourcePath { get; }
    }
}
