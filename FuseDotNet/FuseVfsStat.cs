using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

#pragma warning disable IDE1006 // Naming Styles

namespace FuseDotNet;

#if NET5_0_OR_GREATER
[SupportedOSPlatform("linux")]
[SupportedOSPlatform("freebsd")]
[SupportedOSPlatform("macos")]
#endif
public struct FuseVfsStat
{
    unsafe static FuseVfsStat()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && RuntimeInformation.OSArchitecture == Architecture.X64)
        {
            pMarshalToNative = (nint pNative, in FuseVfsStat stat) => ((LinuxX64StatVfs*)pNative)->Initialize(stat);
            NativeStructSize = sizeof(LinuxX64StatVfs);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && RuntimeInformation.OSArchitecture == Architecture.Arm64)
        {
            pMarshalToNative = (nint pNative, in FuseVfsStat stat) => ((LinuxX64StatVfs*)pNative)->Initialize(stat);
            NativeStructSize = sizeof(LinuxX64StatVfs);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && RuntimeInformation.OSArchitecture == Architecture.X86)
        {
            pMarshalToNative = (nint pNative, in FuseVfsStat stat) => ((LinuxX86StatVfs*)pNative)->Initialize(stat);
            NativeStructSize = sizeof(LinuxX86StatVfs);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && RuntimeInformation.OSArchitecture == Architecture.Arm)
        {
            pMarshalToNative = (nint pNative, in FuseVfsStat stat) => ((LinuxX86StatVfs*)pNative)->Initialize(stat);
            NativeStructSize = sizeof(LinuxX86StatVfs);
        }
#if NETCOREAPP
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
        {
            pMarshalToNative = (nint pNative, in FuseVfsStat stat) => ((FreeBSDStatVfs*)pNative)->Initialize(stat);
            NativeStructSize = sizeof(FreeBSDStatVfs);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            pMarshalToNative = (nint pNative, in FuseVfsStat stat) => ((MacOSStatFs*)pNative)->Initialize(stat);
            NativeStructSize = sizeof(MacOSStatFs);
        }
#endif
        else
        {
            throw new PlatformNotSupportedException($"Current platform {RuntimeInformation.OSDescription} {RuntimeInformation.OSArchitecture} not supported by FuseDotNet library");
        }
    }

    private unsafe delegate void fMarshalToNative(nint pNative, in FuseVfsStat stat);

    private static readonly fMarshalToNative pMarshalToNative;

    public static int NativeStructSize { get; }

    public readonly unsafe void MarshalToNative(nint pNative) => pMarshalToNative(pNative, this);

    public ulong f_bsize { get; set; }
    public ulong f_frsize { get; set; }
    public ulong f_blocks { get; set; }
    public ulong f_bfree { get; set; }
    public ulong f_bavail { get; set; }
    public ulong f_files { get; set; }
    public ulong f_ffree { get; set; }
    public ulong f_favail { get; set; }
    public ulong f_fsid { get; set; }
    public ulong f_flag { get; set; }
    public ulong f_namemax { get; set; }
}

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public struct LinuxX64StatVfs
{
    public void Initialize(in FuseVfsStat stat)
    {
        f_bsize = stat.f_bsize;
        f_frsize = stat.f_frsize;
        f_blocks = stat.f_blocks;
        f_bfree = stat.f_bfree;
        f_bavail = stat.f_bavail;
        f_files = stat.f_files;
        f_ffree = stat.f_ffree;
        f_favail = stat.f_favail;
        f_fsid = stat.f_fsid;
        f_flag = stat.f_flag;
        f_namemax = stat.f_namemax;
    }

    public ulong f_bsize;
    public ulong f_frsize;
    public ulong f_blocks;
    public ulong f_bfree;
    public ulong f_bavail;
    public ulong f_files;
    public ulong f_ffree;
    public ulong f_favail;
    public ulong f_fsid;
    public ulong f_flag;
    public ulong f_namemax;
    public unsafe fixed int __f_spare[6];
}

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct LinuxX86StatVfs
{
    public void Initialize(in FuseVfsStat stat)
    {
        checked
        {
            f_bsize = (uint)stat.f_bsize;
            f_frsize = (uint)stat.f_frsize;
            f_blocks = stat.f_blocks;
            f_bfree = stat.f_bfree;
            f_bavail = stat.f_bavail;
            f_files = stat.f_files;
            f_ffree = stat.f_ffree;
            f_favail = stat.f_favail;
            f_fsid = stat.f_fsid;
            f_flag = (uint)stat.f_flag;
            f_namemax = (uint)stat.f_namemax;
        }
    }

    public uint f_bsize;
    public uint f_frsize;
    public ulong f_blocks;
    public ulong f_bfree;
    public ulong f_bavail;
    public ulong f_files;
    public ulong f_ffree;
    public ulong f_favail;
    public ulong f_fsid;
    public uint f_flag;
    public uint f_namemax;
    public unsafe fixed int __f_spare[6];
}

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public struct FreeBSDStatVfs
{
    public void Initialize(in FuseVfsStat stat)
    {
        checked
        {
            f_bsize = stat.f_bsize;
            f_iosize = stat.f_frsize == 0 ? stat.f_bsize : stat.f_frsize;
            f_blocks = stat.f_blocks;
            f_bfree = stat.f_bfree;
            f_bavail = (long)stat.f_bavail;
            f_files = stat.f_files;
            f_ffree = (long)stat.f_ffree;
            f_fsid = stat.f_fsid;
            f_flags = stat.f_flag;
            f_namemax = (uint)stat.f_namemax;
        }
    }

    public ulong f_version;
    public ulong f_type;
    public ulong f_flags;
    public ulong f_bsize;
    public ulong f_iosize;
    public ulong f_blocks;
    public ulong f_bfree;
    public long f_bavail;
    public ulong f_files;
    public long f_ffree;
    public ulong f_syncwrites;
    public ulong f_asyncwrites;
    public ulong f_syncreads;
    public ulong f_asyncreads;
    public ulong f_fsid;
    public uint f_namemax;
    public uint f_owner;
    public uint f_spare0;
    public ulong f_charspare1;
    public unsafe fixed ulong f_spare[2];
    public unsafe fixed byte f_fstypename[16];
    public unsafe fixed byte f_mntfromname[1024];
    public unsafe fixed byte f_mntonname[1024];
}

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct MacOSStatFs
{
    public void Initialize(in FuseVfsStat stat)
    {
        checked
        {
            f_bsize = (uint)stat.f_bsize;
            f_iosize = (int)(stat.f_frsize == 0 ? stat.f_bsize : stat.f_frsize);
            f_blocks = stat.f_blocks;
            f_bfree = stat.f_bfree;
            f_bavail = stat.f_bavail;
            f_files = stat.f_files;
            f_ffree = stat.f_ffree;
            f_fsid = stat.f_fsid;
            f_owner = 0;
            f_type = 0;
            f_flags = (uint)stat.f_flag;
            f_fssubtype = 0;
            f_fstypename = default;
            f_mntonname = default;
            f_mntfromname = default;
            f_reserved = default;
        }
    }

    public uint f_bsize;
    public int f_iosize;
    public ulong f_blocks;
    public ulong f_bfree;
    public ulong f_bavail;
    public ulong f_files;
    public ulong f_ffree;
    public ulong f_fsid;
    public uint f_owner;
    public uint f_type;
    public uint f_flags;
    public uint f_fssubtype;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
    public byte[] f_fstypename;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024)]
    public byte[] f_mntonname;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024)]
    public byte[] f_mntfromname;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    public uint[] f_reserved;
}
