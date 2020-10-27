using Godot;
using SpaceDodgeRL.scenes.encounter;
using System;
using System.Collections.ObjectModel;

namespace SpaceDodgeRL.scenes {

  public class SceneManager : Node {

    private Viewport root;

    private AutopilotMenu _autopilotMenu;
    private ReadOnlyCollection<EncounterZone> _autopilotMenuZones;

    public override void _Ready() {
      root = this.GetTree().Root;

      this._autopilotMenu = GD.Load<PackedScene>("res://scenes/encounter/AutopilotMenu.tscn").Instance() as AutopilotMenu;
    }

    public void ShowAutopilotMenu(ReadOnlyCollection<EncounterZone> zones) {
      // We're doing this awkward local state workaround because Godot can't serialize ReadOnlyCollection
      // We could get around this by using a List and also making EncounterZone inherit from Godot.Object
      this._autopilotMenuZones = zones;
      CallDeferred(nameof(DeferredShowAutopilotMenu));
    }

    private void DeferredShowAutopilotMenu() {
      var currentScene = root.GetChild(root.GetChildCount() - 1);
      root.RemoveChild(currentScene);
      root.AddChild(_autopilotMenu);
    }
  }
}