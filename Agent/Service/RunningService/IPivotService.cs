using Agent.Communication;
using Agent.Service.Pivoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Service
{
    public interface IPivotService : IRunningService
    {
        List<PivotServer> Pivots { get; }

        bool AddPivot(ConnexionUrl conn, string serverKey);
        bool RemovePivot(ConnexionUrl conn);

        bool HasPivots();
    }
}
