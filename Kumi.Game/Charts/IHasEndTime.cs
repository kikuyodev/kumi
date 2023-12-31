﻿namespace Kumi.Game.Charts;

public interface IHasEndTime
{
    /// <summary>
    /// The time in milliseconds at which this <see cref="IHasEndTime" /> ends.
    /// </summary>
    double EndTime { get; set; }
}
