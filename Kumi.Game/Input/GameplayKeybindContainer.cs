﻿using Kumi.Game.Input.Bindings;
using osu.Framework.Input.Bindings;

namespace Kumi.Game.Input;

public partial class GameplayKeybindContainer : RealmBackedKeyBindingContainer<GameplayAction>
{
    protected override bool HandleRepeats => false;

    public GameplayKeybindContainer()
        : base(KeybindType.Gameplay, SimultaneousBindingMode.Unique)
    {
    }

    public override IEnumerable<IKeyBinding> DefaultKeyBindings
        => new[]
        {
            new KeyBinding(InputKey.D, GameplayAction.LeftRim),
            new KeyBinding(InputKey.F, GameplayAction.LeftCentre),
            new KeyBinding(InputKey.J, GameplayAction.RightCentre),
            new KeyBinding(InputKey.K, GameplayAction.RightRim),
        };
}
