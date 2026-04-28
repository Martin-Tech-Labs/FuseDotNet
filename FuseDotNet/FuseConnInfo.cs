using System;
using System.Runtime.InteropServices;

#pragma warning disable 649,169

namespace FuseDotNet;

/**
 * Connection information, passed to the ->init() method
 *
 * Some of the elements are read-write, these can be changed to
 * indicate the value requested by the filesystem.  The requested
 * value must usually be smaller than the indicated value.
 */
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct FuseConnInfo
{
    /**
     * Major version of the protocol (read-only)
     */
    public readonly uint proto_major;

    /**
     * Minor version of the protocol (read-only)
     */
    public readonly uint proto_minor;

    /**
     * Maximum readahead
     */
    public uint max_readahead;

    /**
     * Capability flags, that the kernel supports
     */
    public readonly FuseCapabilities capable;

    /**
     * Capability flags, that the filesystem wants to enable
     */
    public FuseCapabilities want;

    /**
     * Maximum size of the write buffer
     */
    public uint max_write;

    /**
     * Maximum number of backgrounded requests
     */
    public ushort max_background;

    /**
     * Kernel congestion threshold parameter
     */
    public ushort congestion_threshold;

    /**
     * Maximum size of pages requested
     */
    public uint max_pages;

    /**
     * Map alignment requirement
     */
    public uint map_alignment;

    /**
	 * When FUSE_CAP_WRITEBACK_CACHE is enabled, the kernel is responsible
	 * for updating mtime and ctime when write requests are received. The
	 * updated values are passed to the filesystem with setattr() requests.
	 * However, if the filesystem does not support the full resolution of
	 * the kernel timestamps (nanoseconds), the mtime and ctime values used
	 * by kernel and filesystem will differ (and result in an apparent
	 * change of times after a cache flush).
	 *
	 * To prevent this problem, this variable can be used to inform the
	 * kernel about the timestamp granularity supported by the file-system.
	 * The value should be power of 10.  The default is 1, i.e. full
	 * nano-second resolution. Filesystems supporting only second resolution
	 * should set this to 1000000000.
	 */
    public uint time_gran;

    /**
     * Maximum number of stacked backing filesystems supported.
     */
    public ushort max_backing_stack_depth;

    /**
     * Padding/flags area in current headers.
     */
    public ushort padding;

    /**
     * Extended capability flags, supported by kernel.
     */
    public ulong capable_ext;

    /**
     * Extended capability flags, requested by filesystem.
     */
    public ulong want_ext;

    /**
     * Darwin-specific capabilities supported by kernel.
     */
    public ulong capable_darwin;

    /**
     * Darwin-specific capabilities requested by filesystem.
     */
    public ulong want_darwin;

    /**
     * For future use.
     */
    public fixed uint reserved[10];
}

[Flags]
public enum FuseCapabilities : uint
{
    AsyncRead = 1 << 0,
    PosixLocks = 1 << 1,
    AtomicOTrunc = 1 << 3,
    ExportSupport = 1 << 4,
    DontMask = 1 << 6,
    SpliceWrite = 1 << 7,
    SpliceMove = 1 << 8,
    SpliceRead = 1 << 9,
    FLockLocks = 1 << 10,
    IoCtlDir = 1 << 11,
    AutoInvalData = 1 << 12,
    ReadDirPlus = 1 << 13,
    ReadDirPlusAuto = 1 << 14,
    AsyncDirectIo = 1 << 15,
    WritebackCache = 1 << 16,
    NoOpenSupport = 1 << 17,
    ParallelDirOps = 1 << 18,
    PosixAcls = 1 << 19,
    HandleKillPriv = 1 << 20,
    CacheSymlinks = 1 << 23,
    NoOpenDirSupport = 1 << 24,
    ExplicitInvalData = 1 << 25,
}
