using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commander
{
    public static class Extensions
    {
        public static string ToShortGuid(this String guid)
        {
            if (guid.Length < 10)
                return guid;
            return guid.Replace("-", "").Substring(0, 10);
        }
    }
}
