using Agent.Communication;
using Agent.Service.Pivoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Service
{
    public interface IWebHostService : IRunningService
    {
        Dictionary<string, byte[]> Files { get; }

        void Start(ConnexionUrl conn);
    }
    
}
