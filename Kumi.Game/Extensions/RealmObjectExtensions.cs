﻿using Kumi.Game.Charts;
using Kumi.Game.Database;
using Realms;

namespace Kumi.Game.Extensions;

public static class RealmObjectExtensions
{
    /// <summary>
    /// Gets a displayable string for a model, typically for exporting and the likes.
    /// </summary>
    public static string GetModelDisplayString<T>(this T model, Realm realm)
        where T : RealmObject, IHasGuidPrimaryKey
    {
        if (!model.IsManaged)
            model = realm.Find<T>(model.ID)!;

        if (model is not IHasGuidPrimaryKey modelPrimary)
            return model.GetType().Name;

        switch (model)
        {
            case ChartSetInfo setInfo:
                return $"{setInfo.Metadata.ArtistRomanised} - {setInfo.Metadata.TitleRomanised} [{setInfo.Creator.Username}]";

            case ChartInfo chart:
                return $"{chart.Metadata.ArtistRomanised} - {chart.Metadata.TitleRomanised} [{chart.Metadata.Creator.Username}] ({chart.DifficultyName})";

            default:
                // Should never happen
                return $"{modelPrimary.GetType().Name} {modelPrimary.ID}";
        }
    }
}
