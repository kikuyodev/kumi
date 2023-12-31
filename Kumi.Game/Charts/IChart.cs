﻿using Kumi.Game.Charts.Events;
using Kumi.Game.Charts.Objects;
using Kumi.Game.Charts.Timings;

namespace Kumi.Game.Charts;

public interface IChart
{
    /// <summary>
    /// The <see cref="ChartInfo" /> of this <see cref="IChart" />.
    /// </summary>
    ChartInfo ChartInfo { get; set; }

    /// <summary>
    /// The <see cref="ChartMetadata" /> of this <see cref="IChart" />.
    /// </summary>
    ChartMetadata Metadata { get; }

    /// <summary>
    /// The <see cref="IEvent" />s that occur throughout this <see cref="IChart" />.
    /// </summary>
    IReadOnlyList<IEvent> Events { get; }

    /// <summary>
    /// A <see cref="TimingPointHandler" /> that handles all timing points in this <see cref="IChart" />.
    /// </summary>
    IReadOnlyList<ITimingPoint> TimingPoints { get; }

    /// <summary>
    /// The <see cref="IReadOnlyList{T}" /> of <see cref="INote" />s in this <see cref="IChart" />.
    /// </summary>
    IReadOnlyList<INote> Notes { get; }
}
