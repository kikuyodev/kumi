﻿using Kumi.Game.Graphics;
using Kumi.Game.Graphics.UserInterface;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osuTK;
using osuTK.Input;

namespace Kumi.Game.Overlays.Control;

public abstract partial class Notification : Container
{
    public event Action? Closed;
    public event Func<bool>? Activated;

    public bool IsClosed;

    public LocalisableString? Header { get; set; } = null;
    public LocalisableString? Message { get; set; } = null;
    public bool Read { get; set; }
    protected DateTimeOffset Timestamp { get; } = DateTimeOffset.Now;

    private FillFlowContainer content = null!;

    protected override Container<Drawable> Content => content;

    /// <summary>
    /// Whether the notification can be closed by the user.
    /// </summary>
    public bool Closeable
    {
        get => isClosable.Value;
        set => isClosable.Value = value;
    }

    protected Notification()
    {
        Masking = true;
        CornerRadius = 5;
        RelativeSizeAxes = Axes.X;
        AutoSizeAxes = Axes.Y;
    }
    
    private Bindable<bool> isClosable = new Bindable<bool>(true);
    private KumiIconButton closeButton = null!;

    [BackgroundDependencyLoader]
    private void load()
    {
        InternalChildren = new Drawable[]
        {
            new Box
            {
                Colour = Colours.Gray(0.1f),
                RelativeSizeAxes = Axes.Both
            },
            new Container
            {
                Padding = new MarginPadding(4),
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Child = new GridContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    RowDimensions = new[]
                    {
                        new Dimension(GridSizeMode.AutoSize),
                    },
                    ColumnDimensions = new[]
                    {
                        new Dimension(GridSizeMode.AutoSize),
                        new Dimension(),
                        new Dimension(GridSizeMode.AutoSize),
                    },
                    Content = new[]
                    {
                        new[]
                        {
                            CreateIcon(),
                            new Container
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Padding = new MarginPadding { Horizontal = 8 },
                                Children = new Drawable[]
                                {
                                    content = new FillFlowContainer
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Direction = FillDirection.Vertical,
                                        Spacing = new Vector2(0, 2),
                                    },
                                }
                            },
                            closeButton = new KumiIconButton
                            {
                                Icon = FontAwesome.Solid.TimesCircle,
                                Anchor = Anchor.TopRight,
                                Origin = Anchor.TopRight,
                                Size = new Vector2(16),
                                IconScale = new Vector2(0.75f),
                                IconColour = Colours.GRAY_6,
                                Action = Close
                            }
                        }
                    }
                }
            }
        };
        
        isClosable.BindValueChanged(v =>
        {
            if (closeButton is null)
                return;
            
            if (v.NewValue)
                closeButton.Show();
            else
                closeButton.Hide();
        }, true);

        CreateContent();
    }

    protected abstract Drawable CreateIcon();

    protected virtual void CreateContent()
    {
        content.AddRange(new[]
        {
            new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Horizontal,
                Spacing = new Vector2(4, 0),
                Children = new[]
                {
                    Header is null
                        ? Empty()
                        : new SpriteText
                        {
                            Text = Header.Value,
                            Font = KumiFonts.GetFont(FontFamily.Montserrat, FontWeight.SemiBold),
                        },
                    // For centering
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Child = new SpriteText
                        {
                            Text = Timestamp.ToLocalTime().ToString("HH:mm"),
                            Font = KumiFonts.GetFont(size: 12),
                            Colour = Colours.GRAY_6,
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft
                        }
                    },
                }
            },
            Message is null
                ? Empty()
                : new TextFlowContainer(c => c.Font = KumiFonts.GetFont(size: 12))
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Text = Message.Value,
                    Colour = Colours.GRAY_C
                }
        });
    }

    protected override bool OnMouseDown(MouseDownEvent e)
    {
        if (e.Button != MouseButton.Left)
        {
            Close();
            return true;
        }
        
        return base.OnMouseDown(e);
    }

    protected override bool OnClick(ClickEvent e)
    {
        if (e.Button == MouseButton.Left && Activated?.Invoke() == false)
            return true;
        
        Close();
        return true;
    }

    public void Close() => Close(false);

    public virtual void Close(bool force = false)
    {
        if (IsClosed || (!force && !Closeable))
            return;

        Closed?.Invoke();
        IsClosed = true;

        Schedule(() =>
        {
            this.FadeOut(100);
            Expire();
        });
    }
}
