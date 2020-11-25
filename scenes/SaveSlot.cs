using System;
using Godot;
using SpaceDodgeRL.scenes;
using SpaceDodgeRL.scenes.encounter.state;

public class SaveSlot : Button {
  private static PackedScene _encounterPrefab = GD.Load<PackedScene>("res://scenes/EncounterScene.tscn");

  private string saveLocation;
  private bool hasSaveData = false;

  public override void _Ready() {
    this.saveLocation = string.Format("user://{0}.dat", Name);

    Godot.File file = new Godot.File();
    file.Open(this.saveLocation, File.ModeFlags.Read);
    if(file.IsOpen()) {
      this.hasSaveData = true;
    }
    file.Close();

    if (this.hasSaveData) {
      this.Text += "(data)";
    } else {
      this.Text += " (Empty)";
    }
  }

  private void OnSaveSlotPressed() {
    if (hasSaveData == false) {
      // Initialize new EncounterState, EncounterScene
      var scene = _encounterPrefab.Instance() as EncounterScene;
      var newState = EncounterState.Create(this.saveLocation);
      newState.SetStateForNewGame();
      scene.SetEncounterState(newState);

      // Save to slot
      Godot.File write = new Godot.File();
      write.Open(this.saveLocation, File.ModeFlags.Write);
      write.StoreString(newState.ToSaveData());
      write.Close();

      var sceneManager = (SceneManager)GetNode("/root/SceneManager");
      sceneManager.ShowEncounterScene(scene);
    } else {
      var scene = _encounterPrefab.Instance() as EncounterScene;

      Godot.File file = new Godot.File();
      file.Open(this.saveLocation, File.ModeFlags.Read);
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
