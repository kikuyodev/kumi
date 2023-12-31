﻿using Kumi.Game.Charts;
using Kumi.Game.Charts.Drawables;
using Kumi.Game.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osuTK;
using osuTK.Graphics;

namespace Kumi.Game.Screens.Select.List;

public partial class ChartListItem : CompositeDrawable, IHasContextMenu
{
    private const float height = 50;
    private const float height_selected = 48;

    public readonly ChartInfo ChartInfo;
    public readonly BindableBool Selected = new BindableBool();
    
    public Action? RequestSelect;

    private Container content = null!;

    public ChartListItem(ChartInfo chartInfo)
    {
        ChartInfo = chartInfo;
    }

    private MenuItem[]? selectMenuItems;

    [BackgroundDependencyLoader]
    private void load(SelectScreen select)
    {
        RelativeSizeAxes = Axes.X;
        Width = 0.75f;
        Height = height;
        Masking = true;
        CornerRadius = 5;

        TextFlowContainer textFlowContainer;

        selectMenuItems = select.CreateContextMenuItemsForChart(ChartInfo);
        
        InternalChildren = new Drawable[]
        {
            new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = getDifficultyColor()
            },
            content = new Container
            {
                RelativeSizeAxes = Axes.X,
                Height = height,
                Masking = true,
                CornerRadius = 5,
                EdgeEffect = new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Glow,
                    Colour = getDifficultyColor().Opacity(0f),
                    Radius = 12,
                    Roundness = 5
                },
                Padding = new MarginPadding
                {
                    Right = 28
                },
                Children = new Drawable[]
                {
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Masking = true,
                        CornerRadius = 5,
                        Child = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Colours.Gray(0.05f)
                        },
                    },
                    new FillFlowContainer
                    {
                        Direction = FillDirection.Vertical,
                        RelativeSizeAxes = Axes.Both,
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        Spacing = new Vector2(0, 4),
                        Padding = new MarginPadding
                        {
                            Vertical = 6,
                            Horizontal = 12
                        },
                        Children = new Drawable[]
                        {
                            new FillFlowContainer
                            {
                                Direction = FillDirection.Horizontal,
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Anchor = Anchor.CentreRight,
                                Origin = Anchor.CentreRight,
                                Spacing = new Vector2(8, 0),
                                Children = new Drawable[]
                                {
                                    textFlowContainer = new TextFlowContainer((s) =>
                                    {
                                        s.Font = KumiFonts.GetFont(size: 8);
                                    })
                                    {
                                        AutoSizeAxes = Axes.Both,
                                        Anchor = Anchor.CentreRight,
                                        Origin = Anchor.CentreRight
                                    },
                                    new DifficultyPill(0f)
                                    {
                                        Anchor = Anchor.CentreRight,
                                        Origin = Anchor.CentreRight
                                    },
                                }
                            },
                            new SpriteText
                            {
                                Anchor = Anchor.CentreRight,
                                Origin = Anchor.CentreRight,
                                Text = ChartInfo.DifficultyName,
                                Font = KumiFonts.GetFont(weight: FontWeight.SemiBold, size: 12)
                            }
                        }
                    }
                }
            }
        };

        textFlowContainer.AddText("Charted by ");
        textFlowContainer.AddText(ChartInfo.ChartSet?.Creator?.Username ?? "Unknown", s =>
        {
            s.Colour = Colours.GRAY_9;
            s.Font = KumiFonts.GetFont(weight: FontWeight.SemiBold, size: 8);
        });

        Selected.BindValueChanged(onSelectionChanged, true);
    }

    protected override bool OnClick(ClickEvent e)
    {
        RequestSelect?.Invoke();
        
        return base.OnClick(e);
    }

    private void onSelectionChanged(ValueChangedEvent<bool> val)
    {
        if (val.NewValue)
        {
            this.ResizeWidthTo(0.9f, 200, Easing.OutQuint);
            content.ResizeHeightTo(height_selected, 200, Easing.OutQuint);
            content.FadeEdgeEffectTo(getDifficultyColor().Opacity(0.25f), 200, Easing.OutQuint);
        }
        else
        {
            this.ResizeWidthTo(0.75f, 200, Easing.OutQuint);
            content.ResizeHeightTo(height, 200, Easing.OutQuint);
            content.FadeEdgeEffectTo(getDifficultyColor().Opacity(0.25f), 200, Easing.OutQuint);
        }
    }

    private Color4 getDifficultyColor()
    {
        // TODO
        return Colours.Gray(0.2f);
    }

    public MenuItem[]? ContextMenuItems
    {
        get
        {
            var items = new List<MenuItem>();

            if (selectMenuItems != null)
                items.AddRange(selectMenuItems);

            return items.ToArray();
        }
    }
}
