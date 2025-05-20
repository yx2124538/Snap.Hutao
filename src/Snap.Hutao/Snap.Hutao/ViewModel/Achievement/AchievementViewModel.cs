// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using CommunityToolkit.WinUI.Collections;
using Microsoft.UI.Xaml.Controls;
using Snap.Hutao.Core;
using Snap.Hutao.Core.Database;
using Snap.Hutao.Core.ExceptionService;
using Snap.Hutao.Core.IO;
using Snap.Hutao.Core.Logging;
using Snap.Hutao.Model.InterChange.Achievement;
using Snap.Hutao.Service.Achievement;
using Snap.Hutao.Service.Metadata;
using Snap.Hutao.Service.Metadata.ContextAbstraction;
using Snap.Hutao.Service.Navigation;
using Snap.Hutao.Service.Notification;
using Snap.Hutao.UI.Xaml.Data;
using Snap.Hutao.UI.Xaml.View.Dialog;
using System.Collections.Immutable;
using System.Text.RegularExpressions;
using EntityArchive = Snap.Hutao.Model.Entity.AchievementArchive;
using MetadataAchievementGoal = Snap.Hutao.Model.Metadata.Achievement.AchievementGoal;

namespace Snap.Hutao.ViewModel.Achievement;

[ConstructorGenerated]
[Injection(InjectAs.Scoped)]
internal sealed partial class AchievementViewModel : Abstraction.ViewModel, INavigationRecipient
{
    public const string ImportUIAFFromClipboard = nameof(ImportUIAFFromClipboard);

    private readonly SortDescription achievementUncompletedItemsFirstSortDescription = new(nameof(AchievementView.IsChecked), SortDirection.Ascending);
    private readonly SortDescription achievementCompletionTimeSortDescription = new(nameof(AchievementView.Time), SortDirection.Descending);
    private readonly SortDescription achievementGoalUncompletedItemsFirstSortDescription = new(nameof(AchievementGoalView.FinishPercent), SortDirection.Ascending);

    private readonly SortDescription achievementDefaultSortDescription = new(nameof(AchievementView.Order), SortDirection.Ascending);
    private readonly SortDescription achievementGoalDefaultSortDescription = new(nameof(AchievementGoalView.Order), SortDirection.Ascending);

    private readonly AchievementViewModelScopeContext scopeContext;

    public partial RuntimeOptions RuntimeOptions { get; }

    public IAdvancedDbCollectionView<EntityArchive>? Archives
    {
        get;
        set
        {
            AdvancedCollectionViewCurrentChanged.Detach(field, OnCurrentArchiveChanged);
            SetProperty(ref field, value);
            AdvancedCollectionViewCurrentChanged.Attach(value, OnCurrentArchiveChanged);
        }
    }

    public IAdvancedCollectionView<AchievementView>? Achievements { get; set => SetProperty(ref field, value); }

    public IAdvancedCollectionView<AchievementGoalView>? AchievementGoals
    {
        get;
        set
        {
            AdvancedCollectionViewCurrentChanged.Detach(field, OnCurrentAchievementGoalChanged);
            SetProperty(ref field, value);
            AdvancedCollectionViewCurrentChanged.Attach(value, OnCurrentAchievementGoalChanged);
        }
    }

    public string SearchText { get; set => SetProperty(ref field, value); } = string.Empty;

    public bool IsUncompletedItemsFirst { get; set => SetProperty(ref field, value); } = true;

    public bool FilterDailyQuestItems { get; set => SetProperty(ref field, value); }

    public string? FinishDescription { get; set => SetProperty(ref field, value); }

    [GeneratedRegex("\\d\\.\\d")]
    private static partial Regex VersionRegex { get; }

    public async ValueTask<bool> ReceiveAsync(INavigationExtraData data)
    {
        if (!await Initialization.Task.ConfigureAwait(false))
        {
            return false;
        }

        if (data.Data is ImportUIAFFromClipboard)
        {
            await ImportUIAFFromClipboardAsync().ConfigureAwait(false);
            return true;
        }

        return false;
    }

