// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Snap.Hutao.Win32.Graphics.Direct3D11;

[SupportedOSPlatform("windows6.1")]
internal static unsafe class ID3D11PixelShader
{
    internal static ref readonly Guid IID
    {
        get => ref MemoryMarshal.AsRef<Guid>([0x0D, 0xE4, 0x82, 0xEA, 0xDC, 0x51, 0x33, 0x4F, 0x93, 0xD4, 0xDB, 0x7C, 0x91, 0x25, 0xAE, 0x8C]);
    }

    internal readonly struct Vftbl
    {
        internal readonly ID3D11DeviceChild.Vftbl ID3D11DeviceChildVftbl;
    }
}