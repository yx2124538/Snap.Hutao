// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Windows.Storage;

namespace Snap.Hutao.Core.Scripting;

[SuppressMessage("", "SH001", Justification = "IHutaoDiagnostics must be public in order to be exposed to the scripting environment")]
public interface IHutaoDiagnostics
{
    ApplicationDataContainer LocalSetting { get; }

    ValueTask<int> ExecuteSqlAsync(string sql);

    ValueTask<string> GetPathFromApplicationUrlAsync(string url);
}