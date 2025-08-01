// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Core;
using Snap.Hutao.Model.Entity;
using Snap.Hutao.Service.Game.Scheme;
using Snap.Hutao.ViewModel.Game;
using Snap.Hutao.ViewModel.User;

namespace Snap.Hutao.Service.Game.Launching;

[ConstructorGenerated]
internal sealed partial class LaunchExecutionContext : IDisposable
{
    private readonly Lock syncRoot = new();
    private IGameFileSystem? gameFileSystem;
    private bool disposed;

    public LaunchExecutionContext(IServiceProvider serviceProvider, IViewModelSupportLaunchExecution viewModel, UserAndUid? userAndUid)
        : this(serviceProvider)
    {
        ViewModel = new(viewModel);

        LaunchScheme? currentScheme = viewModel.Shared.GetCurrentLaunchSchemeFromConfigFile();
        ArgumentNullException.ThrowIfNull(currentScheme);
        CurrentScheme = currentScheme;
        TargetScheme = viewModel.SelectedScheme ?? currentScheme;

        Account = viewModel.SelectedGameAccount;
        UserAndUid = userAndUid;
    }

    public LaunchExecutionResult Result { get; } = new();

    public partial IServiceProvider ServiceProvider { get; }

    public partial ITaskContext TaskContext { get; }

    public partial ILogger<LaunchExecutionContext> Logger { get; }

    public partial LaunchOptions Options { get; }

    public WeakReference<IViewModelSupportLaunchExecution> ViewModel { get; }

    public LaunchScheme CurrentScheme { get; }

    public LaunchScheme TargetScheme { get; }

    public GameAccount? Account { get; }

    public string? AuthTicket { get; set; }

    public UserAndUid? UserAndUid { get; }

    public bool ChannelOptionsChanged { get; set; }

    /// <summary>
    /// Requires <see cref="Handler.LaunchExecutionStatusProgressHandler"/> to execute before getting the value.
    /// </summary>
    public IProgress<LaunchStatus?> Progress { get; set; } = default!;

    /// <summary>
    /// Requires <see cref="Handler.LaunchExecutionGameProcessInitializationHandler"/> to execute before getting the value.
    /// </summary>
    public System.Diagnostics.Process Process { get; set; } = default!;

    public bool TryGetGameFileSystem([NotNullWhen(true)] out IGameFileSystemView? gameFileSystemView)
    {
        lock (syncRoot)
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            if (this.gameFileSystem is not null)
            {
                gameFileSystemView = this.gameFileSystem;
                return true;
            }

            if (!Options.TryGetGameFileSystem(out IGameFileSystem? gameFileSystem))
            {
                Result.Kind = LaunchExecutionResultKind.NoActiveGamePath;
                Result.ErrorMessage = SH.ServiceGameLaunchExecutionGamePathNotValid;
                gameFileSystemView = default;
                return false;
            }

            this.gameFileSystem = gameFileSystem;
            gameFileSystemView = gameFileSystem;
            return true;
        }
    }

    public void PerformGamePathEntrySynchronization()
    {
        lock (syncRoot)
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            DisposableMarshal.DisposeAndClear(ref gameFileSystem);

            if (ViewModel.TryGetTarget(out IViewModelSupportLaunchExecution? viewModel))
            {
                viewModel.SetGamePathEntriesAndSelectedGamePathEntry(Options);
            }
        }
    }

    public void UpdateGamePath(string gamePath)
    {
        lock (syncRoot)
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            DisposableMarshal.DisposeAndClear(ref gameFileSystem);

            Options.GamePath = gamePath;
            PerformGamePathEntrySynchronization();
        }
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        lock (syncRoot)
        {
            disposed = true;
            DisposableMarshal.DisposeAndClear(ref gameFileSystem);
        }
    }
}