using System.Runtime.InteropServices;

namespace FuseDotNet;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
internal struct FuseDarwinAttr
{
    public FuseFileStat attr;
    public double attr_timeout;
    public double entry_timeout;
}
