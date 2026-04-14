using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace FuseDotNet;

#if NET5_0_OR_GREATER
[SupportedOSPlatform("linux")]
[SupportedOSPlatform("freebsd")]
[SupportedOSPlatform("macos")]
#endif
public readonly struct FuseContext
{
    internal FuseContext(nint fuseHandle, uint uid, uint gid, int pid, nint privateData, uint umask)
    {
        FuseHandle = fuseHandle;
        Uid = uid;
        Gid = gid;
        Pid = pid;
        PrivateData = privateData;
        Umask = umask;
    }

    public nint FuseHandle { get; }
    public uint Uid { get; }
    public uint Gid { get; }
    public int Pid { get; }
    public nint PrivateData { get; }
    public uint Umask { get; }

    public override string ToString() => $"Pid={Pid}, Uid={Uid}, Gid={Gid}, Umask={Umask}";

    [StructLayout(LayoutKind.Sequential)]
    internal struct NativeFuseContext
    {
        public nint fuse;
        public uint uid;
        public uint gid;
        public int pid;
        public nint private_data;
        public uint umask;
    }
}
