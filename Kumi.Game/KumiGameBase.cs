﻿using Kumi.Game.Charts;
using Kumi.Game.Database;
using Kumi.Game.Graphics;
using Kumi.Game.Screens;
using Kumi.Game.Screens.Backgrounds;
using Kumi.Resources;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;
using osuTK;

namespace Kumi.Game;

public partial class KumiGameBase : osu.Framework.Game
{
    protected Storage Storage { get; set; }
    
    private RealmAccess realm = null!;
    private ChartManager chartManager = null!;
    protected KumiScreenStack ScreenStack = null!;
    protected Colors GameColors { get; private set; }
    protected override Container<Drawable> Content { get; }

    private DependencyContainer dependencies;
    
    protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
    {
        dependencies = new DependencyContainer(base.CreateChildDependencies(parent));
        return dependencies;
    }

    protected KumiGameBase()
    {
        base.Content.Add(Content = new DrawSizePreservingFillContainer
        {
            TargetDrawSize = new Vector2(1280, 720)
        });
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        // Resources
        Resources.AddStore(new DllResourceStore(KumiResources.Assembly));
        dependencies.Cache(GameColors = new Colors());
        
        // Realm Database & Storage
        dependencies.Cache(realm = new RealmAccess(Storage));
        dependencies.CacheAs(Storage);
        var defaultChart = new DummyWorkingChart(Audio, Textures);
        dependencies.Cache(chartManager = new ChartManager(Storage, realm, Audio, Resources, Host, defaultChart));
        
        dependencies.Cache(ScreenStack = new KumiScreenStack()
        {
            RelativeSizeAxes = Axes.Both
        });
        dependencies.CacheAs(ScreenStack.BackgroundStack);
    }

    public override void SetHost(GameHost host)
    {
        base.SetHost(host);

        Storage ??= host.Storage;
    }
}
