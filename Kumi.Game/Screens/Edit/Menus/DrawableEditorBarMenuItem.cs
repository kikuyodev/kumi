﻿using Kumi.Game.Graphics;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osuTK.Graphics;
using Box = osu.Framework.Graphics.Shapes.Box;

namespace Kumi.Game.Screens.Edit.Menus;

public partial class DrawableEditorBarMenuItem : osu.Framework.Graphics.UserInterface.Menu.DrawableMenuItem
{
    private Box background = null!;

    protected TextContainer TextContent = null!;

    public DrawableEditorBarMenuItem(MenuItem item)
        : base(item)
    {
        Padding = new MarginPadding(2);
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();

        Item.Action.BindDisabledChanged(_ => updateState(), true);
        FinishTransforms();
    }

    protected override Drawable CreateContent() => TextContent = new TextContainer();

    protected override Drawable CreateBackground() => new Container
    {
        RelativeSizeAxes = Axes.Both,
        Masking = true,
        CornerRadius = 3,
        Children = new Drawable[]
        {
            background = new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = Color4.White.Opacity(0.1f),
            },
        }
    };

    protected override bool OnHover(HoverEvent e)
    {
        updateState();
        return base.OnHover(e);
    }

    protected override void OnHoverLost(HoverLostEvent e)
    {
        updateState();
        base.OnHoverLost(e);
    }

    private void updateState()
    {
        Alpha = IsActionable ? 1 : 0.25f;

        if (IsHovered && IsActionable)
        {
            TextContent.BoldText.FadeIn(100, Easing.OutQuint);
            TextContent.NormalText.FadeOut(100, Easing.OutQuint);
            background.FadeIn(100, Easing.OutQuint);
        }
        else
        {
            TextContent.BoldText.FadeOut(100, Easing.OutQuint);
            TextContent.NormalText.FadeIn(100, Easing.OutQuint);
            background.FadeOut(100, Easing.OutQuint);
        }
    }

    protected partial class TextContainer : Container, IHasText
    {
        public LocalisableString Text
        {
            get => NormalText.Text;
            set
            {
                NormalText.Text = value;
                BoldText.Text = value;
            }
        }

        public readonly SpriteText NormalText;
        public readonly SpriteText BoldText;

        public TextContainer()
        {
            Anchor = Anchor.CentreLeft;
            Origin = Anchor.CentreLeft;

            AutoSizeAxes = Axes.Both;

            Children = new Drawable[]
            {
                NormalText = new SpriteText
                {
                    AlwaysPresent = true,
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Font = KumiFonts.GetFont(size: 12),
                    Margin = new MarginPadding { Horizontal = 8 },
                    Colour = Colours.GRAY_C
                },
                BoldText = new SpriteText
                {
                    AlwaysPresent = true,
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Font = KumiFonts.GetFont(weight: FontWeight.SemiBold, size: 12),
                    Margin = new MarginPadding { Horizontal = 8 },
                    Colour = Colours.GRAY_C
                }
            };
        }
    }
}