    protected override async ValueTask<bool> LoadOverrideAsync()
    {
        if (!await scopeContext.MetadataService.InitializeAsync().ConfigureAwait(false))
        {
            return false;
        }

        IAdvancedCollectionView<AchievementGoalView> sortedGoals;

        using (await EnterCriticalSectionAsync().ConfigureAwait(false))
        {
            ImmutableArray<MetadataAchievementGoal> goals = await scopeContext.MetadataService
                .GetAchievementGoalArrayAsync(CancellationToken)
                .ConfigureAwait(false);

            sortedGoals = goals.OrderBy(goal => goal.Order).Select(AchievementGoalView.From).ToList().AsAdvancedCollectionView();
        }

        IAdvancedDbCollectionView<EntityArchive> archives = await scopeContext.AchievementService.GetArchiveCollectionAsync(CancellationToken).ConfigureAwait(false);
        await scopeContext.TaskContext.SwitchToMainThreadAsync();

        AchievementGoals = sortedGoals;
        Archives = archives;
        Archives.MoveCurrentTo(Archives.Source.SelectedOrDefault());
        return true;
    }

    protected override void UninitializeOverride()
    {
        using (Archives?.SuppressChangeCurrentItem())
        {
            Archives = default;
        }

        AchievementGoals = default;
    }

    private void OnCurrentArchiveChanged(object? sender, object? e)
    {
        UpdateAchievementsAsync(Archives?.CurrentItem).SafeForget();
    }

    private void OnCurrentAchievementGoalChanged(object? sender, object? e)
    {
        SearchText = string.Empty;
        UpdateAchievementsFilterByGoal(AchievementGoals?.CurrentItem);
    }

    [Command("AddArchiveCommand")]
    private async Task AddArchiveAsync()
    {
        SentrySdk.AddBreadcrumb(BreadcrumbFactory.CreateUI("Add archive", "AchievementViewModel.Command"));

        if (Archives is null)
        {
            return;
        }

        AchievementArchiveCreateDialog dialog = await scopeContext.ContentDialogFactory.CreateInstanceAsync<AchievementArchiveCreateDialog>(scopeContext.ServiceProvider).ConfigureAwait(false);
        (bool isOk, string name) = await dialog.GetInputAsync().ConfigureAwait(false);

        if (!isOk)
        {
            return;
        }

        switch (await scopeContext.AchievementService.AddArchiveAsync(EntityArchive.From(name)).ConfigureAwait(false))
        {
            case ArchiveAddResultKind.Added:
                await scopeContext.TaskContext.SwitchToMainThreadAsync();
                scopeContext.InfoBarService.Success(SH.FormatViewModelAchievementArchiveAdded(name));
                break;
            case ArchiveAddResultKind.InvalidName:
                scopeContext.InfoBarService.Warning(SH.ViewModelAchievementArchiveInvalidName);
                break;
            case ArchiveAddResultKind.AlreadyExists:
                scopeContext.InfoBarService.Warning(SH.FormatViewModelAchievementArchiveAlreadyExists(name));
                break;
            default:
                throw HutaoException.NotSupported();
        }
    }

