using Kumi.Game.Charts;
using Kumi.Game.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;

namespace Kumi.Game.Screens.Menu;

public partial class MenuMusicOverlay : FillFlowContainer
{
    private readonly SpriteText title;
    private readonly SpriteText artist;

    public MenuMusicOverlay()
    {
        Direction = FillDirection.Vertical;
        AutoSizeAxes = Axes.Both;
        Anchor = Anchor.TopLeft;
        Origin = Anchor.TopLeft;
        Alpha = 0;
        AlwaysPresent = true;

        Children = new Drawable[]
        {
            new SpriteText
            {
                Text = "NOW PLAYING",
                Font = KumiFonts.GetFont(FontFamily.Montserrat, size: 12),
                Margin = new MarginPadding { Bottom = 5 },
                Shadow = true
            },
            title = new SpriteText
            {
                Font = KumiFonts.GetFont(FontFamily.Montserrat, FontWeight.Bold, 20),
                Shadow = true
            },
            artist = new SpriteText
            {
                Font = KumiFonts.GetFont(FontFamily.Montserrat, size: 16),
                Shadow = true
            }
        };
    }

    [BackgroundDependencyLoader]
    private void load(IBindable<WorkingChart> chart)
    {
        chart.ValueChanged += c => chartChanged(c.NewValue);
    }

    private void chartChanged(WorkingChart chart)
    {
        var metadata = chart.Metadata;

        artist.Text = new RomanisableString(metadata.Artist, metadata.ArtistRomanised);
        title.Text = new RomanisableString(metadata.Title, metadata.TitleRomanised);

        Scheduler.AddOnce(() =>
        {
            // 500 delay so that the track can fade in before the overlay does.
            this.Delay(500).FadeIn(1000, Easing.OutQuint).Then(5000).FadeOut(1000, Easing.InQuint);
        });
    }
}
