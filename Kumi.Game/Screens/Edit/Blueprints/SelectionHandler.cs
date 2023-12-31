﻿using Kumi.Game.Charts.Objects;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osuTK;
using osuTK.Input;

namespace Kumi.Game.Screens.Edit.Blueprints;

public partial class SelectionHandler : CompositeDrawable, IKeyBindingHandler<PlatformAction>
{
    [Resolved]
    private EditorChart editorChart { get; set; } = null!;

    [Resolved]
    private EditorHistoryHandler historyHandler { get; set; } = null!;
    
    [Resolved]
    private Editor editor { get; set; } = null!;

    [Resolved]
    private EditorClock editorClock { get; set; } = null!;

    public IReadOnlyList<SelectionBlueprint<Note>> SelectedBlueprints => selectedBlueprints;

    public readonly BindableList<Note> SelectedItems = new BindableList<Note>();

    private readonly List<SelectionBlueprint<Note>> selectedBlueprints;

    public SelectionHandler()
    {
        selectedBlueprints = new List<SelectionBlueprint<Note>>();

        RelativeSizeAxes = Axes.Both;
        AlwaysPresent = true;
    }

    public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;


    public bool OnPressed(KeyBindingPressEvent<PlatformAction> e)
    {
        switch (e.Action)
        {
            case PlatformAction.Delete:
                DeleteSelected();
                return true;
        }

        return false;
    }

    public void OnReleased(KeyBindingReleaseEvent<PlatformAction> e)
    {
    }

    #region Deletion

    protected void DeleteSelected()
    {
        editorChart.RemoveRange(SelectedItems.ToArray());
        DeselectAll();
    }

    #endregion

    #region Selection

    protected void DeselectAll()
        => SelectedItems.Clear();

    internal void HandleSelected(SelectionBlueprint<Note> blueprint)
    {
        if (!SelectedItems.Contains(blueprint.Item))
            SelectedItems.Add(blueprint.Item);

        selectedBlueprints.Add(blueprint);
    }

    internal void HandleDeselected(SelectionBlueprint<Note> blueprint)
    {
        SelectedItems.Remove(blueprint.Item);
        selectedBlueprints.Remove(blueprint);
    }

    internal virtual bool MouseDownSelectionRequested(SelectionBlueprint<Note> blueprint, MouseButtonEvent e)
    {
        if (e.ShiftPressed && e.Button == MouseButton.Right)
        {
            handleQuickDeletion(blueprint);
            return true;
        }
        
        if (e is { ShiftPressed: true, Button: MouseButton.Left } && !blueprint.IsSelected)
        {
            blueprint.ToggleSelection();
            return true;
        }

        if (blueprint.IsSelected)
            return false;

        DeselectAll();
        blueprint.Select();
        return true;
    }

    internal virtual bool MouseUpSelectionRequested(SelectionBlueprint<Note> blueprint, MouseButtonEvent e)
    {
        if (blueprint.IsSelected)
        {
            blueprint.ToggleSelection();
            return true;
        }

        return false;
    }

    private void handleQuickDeletion(SelectionBlueprint<Note> blueprint)
    {
        if (!blueprint.IsSelected)
            DeleteItems(new[] { blueprint.Item });
        else
            DeleteSelected();
    }

    public void DeleteItems(IEnumerable<Note> items)
    {
        editorChart.RemoveRange(items.ToArray());
    }

    #endregion
}
