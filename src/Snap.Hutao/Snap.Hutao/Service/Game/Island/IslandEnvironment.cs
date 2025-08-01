// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Win32.Foundation;

namespace Snap.Hutao.Service.Game.Island;

internal struct IslandEnvironment
{
    public uint Size;
    public IslandState State;
    public WIN32_ERROR LastError;
    public uint Uid;

    public IslandFunctionOffsets FunctionOffsets;

    public BOOL EnableSetFieldOfView;
    public float FieldOfView;
    public BOOL FixLowFovScene;
    public BOOL DisableFog;
    public BOOL EnableSetTargetFrameRate;
    public int TargetFrameRate;
    public BOOL RemoveOpenTeamProgress;
    public BOOL HideQuestBanner;
    public BOOL DisableEventCameraMove;
    public BOOL DisableShowDamageText;
    public BOOL UsingTouchScreen;
    public BOOL RedirectCombineEntry;
}