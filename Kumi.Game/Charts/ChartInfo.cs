﻿using System.Diagnostics;
using JetBrains.Annotations;
using Kumi.Game.Database;
using Kumi.Game.Extensions;
using Kumi.Game.Models;
using Kumi.Game.Scoring;
using Newtonsoft.Json;
using Realms;

namespace Kumi.Game.Charts;

[MapTo("Chart")]
public class ChartInfo : RealmObject, IHasGuidPrimaryKey, IChartInfo, IEquatable<ChartInfo>
{
    [PrimaryKey]
    public Guid ID { get; set; }

    public string DifficultyName { get; set; } = string.Empty;

    public ChartMetadata Metadata { get; set; } = null!;

    [Backlink(nameof(ScoreInfo.ChartInfo))]
    public IQueryable<ScoreInfo> Scores { get; } = null!;

    public float InitialScrollSpeed { get; set; }

    [JsonIgnore]
    public int? OnlineID { get; set; }

    public ChartInfo(ChartMetadata? metadata = null)
    {
        ID = Guid.NewGuid();
        Metadata = metadata ?? new ChartMetadata();
    }

    [UsedImplicitly]
    private ChartInfo()
    {
    }

    public ChartSetInfo? ChartSet { get; set; }

    [Ignored]
    public RealmNamedFileUsage? File => ChartSet?.Files.FirstOrDefault(f => f.File.Hash == Hash);

    [Ignored]
    public string? Path => File?.FileName;

    public string Hash { get; set; } = string.Empty;

    public int ChartVersion { get; set; }

    public void UpdateLocalScores(Realm realm)
    {
        foreach (var score in Scores)
            score.ChartInfo = null;

        foreach (var score in realm.All<ScoreInfo>().Where(s => s.ChartHash == Hash))
            score.ChartInfo = this;
    }

    public bool Equals(ChartInfo? other)
    {
        if (ReferenceEquals(this, other)) return true;
        if (other == null) return false;

        return ID == other.ID;
    }

    public bool Equals(IChartInfo? other) => other is ChartInfo c && Equals(c);

    public bool AudioEquals(ChartInfo? other) => other != null &&
                                                 ChartSet != null &&
                                                 other.ChartSet != null &&
                                                 compareFiles(this, other, m => m.AudioFile);

    public bool BackgroundEquals(ChartInfo? other) => other != null &&
                                                      ChartSet != null &&
                                                      other.ChartSet != null &&
                                                      compareFiles(this, other, m => m.BackgroundFile);

    private static bool compareFiles(ChartInfo x, ChartInfo y, Func<ChartMetadata, string> getFilename)
    {
        Debug.Assert(x.ChartSet != null);
        Debug.Assert(y.ChartSet != null);

        var fileHashX = x.ChartSet.GetFile(getFilename(x.Metadata))?.File.Hash;
        var fileHashY = y.ChartSet.GetFile(getFilename(y.Metadata))?.File.Hash;

        return fileHashX == fileHashY;
    }

    #region IChartInfo implementation

    IChartMetadata IChartInfo.Metadata => Metadata;
    IChartSetInfo? IChartInfo.ChartSet => ChartSet;

    #endregion

}
