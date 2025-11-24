#define CONSOLE_LOGGER

using FuseDotNet.Logging;
using FuseDotNet.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;

#pragma warning disable IDE0079 // Remove unnecessary suppression

namespace FuseDotNet;

/// <summary>
/// Helper and extension methods to %Fuse.
/// </summary>
#if NET5_0_OR_GREATER
[SupportedOSPlatform("linux")]
[SupportedOSPlatform("freebsd")]
#endif
public static class Fuse
{
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP
    public static nint StringToCoTaskMemUTF8(string? arg)
        => Marshal.StringToCoTaskMemUTF8(arg);
#else
    public static unsafe nint StringToCoTaskMemUTF8(string? arg)
    {
        if (arg == null)
        {
            return 0;
        }

        var utf8bufferLength = Encoding.UTF8.GetByteCount(arg) + 1;
        var ptr = Marshal.AllocCoTaskMem(utf8bufferLength);
        var bytes = new Span<byte>((byte*)ptr, utf8bufferLength);

        fixed (char* argptr = arg)
        {
            var utf8ByteCount = Encoding.UTF8.GetBytes(argptr, arg.Length, (byte*)ptr, utf8bufferLength);
            bytes[utf8ByteCount] = 0;
        }

        return ptr;
    }
#endif

    public static void FreeCoTaskMemUTF8(nint ptr)
    {
        if (ptr != 0)
        {
            Marshal.FreeCoTaskMem(ptr);
        }
    }

    /// <summary>
    /// Call %Fuse without file system operations. Useful for example to list available command line
    /// switches, version etc.
    /// </summary>
    /// <param name="args">Command line arguments to pass to fuse_main().</param>
    /// <returns><see cref="PosixResult"/> from fuse_main()</returns>
    public static PosixResult CallMain(IEnumerable<string> args)
    {
        var utf8args = args.Select(StringToCoTaskMemUTF8).ToArray();

        var status = NativeMethods.fuse_main_real(utf8args.Length, utf8args, null, 0, 0);

        Array.ForEach(utf8args, FreeCoTaskMemUTF8);

        return status;
    }

    /// <summary>
    /// Mount a new %Fuse Volume.
    /// This function block until the device is unmounted.
    /// </summary>
    /// <param name="operations">Instance of <see cref="IFuseOperations"/> that will be called for each request made by the kernel.</param>
    /// <param name="args">Command line arguments to pass to fuse_main().</param>
    /// <param name="logger"><see cref="ILogger"/> that will log all FuseDotNet debug information.</param>
    /// <exception cref="PosixException">If the mount fails.</exception>
    public static void Mount(this IFuseOperations operations, IEnumerable<string> args, ILogger? logger = null)
    {
        logger ??= new NullLogger();

        var fuseOperationProxy = new FuseOperationProxy(operations, logger);

        var fuseOperations = new FuseOperations
        {
            getattr = fuseOperationProxy.getattr,
            readlink = fuseOperationProxy.readlink,
            mknod = null, // fuseOperationProxy.mknod,
            mkdir = fuseOperationProxy.mkdir,
            unlink = fuseOperationProxy.unlink,
            rmdir = fuseOperationProxy.rmdir,
            symlink = fuseOperationProxy.symlink,
            rename = fuseOperationProxy.rename,
            link = fuseOperationProxy.link,
            chmod = fuseOperationProxy.chmod,
            chown = fuseOperationProxy.chown,
            truncate = fuseOperationProxy.truncate,
            open = fuseOperationProxy.open,
            read = fuseOperationProxy.read,
            write = fuseOperationProxy.write,
            statfs = fuseOperationProxy.statfs,
            flush = fuseOperationProxy.flush,
            release = fuseOperationProxy.release,
            fsync = fuseOperationProxy.fsync,
            setxattr = null, // fuseOperationProxy.setxattr,
            getxattr = null, // fuseOperationProxy.getxattr,
            listxattr = null, // fuseOperationProxy.listxattr,
            removexattr = null, // fuseOperationProxy.removexattr,
            opendir = fuseOperationProxy.opendir,
            readdir = fuseOperationProxy.readdir,
            releasedir = fuseOperationProxy.releasedir,
            fsyncdir = fuseOperationProxy.fsyncdir,
            init = fuseOperationProxy.init,
            destroy = fuseOperationProxy.destroy,
            access = fuseOperationProxy.access,
            create = fuseOperationProxy.create,
            @lock = null, // fuseOperationProxy.@lock,
            utimens = fuseOperationProxy.utimens,
            bmap = null, // fuseOperationProxy.bmap,
            ioctl = fuseOperationProxy.ioctl,
            poll = null, // fuseOperationProxy.poll,
            write_buf = null, // fuseOperationProxy.write_buf,
            read_buf = null, // fuseOperationProxy.read_buf,
            flock = null, // fuseOperationProxy.flock,
            fallocate = fuseOperationProxy.fallocate,
            copy_file_range = null, // fuseOperationProxy.copy_file_range
            lseek = null, // fuseOperationProxy.lseek
        };

        var utf8args = args.Select(StringToCoTaskMemUTF8).ToArray();

        var status = NativeMethods.fuse_main_real(utf8args.Length, utf8args, fuseOperations, Marshal.SizeOf(fuseOperations), 0);

        Array.ForEach(utf8args, FreeCoTaskMemUTF8);

        GC.KeepAlive(fuseOperations);

        if (status != PosixResult.Success)
        {
            throw new PosixException(status);
        }
    }

    public static void Unmount(string dir)
    {
        if (!TryUnmount(dir, out var result))
        {
            throw new PosixException(result);
        }
    }

    public static bool TryUnmount(string dir, out PosixResult result)
    {
        int rc;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            rc = NativeMethods.umount(dir);
        }
#if NETCOREAPP
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
        {
            rc = NativeMethods.unmount(dir, 0);
        }
#endif
        else
        {
            throw new PlatformNotSupportedException();
        }

        if (rc == 0)
        {
            result = PosixResult.Success;
            return true;
        }

        result = Marshal.GetLastWin32Error();
        return false;
    }
}
