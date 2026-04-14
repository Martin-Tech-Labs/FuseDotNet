using System.Runtime.InteropServices;
using System.Runtime.Versioning;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable CA2101 // Specify marshaling for P/Invoke string arguments
#pragma warning disable SYSLIB1054 // Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time

namespace FuseDotNet.Native;

/// <summary>
/// Native API to the kernel Fuse driver.
/// </summary>
#if NET5_0_OR_GREATER
[SupportedOSPlatform("linux")]
[SupportedOSPlatform("freebsd")]
[SupportedOSPlatform("macos")]
#endif
internal static class NativeMethods
{
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP
    public const UnmanagedType UnmanagedStringType = UnmanagedType.LPUTF8Str;
#else
    public const UnmanagedType UnmanagedStringType = UnmanagedType.LPStr;
#endif

    private const string LIB_FUSE = "fuse3";
    private const string LIB_FUSE_MACOS = "/usr/local/lib/libfuse3.dylib";

    private const string LIB_C = "libc";

    /// <summary>
    /// Mount a new Fuse Volume.
    /// This function block until the device is unmounted.
    /// If the mount fails, it will directly return an error.
    /// </summary>
    /// <param name="argc"></param>
    /// <param name="argv">Array of pointers to UTF8 encoded arguments that describe the mount.</param>
    /// <param name="operations">Instance of <see cref="FuseOperations"/> that will be called for each request made by the kernel.</param>
    /// <param name="operationsSize"></param>
    /// <param name="userData"></param>
    /// <returns><see cref="PosixResult"/></returns>
    [DllImport(LIB_FUSE, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fuse_main_real", ExactSpelling = true)]
    private static extern PosixResult fuse_main_real_fuse3(int argc,
        [In, MarshalAs(UnmanagedType.LPArray)] nint[] argv,
        [In] FuseOperations? operations, nint operationsSize, nint userData);

    [DllImport(LIB_FUSE_MACOS, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fuse_main_real", ExactSpelling = true)]
    private static extern PosixResult fuse_main_real_macos(int argc,
        [In, MarshalAs(UnmanagedType.LPArray)] nint[] argv,
        [In] FuseOperations? operations, nint operationsSize, nint userData);

    internal static PosixResult fuse_main_real(int argc,
        [In, MarshalAs(UnmanagedType.LPArray)] nint[] argv,
        [In] FuseOperations? operations, nint operationsSize, nint userData)
    {
#if NET5_0_OR_GREATER
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return fuse_main_real_macos(argc, argv, operations, operationsSize, userData);
        }
#endif
        return fuse_main_real_fuse3(argc, argv, operations, operationsSize, userData);
    }

    [DllImport(LIB_C, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = true)]
    internal static extern int unmount([In, MarshalAs(UnmanagedStringType)] string dir, int flags);

    [DllImport(LIB_C, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = true)]
    internal static extern int umount2([In, MarshalAs(UnmanagedStringType)] string dir, int flags);

    [DllImport(LIB_C, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = true)]
    internal static extern int umount([In, MarshalAs(UnmanagedStringType)] string dir);

    [DllImport(LIB_C, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = true)]
    internal static extern nint strlen(nint ptr);

    [DllImport(LIB_C, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = true)]
    internal static extern TimeSpec time(out TimeSpec timespec);

    [DllImport(LIB_C, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = true)]
    internal static extern nint strerror(int errno);

    [DllImport(LIB_FUSE, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fuse_get_context", ExactSpelling = true)]
    private static extern nint fuse_get_context_fuse3();

    [DllImport(LIB_FUSE_MACOS, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fuse_get_context", ExactSpelling = true)]
    private static extern nint fuse_get_context_macos();

    internal static nint fuse_get_context()
    {
#if NET5_0_OR_GREATER
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return fuse_get_context_macos();
        }
#endif
        return fuse_get_context_fuse3();
    }
}
