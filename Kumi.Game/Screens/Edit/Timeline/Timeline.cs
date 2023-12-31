﻿using Kumi.Game.Charts;
using Kumi.Game.Charts.Timings;
using Kumi.Game.Graphics;
using Kumi.Game.Graphics.Containers;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Audio;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Input.Events;
using osuTK;
using osuTK.Input;

namespace Kumi.Game.Screens.Edit.Timeline;

[Cached]
public partial class Timeline : ZoomableScrollContainer, ISnapProvider
{
    public const float HEIGHT = 72;
    
    public readonly BindableBool WaveformVisible = new BindableBool(true);
    
    private readonly Drawable screenContent;
    
    [Resolved]
    private EditorClock editorClock { get; set; } = null!;
    
    [Resolved]
    private EditorChart editorChart { get; set; } = null!;

    private float lastScrollPosition;
    private double lastTrackTime;
    private bool handlingDragInput;
    private bool trackWasPlaying;
    private float defaultTimelineZoom;

    public Timeline(Drawable screenContent)
    {
        this.screenContent = screenContent;
        
        RelativeSizeAxes = Axes.X;
        Height = 94;

        ZoomDuration = 200;
        ZoomEasing = Easing.OutQuint;
        ScrollbarVisible = false;
    }

    private WaveformGraph waveform = null!;

    private TimelineTickDisplay ticks = null!;

    private Container mainContent = null!;

    private Bindable<float> waveformOpacity = null!;

    private double trackLengthForZoom;

    private readonly IBindable<Track> track = new Bindable<Track>();
    
    protected InputManager InputManger { get; private set; } = null!;

    [BackgroundDependencyLoader]
    private void load(IBindable<WorkingChart> chart)
    {
        AddRange(new Drawable[]
        {
            new TimelineTimingPointDisplay
            {
                RelativeSizeAxes = Axes.Both
            },
            mainContent = new Container
            {
                RelativeSizeAxes = Axes.X,
                Height = 72,
                Depth = float.MaxValue,
                Padding = new MarginPadding
                {
                    Vertical = 4
                },
                Children = new[]
                {
                    waveform = new WaveformGraph
                    {
                        RelativeSizeAxes = Axes.Both,
                        BaseColour = Colours.BLUE_LIGHT,
                        LowColour = Colours.BLUE,
                        MidColour = Colours.BLUE_LIGHT,
                        HighColour = Colours.BLUE_LIGHTER
                    },
                    ticks = new TimelineTickDisplay(),
                    screenContent
                }
            }
        });

        waveformOpacity = new Bindable<float>(0.25f);
        
        track.BindTo(editorClock.Track);
        track.BindValueChanged(_ => waveform.Waveform = chart.Value.Waveform, true);

        Zoom = defaultTimelineZoom;
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();
        
        WaveformVisible.BindValueChanged(_ => updateWaveformOpacity());
        waveformOpacity.BindValueChanged(_ => updateWaveformOpacity(), true);

        InputManger = GetContainingInputManager();
    }

    private void updateWaveformOpacity()
        => waveform.FadeTo(WaveformVisible.Value ? waveformOpacity.Value : 0, 200, Easing.OutQuint);

    protected override void Update()
    {
        base.Update();

        if (handlingDragInput)
            handleScrollViaDrag();

        Content.Margin = new MarginPadding { Horizontal = DrawWidth / 2f };
        
        if (editorClock.IsRunning)
            scrollToTrackTime();

        if (editorClock.TrackLength != trackLengthForZoom)
        {
            var minimumZoom = defaultTimelineZoom = getZoomLevelForVisibleMilliseconds(10_000);
            var maximumZoom = getZoomLevelForVisibleMilliseconds(500);

            var initialZoom = Math.Clamp(defaultTimelineZoom, minimumZoom, maximumZoom);
            
            SetupZoom(initialZoom, minimumZoom, maximumZoom);
            
            float getZoomLevelForVisibleMilliseconds(double milliseconds) => Math.Max(1, (float) (editorClock.TrackLength / milliseconds));

            trackLengthForZoom = editorClock.TrackLength;
        }
    }

    protected override bool OnScroll(ScrollEvent e)
    {
        if (e is { AltPressed: false, IsPrecise: false })
            return false;
        
        return base.OnScroll(e);
    }

    protected override void UpdateAfterChildren()
    {
        base.UpdateAfterChildren();
        
        if (handlingDragInput)
            seekTrackToCurrent();
        else if (!editorClock.IsRunning)
        {
            if (Current != lastScrollPosition && editorClock.CurrentTime == lastTrackTime)
                seekTrackToCurrent();
            else
                scrollToTrackTime();
        }

        lastScrollPosition = Current;
        lastTrackTime = editorClock.CurrentTime;
    }

    private void seekTrackToCurrent()
    {
        var target = TimeAtPosition(Current);
        editorClock.Seek(Math.Min(editorClock.TrackLength, target));
    }

    private void scrollToTrackTime()
    {
        if (editorClock.TrackLength == 0)
            return;
        
        if (handlingDragInput)
            editorClock.Stop();

        var position = PositionAtTime(editorClock.CurrentTime);
        ScrollTo(position, false);
    }

    protected override bool OnMouseDown(MouseDownEvent e)
    {
        if (base.OnMouseDown(e))
            beginUserDrag();
        
        return e.Button == MouseButton.Left;
    }

    protected override void OnMouseUp(MouseUpEvent e)
    {
        endUserDrag();
        base.OnMouseUp(e);
    }

    private void beginUserDrag()
    {
        handlingDragInput = true;
        trackWasPlaying = editorClock.IsRunning;
        editorClock.Stop();
    }

    private void endUserDrag()
    {
        handlingDragInput = false;
        if (trackWasPlaying)
            editorClock.Start();
    }

    private void handleScrollViaDrag()
    {
        var mouseX = InputManger.CurrentState.Mouse.Position.X;
        
        if (mouseX > ScreenSpaceDrawQuad.TopRight.X)
            ScrollBy((float) ((mouseX - ScreenSpaceDrawQuad.TopRight.X) / 10 * Clock.ElapsedFrameTime));
        else if (mouseX < ScreenSpaceDrawQuad.TopLeft.X)
            ScrollBy((float)((mouseX - ScreenSpaceDrawQuad.TopLeft.X) / 10 * Clock.ElapsedFrameTime));
    }

    public double VisibleRange => editorClock.TrackLength / Zoom;

    public double TimeAtPosition(float x)
        => x / Content.DrawWidth * editorClock.TrackLength;

    public float PositionAtTime(double time)
        => (float) (time / editorClock.TrackLength * Content.DrawWidth);

    public double TimeAtScreenSpacePosition(Vector2 screenSpacePosition)
        => TimeAtPosition(Content.ToLocalSpace(screenSpacePosition).X);

    public double SnapTime(double time, int beatDivisor)
    {
        // Snap to timing points
        var timingPoint = editorChart.TimingPointHandler.GetTimingPointAt<UninheritedTimingPoint>(time, TimingPointType.Uninherited);
        var beatLength = timingPoint.MillisecondsPerBeat / beatDivisor;
        var beats = (Math.Max(time, 0) - timingPoint.StartTime) / beatLength;

        var roundedBeats = (int) Math.Round(beats, MidpointRounding.AwayFromZero);
        var snappedTime = timingPoint.StartTime + roundedBeats * beatLength;

        if (snappedTime >= 0)
            return snappedTime;

        return snappedTime + beatLength;
    }
}
