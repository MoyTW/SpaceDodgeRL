using System;
using Godot;
using SpaceDodgeRL.scenes;

public class SaveSlot : Button {
  private static PackedScene _encounterPrefab = GD.Load<PackedScene>("res://scenes/EncounterScene.tscn");

  private EncounterScene liveState = null;

  public override void _Ready() {
    // TODO: Load from disk
    this.Text += " (Empty)";
  }

  private void OnSaveSlotPressed() {
    if (liveState == null) {
      GetTree().ChangeSceneTo(_encounterPrefab);
    } else {
      throw new NotImplementedException();
    }
  }
}
