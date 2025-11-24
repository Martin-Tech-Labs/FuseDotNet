using FuseDotNet.Native;
using LTRData.Extensions.Async;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;

namespace FuseDotNet;

#if NET5_0_OR_GREATER
[SupportedOSPlatform("linux")]
[SupportedOSPlatform("freebsd")]
#endif
public class FuseService(IFuseOperations operations, string[] args) : IDisposable
{
    public event EventHandler? Dismounting;

    public event EventHandler? Stopped;

    public event ThreadExceptionEventHandler? Error;

    public IFuseOperations Operations { get; } = operations;

    public string? MountPoint => _args.LastOrDefault();

    private readonly string[] _args = args;

    public bool Running => !ServiceTask?.IsCompleted ?? false;

    protected Task? ServiceTask { get; private set; }

    protected int? ThreadId { get; private set; }

    public void Start()
    {
#if NET7_0_OR_GREATER
        ObjectDisposedException.ThrowIf(IsDisposed, this);
#else
        if (IsDisposed)
        {
            throw new ObjectDisposedException(GetType().Name);
        }
#endif

        ServiceTask = Task.Factory.StartNew(
            ServiceThreadProcedure,
            CancellationToken.None,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default);
    }

    private void ServiceThreadProcedure()
    {
        try
        {
            ThreadId = Environment.CurrentManagedThreadId;

            Operations.Mount(_args);

            OnDismounted(EventArgs.Empty);
        }
        catch (Exception ex)
        {
            OnError(new(ex));
        }
        finally
        {
            (Operations as IDisposable)?.Dispose();
        }
    }

    public void WaitForExit()
    {
        if (ServiceTask == null ||
            ThreadId == Environment.CurrentManagedThreadId)
        {
            return;
        }

        ServiceTask.Wait();
    }

    public bool WaitForExit(TimeSpan timeout)
    {
        if (ServiceTask == null ||
            ThreadId == Environment.CurrentManagedThreadId)
        {
            return true;
        }

        return ServiceTask.Wait(timeout);
    }

    public ValueTask WaitForExitAsync()
    {
        if (ServiceTask == null ||
            ThreadId == Environment.CurrentManagedThreadId)
        {
            return default;
        }

        return new(ServiceTask);
    }

    public async ValueTask<bool> WaitForExitAsync(TimeSpan timeout)
    {
        if (ServiceTask == null ||
            ThreadId == Environment.CurrentManagedThreadId)
        {
            return true;
        }

        return ServiceTask == await Task.WhenAny(ServiceTask, Task.Delay(timeout)).ConfigureAwait(false);
    }

    protected virtual void OnError(ThreadExceptionEventArgs e) => Error?.Invoke(this, e);

    protected virtual void OnDismounted(EventArgs e) => Stopped?.Invoke(this, e);

    #region IDisposable Support
    public bool IsDisposed => is_disposed != 0;

    int is_disposed;

    protected virtual void Dispose(bool disposing)
    {
        if (Interlocked.Exchange(ref is_disposed, 1) == 0)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects).
                if (ServiceTask != null && !ServiceTask.IsCompleted &&
                    MountPoint != null && !string.IsNullOrWhiteSpace(MountPoint))
                {
                    Trace.WriteLine($"Requesting dismount for Fuse file system '{MountPoint}'");

                    OnDismounting(EventArgs.Empty);

                    if (new DriveInfo(MountPoint).DriveFormat.StartsWith("fuse", StringComparison.Ordinal)
                        && !Fuse.TryUnmount(MountPoint, out var umountResult))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(@$"
Unmount failed for '{MountPoint}': {umountResult}");
                        Console.ResetColor();
                    }

                    if (ThreadId != Environment.CurrentManagedThreadId)
                    {
                        Trace.WriteLine($"Waiting for Fuse file system '{MountPoint}' service thread to stop");

                        ServiceTask.Wait();

                        Trace.WriteLine($"Fuse file system '{MountPoint}' service thread stopped.");
                    }
                }

                (Operations as IDisposable)?.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.

            // TODO: set large fields to null.
            ServiceTask = null;
        }
    }

    protected virtual void OnDismounting(EventArgs e) => Dismounting?.Invoke(this, e);

    // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
    ~FuseService()
    {
        // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        Dispose(false);
    }

    // This code added to correctly implement the disposable pattern.
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        Dispose(true);
        // TODO: uncomment the following line if the finalizer is overridden above.
        GC.SuppressFinalize(this);
    }
    #endregion
}
