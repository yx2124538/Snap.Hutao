// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Service.Game.Configuration;

internal enum ChannelOptionsErrorKind
{
    None,
    ConfigurationFileNotFound,
    GamePathNullOrEmpty,
    DeviceNotFound,
    GameContentCorrupted,
    SharingViolation,
}