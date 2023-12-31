﻿using Kumi.Game.Screens.Play;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;

namespace Kumi.Game.Gameplay.Mods;

/// <summary>
/// An abstract class that represents a modification to the gameplay.
/// </summary>
public abstract class Mod
{
    /// <summary>
    /// The name of the mod.
    /// </summary>
    public abstract LocalisableString Name { get; }
    
    /// <summary>
    /// The named tag of the mod, usually an abbreviation of the name.
    /// </summary>
    public abstract string Acronym { get; }
    
    /// <summary>
    /// The icon of the mod.
    /// </summary>
    public abstract IconUsage Icon { get; }

    /// <summary>
    /// A description of the mod, used to explain what the mod does.
    /// </summary>
    public virtual LocalisableString Description => "No description provided.";
    
    /// <summary>
    /// A list of mods that are incompatible with this mod.
    /// </summary>
    public virtual Type[] IncompatibleMods => Type.EmptyTypes;

    /// <summary>
    /// Applies the mod to the player.
    /// </summary>
    public virtual void ApplyToPlayer(Player player)
    {
    }

    public virtual void ApplyToTrack(Track track)
    {
    }
}
