﻿using Kumi.Game.Audio;
using Kumi.Game.Charts;
using Kumi.Game.Database;
using Kumi.Game.Gameplay.Mods;
using Kumi.Game.Graphics;
using Kumi.Game.Input;
using Kumi.Game.Input.Bindings;
using Kumi.Game.Online;
using Kumi.Game.Online.API;
using Kumi.Game.Overlays;
using Kumi.Game.Scoring;
using Kumi.Game.Screens;
using Kumi.Resources;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Development;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;
using osu.Framework.Threading;
using osuTK;

namespace Kumi.Game;

public partial class KumiGameBase : osu.Framework.Game, ICanAcceptFiles
{
    private readonly List<string> importTasks = new List<string>();
    private ScheduledDelegate? importTask;

    public Storage? Storage { get; private set; }

    private RealmAccess realm = null!;
    private ChartManager chartManager = null!;
    private ScoreManager scoreManager = null!;
    private KeybindStore keybindStore = null!;

    private Container content = null!;

    protected Colours GameColours { get; private set; } = null!;
    protected IAPIConnectionProvider API { get; set; } = null!;
    public Bindable<PlayerActivity> Activity { get; private set; } = null!;
    protected Bindable<WorkingChart> Chart { get; private set; } = null!;
    protected MusicController MusicController { get; private set; } = null!;
    protected ChannelManager ChannelManager { get; private set; } = null!;

    protected KumiScreenStack ScreenStack = null!;

    [Cached]
    [Cached(typeof(IBindableList<Mod>))]
    protected readonly BindableList<Mod> SelectedMods = new BindableList<Mod>();

    private readonly List<ICanAcceptFiles> fileAcceptors = new List<ICanAcceptFiles>();

    protected override Container<Drawable> Content => content;

    protected DependencyContainer DependencyContainer = null!;

    protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
    {
        DependencyContainer = new DependencyContainer(base.CreateChildDependencies(parent));
        return DependencyContainer;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        // Resources
        Resources.AddStore(new DllResourceStore(KumiResources.Assembly));
        DependencyContainer.Cache(GameColours = new Colours());

        Audio.Tracks.AddAdjustment(AdjustableProperty.Volume, new BindableDouble(0.8));

        DependencyContainer.CacheAs(this);

        loadFonts();

        // Realm Database, Storage, and Charts
        DependencyContainer.Cache(realm = new RealmAccess(Storage!, thread: Host.UpdateThread));
        DependencyContainer.CacheAs(Storage);

        // TODO
        API = new APIConnection(DebugUtils.IsDebugBuild
                                    ? new DevelopmentServerConfiguration()
                                    : new ProductionServerConfiguration());

        DependencyContainer.CacheAs(API);
        base.Content.Add((APIConnection) API);

        DependencyContainer.Cache(Activity = new Bindable<PlayerActivity>());

        var defaultChart = new DummyWorkingChart(Audio, Textures);
        DependencyContainer.Cache(chartManager = new ChartManager(Storage!, realm, Audio, Resources, Host, defaultChart));

        Chart = new NonNullableBindable<WorkingChart>(defaultChart);

        DependencyContainer.CacheAs<IBindable<WorkingChart>>(Chart);
        DependencyContainer.CacheAs(Chart);

        DependencyContainer.Cache(scoreManager = new ScoreManager(Storage!, realm));

        var largeStore = new LargeTextureStore(Host.Renderer, Host.CreateTextureLoaderStore(new NamespacedResourceStore<byte[]>(Resources, "Textures")));
        largeStore.AddTextureSource(Host.CreateTextureLoaderStore(new OnlineStore()));
        DependencyContainer.Cache(largeStore);

        GlobalKeybindContainer globalKeybindContainer;

        base.Content.Add(new SafeAreaContainer
        {
            RelativeSizeAxes = Axes.Both,
            Child = new DrawSizePreservingFillContainer() // TODO: Add a way to change the resolution and UI scale dynamically.
            {
                TargetDrawSize = new Vector2(1280, 720),
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    globalKeybindContainer = new GlobalKeybindContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Child = content = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                        }
                    }
                }
            }
        });

        TrackPreviewManager previewManager;
        DependencyContainer.Cache(previewManager = new TrackPreviewManager(chartManager.ChartTrackStore));
        base.Content.Add(previewManager);

        ChartUploadTransmit chartUploadTransmit;
        DependencyContainer.Cache(chartUploadTransmit = new ChartUploadTransmit(realm, API));
        base.Content.Add(chartUploadTransmit);

        ChartDownloadTransmit chartDownloadTransmit;
        DependencyContainer.Cache(chartDownloadTransmit = new ChartDownloadTransmit(realm, API));
        base.Content.Add(chartDownloadTransmit);

        base.Content.Add(MusicController = new MusicController());
        DependencyContainer.CacheAs(MusicController);

        base.Content.Add(ChannelManager = new ChannelManager());
        DependencyContainer.CacheAs(ChannelManager);

        fileAcceptors.Add(chartManager);

        DependencyContainer.Cache(globalKeybindContainer);
        DependencyContainer.Cache(keybindStore = new KeybindStore(realm));
        keybindStore.AssignDefaultsFor(globalKeybindContainer);
        keybindStore.AssignDefaultsFor(new GameplayKeybindContainer());
        keybindStore.RegisterDefaults();
    }

    private void loadFonts()
    {
        // This is placed here first because for whatever reason some fonts that the framework use uses *this* one instead of its own.
        AddFont(Resources, @"Fonts/Inter/Inter");
        AddFont(Resources, @"Fonts/Inter/Inter-Italic");

        AddFont(Resources, @"Fonts/Inter/Inter-Thin");
        AddFont(Resources, @"Fonts/Inter/Inter-ThinItalic");
        AddFont(Resources, @"Fonts/Inter/Inter-ExtraLight");
        AddFont(Resources, @"Fonts/Inter/Inter-ExtraLightItalic");
        AddFont(Resources, @"Fonts/Inter/Inter-Light");
        AddFont(Resources, @"Fonts/Inter/Inter-LightItalic");
        AddFont(Resources, @"Fonts/Inter/Inter-Medium");
        AddFont(Resources, @"Fonts/Inter/Inter-MediumItalic");
        AddFont(Resources, @"Fonts/Inter/Inter-SemiBold");
        AddFont(Resources, @"Fonts/Inter/Inter-SemiBoldItalic");
        AddFont(Resources, @"Fonts/Inter/Inter-Bold");
        AddFont(Resources, @"Fonts/Inter/Inter-BoldItalic");
        AddFont(Resources, @"Fonts/Inter/Inter-ExtraBold");
        AddFont(Resources, @"Fonts/Inter/Inter-ExtraBoldItalic");
        AddFont(Resources, @"Fonts/Inter/Inter-Black");
        AddFont(Resources, @"Fonts/Inter/Inter-BlackItalic");

        AddFont(Resources, @"Fonts/Montserrat/Montserrat-Thin");
        AddFont(Resources, @"Fonts/Montserrat/Montserrat-ThinItalic");
        AddFont(Resources, @"Fonts/Montserrat/Montserrat-ExtraLight");
        AddFont(Resources, @"Fonts/Montserrat/Montserrat-ExtraLightItalic");
        AddFont(Resources, @"Fonts/Montserrat/Montserrat-Light");
        AddFont(Resources, @"Fonts/Montserrat/Montserrat-LightItalic");
        AddFont(Resources, @"Fonts/Montserrat/Montserrat");
        AddFont(Resources, @"Fonts/Montserrat/Montserrat-Italic");
        AddFont(Resources, @"Fonts/Montserrat/Montserrat-Medium");
        AddFont(Resources, @"Fonts/Montserrat/Montserrat-MediumItalic");
        AddFont(Resources, @"Fonts/Montserrat/Montserrat-SemiBold");
        AddFont(Resources, @"Fonts/Montserrat/Montserrat-SemiBoldItalic");
        AddFont(Resources, @"Fonts/Montserrat/Montserrat-Bold");
        AddFont(Resources, @"Fonts/Montserrat/Montserrat-BoldItalic");
        AddFont(Resources, @"Fonts/Montserrat/Montserrat-ExtraBold");
        AddFont(Resources, @"Fonts/Montserrat/Montserrat-ExtraBoldItalic");
        AddFont(Resources, @"Fonts/Montserrat/Montserrat-Black");
        AddFont(Resources, @"Fonts/Montserrat/Montserrat-BlackItalic");
    }

    public async Task Import(params string[] paths)
    {
        if (paths.Length == 0)
            return;

        foreach (var path in paths)
        {
            var file = new FileInfo(path);
            var acceptor = fileAcceptors.FirstOrDefault(a => a.HandledFileExtensions.Contains(file.Extension));
            if (acceptor == null)
                continue;

            await acceptor.Import(path);
        }
    }

    public Task Import(ImportTask[] tasks) => Task.WhenAll(tasks.Select(t => Import(t.Path)));

    public IEnumerable<string> HandledFileExtensions => fileAcceptors.SelectMany(a => a.HandledFileExtensions);

    public void RegisterFileAcceptor(ICanAcceptFiles acceptor) => fileAcceptors.Add(acceptor);

    public void UnregisterFileAcceptor(ICanAcceptFiles acceptor) => fileAcceptors.Remove(acceptor);

    private void handleDragDropPath(string path)
    {
        lock (importTasks)
            importTasks.Add(path);

        if (importTask != null)
            importTask.Cancel();

        importTask = Scheduler.AddDelayed(handlePendingImports, 1000);
    }

    private void handlePendingImports()
    {
        lock (importTasks)
        {
            if (importTasks.Count == 0)
                return;

            Import(importTasks.ToArray()).ConfigureAwait(false);
            importTasks.Clear();
        }
    }

    public override void SetHost(GameHost host)
    {
        base.SetHost(host);

        Storage ??= host.Storage;
        host.ExceptionThrown += onExceptionThrown;

        if (host.Window != null)
        {
            host.Window.DragDrop += handleDragDropPath;
        }
    }

    private bool onExceptionThrown(Exception _)
    {
        return !DebugUtils.IsDebugBuild;
    }

    protected override void Dispose(bool isDisposing)
    {
        base.Dispose(isDisposing);

        realm.Dispose();

        if (Host != null)
            Host.ExceptionThrown -= onExceptionThrown;
    }
}
