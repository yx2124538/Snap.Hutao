// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Web.Hoyolab.Takumi.GameRecord.DailyNote;

internal sealed class AttendanceReward
{
    [JsonPropertyName("status")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public AttendanceRewardStatus Status { get; set; }

    public string StatusFormatted { get => Status.GetLocalizedDescription(); }

    [JsonPropertyName("progress")]
    public int Progress { get; set; }
}