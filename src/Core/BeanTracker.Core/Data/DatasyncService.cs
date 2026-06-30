using CommunityToolkit.Datasync.Client.Offline;

namespace BeanTracker.Core.Data;

/// <summary>
/// Wraps Datasync push/pull operations for the MAUI app to call.
/// Push sends local changes to the server; Pull fetches remote changes into the local store.
/// </summary>
public sealed class DatasyncService(BeanTrackerDbContext context)
{
    /// <summary>Pushes all pending local changes to the server.</summary>
    public async Task<PushResult> PushAsync(CancellationToken cancellationToken = default)
        => await context.PushAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

    /// <summary>Pulls latest changes from the server into the local store.</summary>
    public async Task PullAsync(CancellationToken cancellationToken = default)
        => await context.PullAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

    /// <summary>Performs a full synchronization: push first, then pull.</summary>
    public async Task<PushResult> SynchronizeAsync(CancellationToken cancellationToken = default)
    {
        var pushResult = await PushAsync(cancellationToken).ConfigureAwait(false);
        await PullAsync(cancellationToken).ConfigureAwait(false);
        return pushResult;
    }
}
