﻿using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osuTK;

namespace Kumi.Game.Graphics.UserInterface;

// TODO: Replace with a proper implementation instead of copying BasicFileSelector
// Only reason this is copied is because BasicFileSelector didn't include the constructor,
// hence, no way to set the initial path nor the valid extensions.
public partial class KumiFileSelector : FileSelector
{
    protected override DirectorySelectorBreadcrumbDisplay CreateBreadcrumb() => new BasicDirectorySelectorBreadcrumbDisplay();

    protected override Drawable CreateHiddenToggleButton() => new BasicButton
    {
        Size = new Vector2(200, 25),
        Text = "Toggle hidden items",
        Action = ShowHiddenItems.Toggle,
    };

    public KumiFileSelector(string? initialPath = null, string[]? validExtensions = null)
        : base(initialPath, validExtensions)
    {
    }

    protected override DirectorySelectorDirectory CreateDirectoryItem(DirectoryInfo directory, string? displayName = null)
        => new BasicDirectorySelectorDirectory(directory, displayName);

    protected override DirectorySelectorDirectory CreateParentDirectoryItem(DirectoryInfo directory) => new BasicDirectorySelectorParentDirectory(directory);

    protected override ScrollContainer<Drawable> CreateScrollContainer() => new BasicScrollContainer();

    protected override DirectoryListingFile CreateFileItem(FileInfo file) => new BasicFilePiece(file);

    protected override void NotifySelectionError()
    {
        this.FlashColour(Colour4.Red, 300);
    }

    private partial class BasicFilePiece : DirectoryListingFile
    {
        public BasicFilePiece(FileInfo file)
            : base(file)
        {
        }

        protected override IconUsage? Icon
        {
            get
            {
                switch (File.Extension)
                {
                    case ".ogg":
                    case ".mp3":
                    case ".wav":
                        return FontAwesome.Regular.FileAudio;

                    case ".jpg":
                    case ".jpeg":
                    case ".png":
                        return FontAwesome.Regular.FileImage;

                    case ".mp4":
                    case ".avi":
                    case ".mov":
                    case ".flv":
                        return FontAwesome.Regular.FileVideo;

                    default:
                        return FontAwesome.Regular.File;
                }
            }
        }

        protected override SpriteText CreateSpriteText() => new SpriteText
        {
            Font = FrameworkFont.Regular.With(size: FONT_SIZE)
        };
    }
}
