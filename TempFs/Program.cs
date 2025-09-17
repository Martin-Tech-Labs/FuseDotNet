using FuseDotNet;
using System;
using System.Runtime.Versioning;

namespace TempFs;

public static class Program
{
#if NET5_0_OR_GREATER
    [SupportedOSPlatform("linux")]
    [SupportedOSPlatform("freebsd")]
#endif
    public static int Main(params string[] args)
    {
        try
        {
            using var operations = new TempFsOperations();

            operations.Mount(args);

            Console.WriteLine($"Fuse exit");

            return 0;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine(ex);
            Console.ResetColor();
            return ex is PosixException pex ? (int)pex.NativeErrorCode : ex.HResult;
        }
    }
}
