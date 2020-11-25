using Godot;
using SpaceDodgeRL.scenes;
using SpaceDodgeRL.scenes.encounter.state;
using System;

public class SaveSlotScene : HBoxContainer {
  private static PackedScene _encounterPrefab = GD.Load<PackedScene>("res://scenes/EncounterScene.tscn");

  private Button _loadButton;
  private Label _lastPlayedLabel;
  private Button _clearButton;

  [Export]
  public int SlotNumber { get; private set; }
  public string SaveLocation { get; private set; }
  public bool HasSaveData { get; private set; }

  public override void _Ready() {
    this._loadButton = this.GetNode<Button>("LoadButton");
    this._loadButton.Connect("pressed", this, nameof(OnSaveSlotPressed));
    this._lastPlayedLabel = this.GetNode<Label>("LastPlayedLabel");
    this._clearButton = this.GetNode<Button>("ClearButton");

    this.SaveLocation = string.Format("user://SaveData_{0}.dat", Name);

    this.RefreshDisplay();
  }

  /**
   * Surprisingly EnterTree happens BEFORE _Ready, and I can't be bothered to go find a non-hacky way to get this to line up.
   * Hence this null check on _loadButton.
   */
  public override void _EnterTree() {
    base._EnterTree();
    if (this._loadButton == null) {
      return;
    } else {
      this.RefreshDisplay();
    }
  }

  private void RefreshDisplay() {
    Godot.File file = new Godot.File();
    file.Open(this.SaveLocation, File.ModeFlags.Read);
    if(file.IsOpen()) {
      this.HasSaveData = true;
    }
    file.Close();

    if (this.HasSaveData) {
      this._loadButton.Text = String.Format("Save Slot {0} - (data)", this.SlotNumber);
      var modified = file.GetModifiedTime(this.SaveLocation);
      this._lastPlayedLabel.Text = String.Format("Last Played: {0}", modified);
      this._clearButton.Disabled = false;
    } else {
      this._loadButton.Text = String.Format("Save Slot {0} - (empty)", this.SlotNumber);
      this._lastPlayedLabel.Text = "Last Played: never";
      this._clearButton.Disabled = true;
    }
  }

  private void OnSaveSlotPressed() {
    if (this.HasSaveData == false) {
      // Initialize new EncounterState, EncounterScene
      var scene = _encounterPrefab.Instance() as EncounterScene;
      var newState = EncounterState.Create(this.SaveLocation);
      newState.SetStateForNewGame();
      scene.SetEncounterState(newState);

      // Save to slot
      Godot.File write = new Godot.File();
      write.Open(this.SaveLocation, File.ModeFlags.Write);
      write.StoreString(newState.ToSaveData());
      write.Close();

      var sceneManager = (SceneManager)GetNode("/root/SceneManager");
      sceneManager.ShowEncounterScene(scene);
    } else {
      var scene = _encounterPrefab.Instance() as EncounterScene;

      Godot.File file = new Godot.File();
      file.Open(this.SaveLocation, File.ModeFlags.Read);
      var saveData = file.GetAsText();
      file.Close();

      var oldState = EncounterState.FromSaveData(saveData);
      scene.SetEncounterState(oldState);

      var sceneManager = (SceneManager)GetNode("/root/SceneManager");
      sceneManager.ShowEncounterScene(scene);
    }
  }
}
