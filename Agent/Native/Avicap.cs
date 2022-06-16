using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Native
{
    public class Avicap
    {
        [DllImport("avicap32.dll")]
        public static extern bool capGetDriverDescriptionA(short wDriverIndex,
       [MarshalAs(UnmanagedType.VBByRefStr)] ref String lpszName,
      int cbName, [MarshalAs(UnmanagedType.VBByRefStr)] ref String lpszVer, int cbVer);

        //This function enables create a  window child with so that you can display it in a picturebox for example
        [DllImport("avicap32.dll")]
        public static extern int capCreateCaptureWindowA([MarshalAs(UnmanagedType.VBByRefStr)] ref string lpszWindowName,
            int dwStyle, int x, int y, int nWidth, int nHeight, int hWndParent, int nID);

    }
}
