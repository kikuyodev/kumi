﻿using Kumi.Game.Bindables;
using Kumi.Game.Charts.Objects.Windows;
using Kumi.Game.Graphics;
using osu.Framework.Bindables;
using osuTK.Graphics;

namespace Kumi.Game.Charts.Objects;

/// <summary>
/// A representation of a clickable object in a <see cref="IChart" />.
/// A majority of the properties are bindable, so that they can be used in the editor.
/// </summary>
public class Note : INote
{
    /// <summary>
    /// The scale or a note with <see cref="NoteFlags.Big" /> set.
    /// </summary>
    public const float BIG_NOTE_SCALE = 0.0f;

    /// <summary>
    /// The delimiter used to split the input string.
    /// </summary>
    public const char DELIMITER = ',';

    public double StartTime
    {
        get => StartTimeBindable.Value;
        set
        {
            if (StartTimeBindable.Value == value)
                return;

            StartTimeBindable.Value = value;
        }
    }

    public LazyBindable<NoteType> Type { get; } = new LazyBindable<NoteType>();
    public LazyBindable<NoteFlags> Flags { get; } = new LazyBindable<NoteFlags>();
    public LazyBindable<Color4> NoteColor { get; } = new LazyBindable<Color4>();
    public Bindable<double> StartTimeBindable { get; } = new Bindable<double>();

    // TODO: Initialize this somewhere, we're not guaranteed to have a window for every note,
    //       especially when we're just decoding the chart.
    public NoteWindows Windows { get; set; } = null!;

    protected Note(float startTime, NoteType type)
    {
        StartTime = startTime;
        Type.Value = type;
        Type.Bindable.ValueChanged += v => NoteColor.Value = getColorFromType(v.NewValue);
    }

    /// <summary>
    /// A function that applies various defaults to an object within a <see cref="IChart" />.
    /// This is primarily for figuring out which of the two windows to use for a note, between
    /// an upper range and a lower range. The ranges for difficulty are defined in <see cref="NoteWindows" />.
    /// </summary>
    /// <param name="chart"></param>
    public void ApplyChartDefaults(IChart chart)
    {
        Windows ??= new NoteWindows();
        
        // TODO: Set windows based on difficulty rating.
        Windows.ApplyDifficultyRating(0.0f);
    }

    /// <summary>
    /// Gets the total duration of this note.
    /// ...I'm not sure if this is useful, but it's here just in case.
    /// </summary>
    public double GetDuration() => this is IHasEndTime ? (this as IHasEndTime)!.EndTime - StartTime : 0.0f;

    private Color4 getColorFromType(NoteType type)
    {
        switch (type)
        {
            case NoteType.Don:
                return Colours.DON_COLOR;

            case NoteType.Kat:
                return Colours.KAT_COLOR;
        }

        return Color4.White;
    }
}

public static class NoteExtensions
{
    public static double GetEndTime(this INote note)
        => (note as IHasEndTime)?.EndTime ?? note.StartTime;
}
