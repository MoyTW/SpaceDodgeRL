using Godot;
using SpaceDodgeRL.library;
using SpaceDodgeRL.scenes;
using SpaceDodgeRL.scenes.encounter.state;
using SpaceDodgeRL.scenes.singletons;
using System;
using System.Diagnostics;

public class SaveSlotScene : HBoxContainer {
  private static PackedScene _encounterPrefab = GD.Load<PackedScene>("res://scenes/EncounterScene.tscn");

  private Button _loadButton;
  private Label _lastPlayedLabel;
  private Button _clearButton;
  private ConfirmationDialog _clearConfirmationDialog;

  [Export]
  public int SlotNumber { get; private set; }
  public string SaveLocation { get; private set; }
  public bool HasSaveData { get; private set; }

  public override void _Ready() {
    this._loadButton = this.GetNode<Button>("LoadButton");
    this._loadButton.Connect("pressed", this, nameof(OnLoadButtonPressed));
    this._lastPlayedLabel = this.GetNode<Label>("LastPlayedLabel");
    this._clearButton = this.GetNode<Button>("ClearButton");
    this._clearButton.Connect("pressed", this, nameof(OnClearButtonPressed));
    this._clearConfirmationDialog = this.GetNode<ConfirmationDialog>("ClearConfirmationDialog");
    this._clearConfirmationDialog.GetOk().Connect("pressed", this, nameof(OnClearConfirmationDialogAccepted));

    this.SaveLocation = string.Format("user://SaveData_{0}.dat", Name);

    this.RefreshDisplay();
  }

  public void FocusLoadButton() {
    this._loadButton.GrabFocus();
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
    } else {
      this.HasSaveData = false;
    }
    file.Close();

    if (this.HasSaveData) {
      this._loadButton.Text = String.Format("Save Slot {0} - (data)", this.SlotNumber);
      var modified = file.GetModifiedTime(this.SaveLocation);
      var modifiedDate = DateTimeOffset.FromUnixTimeSeconds((long)modified).ToLocalTime().ToString("yyyy-MM-dd");
      this._lastPlayedLabel.Text = String.Format("Last Played: {0}", modifiedDate);
      this._clearButton.Disabled = false;
    } else {
      this._loadButton.Text = String.Format("Save Slot {0} - (empty)", this.SlotNumber);
      this._lastPlayedLabel.Text = "Last Played: never";
      this._clearButton.Disabled = true;
    }
  }

  private void OnLoadButtonPressed() {
    if (this.HasSaveData == false) {
      // Initialize new EncounterState, EncounterScene
      var scene = _encounterPrefab.Instance() as EncounterScene;
      var newState = EncounterState.Create(this.SaveLocation);
      newState.SetStateForNewGame();
      scene.SetEncounterState(newState);

      // Save to slot
      newState.WriteToFile();

      var sceneManager = (SceneManager)GetNode("/root/SceneManager");
      sceneManager.ShowEncounterScene(scene);
    } else {
      var scene = _encounterPrefab.Instance() as EncounterScene;

      Stopwatch stopwatch = new Stopwatch();
      stopwatch.Start();

      Godot.File file = new Godot.File();
      file.Open(this.SaveLocation, File.ModeFlags.Read);
      var saveData = file.GetAsText();
      saveData = StringCompression.DecompressString(saveData);
      file.Close();

      var oldState = EncounterState.FromSaveData(saveData);
      stopwatch.Stop();
      GD.Print("SaveSlotScene load completed, elapsed ms: ", stopwatch.ElapsedMilliseconds);
      scene.SetEncounterState(oldState);

      var sceneManager = (SceneManager)GetNode("/root/SceneManager");
      sceneManager.ShowEncounterScene(scene);
    }
  }

  private void OnClearButtonPressed() {
    this._clearConfirmationDialog.PopupCentered();
    this._clearConfirmationDialog.GetCancel().GrabFocus();
  }

  private void OnClearConfirmationDialogAccepted() {
    var dir = new Directory();
    dir.Remove(this.SaveLocation);
    this.RefreshDisplay();
  }
}
