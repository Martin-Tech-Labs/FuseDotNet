using System;
using System.Runtime.Versioning;

namespace FuseDotNet;

public class PosixException : Exception
{
    public PosixResult NativeErrorCode { get; }

#if NET5_0_OR_GREATER
    [SupportedOSPlatform("linux")]
    [SupportedOSPlatform("freebsd")]
#endif
    public PosixException(PosixResult errno)
        : base(errno.ToString())
    {
        NativeErrorCode = errno;
    }

#if NET5_0_OR_GREATER
    [SupportedOSPlatform("linux")]
    [SupportedOSPlatform("freebsd")]
#endif
    public PosixException(PosixResult errno, Exception? innerException)
        : base(errno.ToString(), innerException)
    {
        NativeErrorCode = errno;
    }
}
