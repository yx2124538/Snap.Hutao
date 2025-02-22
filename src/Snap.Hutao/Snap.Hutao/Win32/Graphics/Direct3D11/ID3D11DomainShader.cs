// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Snap.Hutao.Win32.Graphics.Direct3D11;

[SupportedOSPlatform("windows6.1")]
internal static unsafe class ID3D11DomainShader
{
    internal static ref readonly Guid IID
    {
        get => ref MemoryMarshal.AsRef<Guid>([0x08, 0xC5, 0x82, 0xF5, 0x36, 0x0F, 0x0C, 0x49, 0x99, 0x77, 0x31, 0xEE, 0xCE, 0x26, 0x8C, 0xFA]);
    }

    internal readonly struct Vftbl
    {
        internal readonly ID3D11DeviceChild.Vftbl ID3D11DeviceChildVftbl;
    }
}