    [Command("RemoveArchiveCommand")]
    private async Task RemoveArchiveAsync()
    {
        SentrySdk.AddBreadcrumb(BreadcrumbFactory.CreateUI("Remove archive", "AchievementViewModel.Command"));

        if (Archives?.CurrentItem is not { } current)
        {
            return;
        }

        ContentDialogResult result = await scopeContext.ContentDialogFactory
            .CreateForConfirmCancelAsync(
                SH.FormatViewModelAchievementRemoveArchiveTitle(current.Name),
                SH.ViewModelAchievementRemoveArchiveContent)
            .ConfigureAwait(false);

        if (result is not ContentDialogResult.Primary)
        {
            return;
        }

        try
        {
            using (await EnterCriticalSectionAsync().ConfigureAwait(false))
            {
                await scopeContext.AchievementService.RemoveArchiveAsync(current).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    [Command("ExportAsUIAFToFileCommand")]
    private async Task ExportAsUIAFToFileAsync()
    {
        SentrySdk.AddBreadcrumb(BreadcrumbFactory.CreateUI("Export UIAF file", "AchievementViewModel.Command"));

        if (Archives?.CurrentItem is null || Achievements is null)
        {
            return;
        }

        (bool isOk, ValueFile file) = scopeContext.FileSystemPickerInteraction.SaveFile(
            SH.ViewModelAchievementUIAFExportPickerTitle,
            $"{Archives.CurrentItem.Name}.json",
            SH.ViewModelAchievementExportFileType,
            "*.json");

        if (!isOk)
        {
            return;
        }

        UIAF uiaf = await scopeContext.AchievementService.ExportToUIAFAsync(Archives.CurrentItem).ConfigureAwait(false);
        if (await file.SerializeToJsonAsync(uiaf, scopeContext.JsonSerializerOptions).ConfigureAwait(false))
        {
            scopeContext.InfoBarService.Success(SH.ViewModelExportSuccessTitle, SH.ViewModelExportSuccessMessage);
        }
        else
        {
            scopeContext.InfoBarService.Warning(SH.ViewModelExportWarningTitle, SH.ViewModelExportWarningMessage);
        }
    }

    [Command("ImportUIAFFromEmbeddedYaeCommand")]
    private async Task ImportUIAFFromEmbeddedYaeAsync()
    {
        SentrySdk.AddBreadcrumb(BreadcrumbFactory2.CreateUI("Import UIAF", "AchievementViewModel.Command", [("source", "Embedded Yae")]));

        if (await scopeContext.AchievementImporter.FromEmbeddedYaeAsync(scopeContext).ConfigureAwait(false))
        {
            await UpdateAchievementsAsync(Archives?.CurrentItem).ConfigureAwait(false);
        }
    }

    [Command("ImportUIAFFromClipboardCommand")]
    private async Task ImportUIAFFromClipboardAsync()
    {
        SentrySdk.AddBreadcrumb(BreadcrumbFactory2.CreateUI("Import UIAF", "AchievementViewModel.Command", [("source", "Clipboard")]));

        if (await scopeContext.AchievementImporter.FromClipboardAsync(scopeContext).ConfigureAwait(false))
        {
            await UpdateAchievementsAsync(Archives?.CurrentItem).ConfigureAwait(false);
        }
    }

    [Command("ImportUIAFFromFileCommand")]
    private async Task ImportUIAFFromFileAsync()
    {
        SentrySdk.AddBreadcrumb(BreadcrumbFactory2.CreateUI("Import UIAF", "AchievementViewModel.Command", [("source", "File")]));

        if (await scopeContext.AchievementImporter.FromFileAsync(scopeContext).ConfigureAwait(false))
        {
            await UpdateAchievementsAsync(Archives?.CurrentItem).ConfigureAwait(false);
        }
    }

    private async ValueTask UpdateAchievementsAsync(EntityArchive? archive)
    {
        await scopeContext.TaskContext.SwitchToMainThreadAsync();
        Achievements = default;

        if (archive is null)
        {
            return;
        }

        AchievementServiceMetadataContext context = await scopeContext.MetadataService
            .GetContextAsync<AchievementServiceMetadataContext>(CancellationToken)
            .ConfigureAwait(false);

        if (!TryGetAchievements(archive, context, out IAdvancedCollectionView<AchievementView>? combined))
        {
            return;
        }

        await scopeContext.TaskContext.SwitchToMainThreadAsync();
        Achievements = combined;
        AchievementFinishPercent.Update(this);
        UpdateAchievementsFilterByGoal(AchievementGoals?.CurrentItem);
        UpdateAchievementsSort();
    }

    private bool TryGetAchievements(EntityArchive archive, AchievementServiceMetadataContext context, [NotNullWhen(true)] out IAdvancedCollectionView<AchievementView>? view)
    {
        try
        {
            view = scopeContext.AchievementService.GetAchievementViewCollection(archive, context);
            return true;
        }
        catch (HutaoException ex)
        {
            scopeContext.InfoBarService.Error(ex);
            view = default;
            return false;
        }
    }

    [Command("SortUncompletedSwitchCommand")]
    private void UpdateAchievementsSort()
    {
        SentrySdk.AddBreadcrumb(BreadcrumbFactory2.CreateUI("Sort by IsCompleted", "AchievementViewModel.Command", [("value", $"{IsUncompletedItemsFirst}")]));

        if (Achievements is null || AchievementGoals is null)
        {
            return;
        }

        using (Achievements.DeferRefresh())
        {
            using (AchievementGoals.DeferRefresh())
            {
                Achievements.SortDescriptions.Clear();
                AchievementGoals.SortDescriptions.Clear();

                if (IsUncompletedItemsFirst)
                {
                    Achievements.SortDescriptions.Add(achievementUncompletedItemsFirstSortDescription);
                    Achievements.SortDescriptions.Add(achievementCompletionTimeSortDescription);
                    AchievementGoals.SortDescriptions.Add(achievementGoalUncompletedItemsFirstSortDescription);
                }

                Achievements.SortDescriptions.Add(achievementDefaultSortDescription);
                AchievementGoals.SortDescriptions.Add(achievementGoalDefaultSortDescription);
            }
        }
    }

    private void UpdateAchievementsFilterByGoal(AchievementGoalView? goal)
    {
        if (Achievements is null)
        {
            return;
        }

        Achievements.Filter = AchievementFilter.Compile(FilterDailyQuestItems, goal);
    }

    [Command("SearchAchievementCommand")]
    private void UpdateAchievementsFilterBySearch(string? search)
    {
        SentrySdk.AddBreadcrumb(BreadcrumbFactory2.CreateUI("Search", "AchievementViewModel.Command", [("text", search ?? "<null>")]));

        if (Achievements is null || AchievementGoals is null)
        {
            return;
        }

        AchievementGoals.MoveCurrentTo(default);

        if (string.IsNullOrEmpty(search))
        {
            Achievements.Filter = AchievementFilter.Compile(FilterDailyQuestItems);
            AchievementGoals.Filter = AchievementFilter.GoalCompile(Achievements);
            return;
        }

        if (uint.TryParse(search, out uint achievementId))
        {
            Achievements.Filter = AchievementFilter.Compile(FilterDailyQuestItems, achievementId);
            AchievementGoals.Filter = AchievementFilter.GoalCompile(Achievements);
            return;
        }

        if (VersionRegex.IsMatch(search))
        {
            Achievements.Filter = AchievementFilter.CompileForVersion(FilterDailyQuestItems, search);
            AchievementGoals.Filter = AchievementFilter.GoalCompile(Achievements);
            return;
        }

        Achievements.Filter = AchievementFilter.CompileForTitleOrDescription(FilterDailyQuestItems, search);
        AchievementGoals.Filter = AchievementFilter.GoalCompile(Achievements);
    }

    [Command("SaveAchievementCommand")]
    private void SaveAchievement(AchievementView? achievement)
    {
        SentrySdk.AddBreadcrumb(BreadcrumbFactory.CreateUI("Save single achievement", "AchievementViewModel.Command"));

        if (achievement is null)
        {
            return;
        }

        scopeContext.AchievementService.SaveAchievement(achievement);
        AchievementFinishPercent.Update(this);
    }

    [Command("FilterDailyQuestSwitchCommand")]
    private void UpdateAchievementsFilterByDailyQuest()
    {
        SentrySdk.AddBreadcrumb(BreadcrumbFactory2.CreateUI("Filter by IsDailyQuest", "AchievementViewModel.Command", [("value", $"{IsUncompletedItemsFirst}")]));

        if (Achievements is null || AchievementGoals is null)
        {
            return;
        }

        Achievements.Filter = AchievementFilter.Compile(FilterDailyQuestItems);
        AchievementGoals.Filter = AchievementFilter.GoalCompile(Achievements);
    }
}