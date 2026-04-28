using System;
using System.Runtime.InteropServices;

#pragma warning disable 649,169

namespace FuseDotNet;

[Flags]
public enum FuseFileInfoOptions : ulong
{
    none = 0x0,
    write_page = 0x1,
    direct_io = 0x2,
    keep_cache = 0x4,
    flush = 0x8,
    nonseekable = 0x10,
    flock_release = 0x20,
    cache_readdir = 0x40,
    noflush = 0x80,
    parallel_direct_writes = 0x100,
}

/// <summary>
/// %Fuse file information on the current operation.
/// </summary>
/// <remarks>
/// This class cannot be instantiated in C#, it is created by the kernel %Fuse driver.
/// This is the same structure as <c>fileInfo</c> (fuse_common.h) in the C version of Fuse.
/// </remarks>
[StructLayout(LayoutKind.Sequential)]
public struct FuseFileInfo
{
    /** Open flags. Available in open() and release() */
    public readonly PosixOpenFlags flags;

    /** Bitfield options area from fuse_file_info */
    private ulong bitfieldOptions;

    /** File handle. May be filled in by filesystem in open(). Available in all other file operations */
    private ulong fh;

    /** Lock owner id. Available in locking operations and flush */
    public ulong lock_owner;

    /** Requested poll events. Available in ->poll. */
    public readonly uint poll_events;

    /** Backing id / reserved future field in newer headers */
    public int backing_id;

    /** Compatibility padding / future tail */
    private ulong compat1;
    private ulong compat2;

    public FuseFileInfoOptions options
    {
        readonly get => (FuseFileInfoOptions)bitfieldOptions;
        set => bitfieldOptions = (ulong)value;
    }

    /// <summary>
    /// Gets or sets context that can be used to carry information between operations.
    /// This is mapped onto the native file handle storage (`fh`).
    /// </summary>
    public object? Context
    {
        readonly get
        {
            if (fh != 0)
            {
                var gch = (GCHandle)(nint)fh;
                if (gch.IsAllocated)
                {
                    return gch.Target;
                }
            }

            return null;
        }

        set
        {
            if (fh != 0)
            {
                ((GCHandle)(nint)fh).Free();
                fh = 0;
            }

            if (value != null)
            {
                fh = (ulong)(nint)GCHandle.Alloc(value);
            }
        }
    }

    public override readonly string ToString()
        => FormatProviders.FuseFormat($"{{Context = {Context}, Options = {options}, Flags = {flags}, FileHandle = 0x{fh:X}}}");
}
