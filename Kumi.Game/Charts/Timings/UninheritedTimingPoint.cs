﻿using osu.Framework.Bindables;

namespace Kumi.Game.Charts.Timings;

/// <summary>
/// A timing point that doesn't inherit it's properties from previous timing points.
/// </summary>
public class UninheritedTimingPoint : TimingPoint
{
    /// <summary>
    /// The BPM of this timing point.
    /// </summary>
    public float BPM
    {
        get => bpmBindable.Value;
        set => bpmBindable.Value = value;
    }

    /// <summary>
    /// The time signature of this timing point.
    /// </summary>
    public TimeSignature TimeSignature
    {
        get => TimeSignatureBindable.Value;
        set => TimeSignatureBindable.Value = value;
    }

    public readonly Bindable<TimeSignature> TimeSignatureBindable = new Bindable<TimeSignature>(TimeSignature.COMMON_TIME);

    public UninheritedTimingPoint(float time)
        : base(time)
    {
    }

    /// <summary>
    /// The milliseconds necessary to elapse a beat for this timing point.
    /// </summary>
    public float MillisecondsPerBeat => 60000f / BPM * TimeSignature.NoteSubdivision;

    public Bindable<float> GetBindableBPM() => bpmBindable;

    private readonly Bindable<float> bpmBindable = new Bindable<float>();

    public override TimingPoint DeepClone()
        => new UninheritedTimingPoint((float) StartTime)
        {
            PointType = PointType,
            Flags = Flags,
            Volume = Volume,
            RelativeScrollSpeed = RelativeScrollSpeed,
            BPM = BPM,
            TimeSignature = TimeSignature
        };
}
