using Godot;
using SpaceDodgeRL.scenes;
using SpaceDodgeRL.scenes.encounter.state;
using System;

public class IntroMenuScene : Control {
  private static PackedScene _encounterPrefab = GD.Load<PackedScene>("res://scenes/EncounterScene.tscn");

  public override void _Ready() {
    var slot1 = GetNode<SaveSlot>("CenterContainer/SaveSlot1");
    slot1.Connect("pressed", this, nameof(OnSaveSlotPressed), new Godot.Collections.Array() { slot1 });
    var slot2 = GetNode<SaveSlot>("CenterContainer/SaveSlot2");
    slot2.Connect("pressed", this, nameof(OnSaveSlotPressed), new Godot.Collections.Array() { slot2 });
    var slot3 = GetNode<SaveSlot>("CenterContainer/SaveSlot3");
    slot3.Connect("pressed", this, nameof(OnSaveSlotPressed), new Godot.Collections.Array() { slot3 });
  }

  private void OnSaveSlotPressed(SaveSlot slot) {
    if (slot.HasSaveData == false) {
      // Initialize new EncounterState, EncounterScene
      var scene = _encounterPrefab.Instance() as EncounterScene;
      var newState = EncounterState.Create(slot.SaveLocation);
      newState.SetStateForNewGame();
      scene.SetEncounterState(newState);

      // Save to slot
      Godot.File write = new Godot.File();
      write.Open(slot.SaveLocation, File.ModeFlags.Write);
      write.StoreString(newState.ToSaveData());
      write.Close();

      var sceneManager = (SceneManager)GetNode("/root/SceneManager");
      sceneManager.ShowEncounterScene(scene);
    } else {
      var scene = _encounterPrefab.Instance() as EncounterScene;

      Godot.File file = new Godot.File();
      file.Open(slot.SaveLocation, File.ModeFlags.Read);
      var saveData = file.GetAsText();
      file.Close();

      var oldState = EncounterState.FromSaveData(saveData);
      oldState.SetStateForNewGame();
      scene.SetEncounterState(oldState);

      var sceneManager = (SceneManager)GetNode("/root/SceneManager");
      sceneManager.ShowEncounterScene(scene);
    }
  }
}
